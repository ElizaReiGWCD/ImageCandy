using Sogyo.CQRS.Library.Infrastructure.Bus;
using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS.Commands
{
    public class DeleteAlbum : Command
    {
        public Guid Id { get; set; }
        public Guid AlbumId { get; set; }
        public Guid User { get; set; }
        public ICommandSender Bus { get; set; }
    }
}
