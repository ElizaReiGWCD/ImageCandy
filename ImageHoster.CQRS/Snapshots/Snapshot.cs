using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using Sogyo.CQRS.Library.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS.Snapshots
{
    public class Snapshot : ISnapshot
    {
        Func<Guid, Event> generator;

        public Snapshot(Func<Guid, Event> generator) : base(Guid.NewGuid(), 0)
        {
            this.generator = generator;
        }

        public override void MakeSnapshot(IEnumerable<Guid> aggregates, int version)
        {
            this.Version = version;

            foreach (var id in aggregates)
            {
                Event e = generator(id);

                if(e != null)
                    RestoreEvents.Add(e);
            }
        }

        public override void RestoreReadModel()
        {
            throw new NotImplementedException();
        }
    }
}
