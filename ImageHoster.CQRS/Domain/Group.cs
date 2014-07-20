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
    public class Group : AggregateRoot
    {
        private Guid _id;
        private List<Guid> Members { get; set; }
        private Guid Admin { get; set; }
        private List<Guid> Moderators { get; set; }
        private List<Guid> BannedUsers { get; set; }
        private GroupPrivacy Privacy { get; set; }
        private List<Guid> JoinRequests { get; set; }

        private List<Guid> Photos { get; set; }
        private List<Guid> Albums { get; set; }

        private List<Guid> BannedPhotos { get; set; }
        private List<Guid> BannedAlbums { get; set; }

        private List<Guid> PendingPhotos { get; set; }
        private List<Guid> PendingAlbums { get; set; }

        public override Guid Id
        {
            get { return _id; }
            protected set { _id = value; }
        }

        private void Initialize()
        {
            Members = new List<Guid>();
            Moderators = new List<Guid>();
            BannedUsers = new List<Guid>();
            JoinRequests = new List<Guid>();

            Photos = new List<Guid>();
            Albums = new List<Guid>();

            BannedAlbums = new List<Guid>();
            BannedPhotos = new List<Guid>();

            PendingAlbums = new List<Guid>();
            PendingPhotos = new List<Guid>();
        }

        public Group()
        {
            Initialize();
        }

        private void Apply(CreatedGroup e)
        {
            this._id = e.Id;
            this.Members.Add(e.UserId);
            this.Moderators.Add(e.UserId);
            this.Admin = e.UserId;
            this.Privacy = e.Privacy;
        }

        private void Apply(MemberAddedToGroup e)
        {
            Members.Add(e.UserId);
        }

        private void Apply(RemovedMemberFromGroup e)
        {
            Members.RemoveAll(m => m == e.UserId);
        }

        private void Apply(AddedModeratorToGroup e)
        {
            Moderators.Add(e.UserId);
        }

        private void Apply(RemovedModeratorFromGroup e)
        {
            Moderators.RemoveAll(u => u == e.UserId);
        }

        private void Apply(ChangedGroupInfo e)
        {
            this.Privacy = e.Privacy;
        }

        private void Apply(SubmittedJoinRequest e)
        {
            this.JoinRequests.Add(e.UserId);
        }

        private void Apply(AcceptedJoinRequest e)
        {
            this.Members.Add(e.UserId);
            this.JoinRequests.Remove(e.UserId);
        }

        private void Apply(RejectedJoinRequest e)
        {
            this.JoinRequests.Remove(e.UserId);
        }

        private void Apply(AcceptedInviteGroup e)
        {
            this.Members.Add(e.UserId);
        }

        private void Apply(AddedAlbumToGroup e)
        {
            this.Albums.Add(e.AlbumId);
        }

        private void Apply(DeletedAlbumFromGroup e)
        {
            this.Albums.Remove(e.AlbumId);
        }

        private void Apply(AddedPhotoToGroup e)
        {
            this.Photos.Add(e.PhotoId);
        }

        private void Apply(DeletedPhotoFromGroup e)
        {
            this.Photos.Remove(e.PhotoId);
        }

        private void Apply(BannedPhotoFromGroup e)
        {
            this.Photos.Remove(e.PhotoId);
            this.BannedPhotos.Add(e.PhotoId);
        }

        private void Apply(BannedAlbumFromGroup e)
        {
            this.Albums.Remove(e.AlbumId);
            this.BannedAlbums.Add(e.AlbumId);
        }

        private void Apply(BannedUserFromGroup e)
        {
            this.BannedUsers.Add(e.UserId);
            this.Members.Remove(e.UserId);
        }

        private void Apply(AddedPendingPhotoToGroup e)
        {
            this.PendingPhotos.Add(e.PhotoId);
        }

        private void Apply(AddedPendingAlbumToGroup e)
        {
            this.PendingAlbums.Add(e.AlbumId);
        }

        private void Apply(AcceptedPendingPhoto e)
        {
            this.PendingPhotos.Remove(e.PhotoId);
            this.Photos.Add(e.PhotoId);
        }

        private void Apply(RejectedPendingPhoto e)
        {
            this.PendingPhotos.Remove(e.PhotoId);
        }

        private void Apply(AcceptedPendingAlbum e)
        {
            this.PendingAlbums.Remove(e.AlbumId);
            this.Albums.Add(e.AlbumId);
        }

        private void Apply(RejectedPendingAlbum e)
        {
            this.PendingAlbums.Remove(e.AlbumId);
        }

        public Group(Guid id, Guid userid, string name, string description, IEnumerable<string> existingNames, GroupPrivacy privacy)
        {
            if (!existingNames.Contains(name))
            {
                Initialize();

                this._id = id;
                this.Members.Add(userid);
                this.Moderators.Add(userid);
                this.Admin = userid;
                this.Privacy = privacy;
                ApplyChange(new CreatedGroup() { Id = id, Description = description, Name = name, UserId = userid, Privacy = privacy });
            }
        }

        public void AddMember(Guid id, Guid groupid, Guid userid)
        {
            if (!Members.Contains(userid) && !BannedUsers.Contains(userid) && Privacy.Level == GroupPrivacyLevel.Public)
            {
                this.Members.Add(userid);
                ApplyChange(new MemberAddedToGroup() { Id = id, UserId = userid, GroupId = groupid });
            }
        }

        public void AddAlbum(Guid id)
        {
            if (!this.Albums.Contains(id) && !this.BannedAlbums.Contains(id) && !this.PendingAlbums.Contains(id))
            {
                if (!Privacy.PhotosAddedAfterAccepting)
                {
                    this.Albums.Add(id);
                    ApplyChange(new AddedAlbumToGroup() { AlbumId = id, GroupId = this.Id });
                }
                else
                {
                    this.PendingAlbums.Add(id);
                    ApplyChange(new AddedPendingAlbumToGroup() { AlbumId = id, GroupId = this.Id });
                }
            }
        }

        public void AcceptAlbum(Guid id, Guid userid, List<Guid> photos)
        {
            if (this.Moderators.Contains(userid) && this.PendingAlbums.Contains(id))
            {
                this.PendingAlbums.Remove(id);
                this.Albums.Add(id);

                foreach (var photo in photos)
                    AcceptPhoto(photo, userid);

                ApplyChange(new AcceptedPendingAlbum() { AlbumId = id, GroupId = this.Id });
            }
        }

        public void RejectAlbum(Guid id, Guid userid, List<Guid> photos)
        {
            if (this.Moderators.Contains(userid) && this.PendingAlbums.Contains(id))
            {
                this.PendingAlbums.Remove(id);

                foreach (var photo in photos)
                    RejectPhoto(photo, userid);

                ApplyChange(new RejectedPendingAlbum() { AlbumId = id, GroupId = this.Id });
            }
        }

        public void RemoveAlbum(Guid id)
        {
            if (this.Albums.Contains(id) || this.PendingAlbums.Contains(id))
            {
                this.Albums.RemoveAll(a => a == id);
                this.PendingAlbums.RemoveAll(a => a == id);
                ApplyChange(new DeletedAlbumFromGroup() { AlbumId = id, GroupId = this.Id });
            }
        }

        public void AddPhoto(Guid id)
        {
            if (!this.Photos.Contains(id) && !this.BannedPhotos.Contains(id) && !this.PendingPhotos.Contains(id))
            {
                if (!Privacy.PhotosAddedAfterAccepting)
                {
                    this.Photos.Add(id);
                    ApplyChange(new AddedPhotoToGroup() { PhotoId = id, GroupId = this.Id });
                }
                else
                {
                    this.PendingPhotos.Add(id);
                    ApplyChange(new AddedPendingPhotoToGroup() { GroupId = this.Id, PhotoId = id });
                }
            }
        }

        public void AcceptPhoto(Guid id, Guid userid)
        {
            if (this.Moderators.Contains(userid) && this.PendingPhotos.Contains(id))
            {
                this.PendingPhotos.Remove(id);
                this.Photos.Add(id);

                ApplyChange(new AcceptedPendingPhoto() { GroupId = this.Id, PhotoId = id });
            }
        }

        public void RejectPhoto(Guid id, Guid userid)
        {
            if (this.Moderators.Contains(userid) && this.PendingPhotos.Contains(id))
            {
                this.PendingPhotos.Remove(id);

                ApplyChange(new RejectedPendingPhoto() { GroupId = this.Id, PhotoId = id });
            }
        }

        public void RemovePhoto(Guid id)
        {
            if (this.Photos.Contains(id) || this.PendingPhotos.Contains(id))
            {
                this.Photos.RemoveAll(p => p == id);
                this.PendingPhotos.RemoveAll(p => p == id);
                ApplyChange(new DeletedPhotoFromGroup() { PhotoId = id, GroupId = this.Id });
            }
        }

        public void EditAlbum(Guid id, Privacy privacy)
        {
            if (privacy.Level != PrivacyLevel.Group)
                RemoveAlbum(id);
            else
            {
                if (privacy.VisibleToGroups.Contains(this.Id))
                    AddAlbum(id);
                else
                    RemoveAlbum(id);
            }
        }

        public void EditPhoto(Guid id, Privacy privacy)
        {
            if (privacy.Level != PrivacyLevel.Group)
                RemovePhoto(id);
            else
            {
                if (privacy.VisibleToGroups.Contains(this.Id))
                    AddPhoto(id);
                else
                    RemovePhoto(id);
            }
        }

        public void BanPhoto(Guid id, Guid actingUser)
        {
            if (this.Moderators.Contains(actingUser))
            {
                if (this.Photos.Contains(id))
                {
                    this.Photos.Remove(id);
                    this.BannedPhotos.Add(id);
                    ApplyChange(new BannedPhotoFromGroup() { GroupId = this.Id, PhotoId = id });
                }
            }
        }

        public void BanAlbum(Guid id, List<Guid> photos, Guid actingUser)
        {
            if (this.Moderators.Contains(actingUser))
            {
                if (this.Albums.Contains(id))
                {
                    this.Albums.Remove(id);
                    this.BannedAlbums.Add(id);

                    foreach (var p in photos)
                        BanPhoto(p, actingUser);

                    ApplyChange(new BannedAlbumFromGroup() { AlbumId = id, GroupId = this.Id });
                }
            }
        }

        public void SubmitJoinRequest(Guid userid)
        {
            if (this.Privacy.Level == GroupPrivacyLevel.SemiPublic && !this.JoinRequests.Contains(userid) && !this.BannedUsers.Contains(userid))
            {
                this.JoinRequests.Add(userid);
                ApplyChange(new SubmittedJoinRequest() { UserId = userid, GroupId = this.Id });
            }
        }

        public void AcceptJoinRequest(Guid userid, Guid actingUser)
        {
            HandleJoinRequest(userid, actingUser, true);
        }

        public void RejectJoinRequest(Guid userid, Guid actingUser)
        {
            HandleJoinRequest(userid, actingUser, false);
        }

        private void HandleJoinRequest(Guid userid, Guid actingUser, bool accept)
        {
            if (this.JoinRequests.Contains(userid) && Moderators.Contains(actingUser))
            {
                this.JoinRequests.Remove(userid);

                if (accept)
                {
                    this.Members.Add(userid);
                    ApplyChange(new AcceptedJoinRequest() { UserId = userid, GroupId = this.Id });
                }
                else
                {
                    ApplyChange(new RejectedJoinRequest() { UserId = userid, GroupId = this.Id });
                }
            }
        }

        public void InviteUser(Guid userId, Guid actingUser, ICommandSender bus)
        {
            if (actingUser == Admin && !this.Members.Contains(userId) && this.Privacy.Level == GroupPrivacyLevel.Private)
            {
                bus.Send(new AddInviteToUser() { GroupId = this.Id, UserId = userId });
            }
        }

        public void AcceptInvite(Guid userId)
        {
            if (!this.Members.Contains(userId))
            {
                this.Members.Add(userId);
                ApplyChange(new AcceptedInviteGroup() { UserId = userId, GroupId = this.Id });
            }
        }

        public void RemoveMember(Guid id, Guid groupid, Guid userid)
        {
            if (Members.Contains(userid) && userid != Admin)
            {
                this.Members.RemoveAll(u => u == userid);
                this.Moderators.RemoveAll(u => u == userid);

                ApplyChange(new RemovedMemberFromGroup() { Id = id, UserId = userid, GroupId = groupid });
            }
        }

        public void BanUser(Guid groupId, Guid actingUser, Guid toRemove)
        {
            if (Moderators.Contains(actingUser))
            {
                if (Moderators.Contains(toRemove))
                    if (Admin == actingUser && toRemove != Admin)
                    {
                        Moderators.RemoveAll(g => g == toRemove);
                        Members.RemoveAll(g => g == toRemove);
                        BannedUsers.Add(toRemove);
                        ApplyChange(new BannedUserFromGroup() { GroupId = groupId, ActingUser = actingUser, UserId = toRemove });
                    }
                    else { }
                else
                {
                    Members.RemoveAll(g => g == toRemove);
                    BannedUsers.Add(toRemove);
                    ApplyChange(new BannedUserFromGroup() { GroupId = groupId, ActingUser = actingUser, UserId = toRemove });
                }
            }
        }

        public void AddModerator(Guid groupid, Guid userToAdd, Guid userPerforming)
        {
            if (userPerforming == Admin)
            {
                if (this.Members.Contains(userToAdd) && !this.Moderators.Contains(userToAdd))
                {
                    this.Moderators.Add(userToAdd);
                    ApplyChange(new AddedModeratorToGroup() { GroupId = groupid, UserId = userToAdd });
                }
            }
        }

        public void RemoveModerator(Guid groupid, Guid userToAdd, Guid userPerforming)
        {
            if (userPerforming == Admin)
            {
                if (this.Members.Contains(userToAdd) && this.Moderators.Contains(userToAdd))
                {
                    this.Moderators.RemoveAll(g => g == userToAdd);
                    ApplyChange(new RemovedModeratorFromGroup() { GroupId = groupid, UserId = userToAdd });
                }
            }
        }

        public void DeleteGroup(Guid id, Guid user, Guid groupid)
        {
            if (user == Admin)
            {
                ApplyChange(new DeletedGroup() { Id = id, GroupId = groupid });
            }
        }

        public void ChangeGroupInfo(Guid id, Guid user, Guid groupid, string name, string description, GroupPrivacy privacy, ICommandSender bus)
        {
            if (user == Admin)
            {
                ApplyChange(new ChangedGroupInfo() { Id = id, Description = description, GroupId = groupid, Name = name, Privacy = privacy, Version = this.Version });

                if (!privacy.Equals(this.Privacy))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("The privacy settings have been changed to:")
                        .Append("Privacy level: ")
                        .Append(privacy.Level)
                        .Append("\n")
                        .Append("Photos only added after accepted by moderator: ")
                        .Append(privacy.PhotosAddedAfterAccepting);

                    bus.Send(new MakeAnnouncement() { Announcement = sb.ToString(), AnnouncerId = user, GroupId = this.Id, TimeOfAnnouncement = DateTime.UtcNow, Title = "Privacy settings changed" });
                }

                this.Privacy = privacy;
            }
        }

        public void MakeAnnouncement(Guid announcer, string title, string announcement, DateTime time)
        {
            if (this.Moderators.Contains(announcer))
            {
                ApplyChange(new MadeAnnouncement() { Announcement = announcement, AnnouncerId = announcer, GroupId = this.Id, TimeOfAnnouncement = time, Title = title });
            }
        }
    }
}
