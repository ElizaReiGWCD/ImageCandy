using ImageHoster.CQRS.ReadModel.Dto;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageHoster.GUI.Models.Album
{
    public class AlbumDetailViewModel
    {
        public AlbumDto Album { get; set; }
        public PagedList<PhotoDto> Photos { get; set; }
        public string Username { get; set; }
    }
}