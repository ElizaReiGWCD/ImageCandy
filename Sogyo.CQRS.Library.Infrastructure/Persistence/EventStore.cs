﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;

namespace Sogyo.CQRS.Library.Infrastructure.Persistence
{
    public class EventStore : IEventStore
    {
        private readonly IEventPublisher _publisher;

        public Dictionary<Guid, List<EventDescriptor>> EventsStored
        {
            get
            {
                return _current;
            }
        }

        public struct EventDescriptor
        {
            
            public readonly Event EventData;
            public readonly Guid Id;
            public readonly int Version;

            public EventDescriptor(Guid id, Event eventData, int version)
            {
                EventData = eventData;
                Version = version;
                Id = id;
            }
        }

        public EventStore(IEventPublisher publisher)
        {
            _publisher = publisher;
        }

        private readonly Dictionary<Guid, List<EventDescriptor>> _current = new Dictionary<Guid, List<EventDescriptor>>(); 
        
        public void SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion)
        {
            List<EventDescriptor> eventDescriptors;
            if(!_current.TryGetValue(aggregateId, out eventDescriptors))
            {
                eventDescriptors = new List<EventDescriptor>();
                _current.Add(aggregateId,eventDescriptors);
            }
            else if(eventDescriptors[eventDescriptors.Count - 1].Version != expectedVersion && expectedVersion != -1)
            {
                throw new ConcurrencyException();
            }
            var i = expectedVersion;
            foreach (var @event in events)
            {
                i++;
                @event.Version = i;
                eventDescriptors.Add(new EventDescriptor(aggregateId,@event,i));
                _publisher.Publish(@event);
            }
        }

        public  List<Event> GetEventsForAggregate(Guid aggregateId)
        {
            List<EventDescriptor> eventDescriptors;
            if (!_current.TryGetValue(aggregateId, out eventDescriptors))
            {
                throw new AggregateNotFoundException();
            }
            return eventDescriptors.Select(desc => desc.EventData).ToList();
        }

        public T GetLatestAggregate<T>(Guid id) where T : Domain.AggregateRoot
        {
            return null;
        }

        public void SaveLatestAggregate<T>(T aggregate) where T : Domain.AggregateRoot
        {
            
        }
    }
}