using ImageHoster.CQRS.Events;
using ImageHoster.CQRS.ReadModel.Dto;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.ReadModel.EFEventhandlers
{
    public class GroupEventHandler : Handles<CreatedGroup>, Handles<MemberAddedToGroup>, Handles<RemovedMemberFromGroup>, Handles<DeletedGroup>, Handles<ChangedGroupInfo>, Handles<AddedModeratorToGroup>,
        Handles<RemovedModeratorFromGroup>, Handles<BannedUserFromGroup>, Handles<SubmittedJoinRequest>, Handles<AcceptedJoinRequest>, Handles<RejectedJoinRequest>, Handles<AcceptedInviteGroup>, 
        Handles<MadeAnnouncement>, Handles<AddedAlbumToGroup>, Handles<DeletedAlbumFromGroup>, Handles<AddedPhotoToGroup>, Handles<DeletedPhotoFromGroup>, Handles<BannedPhotoFromGroup>, Handles<BannedAlbumFromGroup>, 
        Handles<AddedPendingAlbumToGroup>, Handles<AddedPendingPhotoToGroup>, Handles<AcceptedPendingPhoto>, Handles<RejectedPendingPhoto>, Handles<AcceptedPendingAlbum>, Handles<RejectedPendingAlbum>
    {
        private EntityFrameworkDatabase context;

        public GroupEventHandler(string connectionstring)
        {
            context = new EntityFrameworkDatabase(connectionstring);
        }

        public  void Handle(CreatedGroup message)
        {
            var user =  context.Users.Find(message.UserId);

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
            context.Groups.Add(dto);

             context.SaveChanges();
        }

        public  void Handle(MemberAddedToGroup message)
        {
            var user =  context.Users.Find(message.UserId);
            var group =  context.Groups.Find(message.GroupId);

            group.Members.Add(user);
            user.Groups.Add(group);

             context.SaveChanges();
        }

        public  void Handle(RemovedMemberFromGroup message)
        {
            var user =  context.Users.Find(message.UserId);
            var group =  context.Groups.Find(message.GroupId);

            group.Members.Remove(user);
            group.Moderators.Remove(user);
            user.Groups.Remove(group);

             context.SaveChanges();
        }

        public  void Handle(DeletedGroup message)
        {
            var group =  context.Groups.Find(message.GroupId);

            foreach (var user in group.Members)
                user.Groups.Remove(group);
            context.Groups.Remove(group);

             context.SaveChanges();
        }

        public  void Handle(ChangedGroupInfo message)
        {
            var group =  context.Groups.Find(message.GroupId);
            
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

             context.SaveChanges();
        }

        public  void Handle(AddedModeratorToGroup message)
        {
            var user =  context.Users.Find(message.UserId);
            var group =  context.Groups.Find(message.GroupId);

            group.Moderators.Add(user);

             context.SaveChanges();
        }

        public  void Handle(RemovedModeratorFromGroup message)
        {
            var user =  context.Users.Find(message.UserId);
            var group =  context.Groups.Find(message.GroupId);

            group.Moderators.Remove(user);

             context.SaveChanges();
        }

        public  void Handle(BannedUserFromGroup message)
        {
            var user =  context.Users.Find(message.UserId);
            var group =  context.Groups.Find(message.GroupId);

            group.Members.Remove(user);
            group.Moderators.Remove(user);
            user.Groups.Remove(group);

             context.SaveChanges();
        }

        public  void Handle(SubmittedJoinRequest message)
        {
            var user =  context.Users.Find(message.UserId);
            var group =  context.Groups.Find(message.GroupId);

            group.JoinRequests.Add(user);

             context.SaveChanges();
        }

        public  void Handle(AcceptedJoinRequest message)
        {
            var user =  context.Users.Find(message.UserId);
            var group =  context.Groups.Find(message.GroupId);

            group.JoinRequests.Remove(user);
            group.Members.Add(user);
            user.Groups.Add(group);

             context.SaveChanges();
        }

        public  void Handle(RejectedJoinRequest message)
        {
            var user =  context.Users.Find(message.UserId);
            var group =  context.Groups.Find(message.GroupId);

            group.JoinRequests.Remove(user);

             context.SaveChanges();
        }

        public  void Handle(AcceptedInviteGroup message)
        {
            var user =  context.Users.Find(message.UserId);
            var group =  context.Groups.Find(message.GroupId);

            group.Members.Add(user);

             context.SaveChanges();
        }

        public  void Handle(MadeAnnouncement message)
        {
            var user =  context.Users.Find(message.AnnouncerId);
            var group =  context.Groups.Find(message.GroupId);

            group.Announcements.Add(new AnnouncementDto() { Announcement = message.Announcement, Announcer = user, Time = message.TimeOfAnnouncement, Title = message.Title, Group = group });

            foreach (UserDto u in group.Members)
            {
                u.NewsCount++;
            }

             context.SaveChanges();
        }

        public  void Handle(AddedAlbumToGroup message)
        {
            var group =  context.Groups.Find(message.GroupId);
            var album =  context.Albums.Find(message.AlbumId);

            group.Albums.Add(album);

             context.SaveChanges();
        }

        public  void Handle(DeletedAlbumFromGroup message)
        {
            var group =  context.Groups.Find(message.GroupId);
            var album =  context.Albums.Find(message.AlbumId);

            group.Albums.Remove(album);
            group.PendingAlbums.Remove(album);

             context.SaveChanges();
        }

        public  void Handle(AddedPhotoToGroup message)
        {
            var group =  context.Groups.Find(message.GroupId);
            var photo =  context.Photos.Find(message.PhotoId);

            group.Photos.Add(photo);

             context.SaveChanges();
        }

        public  void Handle(DeletedPhotoFromGroup message)
        {
            var group =  context.Groups.Find(message.GroupId);
            var photo =  context.Photos.Find(message.PhotoId);

            group.Photos.Remove(photo);
            group.PendingPhotos.Remove(photo);

             context.SaveChanges();
        }

        public  void Handle(BannedPhotoFromGroup message)
        {
            var group =  context.Groups.Find(message.GroupId);
            var photo =  context.Photos.Find(message.PhotoId);

            group.Photos.Remove(photo);

             context.SaveChanges();
        }

        public  void Handle(BannedAlbumFromGroup message)
        {
            var group =  context.Groups.Find(message.GroupId);
            var album =  context.Albums.Find(message.AlbumId);

            group.Albums.Remove(album);

             context.SaveChanges();
        }

        public  void Handle(AddedPendingAlbumToGroup message)
        {
            var group =  context.Groups.Find(message.GroupId);
            var album =  context.Albums.Find(message.AlbumId);

            group.PendingAlbums.Add(album);

             context.SaveChanges();
        }

        public  void Handle(AddedPendingPhotoToGroup message)
        {
            var group =  context.Groups.Find(message.GroupId);
            var photo =  context.Photos.Find(message.PhotoId);

            group.PendingPhotos.Add(photo);

             context.SaveChanges();
        }

        public  void Handle(AcceptedPendingPhoto message)
        {
            var group =  context.Groups.Find(message.GroupId);
            var photo =  context.Photos.Find(message.PhotoId);

            group.Photos.Add(photo);
            group.PendingPhotos.Remove(photo);

             context.SaveChanges();
        }

        public  void Handle(RejectedPendingPhoto message)
        {
            var group =  context.Groups.Find(message.GroupId);
            var photo =  context.Photos.Find(message.PhotoId);

            group.PendingPhotos.Remove(photo);

             context.SaveChanges();
        }

        public  void Handle(AcceptedPendingAlbum message)
        {
            var group =  context.Groups.Find(message.GroupId);
            var album =  context.Albums.Find(message.AlbumId);

            group.PendingAlbums.Remove(album);
            group.Albums.Add(album);

             context.SaveChanges();
        }

        public  void Handle(RejectedPendingAlbum message)
        {
            var group =  context.Groups.Find(message.GroupId);
            var album =  context.Albums.Find(message.AlbumId);

            group.PendingAlbums.Remove(album);

             context.SaveChanges();
        }
    }
}
