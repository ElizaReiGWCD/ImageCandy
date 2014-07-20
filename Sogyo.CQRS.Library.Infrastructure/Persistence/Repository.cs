using System;
using Sogyo.CQRS.Library.Infrastructure.Domain;

namespace Sogyo.CQRS.Library.Infrastructure.Persistence
{
    public class Repository<T> : IRepository<T> where T: AggregateRoot, new() //shortcut you can do as you see fit with new()
    {
        private readonly IEventStore _storage;
        private int NoOfEventsToSave;

        public Repository(IEventStore storage)
        {
            _storage = storage;
            NoOfEventsToSave = 3;
        }

        public void Save(AggregateRoot aggregate, int expectedVersion)
        {
            _storage.SaveEvents(aggregate.Id, aggregate.GetUncommittedChanges(), expectedVersion);
        }

        public T GetById(Guid id)
        {
            var obj = _storage.GetLatestAggregate<T>(id);//lots of ways to do this

            if (obj == null)
                obj = new T();

            var e = _storage.GetEventsForAggregate(id);
            e.Sort((e1, e2) => e1.Version - e2.Version);
            obj.LoadsFromHistory(e);

            if (e.Count > NoOfEventsToSave)
            {
                _storage.SaveLatestAggregate(obj);
            }

            return obj;
        }
    }
}