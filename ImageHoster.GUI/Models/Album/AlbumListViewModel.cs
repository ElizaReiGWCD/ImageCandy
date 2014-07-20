using ImageHoster.CQRS.ReadModel.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageHoster.GUI.Models.Album
{
    public class AlbumListViewModel
    {
        public List<AlbumDto> Albums { get; set; }
        public string Username { get; set; }
    }
}