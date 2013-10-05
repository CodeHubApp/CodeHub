using System;
using GitHubSharp.Models;
using System.Collections.Generic;
using MonoTouch;
using CodeFramework.Controllers;
using GitHubSharp;

namespace CodeHub.Controllers
{
    public class EventsController : ListController<EventModel>
    {
        protected const int DataLimit = 30;

        public string Username { get; private set; }

        public EventsController(IListView<EventModel> view, string username)
			: base(view)
        {
            Username = username;
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(CreateRequest(), forceDataRefresh, response => {
                RenderView(ExpandConsolidatedEvents(response.Data), this.CreateMore(response, ExpandConsolidatedEvents));
            });
        }

        
        public static List<EventModel> ExpandConsolidatedEvents(List<EventModel> events)
        {
            //This is a cheap hack to seperate out events that contain more than one peice of information
            var newEvents = new List<EventModel>();
            events.ForEach(x => {
                if (x.PayloadObject is EventModel.PushEvent)
                {
                    //Break down the description
                    var pushEvent = (EventModel.PushEvent)x.PayloadObject;
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
                        Utilities.LogException("Unable to deserialize a 'pushed' event description!", e);
                    }
                }
                else
                {
                    newEvents.Add(x);
                }
            });

            return events;
        }

        protected virtual GitHubRequest<List<EventModel>> CreateRequest(int start = 0, int limit = DataLimit)
        {
            return Application.Client.Users[Username].GetEvents(start, limit);
        }
    }
}