using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using CodeHub.iOS;
using CodeHub.Core.ViewModels.Events;
using ReactiveUI;
using System.Reactive.Linq;
using SDWebImage;
using GitHubSharp.Models;
using Humanizer;

namespace CodeHub.iOS.Cells
{
    public partial class NewsCellView : ReactiveTableViewCell<EventItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("NewsCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("NewsCellView");
        public static readonly UIEdgeInsets EdgeInsets = new UIEdgeInsets(0, 48f, 0, 0);
        public static UIColor LinkColor = Theme.MainTitleColor;
        public static UIFont LinkFont = UIFont.BoldSystemFontOfSize(13f);

        public static UIFont TimeFont
        {
            get { return UIFont.SystemFontOfSize(12f); }
        }

        public static UIFont HeaderFont
        {
            get { return UIFont.SystemFontOfSize(13f); }
        }

        public static UIFont BodyFont
        {
            get { return UIFont.SystemFontOfSize(13f); }
        }

        public NSMutableAttributedString HeaderString { get; private set; }

        public NSMutableAttributedString BodyString { get; private set; }

        public class Link
        {
            public NSRange Range;
            public NSAction Callback;
            public int Id;
        }

        public NewsCellView(IntPtr handle) : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Image.Layer.MasksToBounds = true;
            Image.Layer.CornerRadius = Image.Bounds.Height / 2f;
            ContentView.Opaque = true;

            SeparatorInset = EdgeInsets;
            Body.LinkAttributes = new NSDictionary();
            Body.ActiveLinkAttributes = new NSMutableDictionary();
            Body.ActiveLinkAttributes[MonoTouch.CoreText.CTStringAttributeKey.UnderlineStyle] = NSNumber.FromBoolean(true);
            Body.Lines = 0;
            Body.LineBreakMode = UILineBreakMode.TailTruncation;

            Header.LinkAttributes = new NSDictionary();
            Header.ActiveLinkAttributes = new NSMutableDictionary();
            Header.ActiveLinkAttributes[MonoTouch.CoreText.CTStringAttributeKey.UnderlineStyle] = NSNumber.FromBoolean(true);
            Header.Lines = 2;
            Header.LineBreakMode = UILineBreakMode.TailTruncation;

            ActionImage.TintColor = Time.TextColor;

            // Special for large fonts
//            if (Theme.CurrentTheme.FontSizeRatio > 1.0f)
//            {
//                Header.Font = HeaderFont;
//                Body.Font = BodyFont;
//                Time.Font = TimeFont;
//
//                var timeSectionheight = (float)Math.Ceiling(TimeFont.LineHeight);
//                var timeFrame = Time.Frame;
//                timeFrame.Height = timeSectionheight;
//                Time.Frame = timeFrame;
//
//                var imageFrame = ActionImage.Frame;
//                imageFrame.Y += (timeFrame.Height - imageFrame.Height) / 2f;
//                ActionImage.Frame = imageFrame;
//
//                var headerSectionheight = (float)Math.Ceiling(TimeFont.LineHeight);
//                var headerFrame = Header.Frame;
//                headerFrame.Height = headerSectionheight * 2f + (float)Math.Ceiling(3f * Theme.CurrentTheme.FontSizeRatio);
//                headerFrame.Y = 6 + timeFrame.Height + 5f;
//                Header.Frame = headerFrame;
//
//                var picFrame = Image.Frame;
//                picFrame.Y = 6 + timeFrame.Height + 5f;
//                picFrame.Y += (headerFrame.Height - picFrame.Height) / 2f;
//                Image.Frame = picFrame;
//
//                var bodyFrame = Body.Frame;
//                bodyFrame.Y = headerFrame.Y + headerFrame.Height + 4f;
//                Body.Frame = bodyFrame;
//            }

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    var avatar = x.Event.Actor != null ? x.Event.Actor.AvatarUrl : null;
                    if (avatar != null)
                        Image.SetImage(new NSUrl(x.Event.Actor.AvatarUrl), Images.LoginUserUnknown);
                    else
                        Image.Image = null;

                    ActionImage.Image = ChooseImage(x.Event).ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    Time.Text = x.Event.CreatedAt.UtcDateTime.Humanize();

                    List<NewsCellView.Link> headerLinks;
                    HeaderString = CreateAttributedStringFromBlocks(x.HeaderBlocks, out headerLinks);

                    List<NewsCellView.Link> bodyLinks;
                    BodyString = CreateAttributedStringFromBlocks(x.BodyBlocks, out bodyLinks);

                    this.Header.Text = HeaderString;
                    this.Header.Delegate = new LabelDelegate(headerLinks, w => {});

                    this.Body.Text = BodyString;
                    this.Body.Hidden = BodyString.Length == 0;
                    this.Body.Delegate = new LabelDelegate(bodyLinks, w => {});

                    foreach (var b in headerLinks)
                        this.Header.AddLinkToURL(new NSUrl(b.Id.ToString()), b.Range);

                    foreach (var b in bodyLinks)
                        this.Body.AddLinkToURL(new NSUrl(b.Id.ToString()), b.Range);
                });

        }

        class LabelDelegate : MonoTouch.TTTAttributedLabel.TTTAttributedLabelDelegate 
        {

            private readonly List<Link> _links;
            private readonly Action<NSUrl> _webLinkClicked;

            public LabelDelegate(List<Link> links, Action<NSUrl> webLinkClicked)
            {
                _links = links;
                _webLinkClicked = webLinkClicked;
            }

            public override void DidSelectLinkWithURL (MonoTouch.TTTAttributedLabel.TTTAttributedLabel label, NSUrl url)
            {
                try
                {
                    if (url.ToString().StartsWith("http", StringComparison.Ordinal))
                    {
                        if (_webLinkClicked != null)
                            _webLinkClicked(url);
                    }
                    else
                    {
                        var i = Int32.Parse(url.ToString());
                        _links[i].Callback();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to callback on TTTAttributedLabel: {0}", e.Message);
                }
            }
        }

        private static NSMutableAttributedString CreateAttributedStringFromBlocks(IEnumerable<BaseEventsViewModel.TextBlock> blocks, out List<NewsCellView.Link> links)
        {
            var attributedString = new NSMutableAttributedString();
            links = new List<NewsCellView.Link>();

            int lengthCounter = 0;
            int i = 0;

            foreach (var b in blocks)
            {
                var color = Theme.MainTextColor;
                var font = NewsCellView.BodyFont;
                var anchorBlock = b as BaseEventsViewModel.AnchorBlock;

                if (anchorBlock != null)
                {
                    color = LinkColor;
                    //font = LinkFont.WithSize(LinkFont.PointSize);
                }

                var ctFont = new MonoTouch.CoreText.CTFont(font.Name, font.PointSize);
                var str = new NSAttributedString(b.Text, new MonoTouch.CoreText.CTStringAttributes() { ForegroundColor = color.CGColor, Font = ctFont });
                attributedString.Append(str);
                var strLength = str.Length;

                if (anchorBlock != null)
                {
                    links.Add(new NewsCellView.Link { Range = new NSRange(lengthCounter, strLength), Callback = new NSAction(anchorBlock.Tapped), Id = i++ });
                }

                lengthCounter += strLength;
            }

            return attributedString;
        }


        private static UIImage ChooseImage(EventModel eventModel)
        {
            if (eventModel.PayloadObject is EventModel.CommitCommentEvent)
                return Images.Comment;

            var createEvent = eventModel.PayloadObject as EventModel.CreateEvent;
            if (createEvent != null)
            {
                var createModel = createEvent;
                if (createModel.RefType.Equals("repository"))
                    return Images.Repo;
                if (createModel.RefType.Equals("branch"))
                    return Images.Branch;
                if (createModel.RefType.Equals("tag"))
                    return Images.Tag;
            }
            else if (eventModel.PayloadObject is EventModel.DeleteEvent)
                return Images.Trashcan;
            else if (eventModel.PayloadObject is EventModel.FollowEvent)
                return Images.Following;
            else if (eventModel.PayloadObject is EventModel.ForkEvent)
                return Images.Fork;
            else if (eventModel.PayloadObject is EventModel.ForkApplyEvent)
                return Images.Fork;
            else if (eventModel.PayloadObject is EventModel.GistEvent)
                return Images.Gist;
            else if (eventModel.PayloadObject is EventModel.GollumEvent)
                return Images.Globe;
            else if (eventModel.PayloadObject is EventModel.IssueCommentEvent)
                return Images.Comment;
            else if (eventModel.PayloadObject is EventModel.IssuesEvent)
                return Images.IssueOpened;
            else if (eventModel.PayloadObject is EventModel.MemberEvent)
                return Images.Organization;
            else if (eventModel.PayloadObject is EventModel.PublicEvent)
                return Images.Heart;
            else if (eventModel.PayloadObject is EventModel.PullRequestEvent)
                return Images.PullRequest;
            else if (eventModel.PayloadObject is EventModel.PullRequestReviewCommentEvent)
                return Images.Comment;
            else if (eventModel.PayloadObject is EventModel.PushEvent)
                return Images.Commit;
            else if (eventModel.PayloadObject is EventModel.TeamAddEvent)
                return Images.Organization;
            else if (eventModel.PayloadObject is EventModel.WatchEvent)
                return Images.Star;
            else if (eventModel.PayloadObject is EventModel.ReleaseEvent)
                return Images.Tag;
            return Images.Alert;
        }
    }
}

