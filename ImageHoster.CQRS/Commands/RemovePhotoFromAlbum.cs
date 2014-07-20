using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS.Commands
{
    public class RemovePhotoFromAlbum : Command
    {
        public Guid AlbumId { get; set; }
        public Guid PhotoId { get; set; }
        public Guid UserId { get; set; }
    }
}
