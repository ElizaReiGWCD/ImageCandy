using ImageHoster.CQRS.ReadModel.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageHoster.GUI.Models.User
{
    public class UserDetailViewModel
    {
        public UserDto User { get; set; }
        public IEnumerable<GroupDto> Groups { get; set; }
        public IEnumerable<PhotoDto> Photos { get; set; }
        public IEnumerable<AlbumDto> Albums { get; set; }
    }
}