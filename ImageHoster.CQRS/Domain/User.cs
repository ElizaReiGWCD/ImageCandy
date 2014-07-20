using ImageHoster.CQRS.Commands;
using ImageHoster.CQRS.Events;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using Sogyo.CQRS.Library.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.Domain
{
    public class User : AggregateRoot
    {
        private Guid id;
        private List<Guid> GroupInvites { get; set; }

        public User()
        {
            GroupInvites = new List<Guid>();
        }

        public User(Guid id, string username, string password, string salt, string email, string firstname, string lastname, string sex, string about, IEnumerable<string> existingNames)
        {
            GroupInvites = new List<Guid>();

            if (!existingNames.Contains(username))
            {
                this.id = id;
                ApplyChange(new RegisteredUser(id, username, password, salt, email, firstname, lastname, sex, about));
            }
        }

        private void Apply(RegisteredUser e)
        {
            this.id = e.Id;
        }

        private void Apply(AddedInviteToUser e)
        {
            this.GroupInvites.Add(e.GroupId);
        }

        private void Apply(AcceptedInviteUser e)
        {
            this.GroupInvites.Remove(e.GroupId);
        }

        private void Apply(RejectedInviteUser e)
        {
            this.GroupInvites.Remove(e.GroupId);
        }

        public void UpdateUser(Guid id, string password, string firstname, string lastname, string sex, string about)
        {
            ApplyChange(new UpdatedUserInfo(id, password, firstname, lastname, sex, about));
        }

        public void AddInvite(Guid groupId)
        {
            if (!this.GroupInvites.Contains(groupId))
            {
                this.GroupInvites.Add(groupId);
                ApplyChange(new AddedInviteToUser() { GroupId = groupId, UserId = this.id });
            }
        }

        public void AcceptInvite(Guid groupId, ICommandSender bus)
        {
            if(this.GroupInvites.Contains(groupId))
            {
                this.GroupInvites.Remove(groupId);
                bus.Send(new AcceptInviteOfGroup() { UserId = this.id, GroupId = groupId });
                ApplyChange(new AcceptedInviteUser() { UserId = this.id, GroupId = groupId });
            }
        }

        public void RejectInvite(Guid groupId)
        {
            if (this.GroupInvites.Contains(groupId))
            {
                this.GroupInvites.Remove(groupId);
                ApplyChange(new RejectedInviteUser() { UserId = this.id, GroupId = groupId });
            }
        }

        public void ViewNewsPage()
        {
            ApplyChange(new ViewedNewsPage() { UserId = this.Id });
        }

        public override Guid Id
        {
            get { return id; }
            protected set { id = value; }
        }
    }
}
