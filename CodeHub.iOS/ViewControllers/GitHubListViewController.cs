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
        private readonly GitHubList<T> _list;
        private readonly Section _section = new Section();
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
            IApplicationService applicationService = null)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            _list = new GitHubList<T>(applicationService.GitHubClient, uri);

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
                  .Subscribe(x => TableView.TableFooterView = x ? _loading : null));
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
    }
}

