using ImageHoster.CQRS.Events;
using ImageHoster.CQRS.ReadModel.Dto;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.ReadModel.Eventhandlers
{
    public class UserEventHandler : Handles<RegisteredUser>, Handles<UpdatedUserInfo>, Handles<RejectedInviteUser>, Handles<AcceptedInviteUser>, Handles<AddedInviteToUser>, Handles<ViewedNewsPage>
    {
        public void Handle(RegisteredUser message)
        {
            InMemoryDatabase.users.Add(new UserDto(message.Id, message.Username, message.Password, message.Salt, message.Email, message.FirstName, message.LastName, message.Sex, message.About));
        }

        public void Handle(UpdatedUserInfo message)
        {
            UserDto user = InMemoryDatabase.users.Find(u => u.Id == message.Id);
            user.FirstName = message.FirstName ?? user.FirstName;
            user.Password = message.Password ?? user.Password;
            user.LastName = message.LastName ?? user.LastName;
            user.About = message.About ?? user.About;
            user.Sex = message.Sex ?? user.Sex;
        }

        public void Handle(AddedInviteToUser message)
        {
            UserDto user = InMemoryDatabase.users.Find(u => u.Id == message.UserId);
            user.GroupInvites.Add(InMemoryDatabase.groups.Find(g => g.Id == message.GroupId));
        }

        public void Handle(AcceptedInviteUser message)
        {
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            UserDto user = InMemoryDatabase.users.Find(u => u.Id == message.UserId);
            user.GroupInvites.Remove(group);
            user.Groups.Add(group);
        }

        public void Handle(RejectedInviteUser message)
        {
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            UserDto user = InMemoryDatabase.users.Find(u => u.Id == message.UserId);
            user.GroupInvites.Remove(group);
        }

        public void Handle(ViewedNewsPage message)
        {
            UserDto user = InMemoryDatabase.users.Find(u => u.Id == message.UserId);
            user.NewsCount = 0;
        }
    }
}
