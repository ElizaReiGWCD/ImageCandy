using ImageHoster.CQRS.Domain;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using Sogyo.CQRS.Library.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.Commands
{
    public class GroupCommandHandler : Handles<CreateGroup>, Handles<AddMemberToGroup>, Handles<RemoveMemberFromGroup>, Handles<DeleteGroup>, Handles<EditGroupInfo>, Handles<AddModeratorToGroup>,
        Handles<RemoveModeratorFromGroup>, Handles<BanUserFromGroup>, Handles<SubmitJoinRequest>, Handles<AcceptJoinRequest>, Handles<RejectJoinRequest>, Handles<InviteUserToGroup>, Handles<AcceptInviteOfGroup>,
        Handles<MakeAnnouncement>, Handles<AddAlbumToGroup>, Handles<DeleteAlbumFromGroup>, Handles<EditAlbumInGroup>, Handles<AddPhotoToGroup>, Handles<DeletePhotoFromGroup>, Handles<EditPhotoInGroup>, 
        Handles<BanPhotoFromGroup>, Handles<BanAlbumFromGroup>, Handles<RejectAlbumGroup>, Handles<AcceptAlbumGroup>, Handles<AcceptPhotoGroup>, Handles<RejectPhotoGroup>
    {
        private IRepository<Group> repo;

        public GroupCommandHandler(IRepository<Group> repository)
        {
            repo = repository;
        }

        public void Handle(CreateGroup message)
        {
            var item = new Group(message.Id, message.UserId, message.Name, message.Description, message.AlreadyExistingGroups, message.Privacy);

            if(item.Id != Guid.Empty)
                repo.Save(item, -1);
        }

        public void Handle(AddMemberToGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.AddMember(message.Id, message.GroupId, message.UserId);
            repo.Save(item, item.Version);
        }

        public void Handle(RemoveMemberFromGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.RemoveMember(message.Id, message.GroupId, message.UserId);
            repo.Save(item, item.Version);
        }

        public void Handle(DeleteGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.DeleteGroup(message.Id, message.UserId, message.GroupId);
            repo.Save(item, item.Version);
        }

        public void Handle(EditGroupInfo message)
        {
            var item = repo.GetById(message.GroupId);
            item.ChangeGroupInfo(message.Id, message.UserId, message.GroupId, message.Name, message.Description, message.Privacy, message.Bus);
            repo.Save(item, item.Version);
        }

        public void Handle(AddModeratorToGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.AddModerator(message.GroupId, message.UserToAdd, message.UserPerforming);
            repo.Save(item, item.Version);
        }

        public void Handle(RemoveModeratorFromGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.RemoveModerator(message.GroupId, message.UserToAdd, message.UserPerforming);
            repo.Save(item, item.Version);
        }

        public void Handle(BanUserFromGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.BanUser(message.GroupId, message.ActingUser, message.UserToBan);
            repo.Save(item, item.Version);
        }

        public void Handle(SubmitJoinRequest message)
        {
            var item = repo.GetById(message.GroupId);
            item.SubmitJoinRequest(message.UserId);
            repo.Save(item, item.Version);
        }

        public void Handle(AcceptJoinRequest message)
        {
            var item = repo.GetById(message.GroupId);
            item.AcceptJoinRequest(message.UserId, message.ActingUser);
            repo.Save(item, item.Version);
        }

        public void Handle(RejectJoinRequest message)
        {
            var item = repo.GetById(message.GroupId);
            item.RejectJoinRequest(message.UserId, message.ActingUser);
            repo.Save(item, item.Version);
        }

        public void Handle(InviteUserToGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.InviteUser(message.UserId, message.ActingUser, message.Bus);
        }

        public void Handle(AcceptInviteOfGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.AcceptInvite(message.UserId);
            repo.Save(item, item.Version);
        }

        public void Handle(MakeAnnouncement message)
        {
            var item = repo.GetById(message.GroupId);
            item.MakeAnnouncement(message.AnnouncerId, message.Title, message.Announcement, message.TimeOfAnnouncement);
            repo.Save(item, item.Version);
        }

        public void Handle(AddAlbumToGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.AddAlbum(message.AlbumId);
            repo.Save(item, item.Version);
        }

        public void Handle(DeleteAlbumFromGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.RemoveAlbum(message.AlbumId);
            repo.Save(item, item.Version);
        }

        public void Handle(EditAlbumInGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.EditAlbum(message.AlbumId, message.Privacy);
            repo.Save(item, item.Version);
        }

        public void Handle(AddPhotoToGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.AddPhoto(message.PhotoId);
            repo.Save(item, item.Version);
        }

        public void Handle(DeletePhotoFromGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.RemovePhoto(message.PhotoId);
            repo.Save(item, item.Version);
        }

        public void Handle(EditPhotoInGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.EditPhoto(message.PhotoId, message.Privacy);
            repo.Save(item, item.Version);
        }

        public void Handle(BanPhotoFromGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.BanPhoto(message.PhotoId, message.UserId);
            repo.Save(item, item.Version);
        }

        public void Handle(BanAlbumFromGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.BanAlbum(message.AlbumId, message.Photos, message.UserId);
            repo.Save(item, item.Version);
        }

        public void Handle(RejectAlbumGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.RejectAlbum(message.AlbumId, message.ActingUser, message.Photos);
            repo.Save(item, item.Version);
        }

        public void Handle(AcceptAlbumGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.AcceptAlbum(message.AlbumId, message.ActingUser, message.Photos);
            repo.Save(item, item.Version);
        }

        public void Handle(AcceptPhotoGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.AcceptPhoto(message.PhotoId, message.ActingUser);
            repo.Save(item, item.Version);
        }

        public void Handle(RejectPhotoGroup message)
        {
            var item = repo.GetById(message.GroupId);
            item.RejectPhoto(message.PhotoId, message.ActingUser);
            repo.Save(item, item.Version);
        }
    }
}
