using ImageHoster.CQRS.ReadModel.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS.ReadModel
{
    public interface IReadModel
    {
        IEnumerable<UserDto> GetUsers();
        IEnumerable<PhotoDto> GetPhotos();
        IEnumerable<AlbumDto> GetAlbums();
        IEnumerable<GroupDto> GetGroups();
    }
}
