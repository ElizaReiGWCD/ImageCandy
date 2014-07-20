using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sogyo.CQRS.Library.Infrastructure.Persistence
{
    public abstract class ISnapshot
    {
        public Guid SnapshotId { get; set; }
        public int Version { get; set; }
        public List<Event> RestoreEvents { get; private set; }

        public ISnapshot(Guid id, int version)
        {
            SnapshotId = id;
            Version = version;
            RestoreEvents = new List<Event>();
        }

        public abstract void MakeSnapshot(IEnumerable<Guid> aggregates, int version);

        public abstract void RestoreReadModel();
    }
}
