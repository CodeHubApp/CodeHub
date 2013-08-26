using System;
using System.Linq;
using GitHubSharp.Models;
using System.Collections.Generic;
using MonoTouch;
using CodeHub.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Elements;
using CodeHub.ViewControllers;
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

        public override void Update(bool force)
        {
            var response = GetData();
            Model = new ListModel<EventModel> { Data = ExpandConsolidatedEvents(response.Data) };
            Model.More = this.CreateMore(response, ExpandConsolidatedEvents);
        }

        
        private static List<EventModel> ExpandConsolidatedEvents(List<EventModel> events)
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

        protected virtual GitHubResponse<List<EventModel>> GetData(int start = 0, int limit = DataLimit)
        {
            return Application.Client.Users[Username].GetEvents(start, limit);
        }
    }
}