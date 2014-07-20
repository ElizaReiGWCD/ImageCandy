using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS.Events
{
    public class MadeAnnouncement : Event
    {
        public Guid GroupId { get; set; }
        public Guid AnnouncerId { get; set; }
        public string Title { get; set; }
        public string Announcement { get; set; }
        public DateTime TimeOfAnnouncement { get; set; }
    }
}
