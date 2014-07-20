using ImageHoster.CQRS.Events;
using ImageHoster.CQRS.ReadModel.Dto;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.ReadModel.EFEventhandlers
{
    public class UserEventHandler : Handles<RegisteredUser>, Handles<UpdatedUserInfo>, Handles<RejectedInviteUser>, Handles<AcceptedInviteUser>, Handles<AddedInviteToUser>, Handles<ViewedNewsPage>
    {
        private EntityFrameworkDatabase context;

        public UserEventHandler(string connectionstring)
        {
            context = new EntityFrameworkDatabase(connectionstring);
        }

        public void Handle(RegisteredUser message)
        {
            var user = new UserDto(message.Id, message.Username, message.Password, message.Salt, message.Email, message.FirstName, message.LastName, message.Sex, message.About);
            context.Users.Add(user);
            context.SaveChanges();
        }

        public void Handle(UpdatedUserInfo message)
        {
            var user = context.Users.Find(message.Id);
            user.FirstName = message.FirstName ?? user.FirstName;
            user.Password = message.Password ?? user.Password;
            user.LastName = message.LastName ?? user.LastName;
            user.About = message.About ?? user.About;
            user.Sex = message.Sex ?? user.Sex;

            context.SaveChanges();
        }

        public void Handle(AddedInviteToUser message)
        {
            var user = context.Users.Find(message.UserId);
            var group = context.Groups.Find(message.GroupId);

            user.GroupInvites.Add(group);

            context.SaveChanges();
        }

        public void Handle(AcceptedInviteUser message)
        {
            var user = context.Users.Find(message.UserId);
            var group = context.Groups.Find(message.GroupId);

            user.GroupInvites.Remove(group);
            user.Groups.Add(group);

            context.SaveChanges();
        }

        public void Handle(RejectedInviteUser message)
        {
            var user = context.Users.Find(message.UserId);
            var group = context.Groups.Find(message.GroupId);

            user.GroupInvites.Remove(group);

            context.SaveChanges();
        }

        public void Handle(ViewedNewsPage message)
        {
            var user = context.Users.Find(message.UserId);
            user.NewsCount = 0;

            context.SaveChanges();
        }
    }
}
