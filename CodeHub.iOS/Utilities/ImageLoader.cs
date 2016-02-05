// Copyright 2010-2011 Miguel de Icaza
//
// Based on the TweetStation specific ImageStore
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

using Foundation;
using UIKit;
using CoreGraphics;
using MonoTouch.Dialog.Utilities;
using System.Security.Cryptography;

namespace MonoTouch.Dialog.Utilities 
{
    /// <summary>
    ///    This interface needs to be implemented to be notified when an image
    ///    has been downloaded.   The notification will happen on the UI thread.
    ///    Upon notification, the code should call RequestImage again, this time
    ///    the image will be loaded from the on-disk cache or the in-memory cache.
    /// </summary>
    public interface IImageUpdated {
        void UpdatedImage (Uri uri);
    }

    /// <summary>
    ///   Network image loader, with local file system cache and in-memory cache
    /// </summary>
    /// <remarks>
    ///   By default, using the static public methods will use an in-memory cache
    ///   for 50 images and 4 megs total.   The behavior of the static methods 
    ///   can be modified by setting the public DefaultLoader property to a value
    ///   that the user configured.
    /// 
    ///   The instance methods can be used to create different imageloader with 
    ///   different properties.
    ///  
    ///   Keep in mind that the phone does not have a lot of memory, and using
    ///   the cache with the unlimited value (0) even with a number of items in
    ///   the cache can consume memory very quickly.
    /// 
    ///   Use the Purge method to release all the memory kept in the caches on
    ///   low memory conditions, or when the application is sent to the background.
    /// </remarks>

    public class ImageLoader
    {
        public readonly static string BaseDir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "..");
        const int MaxRequests = 6;
        static string PicDir; 

        // Cache of recently used images
        LRUCache<Uri,UIImage> cache;

        // A list of requests that have been issues, with a list of objects to notify.
        static Dictionary<Uri, List<IImageUpdated>> pendingRequests;

        // A list of updates that have completed, we must notify the main thread about them.
        static HashSet<Uri> queuedUpdates;

        // A queue used to avoid flooding the network stack with HTTP requests
        static Stack<Uri> requestQueue;

        static NSString nsDispatcher = new NSString ("x");

        static MD5CryptoServiceProvider checksum = new MD5CryptoServiceProvider ();

        /// <summary>
        ///    This contains the default loader which is configured to be 50 images
        ///    up to 4 megs of memory.   Assigning to this property a new value will
        ///    change the behavior.   This property is lazyly computed, the first time
        ///    an image is requested.
        /// </summary>
        public static ImageLoader DefaultLoader;

        static ImageLoader ()
        {
            PicDir = Path.Combine (BaseDir, "Library/Caches/Pictures.MonoTouch.Dialog/");

            if (!Directory.Exists (PicDir))
                Directory.CreateDirectory (PicDir);

            pendingRequests = new Dictionary<Uri,List<IImageUpdated>> ();
            queuedUpdates = new HashSet<Uri>();
            requestQueue = new Stack<Uri> ();
        }

        /// <summary>
        ///   Creates a new instance of the image loader
        /// </summary>
        /// <param name="cacheSize">
        /// The maximum number of entries in the LRU cache
        /// </param>
        /// <param name="memoryLimit">
        /// The maximum number of bytes to consume by the image loader cache.
        /// </param>
        public ImageLoader (int cacheSize, int memoryLimit)
        {
            cache = new LRUCache<Uri, UIImage> (cacheSize, memoryLimit, sizer);
        }

        static int sizer (UIImage img)
        {
            var cg = img.CGImage;
            return (int)(cg.BytesPerRow * cg.Height);
        }

        /// <summary>
        ///    Purges the contents of the DefaultLoader
        /// </summary>
        public static void Purge ()
        {
            if (DefaultLoader != null)
                DefaultLoader.PurgeCache ();
        }

        /// <summary>
        ///    Purges the cache of this instance of the ImageLoader, releasing 
        ///    all the memory used by the images in the caches.
        /// </summary>
        public void PurgeCache ()
        {
            lock (cache)
                cache.Purge ();
        }

        static int hex (int v)
        {
            if (v < 10)
                return '0' + v;
            return 'a' + v-10;
        }

        static string md5 (string input)
        {
            var bytes = checksum.ComputeHash (Encoding.UTF8.GetBytes (input));
            var ret = new char [32];
            for (int i = 0; i < 16; i++){
                ret [i*2] = (char)hex (bytes [i] >> 4);
                ret [i*2+1] = (char)hex (bytes [i] & 0xf);
            }
            return new string (ret);
        }

        /// <summary>
        ///   Requests an image to be loaded using the default image loader
        /// </summary>
        /// <param name="uri">
        /// The URI for the image to load
        /// </param>
        /// <param name="notify">
        /// A class implementing the IImageUpdated interface that will be invoked when the image has been loaded
        /// </param>
        /// <returns>
        /// If the image has already been downloaded, or is in the cache, this will return the image as a UIImage.
        /// </returns>
        public static UIImage DefaultRequestImage (Uri uri, IImageUpdated notify)
        {
            if (DefaultLoader == null)
                DefaultLoader = new ImageLoader (50, 4*1024*1024);
            return DefaultLoader.RequestImage (uri, notify);
        }

