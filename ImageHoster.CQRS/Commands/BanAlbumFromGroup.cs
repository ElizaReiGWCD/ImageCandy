using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS.Commands
{
    public class BanAlbumFromGroup : Command
    {
        public Guid GroupId { get; set; }
        public Guid AlbumId { get; set; }
        public List<Guid> Photos { get; set; }
        public Guid UserId { get; set; }
    }
}
