using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS.Events
{
    public class DeletedPhoto : Event
    {
        public Guid Id;
        public Guid PhotoId;
    }
}
