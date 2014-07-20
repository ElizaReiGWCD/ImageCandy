using ImageHoster.CQRS.Domain;
using ImageHoster.CQRS.ReadModel.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.ReadModel
{
    public class InMemoryReadModel : IReadModel
    {
        public IEnumerable<UserDto> GetUsers()
        {
            return InMemoryDatabase.users;
        }

        public IEnumerable<PhotoDto> GetPhotos()
        {
            return InMemoryDatabase.photos;
        }

        public IEnumerable<AlbumDto> GetAlbums()
        {
            return InMemoryDatabase.albums;
        }

        public IEnumerable<GroupDto> GetGroups()
        {
            return InMemoryDatabase.groups;
        }
    }
}
