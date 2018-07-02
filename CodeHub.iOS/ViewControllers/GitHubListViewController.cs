using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core;
using CodeHub.Core.Services;
using CodeHub.Core.Utils;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.Views;
using ReactiveUI;
using Splat;
using UIKit;

namespace CodeHub.iOS.ViewControllers
{
    public abstract class GitHubListViewController<T> : TableViewController
    {
        private readonly LoadingIndicatorView _loading = new LoadingIndicatorView();
        private readonly UIView _emptyView = new UIView();
        private readonly GitHubList<T> _list;
        private readonly Section _section = new Section();
        private readonly Octicon _icon;
        private DialogTableViewSource _source;

        private bool _hasMore = true;
        public bool HasMore
        {
            get => _hasMore;
            private set => this.RaiseAndSetIfChanged(ref _hasMore, value);
        }

        public ReactiveCommand<Unit, IReadOnlyList<T>> LoadMoreCommand { get; }

        protected GitHubListViewController(
            Uri uri,
            Octicon icon = null,
            IDictionary<string, string> parameters = null,
            IApplicationService applicationService = null)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            _list = new GitHubList<T>(applicationService.GitHubClient, uri, parameters);
            _icon = icon ?? Octicon.Octoface;

            LoadMoreCommand = ReactiveCommand.CreateFromTask(
                LoadMore,
                this.WhenAnyValue(x => x.HasMore));
            
            LoadMoreCommand
              .ThrownExceptions
              .Select(error => new UserError("Failed To Load More", error))
              .SelectMany(Interactions.Errors.Handle)
              .Subscribe();

            LoadMoreCommand.Subscribe(ConvertAndAdd);

            Appearing
                .Take(1)
                .Select(_ => Unit.Default)
                .InvokeReactiveCommand(LoadMoreCommand);

            OnActivation(d =>
            {
                d(_source
                  .EndReachedObservable
                  .InvokeReactiveCommand(LoadMoreCommand));

                d(LoadMoreCommand.IsExecuting
                  .Subscribe(x => TableView.TableFooterView = x ? _loading : _emptyView));
            });
        }

        private async Task<IReadOnlyList<T>> LoadMore()
        {
            var result = await _list.Next();
            HasMore = _list.HasMore;
            return result;
        }

        private void ConvertAndAdd(IReadOnlyList<T> items)
        {
            var convertedItems = items
                .Select(item =>
                {
                    try
                    {
                        return ConvertToElement(item);
                    }
                    catch
                    {
                        this.Log().Warn($"Unable to convert {item} to Element!");
                        return null;
                    }
                })
                .Where(x => x != null);

            _section.AddAll(convertedItems, UITableViewRowAnimation.Automatic);

            if (_section.Count == 0 && !HasMore)
            {
                SetEmptyView();
            }
        }

        protected abstract Element ConvertToElement(T item);

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _source = new DialogTableViewSource(TableView);
            _source.Root.Add(_section);

            TableView.EstimatedRowHeight = 64f;
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.Source = _source;
        }

        private void SetEmptyView()
        {
            var emptyListView = new EmptyListView(
                _icon.ToEmptyListImage(),
                "There are no items!") { Alpha = 0 };

            TableView.TableFooterView = new UIView();
            TableView.BackgroundView = emptyListView;

            UIView.Animate(0.8, 0, UIViewAnimationOptions.CurveEaseIn,
                           () => emptyListView.Alpha = 1, null);
        }
    }
}

