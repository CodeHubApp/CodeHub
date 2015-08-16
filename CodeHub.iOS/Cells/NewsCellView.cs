using System;
using Foundation;
using UIKit;
using CodeHub.iOS;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Activity;
using System.Collections.Generic;
using CoreText;
using System.Text;

namespace CodeHub.iOS.Cells
{
    public partial class NewsCellView : ReactiveTableViewCell<EventItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("NewsCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("NewsCellView");
        public static readonly UIEdgeInsets EdgeInsets = new UIEdgeInsets(0, 48f, 0, 0);
        public static UIColor LinkColor = Theme.MainTitleColor;
        private bool _isFakeCell;

        public static UIFont LinkFont = UIFont.FromDescriptor(
            UIFont.PreferredSubheadline.FontDescriptor.CreateWithTraits(UIFontDescriptorSymbolicTraits.Bold), 
            UIFont.PreferredSubheadline.PointSize); 

        private static IDictionary<EventItemViewModel.EventType, Octicon> _eventToImage 
        = new Dictionary<EventItemViewModel.EventType, Octicon>
        {
            {EventItemViewModel.EventType.Unknown, Octicon.Alert},
            {EventItemViewModel.EventType.Branch, Octicon.GitBranch},
            {EventItemViewModel.EventType.Comment, Octicon.Comment},
            {EventItemViewModel.EventType.Commit, Octicon.GitCommit},
            {EventItemViewModel.EventType.Delete, Octicon.Trashcan},
            {EventItemViewModel.EventType.Follow, Octicon.Person},
            {EventItemViewModel.EventType.Fork, Octicon.RepoForked},
            {EventItemViewModel.EventType.Gist, Octicon.Gist},
            {EventItemViewModel.EventType.Issue, Octicon.IssueOpened},
            {EventItemViewModel.EventType.Organization, Octicon.Organization},
            {EventItemViewModel.EventType.Public, Octicon.Globe},
            {EventItemViewModel.EventType.PullRequest, Octicon.GitPullRequest},
            {EventItemViewModel.EventType.Repository, Octicon.Repo},
            {EventItemViewModel.EventType.Star, Octicon.Star},
            {EventItemViewModel.EventType.Tag, Octicon.Tag},
            {EventItemViewModel.EventType.Wiki, Octicon.Pencil},
        };

        public class Link
        {
            public NSRange Range;
            public Action Callback;
            public int Id;
        }

        public static NewsCellView Create(bool isFakeCell = false)
        {
            var cell = Nib.Instantiate(null, null).GetValue(0) as NewsCellView;
            cell._isFakeCell = isFakeCell;
            return cell;
        }

        public NewsCellView(IntPtr handle) 
            : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Image.Layer.MasksToBounds = true;
            Image.Layer.CornerRadius = Image.Bounds.Height / 2f;
            ContentView.Opaque = true;
            SeparatorInset = EdgeInsets;
            ActionImage.TintColor = Time.TextColor;

            Header.TextColor = UIColor.FromRGB(41, 41, 41);
            Body.TextColor = UIColor.FromRGB(90, 90, 90);

//            Header.EnabledTextCheckingTypes = MonoTouch.TTTAttributedLabel.NSTextCheckingTypes.NSTextCheckingTypeLink;
//            Body.EnabledTextCheckingTypes = MonoTouch.TTTAttributedLabel.NSTextCheckingTypes.NSTextCheckingTypeLink;

            Header.LinkAttributes = new CTStringAttributes {
                Font = new CTFont(LinkFont.Name, LinkFont.PointSize),
                ForegroundColor = LinkColor.CGColor
            }.Dictionary;

            Body.LinkAttributes = new CTStringAttributes {
                Font = new CTFont(UIFont.PreferredSubheadline.Name, UIFont.PreferredSubheadline.PointSize),
                ForegroundColor = LinkColor.CGColor
            }.Dictionary;

            Header.ActiveLinkAttributes = new CTStringAttributes {
                Font = new CTFont(LinkFont.Name, LinkFont.PointSize),
                ForegroundColor = LinkColor.CGColor,
                UnderlineStyle = CTUnderlineStyle.Single
            }.Dictionary;

            Body.ActiveLinkAttributes = new CTStringAttributes {
                Font = new CTFont(UIFont.PreferredSubheadline.Name, UIFont.PreferredSubheadline.PointSize),
                ForegroundColor = LinkColor.CGColor,
                UnderlineStyle = CTUnderlineStyle.Single
            }.Dictionary;

            this.WhenAnyValue(x => x.ViewModel)
                .IsNotNull()
                .Subscribe(x => {
                    Time.Text = x.CreatedString;

                    ActionImage.Image = _eventToImage.ContainsKey(x.Type) ? 
                        _eventToImage[x.Type].ToImage() : Octicon.Alert.ToImage();

                    List<NewsCellView.Link> headerLinks;
                    Header.Text = CreateAttributedStringFromBlocks(x.HeaderBlocks, out headerLinks);
                    Header.Delegate = new LabelDelegate(headerLinks, w => {});

                    List<NewsCellView.Link> bodyLinks;
                    Body.Text = CreateAttributedStringFromBlocks(x.BodyBlocks, out bodyLinks);
                    Body.Delegate = new LabelDelegate(bodyLinks, w => {});

                    foreach (var b in headerLinks)
                        Header.AddLinkToURL(new NSUrl(b.Id.ToString()), b.Range);

                    foreach (var b in bodyLinks)
                        Body.AddLinkToURL(new NSUrl(b.Id.ToString()), b.Range);
                });

            this.WhenAnyValue(x => x.ViewModel.Avatar)
                .Where(_ => !_isFakeCell)
                .Subscribe(x => Image.SetAvatar(x));
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            ContentView.SetNeedsLayout();
            ContentView.LayoutIfNeeded();
            Header.PreferredMaxLayoutWidth = Header.Frame.Width;
            Body.PreferredMaxLayoutWidth = Body.Frame.Width;
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

        private static NSString CreateAttributedStringFromBlocks(
                IEnumerable<BaseEventsViewModel.TextBlock> blocks,
                out List<NewsCellView.Link> links)
        {
            var sb = new StringBuilder();
            links = new List<NewsCellView.Link>();

            int lengthCounter = 0;
            int i = 0;

            foreach (var b in blocks)
            {
                sb.Append(b.Text);
                var strLength = b.Text.Length;

                var anchorBlock = b as BaseEventsViewModel.AnchorBlock;
                if (anchorBlock != null)
                    links.Add(new NewsCellView.Link { Range = new NSRange(lengthCounter, strLength), Callback = anchorBlock.Tapped, Id = i++ });
                lengthCounter += strLength;
            }

            return new NSString(sb.ToString());
        }
    }
}