        /// <summary>
        ///   Requests an image to be loaded from the network
        /// </summary>
        /// <param name="uri">
        /// The URI for the image to load
        /// </param>
        /// <param name="notify">
        /// A class implementing the IImageUpdated interface that will be invoked when the image has been loaded
        /// </param>
        /// <returns>
        /// If the image has already been downloaded, or is in the cache, this will return the image as a UIImage.
        /// </returns>
        public UIImage RequestImage (Uri uri, IImageUpdated notify)
        {
            UIImage ret;

            lock (cache){
                ret = cache [uri];
                if (ret != null)
                    return ret;
            }

            lock (requestQueue){
                if (pendingRequests.ContainsKey (uri)) {
                    if (!pendingRequests [uri].Contains(notify))
                        pendingRequests [uri].Add (notify);
                    return null;
                }               
            }

            string picfile = uri.IsFile ? uri.LocalPath : PicDir + md5 (uri.AbsoluteUri);
            if (File.Exists (picfile)){
                ret = UIImage.FromFile (picfile);
                if (ret != null){
                    lock (cache)
                        cache [uri] = ret;
                    return ret;
                }
            } 
            if (uri.IsFile)
                return null;
            QueueRequest (uri, notify);
            return null;
        }

        static void QueueRequest (Uri uri, IImageUpdated notify)
        {
            if (notify == null)
                throw new ArgumentNullException ("notify");

            lock (requestQueue){
                if (pendingRequests.ContainsKey (uri)){
                    //Util.Log ("pendingRequest: added new listener for {0}", id);
                    pendingRequests [uri].Add (notify);
                    return;
                }
                var slot = new List<IImageUpdated> (4);
                slot.Add (notify);
                pendingRequests [uri] = slot;

                if (picDownloaders >= MaxRequests)
                    requestQueue.Push (uri);
                else {
                    ThreadPool.QueueUserWorkItem (delegate { 
                        try {
                            StartPicDownload (uri); 
                        } catch (Exception e){
                            Console.WriteLine (e);
                        }
                    });
                }
            }
        }

        static bool Download (Uri uri)
        {
            try {
                NSUrlResponse response;
                NSError error;

                var target =  PicDir + md5 (uri.AbsoluteUri);
                var req = new NSUrlRequest (new NSUrl (uri.AbsoluteUri.ToString ()), NSUrlRequestCachePolicy.UseProtocolCachePolicy, 120);
                var data = NSUrlConnection.SendSynchronousRequest (req, out response, out error);
                return data.Save (target, true, out error);
            } catch (Exception e) {
                Console.WriteLine ("Problem with {0} {1}", uri, e);
                return false;
            }
        }

        static long picDownloaders;

        static void StartPicDownload (Uri uri)
        {
            Interlocked.Increment (ref picDownloaders);
            try {
                _StartPicDownload (uri);
            } catch (Exception e){
                Console.Error.WriteLine ("CRITICAL: should have never happened {0}", e);
            }
            //Util.Log ("Leaving StartPicDownload {0}", picDownloaders);
            Interlocked.Decrement (ref picDownloaders);
        }

        static void _StartPicDownload (Uri uri)
        {
            do {
                bool downloaded = false;

                //System.Threading.Thread.Sleep (5000);
                downloaded = Download (uri);
                //if (!downloaded)
                //  Console.WriteLine ("Error fetching picture for {0} to {1}", uri, target);

                // Cluster all updates together
                bool doInvoke = false;

                lock (requestQueue){
                    if (downloaded){
                        queuedUpdates.Add (uri);

                        // If this is the first queued update, must notify
                        if (queuedUpdates.Count == 1)
                            doInvoke = true;
                    } else
                        pendingRequests.Remove (uri);

                    // Try to get more jobs.
                    if (requestQueue.Count > 0){
                        uri = requestQueue.Pop ();
                        if (uri == null){
                            Console.Error.WriteLine ("Dropping request {0} because url is null", uri);
                            pendingRequests.Remove (uri);
                            uri = null;
                        }
                    } else {
                        //Util.Log ("Leaving because requestQueue.Count = {0} NOTE: {1}", requestQueue.Count, pendingRequests.Count);
                        uri = null;
                    }
                }   
                if (doInvoke)
                    nsDispatcher.BeginInvokeOnMainThread (NotifyImageListeners);

            } while (uri != null);
        }

        // Runs on the main thread
        static void NotifyImageListeners ()
        {
            lock (requestQueue){
                foreach (var quri in queuedUpdates){
                    var list = pendingRequests [quri];
                    pendingRequests.Remove (quri);
                    foreach (var pr in list){
                        try {
                            pr.UpdatedImage (quri);
                        } catch (Exception e){
                            Console.WriteLine (e);
                        }
                    }
                }
                queuedUpdates.Clear ();
            }
        }
    }
}