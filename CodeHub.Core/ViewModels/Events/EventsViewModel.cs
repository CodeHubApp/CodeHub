using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeFramework.Core.ViewModels;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Events
{
    public abstract class EventsViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly CollectionViewModel<EventModel> _events = new CollectionViewModel<EventModel>();

        public CollectionViewModel<EventModel> Events
        {
            get { return _events; }
        }

        public Task Load(bool forceDataRefresh)
        {
            return Task.Run(() => this.RequestModel(CreateRequest(0, 100), forceDataRefresh, response => {
                Events.Items.Reset(ExpandConsolidatedEvents(response.Data));
                this.CreateMore(response, m => Events.MoreItems = m, d => Events.Items.AddRange(ExpandConsolidatedEvents(d)));
            }));
        }
        
        public static List<EventModel> ExpandConsolidatedEvents(List<EventModel> events)
        {
            //This is a cheap hack to seperate out events that contain more than one peice of information
            var newEvents = new List<EventModel>();
            events.ForEach(x =>
            {
                var pushEvent = x.PayloadObject as EventModel.PushEvent;
                if (pushEvent != null)
                {
                    try
                    {
                        pushEvent.Commits.ForEach(y =>  {
                            var newPushEvent = new EventModel.PushEvent { Commits = new List<EventModel.PushEvent.CommitModel>() };
                            newPushEvent.Commits.Add(y);

                            newEvents.Add(new EventModel { 
                                Type = x.Type, Repo = x.Repo, Public = x.Public, 
                                Org = x.Org, Id = x.Id, CreatedAt = x.CreatedAt, Actor = x.Actor,
                                PayloadObject = newPushEvent
                            });
                        });
                    }
                    catch (Exception e) 
                    {
                        ReportError("Unable to deserialize a 'pushed' event description!", e);
                    }
                }
                else
                {
                    newEvents.Add(x);
                }
            });

            return newEvents;
        }

        protected abstract GitHubRequest<List<EventModel>> CreateRequest(int page, int perPage);
    }
}