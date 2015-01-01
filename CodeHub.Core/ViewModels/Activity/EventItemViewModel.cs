using System;
using ReactiveUI;
using GitHubSharp.Models;
using System.Collections.Generic;

namespace CodeHub.Core.ViewModels.Events
{
    public class EventItemViewModel : ReactiveObject
    {
        public EventModel Event { get; private set; }

        public IReadOnlyCollection<BaseEventsViewModel.TextBlock> HeaderBlocks { get; private set; }

        public IReadOnlyCollection<BaseEventsViewModel.TextBlock> BodyBlocks { get; private set; }

        public IReactiveCommand<object> GoToCommand { get; private set; }

        public EventItemViewModel(
            EventModel eventModel, 
            IReadOnlyCollection<BaseEventsViewModel.TextBlock> headerBlocks, 
            IReadOnlyCollection<BaseEventsViewModel.TextBlock> bodyBlocks,
            Action gotoAction = null)
        {
            Event = eventModel;
            HeaderBlocks = headerBlocks ?? new BaseEventsViewModel.TextBlock[0];
            BodyBlocks = bodyBlocks ?? new BaseEventsViewModel.TextBlock[0];
            GoToCommand = ReactiveCommand.Create();

            if (gotoAction != null)
                GoToCommand.Subscribe(x => gotoAction());
        }
    }
}

