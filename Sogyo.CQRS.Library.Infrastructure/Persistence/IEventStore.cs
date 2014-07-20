using System;
using System.Collections.Generic;
using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using Sogyo.CQRS.Library.Infrastructure.Domain;

namespace Sogyo.CQRS.Library.Infrastructure.Persistence
{
    public interface IEventStore
    {
        void SaveEvents(Guid aggregateId, IEnumerable<Event> events, int expectedVersion);
        List<Event> GetEventsForAggregate(Guid aggregateId);
        T GetLatestAggregate<T>(Guid id) where T : AggregateRoot;
        void SaveLatestAggregate<T>(T aggregate) where T : AggregateRoot;
    }
}