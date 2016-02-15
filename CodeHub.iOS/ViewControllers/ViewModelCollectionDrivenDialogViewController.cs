using System;
using System.Linq;
using MvvmCross.Core.ViewModels;
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

                    if (groupedItems == null)
                        RenderList(items, element, viewModel.MoreItems);
                    else
                        RenderGroupedItems(groupedItems, element, viewModel.MoreItems);
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
            viewModel.CollectionChanged += (sender, e) => InvokeOnMainThread(updateDel);

            if (activateNow)
                updateDel();
        }

        protected void RenderList<T>(IEnumerable<T> items, Func<T, Element> select, Action moreAction)
        {
            var sec = new Section();
            if (items != null)
            {
                foreach (var item in items.ToList())
                {
                    try
                    {
                        var element = select(item);
                        if (element != null)
                            sec.Add(element);
                    }
                    catch
                    {
                    }
                }
            }

            RenderSections(new [] { sec }, moreAction);
        }

        protected virtual Section CreateSection(string text)
        {
            return new Section(text);
        }

        protected void RenderGroupedItems<T>(IEnumerable<IGrouping<string, T>> items, Func<T, Element> select, Action moreAction)
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

            RenderSections(sections, moreAction);
        }

        private void RenderSections(IEnumerable<Section> sections, Action moreAction)
        {
            ICollection<Section> newSections = new LinkedList<Section>();

            foreach (var section in sections)
                newSections.Add(section);

            var elements = newSections.Sum(s => s.Elements.Count);

            //There are no items! We must have filtered them out
            if (elements == 0)
                newSections.Add(new Section { new NoItemsElement(NoItemsText) });

            if (moreAction != null)
            {
                var loadMore = new PaginateElement("Load More", "Loading...") { AutoLoadOnVisible = true };
                newSections.Add(new Section { loadMore });
                loadMore.Tapped += async (obj) =>
                {
                    try
                    {
                        await this.DoWorkNoHudAsync(() => Task.Run(moreAction));
                        if (loadMore.GetRootElement() != null)
                        {
                            var section = loadMore.Section;
                            Root.Remove(section, UITableViewRowAnimation.Fade);
                        }
                    }
                    catch (Exception e)
                    {
                        AlertDialogService.ShowAlert("Unable to load more!".t(), e.Message);
                    }

                };    
            }

            Root.Reset(newSections);
        }

        protected void ShowFilterController(FilterViewController filter)
        {
            var nav = new UINavigationController(filter);
            PresentViewController(nav, true, null);
        }
    }
}

