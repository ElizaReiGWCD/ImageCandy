using ImageHoster.CQRS.Domain;
using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageHoster.CQRS.Events
{
    public class UserRestoreEvent : Event
    {
        public Guid Id { get; set; }
        public List<Guid> GroupInvites { get; set; }
    }

    public class AlbumRestoreEvent : Event
    {
        public Guid id;
        public Privacy privacy;
        public Guid madeBy;
        public List<Guid> photos;
    }

    public class PhotoRestoreEvent : Event
    {
        public Guid Id { get; set; }
        public Privacy Privacy { get; set; }
        public Guid UploadedBy { get; set; }
    }

    public class GroupRestoreEvent : Event
    {
        public Guid Id { get; set; }
        public List<Guid> Members { get; set; }
        public Guid Admin { get; set; }
        public List<Guid> Moderators { get; set; }
        public List<Guid> BannedUsers { get; set; }
        public GroupPrivacy Privacy { get; set; }
        public List<Guid> JoinRequests { get; set; }
    }
}
