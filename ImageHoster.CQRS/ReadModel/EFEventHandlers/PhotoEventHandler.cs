using ImageHoster.CQRS.Events;
using ImageHoster.CQRS.ReadModel.Dto;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.ReadModel.EFEventhandlers
{
    public class PhotoEventHandler : Handles<PhotoUploaded>, Handles<ChangedPhotoInfo>, Handles<DeletedPhoto>
    {
        private EntityFrameworkDatabase context;

        public PhotoEventHandler(string connectionstring)
        {
            context = new EntityFrameworkDatabase(connectionstring);
        }

        public  void Handle(PhotoUploaded message)
        {
            var user =  context.Users.Find(message.UserId);
            PhotoDto dto = new PhotoDto() { Id = message.Id, FileName = message.Filename, UploadedBy = user, Privacy = message.Privacy.ToReadModel(context.Groups) };
            
            user.Photos.Add(dto);
            context.Photos.Add(dto);

             context.SaveChanges();
        }

        public  void Handle(ChangedPhotoInfo message)
        {
            PhotoDto dto =  context.Photos.Find(message.PhotoId);

            dto.Description = message.Description;
            dto.Title = message.Title;
            dto.Privacy = message.Privacy.ToReadModel(context.Groups);

             context.SaveChanges();
        }

        public  void Handle(DeletedPhoto message)
        {
            PhotoDto dto =  context.Photos.Find(message.PhotoId);

            foreach (var album in context.Albums)
                album.Photos.Remove(dto);
            foreach (var user in context.Users)
                user.Photos.Remove(dto);
            context.Photos.Remove(dto);

             context.SaveChanges();
        }
    }
}
