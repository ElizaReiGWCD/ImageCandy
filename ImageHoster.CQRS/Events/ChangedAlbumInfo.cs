using ImageHoster.CQRS.Domain;
using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.Events
{
    public class ChangedAlbumInfo : Event
    {
        public Guid Id { get; set; }
        public Guid AlbumId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Privacy Privacy { get; set; }
    }
}
