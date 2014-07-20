using ImageHoster.CQRS;
using ImageHoster.CQRS.Commands;
using ImageHoster.CQRS.Domain;
using ImageHoster.CQRS.ReadModel;
using ImageHoster.CQRS.ReadModel.Dto;
using ImageHoster.GUI.Attributes;
using ImageHoster.GUI.Models;
using ImageHoster.GUI.Models.Album;
using PagedList;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ImageHoster.GUI.Controllers
{
    public class AlbumController : Controller
    {
        IReadModel readModel;
        IBus bus;

        public AlbumController()
        {
            readModel = new EFReadModel(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            bus = ServiceLocator.Bus;
        }

        //
        // GET: /Album/
        [GetUserId]
        public ActionResult Album(Guid id)
        {
            AlbumDto dto = readModel.GetAlbums().FirstOrDefault(a => a.Id == id);

            if (dto.Privacy.CanSee((Guid)HttpContext.Items["userid"]) || (Guid)HttpContext.Items["userid"] == dto.Owner.Id)
            {
                AlbumDetailViewModel model = new AlbumDetailViewModel() { Album = dto };
                model.Username = readModel.GetUsers().FirstOrDefault(u => u.Equals(dto.Owner)).Username;

                return View(model);
            }
            else
                return new HttpUnauthorizedResult();
        }

        [IfLoggedIn]
        public ActionResult Create()
        {
            CreateAlbumViewModel model = new CreateAlbumViewModel();
            UserDto user = (UserDto)Session["user"];
            model.Privacy = new Models.PrivacyViewModel();
            model.Privacy.Level = PrivacyLevel.Public;
            model.Privacy.GroupNames = user.Groups.Select(g => new SelectListItem() { Text = g.Name, Value = g.Id.ToString(), Selected = false });
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult Create(CreateAlbumViewModel model)
        {
            UserDto user = (UserDto)Session["user"];

            bus.Send(new CreateAlbum()
            {
                Id = Guid.NewGuid(),
                Description = model.Description,
                Title = model.Title,
                UserId = user.Id,
                Privacy = new Privacy()
                {
                    OwnerId = user.Id,
                    VisibleToGroups = model.Privacy.SelectedGroups,
                    Published = model.Privacy.Publish,
                    OnlyForLoggedInUsers = model.Privacy.OnlyLoggedInUsers,
                    Level = model.Privacy.Level
                },
                Bus = bus
            });

            return RedirectToAction("Albums", "User", new { username = user.Username });

        }

        [IfLoggedIn]
        public ActionResult Edit(Guid id)
        {
            AlbumDto album = readModel.GetAlbums().FirstOrDefault(a => a.Id == id);
            UserDto user = (UserDto)Session["user"];

            PrivacyViewModel pr = new PrivacyViewModel();
            pr.Level = album.Privacy.Level;
            pr.GroupNames = user.Groups.Select(g => new SelectListItem()
            {
                Text = g.Name,
                Value = g.Id.ToString(),
                Selected = album.Privacy.Level == PrivacyLevel.Group ? ((ImageHoster.CQRS.ReadModel.Dto.GroupPrivacy)(album.Privacy)).GroupsEligible.FirstOrDefault(group => g.Id == group.Id) != null : false
            });
            pr.OnlyLoggedInUsers = album.Privacy.OnlyLoggedIn;
            pr.Publish = album.Privacy.Publicized;

            if (user.Equals(album.Owner))
            {
                EditAlbumViewModel model = new EditAlbumViewModel() { Id = id, Description = album.Description, Title = album.Title, Privacy = pr };
                return View(model);
            }
            else
            {
                return new HttpUnauthorizedResult();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult Edit(EditAlbumViewModel model)
        {
            AlbumDto album = readModel.GetAlbums().FirstOrDefault(a => a.Id == model.Id);
            UserDto user = (UserDto)Session["user"];

            if (user == null)
                return new HttpUnauthorizedResult();
            if (album == null)
                return new HttpNotFoundResult();

            bus.Send(new EditAlbumInfo()
            {
                Id = Guid.NewGuid(),
                User = user.Id,
                AlbumId = album.Id,
                Description = model.Description,
                Title = model.Title,
                Privacy = new Privacy()
                {
                    OwnerId = user.Id,
                    VisibleToGroups = model.Privacy.SelectedGroups ?? new List<Guid>(),
                    Published = model.Privacy.Publish,
                    OnlyForLoggedInUsers = model.Privacy.OnlyLoggedInUsers,
                    Level = model.Privacy.Level
                },
                Bus = bus,
                OldPrivacy = new Privacy()
                {
                    OwnerId = album.Owner.Id,
                    VisibleToGroups = album.Privacy is ImageHoster.CQRS.ReadModel.Dto.GroupPrivacy ? ((ImageHoster.CQRS.ReadModel.Dto.GroupPrivacy)album.Privacy).GroupsEligible.Select(g => g.Id) : new List<Guid>(),
                    Published = album.Privacy.Publicized,
                    OnlyForLoggedInUsers = album.Privacy.OnlyLoggedIn,
                    Level = album.Privacy.Level
                },
                PhotoPrivacies = album.Photos.ToDictionary(p => p.Id, p => new Privacy()
                {
                    OwnerId = p.UploadedBy.Id,
                    VisibleToGroups = p.Privacy is ImageHoster.CQRS.ReadModel.Dto.GroupPrivacy ? ((ImageHoster.CQRS.ReadModel.Dto.GroupPrivacy)p.Privacy).GroupsEligible.Select(g => g.Id) : new List<Guid>(),
                    Published = p.Privacy.Publicized,
                    OnlyForLoggedInUsers = p.Privacy.OnlyLoggedIn,
                    Level = p.Privacy.Level
                })
            });

            return RedirectToAction("Album", new { id = album.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public async Task<ActionResult> Delete(Guid id)
        {
            AlbumDto album = readModel.GetAlbums().FirstOrDefault(p => p.Id == id);
            UserDto user = (UserDto)Session["user"];

            if (album != null)
            {
                bus.Send(new DeleteAlbum() { Id = Guid.NewGuid(), User = user.Id, AlbumId = id, Bus = bus });
                return RedirectToAction("Albums", "User", new { username = user.Username });
            }
            else
                return new HttpNotFoundResult();

        }

        [IfLoggedIn]
        public ActionResult AddImages(Guid id)
        {
            ViewBag.AlbumId = id;
            ViewBag.Mode = "AddImages";
            var user = (UserDto)Session["user"];
            var album = readModel.GetAlbums().FirstOrDefault(a => a.Id == id);

            return View(new PagedList<PhotoDto>(GetPhotosToAdd(user.Photos, album), 1, 24));
        }

        [HttpPost]
        [IfLoggedIn]
        [ValidateAntiForgeryToken]
        public ActionResult AddImages([System.Web.Http.FromBody]Guid albumId, [System.Web.Http.FromBody]IEnumerable<Guid> ids)
        {
            var user = (UserDto)Session["user"];

            foreach (var id in ids)
            {
                bus.Send(new AddPhotoToAlbum() { AlbumId = albumId, Id = Guid.NewGuid(), PhotoId = id, UserId = user.Id });
            }

            return new HttpStatusCodeResult(200);
        }

        [IfLoggedIn]
        public ActionResult RemoveImages(Guid id)
        {
            ViewBag.AlbumId = id;
            ViewBag.Mode = "RemoveImages";
            var user = (UserDto)Session["user"];
            var album = user.Albums.FirstOrDefault(a => a.Id == id);

            if (album != null)
            {
                return View("AddImages", new PagedList<PhotoDto>(album.Photos, 1, 24));
            }
            else
                return new HttpNotFoundResult();
        }

        [HttpPost]
        [IfLoggedIn]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveImages([System.Web.Http.FromBody]Guid albumId, [System.Web.Http.FromBody]IEnumerable<Guid> ids)
        {
            var user = (UserDto)Session["user"];

            foreach (var id in ids)
            {
                bus.Send(new RemovePhotoFromAlbum() { AlbumId = albumId, PhotoId = id, UserId = user.Id });
            }

            return new HttpStatusCodeResult(200);
        }

        [IfLoggedIn]
        public ActionResult GetPhotos(string mode, Guid album, int? page)
        {
            IEnumerable<PhotoDto> allphotos;
            var user = (UserDto)Session["user"];

            if (mode == "RemoveImages")
            {
                allphotos = user.Albums.FirstOrDefault(a => a.Id == album).Photos;
            }
            else
            {
                var albumObj = readModel.GetAlbums().FirstOrDefault(a => a.Id == album);
                allphotos = user.Photos;
                allphotos = GetPhotosToAdd(allphotos, albumObj);
            }

            PagedList<PhotoDto> photos = new PagedList<PhotoDto>(allphotos, page ?? 1, 24);

            return PartialView(photos);
        }

        private IEnumerable<PhotoDto> GetPhotosToAdd(IEnumerable<PhotoDto> photos, AlbumDto album)
        {
            return photos.Where(p =>
            {
                bool canAdd = false;

                if (album.Privacy.Level == PrivacyLevel.Private)
                    canAdd = p.Privacy.OnlyLoggedIn ? album.Privacy.OnlyLoggedIn : true;
                else if (album.Privacy.Level == PrivacyLevel.Group)
                    if (p.Privacy.Level == PrivacyLevel.Public || p.Privacy.Level == PrivacyLevel.Hidden || ((ImageHoster.CQRS.ReadModel.Dto.GroupPrivacy)album.Privacy).GroupsEligible.IsSubsetOf(((ImageHoster.CQRS.ReadModel.Dto.GroupPrivacy)p.Privacy).GroupsEligible))
                        canAdd = p.Privacy.OnlyLoggedIn ? album.Privacy.OnlyLoggedIn : true;
                    else { }
                else if (album.Privacy.Level == PrivacyLevel.Hidden || album.Privacy.Level == PrivacyLevel.Public)
                    if (p.Privacy.Level == PrivacyLevel.Public || p.Privacy.Level == PrivacyLevel.Hidden)
                        canAdd = p.Privacy.OnlyLoggedIn ? album.Privacy.OnlyLoggedIn : true;

                return canAdd;
            });
        }
    }
}
