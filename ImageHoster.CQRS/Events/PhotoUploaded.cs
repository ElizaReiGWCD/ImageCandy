using ImageHoster.CQRS.Domain;
using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.Events
{
    public class PhotoUploaded : Event
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Filename { get; set; }
        public Privacy Privacy { get; set; }

        public PhotoUploaded(Guid id, Guid user, string filename, Privacy privacy)
        {
            Id = id;
            UserId = user;
            Filename = filename;
            Privacy = privacy;
        }
    }
}
