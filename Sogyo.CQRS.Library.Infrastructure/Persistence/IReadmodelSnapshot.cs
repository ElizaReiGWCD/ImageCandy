using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sogyo.CQRS.Library.Infrastructure.Persistence
{
    public interface IReadmodelSnapshot
    {
        void RestoreReadModel(IDocumentSession session);
        void SaveReadModel(IDocumentSession session);
    }
}
