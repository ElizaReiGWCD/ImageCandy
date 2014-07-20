using ImageHoster.CQRS.Events;
using ImageHoster.CQRS.ReadModel.Dto;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.ReadModel.Eventhandlers
{
    public class PhotoEventHandler : Handles<PhotoUploaded>, Handles<ChangedPhotoInfo>, Handles<DeletedPhoto>
    {
        public void Handle(PhotoUploaded message)
        {
            var user = InMemoryDatabase.users.Find(u => u.Id == message.UserId);
            PhotoDto dto = new PhotoDto() { Id = message.Id, FileName = message.Filename, UploadedBy = user, Privacy = message.Privacy.ToReadModel(InMemoryDatabase.groups) };
            user.Photos.Add(dto);
            InMemoryDatabase.photos.Add(dto);
        }

        public void Handle(ChangedPhotoInfo message)
        {
            PhotoDto dto = InMemoryDatabase.photos.Find(p => p.Id == message.PhotoId);
            dto.Description = message.Description;
            dto.Title = message.Title;
            dto.Privacy = message.Privacy.ToReadModel(InMemoryDatabase.groups);
        }

        public void Handle(DeletedPhoto message)
        {
            PhotoDto dto = InMemoryDatabase.photos.Find(p => p.Id == message.PhotoId);
            InMemoryDatabase.albums.ForEach(a => a.Photos.Remove(dto));
            InMemoryDatabase.users.ForEach(u => u.Photos.Remove(dto));
            InMemoryDatabase.photos.Remove(dto);
        }
    }
}
