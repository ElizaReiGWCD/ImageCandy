using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS.Events
{
    public class RemovedPhotoFromAlbum : Event
    {
        public Guid AlbumId { get; set; }
        public Guid PhotoId { get; set; }
        public Guid UserId { get; set; }
    }
}
