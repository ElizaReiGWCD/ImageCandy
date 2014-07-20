using ImageHoster.CQRS.Domain;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.Commands
{
    public class UploadPhoto : Command
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Stream File { get; set; }
        public string Filename { get; set; }
        public string ContentType { get; set; }
        public Privacy Privacy { get; set; }
        public ICommandSender Bus { get; set; }

        public UploadPhoto(Guid id, Guid user, Stream file, string filename, string contenttype, Privacy privacy, ICommandSender bus)
        {
            Id = id;
            UserId = user;
            File = file;
            Filename = filename;
            ContentType = contenttype;
            Privacy = privacy;
            Bus = bus;
        }
    }
}
