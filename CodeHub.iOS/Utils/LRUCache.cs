//
// A simple LRU cache used for tracking the images
//
// Authors:
//   Miguel de Icaza (miguel@gnome.org)
//
// Copyright 2010 Miguel de Icaza
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

namespace MonoTouch.Dialog.Utilities {
	
public class LRUCache<TKey, TValue> where TValue : class, IDisposable  {
	Dictionary<TKey, LinkedListNode <TValue>> dict;
	Dictionary<LinkedListNode<TValue>, TKey> revdict;
	LinkedList<TValue> list;
	int entryLimit, sizeLimit, currentSize;
	Func<TValue,int> slotSizeFunc;
	
	public LRUCache (int entryLimit) : this (entryLimit, 0, null)
	{
	}
		
	public LRUCache (int entryLimit, int sizeLimit, Func<TValue,int> slotSizer)
	{
		list = new LinkedList<TValue> ();
		dict = new Dictionary<TKey, LinkedListNode<TValue>> ();
		revdict = new Dictionary<LinkedListNode<TValue>, TKey> ();
		
		if (sizeLimit != 0 && slotSizer == null)
			throw new ArgumentNullException ("If sizeLimit is set, the slotSizer must be provided");
		
		this.entryLimit = entryLimit;
		this.sizeLimit = sizeLimit;
		this.slotSizeFunc = slotSizer;
	}

	void Evict ()
	{
		var last = list.Last;
		var key = revdict [last];
		
		if (sizeLimit > 0){
			int size = slotSizeFunc (last.Value);
			currentSize -= size;
		}
		
		dict.Remove (key);
		revdict.Remove (last);
		list.RemoveLast ();
		last.Value.Dispose ();
	}

	public void Purge ()
	{
		foreach (var element in list)
			element.Dispose ();
		
		dict.Clear ();
		revdict.Clear ();
		list.Clear ();
		currentSize = 0;
	}

	public TValue this [TKey key] {
		get {
			LinkedListNode<TValue> node;
			
			if (dict.TryGetValue (key, out node)){
				list.Remove (node);
				list.AddFirst (node);

				return node.Value;
			}
			return null;
		}

		set {
			LinkedListNode<TValue> node;
			int size = sizeLimit > 0 ? slotSizeFunc (value) : 0;
			
			if (dict.TryGetValue (key, out node)){
				if (sizeLimit > 0 && node.Value != null){
					int repSize = slotSizeFunc (node.Value);
					currentSize -= repSize;
					currentSize += size;
				}
				
				// If we already have a key, move it to the front
				list.Remove (node);
				list.AddFirst (node);
	
				// Remove the old value
				if (node.Value != null)
					node.Value.Dispose ();
				node.Value = value;
				while (sizeLimit > 0 && currentSize > sizeLimit && list.Count > 1)
					Evict ();
				return;
			}
			if (sizeLimit > 0){
				while (sizeLimit > 0 && currentSize + size > sizeLimit && list.Count > 0)
					Evict ();
			}
			if (dict.Count >= entryLimit)
				Evict ();
			// Adding new node
			node = new LinkedListNode<TValue> (value);
			list.AddFirst (node);
			dict [key] = node;
			revdict [node] = key;
			currentSize += size;
		}
	}

	public override string ToString ()
	{
		return "LRUCache dict={0} revdict={1} list={2}";
	}		
}
}