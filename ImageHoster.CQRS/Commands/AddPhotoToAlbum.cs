using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.Commands
{
    public class AddPhotoToAlbum : Command
    {
        public Guid Id { get; set; }
        public Guid PhotoId { get; set; }
        public Guid AlbumId { get; set; }
        public Guid UserId { get; set; }
    }
}
