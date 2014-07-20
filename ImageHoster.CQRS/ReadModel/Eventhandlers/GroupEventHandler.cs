using ImageHoster.CQRS.Events;
using ImageHoster.CQRS.ReadModel.Dto;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.ReadModel.Eventhandlers
{
    public class GroupEventHandler : Handles<CreatedGroup>, Handles<MemberAddedToGroup>, Handles<RemovedMemberFromGroup>, Handles<DeletedGroup>, Handles<ChangedGroupInfo>, Handles<AddedModeratorToGroup>,
        Handles<RemovedModeratorFromGroup>, Handles<BannedUserFromGroup>, Handles<SubmittedJoinRequest>, Handles<AcceptedJoinRequest>, Handles<RejectedJoinRequest>, Handles<AcceptedInviteGroup>, 
        Handles<MadeAnnouncement>, Handles<AddedAlbumToGroup>, Handles<DeletedAlbumFromGroup>, Handles<AddedPhotoToGroup>, Handles<DeletedPhotoFromGroup>, Handles<BannedPhotoFromGroup>, Handles<BannedAlbumFromGroup>, 
        Handles<AddedPendingAlbumToGroup>, Handles<AddedPendingPhotoToGroup>, Handles<AcceptedPendingPhoto>, Handles<RejectedPendingPhoto>, Handles<AcceptedPendingAlbum>, Handles<RejectedPendingAlbum>
    {
        public void Handle(CreatedGroup message)
        {
            var user = InMemoryDatabase.users.Find(u => u.Id == message.UserId);

            GroupDto dto = new GroupDto()
            {
                Id = message.Id,
                Description = message.Description,
                Admin = user,
                Name = message.Name,
                Privacy = new GroupPrivacySettings()
                    {
                        Level = message.Privacy.Level,
                        GroupVisibleToOutsiders = message.Privacy.GroupVisibleToOutsiders,
                        MembersVisibleToOutsiders = message.Privacy.MembersVisibleToOutsiders,
                        PhotosVisibleToOutsiders = message.Privacy.PhotosVisibleToOutsiders,
                        AddPhotosAfterAccepting = message.Privacy.PhotosAddedAfterAccepting
                    }
            };

            dto.Members.Add(user);
            dto.Moderators.Add(user);
            user.Groups.Add(dto);

            InMemoryDatabase.groups.Add(dto);
        }

        public void Handle(MemberAddedToGroup message)
        {
            var user = InMemoryDatabase.users.Find(u => u.Id == message.UserId);
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            
            group.Members.Add(user);
            user.Groups.Add(group);
        }

        public void Handle(RemovedMemberFromGroup message)
        {
            var user = InMemoryDatabase.users.Find(u => u.Id == message.UserId);
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            group.Members.Remove(user);

            if (group.Moderators.Contains(user))
                group.Moderators.Remove(user);

            
            user.Groups.Remove(group);
        }

        public void Handle(DeletedGroup message)
        {
            var group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);

            InMemoryDatabase.users.ForEach(u => u.Groups.Remove(group));
            InMemoryDatabase.groups.Remove(group);
        }

        public void Handle(ChangedGroupInfo message)
        {
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            group.Name = message.Name;
            group.Description = message.Description;
            group.Privacy = new GroupPrivacySettings()
            {
                Level = message.Privacy.Level,
                GroupVisibleToOutsiders = message.Privacy.GroupVisibleToOutsiders,
                MembersVisibleToOutsiders = message.Privacy.MembersVisibleToOutsiders,
                PhotosVisibleToOutsiders = message.Privacy.PhotosVisibleToOutsiders,
                AddPhotosAfterAccepting = message.Privacy.PhotosAddedAfterAccepting
            };
        }

        public void Handle(AddedModeratorToGroup message)
        {
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            var user = InMemoryDatabase.users.Find(u => u.Id == message.UserId);
            group.Moderators.Add(user);
        }

        public void Handle(RemovedModeratorFromGroup message)
        {
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            var user = InMemoryDatabase.users.Find(u => u.Id == message.UserId);
            group.Moderators.Remove(user);
        }

        public void Handle(BannedUserFromGroup message)
        {
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            var user = InMemoryDatabase.users.Find(u => u.Id == message.UserId);
            group.Members.Remove(user);

            if (group.Moderators.Contains(user))
                group.Moderators.Remove(user);

            user.Groups.Remove(group);
        }

        public void Handle(SubmittedJoinRequest message)
        {
            var user = InMemoryDatabase.users.Find(u => u.Id == message.UserId);
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);

            group.JoinRequests.Add(user);
        }

        public void Handle(AcceptedJoinRequest message)
        {
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            var user = InMemoryDatabase.users.Find(u => u.Id == message.UserId);

            group.JoinRequests.Remove(user);
            group.Members.Add(user);
            user.Groups.Add(group);
        }

        public void Handle(RejectedJoinRequest message)
        {
            var user = InMemoryDatabase.users.Find(u => u.Id == message.UserId);
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            
            group.JoinRequests.Remove(user);
        }

        public void Handle(AcceptedInviteGroup message)
        {
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            var user = InMemoryDatabase.users.Find(u => u.Id == message.UserId);

            group.Members.Add(user);
        }

        public void Handle(MadeAnnouncement message)
        {
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            var user = InMemoryDatabase.users.Find(u => u.Id == message.AnnouncerId);

            group.Announcements.Add(new AnnouncementDto() { Announcement = message.Announcement, Announcer = user, Time = message.TimeOfAnnouncement, Title = message.Title, Group = group });

            foreach (UserDto u in group.Members)
            {
                u.NewsCount++;
            }
        }

        public void Handle(AddedAlbumToGroup message)
        {
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            var album = InMemoryDatabase.albums.Find(a => a.Id == message.AlbumId);

            group.Albums.Add(album);
        }

        public void Handle(DeletedAlbumFromGroup message)
        {
            AlbumDto album = InMemoryDatabase.albums.Find(a => a.Id == message.AlbumId);
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            group.Albums.Remove(album);
            group.PendingAlbums.Remove(album);
        }

        public void Handle(AddedPhotoToGroup message)
        {
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            var photo = InMemoryDatabase.photos.Find(p => p.Id == message.PhotoId);

            group.Photos.Add(photo);
        }

        public void Handle(DeletedPhotoFromGroup message)
        {
            PhotoDto photo = InMemoryDatabase.photos.Find(a => a.Id == message.PhotoId);
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            
            group.Photos.Remove(photo);
            group.PendingPhotos.Remove(photo);
        }

        public void Handle(BannedPhotoFromGroup message)
        {
            PhotoDto photo = InMemoryDatabase.photos.Find(a => a.Id == message.PhotoId);
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);

            group.Photos.Remove(photo);
        }

        public void Handle(BannedAlbumFromGroup message)
        {
            AlbumDto album = InMemoryDatabase.albums.Find(a => a.Id == message.AlbumId);
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);

            group.Albums.Remove(album);
        }

        public void Handle(AddedPendingAlbumToGroup message)
        {
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            var album = InMemoryDatabase.albums.Find(a => a.Id == message.AlbumId);

            group.PendingAlbums.Add(album);
        }

        public void Handle(AddedPendingPhotoToGroup message)
        {
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            var photo = InMemoryDatabase.photos.Find(p => p.Id == message.PhotoId);

            group.PendingPhotos.Add(photo);
        }

        public void Handle(AcceptedPendingPhoto message)
        {
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            var photo = InMemoryDatabase.photos.Find(p => p.Id == message.PhotoId);

            group.Photos.Add(photo);
            group.PendingPhotos.Remove(photo);
        }

        public void Handle(RejectedPendingPhoto message)
        {
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            var photo = InMemoryDatabase.photos.Find(p => p.Id == message.PhotoId);

            group.PendingPhotos.Remove(photo);
        }

        public void Handle(AcceptedPendingAlbum message)
        {
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);
            var album = InMemoryDatabase.albums.Find(a => a.Id == message.AlbumId);

            group.PendingAlbums.Remove(album);
            group.Albums.Add(album);
        }

        public void Handle(RejectedPendingAlbum message)
        {
            var album = InMemoryDatabase.albums.Find(a => a.Id == message.AlbumId);
            GroupDto group = InMemoryDatabase.groups.Find(g => g.Id == message.GroupId);

            group.PendingAlbums.Remove(album);
        }
    }
}
