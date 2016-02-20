using System;
using System.Linq;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.ViewControllers;
using UIKit;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeHub.iOS.Services;
using CodeHub.iOS.DialogElements;
using Foundation;

namespace CodeHub.iOS.ViewControllers
{
    public abstract class ViewModelCollectionDrivenDialogViewController : ViewModelDrivenDialogViewController
    {
        private static NSObject _dumb = new NSObject();
        public string NoItemsText { get; set; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name='push'>True if navigation controller should push, false if otherwise</param>
        protected ViewModelCollectionDrivenDialogViewController(bool push = true)
            : base(push, UITableViewStyle.Plain)
        {
            NoItemsText = "No Items";
            EnableSearch = true;
        }

        protected void BindCollection<TElement>(CollectionViewModel<TElement> viewModel, 
                                                Func<TElement, Element> element, bool activateNow = false)
        {
            var weakVm = new WeakReference<CollectionViewModel<TElement>>(viewModel);
            var weakRoot = new WeakReference<RootElement>(Root);

            Action updateDel = () =>
            {
                try
                {
                    IEnumerable<TElement> items = viewModel.Items;
                    var filterFn = viewModel.FilteringFunction;
                    if (filterFn != null)
                        items = filterFn(items);

                    var sortFn = viewModel.SortingFunction;
                    if (sortFn != null)
                        items = sortFn(items);

                    var groupingFn = viewModel.GroupingFunction;
                    IEnumerable<IGrouping<string, TElement>> groupedItems = null;
                    if (groupingFn != null)
                        groupedItems = groupingFn(items);

                    ICollection<Section> newSections;
                    if (groupedItems == null)
                        newSections = RenderList(items, element, weakVm.Get()?.MoreItems);
                    else
                        newSections = RenderGroupedItems(groupedItems, element, weakVm.Get()?.MoreItems);

                    var elements = newSections.Sum(s => s.Elements.Count);
                    if (elements == 0)
                        newSections.Add(new Section { new NoItemsElement(NoItemsText) });

                    weakRoot.Get()?.Reset(newSections);
                }
                catch
                {
                }
            };

            viewModel.Bind(x => x.GroupingFunction).Subscribe(_ => updateDel());
            viewModel.Bind(x => x.FilteringFunction).Subscribe(_ => updateDel());
            viewModel.Bind(x => x.SortingFunction).Subscribe(_ => updateDel());

            //The CollectionViewModel binds all of the collection events from the observablecollection + more
            //So just listen to it.
            viewModel.CollectionChanged += (sender, e) => _dumb.InvokeOnMainThread(updateDel);

            if (activateNow)
                updateDel();
        }

        protected ICollection<Section> RenderList<T>(IEnumerable<T> items, Func<T, Element> select, Action moreAction)
        {
            items = items ?? Enumerable.Empty<T>();
            var sec = new Section();
            sec.AddAll(items.Select(item =>
            {
                try
                {
                    return @select(item);
                }
                catch
                {
                    return null;
                }
            }).Where(x => x != null));

            return RenderSections(new [] { sec }, moreAction);
        }

        protected virtual Section CreateSection(string text)
        {
            return new Section(text);
        }

        protected ICollection<Section> RenderGroupedItems<T>(IEnumerable<IGrouping<string, T>> items, Func<T, Element> select, Action moreAction)
        {
            var sections = new List<Section>();

            if (items != null)
            {
                foreach (var grp in items.ToList())
                {
                    try
                    {
                        var sec = CreateSection(grp.Key);
                        foreach (var element in grp.Select(select).Where(element => element != null))
                            sec.Add(element);

                        if (sec.Elements.Count > 0)
                            sections.Add(sec);
                    }
                    catch 
                    {
                    }
                }
            }

            return RenderSections(sections, moreAction);
        }

        private static ICollection<Section> RenderSections(IEnumerable<Section> sections, Action moreAction)
        {
            var weakAction = new WeakReference<Action>(moreAction);
            ICollection<Section> newSections = new LinkedList<Section>(sections);

            if (moreAction != null)
            {
                var loadMore = new PaginateElement("Load More", "Loading...") { AutoLoadOnVisible = true };
                newSections.Add(new Section { loadMore });
                loadMore.Tapped += async (obj) =>
                {
                    try
                    {
                        NetworkActivity.PushNetworkActive();

                        var a = weakAction.Get();
                        if (a != null)
                            await Task.Run(a);

                        var root = loadMore.GetRootElement();
                        root?.Remove(loadMore.Section, UITableViewRowAnimation.Fade);
                    }
                    catch (Exception e)
                    {
                        AlertDialogService.ShowAlert("Unable to load more!", e.Message);
                    }
                    finally
                    {
                        NetworkActivity.PopNetworkActive();
                    }

                };    
            }

            return newSections;
        }
    }
}

