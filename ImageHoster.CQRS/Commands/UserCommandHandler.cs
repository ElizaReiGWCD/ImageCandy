using ImageHoster.CQRS.Domain;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sogyo.CQRS.Library.Infrastructure.Domain;

namespace ImageHoster.CQRS.Commands
{
    public class UserCommandHandler : Handles<RegisterUser>, Handles<ChangeUserInfo>, Handles<AddInviteToUser>, Handles<UserAcceptsInvite>, Handles<UserRejectsInvite>, Handles<ViewNewsPage>
    {
        private IRepository<User> repo;

        public UserCommandHandler(IRepository<User> repository)
        {
            repo = repository;
        }

        public void Handle(RegisterUser message)
        {
            var item = new User(message.Id, message.Username, message.Password, message.Salt, message.Email, message.FirstName, message.LastName, message.Sex, message.About, message.AlreadyExistingNames);
            repo.Save(item, -1);
        }

        public void Handle(ChangeUserInfo message)
        {
            var item = repo.GetById(message.Id);
            item.UpdateUser(message.Id, message.Password, message.FirstName, message.LastName, message.Sex, message.About);
            repo.Save(item, item.Version);
        }

        public void Handle(AddInviteToUser message)
        {
            var item = repo.GetById(message.UserId);
            item.AddInvite(message.GroupId);
            repo.Save(item, item.Version);
        }

        public void Handle(UserAcceptsInvite message)
        {
            var item = repo.GetById(message.UserId);
            item.AcceptInvite(message.GroupId, message.Bus);
            repo.Save(item, item.Version);
        }

        public void Handle(UserRejectsInvite message)
        {
            var item = repo.GetById(message.UserId);
            item.RejectInvite(message.GroupId);
            repo.Save(item, item.Version);
        }

        public void Handle(ViewNewsPage message)
        {
            var item = repo.GetById(message.UserId);
            item.ViewNewsPage();
            repo.Save(item, item.Version);
        }
    }
}
