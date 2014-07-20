using ImageHoster.CQRS.Domain;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using Sogyo.CQRS.Library.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.Commands
{
    public class PhotoCommandHandler : Handles<UploadPhoto>, Handles<EditPhotoInfo>, Handles<DeletePhoto>
    {
        private IRepository<Photo> repo;

        public PhotoCommandHandler(IRepository<Photo> repository)
        {
            repo = repository;
        }

        public void Handle(UploadPhoto message)
        {
            var item = new Photo(message.Id, message.UserId, message.Filename, message.Privacy);
            repo.Save(item, -1);
            item.AddPhotoToGroup(message.Bus);
        }

        public void Handle(EditPhotoInfo message)
        {
            var item = repo.GetById(message.PhotoId);
            item.UpdatePhotoInfo(message.Id, message.User, message.PhotoId, message.Title, message.Description, message.Privacy, message.OldPrivacy, message.Bus, message.Albums);
            repo.Save(item, item.Version);
        }

        public void Handle(DeletePhoto message)
        {
            var item = repo.GetById(message.PhotoId);
            item.DeletePhoto(message.Id, message.UserId, message.PhotoId, message.Albums, message.Bus);
            repo.Save(item, item.Version);
        }
    }
}
