using ReactiveUI;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.Cells;
using System.Collections.Generic;
using System.Linq;
using System;

namespace CodeHub.iOS.TableViewSources
{
    public class IssueTableViewSource : ReactiveTableViewSource<IssueItemViewModel>
    {
        private static nfloat HintSize = 69.0f;

        public IssueTableViewSource(UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<IssueItemViewModel> collection) 
            : base(tableView, collection, IssueCellView.Key, HintSize)
        {
            tableView.RegisterNibForCellReuse(IssueCellView.Nib, IssueCellView.Key);
        }

        public IssueTableViewSource(UIKit.UITableView tableView) 
            : base(tableView, HintSize)
        {
            tableView.RegisterNibForCellReuse(IssueCellView.Nib, IssueCellView.Key);
        }

        public void SetData(IEnumerable<IssueGroupViewModel> collections)
        {
            Data = collections.Select(x =>
            {
                return new TableSectionInformation<IssueItemViewModel, IssueCellView>(x.Issues, IssueCellView.Key, (float)HintSize)
                {
                    Header = new TableSectionHeader(x.Name)
                };
            }).ToList();
        }
    }
}

