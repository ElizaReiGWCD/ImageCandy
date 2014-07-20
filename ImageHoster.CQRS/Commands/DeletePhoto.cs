using Sogyo.CQRS.Library.Infrastructure.Bus;
using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS.Commands
{
    public class DeletePhoto : Command
    {
        public Guid Id { get; set; }
        public Guid PhotoId { get; set; }
        public Guid UserId { get; set; }
        public List<Guid> Albums { get; set; }
        public ICommandSender Bus { get; set; }
    }
}
