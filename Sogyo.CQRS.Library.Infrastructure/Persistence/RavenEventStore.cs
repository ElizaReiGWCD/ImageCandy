using Raven.Client.Document;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using Sogyo.CQRS.Library.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sogyo.CQRS.Library.Infrastructure.Persistence
{
    public class RavenEventStore : IEventStore
    {
        private readonly IEventPublisher _publisher;
#if DEBUG
        private readonly DocumentStore raven = new DocumentStore { Url = "http://localhost:62342", DefaultDatabase = "CqrsTest" };
#else
        private readonly DocumentStore raven = new DocumentStore { Url = "http://149.210.162.92:8080/", DefaultDatabase = "leewardcandy" };
#endif
        private long index = 1;

        private class EventDescriptor
        {
            private Event eventData;

            public Event EventData
            {
                get { return eventData; }
                private set { eventData = value; }
            }

            private Guid id;

            public Guid Id
            {
                get { return id; }
                private set { id = value; }
            }

            private Guid aggregateId;

            public Guid AggregateId
            {
                get { return aggregateId; }
                private set { aggregateId = value; }
            }

            private int version;

            public int Version
            {
                get { return version; }
                private set { version = value; }
            }

            private bool saved;

            public bool Saved
            {
                get { return saved; }
                set { saved = value; }
            }

            public EventDescriptor(Guid aggregateId, Event eventData, int version)
            {
                this.eventData = eventData;
                this.version = version;
                this.aggregateId = aggregateId;
                this.id = Guid.NewGuid();

                saved = false;
            }
        }

        public RavenEventStore(IEventPublisher publisher)
        {
            _publisher = publisher;
            raven.Initialize();
            raven.Conventions.CustomizeJsonSerializer = s => s.ContractResolver = new DefaultRavenContractResolver(true) { DefaultMembersSearchFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.SetField | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance };
        }

        public void SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion)
        {
            using (var session = raven.OpenSession())
            {
                var maxVersion = (from desc in session.Query<EventDescriptor>() where desc.AggregateId == aggregateId select desc.Version).ToList();

                if (maxVersion.Count > 0 && maxVersion[maxVersion.Count() - 1] != expectedVersion && expectedVersion != -1)
                {
                    throw new ConcurrencyException();
                }

                var i = expectedVersion;
                foreach (var @event in events)
                {
                    i++;
                    @event.Version = i;
                    session.Store(new EventDescriptor(aggregateId, @event, i));
                }

                session.SaveChanges();

                foreach (var @event in events)
                {
                    _publisher.Publish(@event);
                }
            }
        }

        public T GetLatestAggregate<T>(Guid id) where T : AggregateRoot
        {
            using (var session = raven.OpenSession())
            {
                var aggregate = session.Load<T>(typeof(T).Name + "s/" + id.ToString());
                return aggregate;
            }
        }

        public void SaveLatestAggregate<T>(T aggregate) where T : AggregateRoot
        {
            List<EventDescriptor> events;

            using (var session = raven.OpenSession())
            {
                session.Store(aggregate);
                session.SaveChanges();

                events = (from desc in session.Query<EventDescriptor>() where desc.AggregateId == aggregate.Id select desc).ToList();
            }

            using (var session = raven.OpenSession())
            {
                foreach (var @event in events)
                {
                    var e = session.Load<EventDescriptor>("EventDescriptors/" + @event.Id.ToString());
                    e.Saved = true;
                }

                session.SaveChanges();
            }
        }

        public List<Event> GetEventsForAggregate(Guid aggregateId)
        {
            using (var session = raven.OpenSession())
            {
                var events = from desc in session.Query<EventDescriptor>() where desc.AggregateId == aggregateId && !desc.Saved select desc.EventData;
                return events.ToList();
            }
        }
    }
}
