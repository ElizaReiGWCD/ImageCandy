using ImageHoster.CQRS.Domain;
using ImageHoster.CQRS.ReadModel.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageHoster.CQRS
{
    internal static class InMemoryDatabase
    {
        internal static List<UserDto> users = new List<UserDto>();
        internal static List<PhotoDto> photos = new List<PhotoDto>();
        internal static List<AlbumDto> albums = new List<AlbumDto>();
        internal static List<GroupDto> groups = new List<GroupDto>();
    }
}
