using ImageHoster.CQRS.ReadModel;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ImageHoster.CQRS.ReadModel.Dto;
using ImageHoster.GUI.Models.Photo;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using ImageHoster.CQRS.Commands;
using System.Threading.Tasks;
using ImageHoster.GUI.Models;
using ImageHoster.GUI.Attributes;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using ImageHoster.CQRS.Domain;
using System.Configuration;
using ImageHoster.CQRS;

namespace ImageHoster.GUI.Controllers
{
    public class PhotoController : Controller
    {
        IReadModel readModel;
        IBus bus;

        public PhotoController()
        {
            readModel = new EFReadModel(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            bus = ServiceLocator.Bus;
        }

        [GetUserId]
        public async Task<ActionResult> GetImage(string filename)
        {
            PhotoDto dto = readModel.GetPhotos().FirstOrDefault(p => p.FileName == filename);

            if (dto == null)
                return new HttpNotFoundResult();

            if (!CanSeePhoto(dto, (Guid)HttpContext.Items["userid"]))
                return new HttpUnauthorizedResult();

            var target = await ServiceLocator.Files.Get(filename, "");

            FileContentResult result = File(target.Data.ToArray(), target.ContentType);
            target.Data.Close();
            return result;
        }

        [GetUserId]
        public async Task<ActionResult> GetThumbnail(string filename)
        {
            PhotoDto dto = readModel.GetPhotos().FirstOrDefault(p => p.FileName == filename);

            if (!CanSeePhoto(dto, (Guid)HttpContext.Items["userid"]))
                return new HttpUnauthorizedResult();

            var target = await ServiceLocator.Files.Get(filename, "t");

            FileContentResult result = File(target.Data.ToArray(), target.ContentType);
            target.Data.Close();
            return result;
        }

        [GetUserId]
        public ActionResult Detail(Guid id)
        {
            PhotoDto dto = readModel.GetPhotos().FirstOrDefault(p => p.Id == id);

            if (!CanSeePhoto(dto, (Guid)HttpContext.Items["userid"]))
                return new HttpUnauthorizedResult();

            UserDto user = readModel.GetUsers().FirstOrDefault(u => u.Id == dto.UploadedBy.Id);

            PhotoViewModel model = new PhotoViewModel() { Username = user.Username, Filename = dto.FileName, Description = dto.Description, Title = dto.Title, Id = dto.Id };

            return View(model);
        }

        [IfLoggedIn]
        public ActionResult Upload()
        {
            PhotoUploadViewModel model = new PhotoUploadViewModel();
            model.AlbumSelection = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "", Selected = true } };
            UserDto user = (UserDto)Session["user"];
            model.Privacy = new Models.PrivacyViewModel();
            model.Privacy.Level = CQRS.PrivacyLevel.Public;
            model.Privacy.GroupNames = user.Groups.Select(g => new SelectListItem() { Text = g.Name, Value = g.Id.ToString(), Selected = false });

            foreach (AlbumDto dto in user.Albums)
                model.AlbumSelection.Add(new SelectListItem() { Text = dto.Title, Value = dto.Id.ToString(), Selected = false });

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult Upload(PhotoUploadViewModel model)
        {
            UserDto user = (UserDto)Session["user"];

            switch (model.ImageData.ContentType)
            {
                case "image/gif":
                case "image/jpeg":
                case "image/png":
                    SendUploadCommand(model, user);

                    return RedirectToAction("Photos", "User", new { username = user.Username });
                default:
                    break;
            }

            model.AlbumSelection = new List<SelectListItem>() { new SelectListItem() { Value = "", Text = "", Selected = true } };
            foreach (AlbumDto dto in user.Albums)
                model.AlbumSelection.Add(new SelectListItem() { Text = dto.Title, Value = dto.Id.ToString(), Selected = false });

            return View(model);
        }

        [HttpPost]
        [IfLoggedIn]
        public ActionResult UploadToAlbum(Guid albumId, IEnumerable<HttpPostedFileBase> files)
        {
            UserDto user = (UserDto)Session["user"];
            var album = user.Albums.FirstOrDefault(a => a.Id == albumId);

            if (album != null)
            {
                foreach (var file in files)
                {
                    switch (file.ContentType)
                    {
                        case "image/gif":
                        case "image/jpeg":
                        case "image/png":
                            var model = new PhotoUploadViewModel()
                            {
                                AlbumId = albumId,
                                ImageData = file,
                                Privacy = new Models.PrivacyViewModel()
                                {
                                    Level = album.Privacy.Level,
                                    OnlyLoggedInUsers = album.Privacy.OnlyLoggedIn,
                                    Publish = album.Privacy.Publicized,
                                    SelectedGroups = album.Privacy.Level == CQRS.PrivacyLevel.Group ? ((ImageHoster.CQRS.ReadModel.Dto.GroupPrivacy)album.Privacy).GroupsEligible.Select(x => x.Id).ToList() : null
                                }
                            };
                            SendUploadCommand(model, user);
                            break;
                        default:
                            ModelState.AddModelError("dropzone", "This file has the wrong type");
                            break;
                    }
                }
            }

            return RedirectToAction("Album", "Album", new { id = albumId });
        }

        [IfLoggedIn]
        public ActionResult Edit(Guid id)
        {
            PhotoDto photo = readModel.GetPhotos().FirstOrDefault(p => p.Id == id);
            UserDto user = (UserDto)Session["user"];

            PrivacyViewModel pr = new PrivacyViewModel();
            pr.Level = photo.Privacy.Level;
            pr.GroupNames = user.Groups.Select(g => new SelectListItem()
            {
                Text = g.Name,
                Value = g.Id.ToString(),
                Selected = photo.Privacy.Level == PrivacyLevel.Group ? ((ImageHoster.CQRS.ReadModel.Dto.GroupPrivacy)(photo.Privacy)).GroupsEligible.Contains(g) : false
            });
            pr.OnlyLoggedInUsers = photo.Privacy.OnlyLoggedIn;
            pr.Publish = photo.Privacy.Publicized;

            if (user.Id == photo.UploadedBy.Id)
            {
                EditPhotoViewModel model = new EditPhotoViewModel()
                {
                    Id = id,
                    Description = photo.Description,
                    Title = photo.Title,
                    Privacy = pr
                };
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
        public ActionResult Edit(Guid id, EditPhotoViewModel model)
        {
            PhotoDto photo = readModel.GetPhotos().FirstOrDefault(p => p.Id == model.Id);
            UserDto user = (UserDto)Session["user"];

            if (photo == null)
                return new HttpNotFoundResult();

            var albums = readModel.GetAlbums().Where(a => a.Photos.FirstOrDefault(p => p.Id == photo.Id) != null).Select(a => a.Id).ToList();

            bus.Send(new EditPhotoInfo()
            {
                Id = Guid.NewGuid(),
                User = user.Id,
                PhotoId = photo.Id,
                Description = model.Description,
                Title = model.Title,
                Privacy = new ImageHoster.CQRS.Domain.Privacy()
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
                    OwnerId = photo.UploadedBy.Id,
                    VisibleToGroups = photo.Privacy is ImageHoster.CQRS.ReadModel.Dto.GroupPrivacy ? ((ImageHoster.CQRS.ReadModel.Dto.GroupPrivacy)photo.Privacy).GroupsEligible.Select(g => g.Id) : new List<Guid>(),
                    Published = photo.Privacy.Publicized,
                    OnlyForLoggedInUsers = photo.Privacy.OnlyLoggedIn,
                    Level = photo.Privacy.Level
                },
                Albums = albums
            });
            return RedirectToAction("Detail", new { id = photo.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public async Task<ActionResult> Delete(Guid id)
        {
            PhotoDto photo = readModel.GetPhotos().FirstOrDefault(p => p.Id == id);
            UserDto user = (UserDto)Session["user"];

            if (photo != null)
            {
                try
                {
                    ServiceLocator.Files.Delete(photo.FileName);
                }
                catch
                {

                }

                var albums = readModel.GetAlbums().Where(a => a.Photos.FirstOrDefault(p => p.Id == photo.Id) != null).Select(a => a.Id).ToList();

                bus.Send(new DeletePhoto() { Id = Guid.NewGuid(), UserId = user.Id, PhotoId = id, Bus = bus, Albums = albums });
                return RedirectToAction("Photos", "User", new { username = user.Username });
            }
            else
                return new HttpNotFoundResult();

        }

        private void SendUploadCommand(PhotoUploadViewModel model, UserDto user)
        {
            Guid guid = UploadPhotoToCloud(model);

            bus.Send(new UploadPhoto(guid, user.Id, model.ImageData.InputStream, guid.ToString() + Path.GetFileName(model.ImageData.FileName), model.ImageData.ContentType,
                new ImageHoster.CQRS.Domain.Privacy() { OwnerId = user.Id, VisibleToGroups = model.Privacy.SelectedGroups, Published = model.Privacy.Publish, OnlyForLoggedInUsers = model.Privacy.OnlyLoggedInUsers, Level = model.Privacy.Level }, bus));

            if (model.AlbumId != Guid.Empty)
            {
                var photo = readModel.GetPhotos().FirstOrDefault(p => p.Id == guid);

                while (photo == null)
                {
                    Thread.Sleep(25);
                    photo = readModel.GetPhotos().FirstOrDefault(p => p.Id == guid);
                }

                bus.Send(new AddPhotoToAlbum() { Id = Guid.NewGuid(), UserId = user.Id, AlbumId = model.AlbumId, PhotoId = guid });
            }
        }

        private Guid UploadPhotoToCloud(PhotoUploadViewModel model)
        {
            Guid guid = Guid.NewGuid();
            string path = guid.ToString() + Path.GetFileName(model.ImageData.FileName);

            ServiceLocator.Files.Upload(path, model.ImageData.InputStream, model.ImageData.ContentType, new Size(220, 150));


            return guid;
        }

        private bool CanSeePhoto(PhotoDto dto, Guid userid)
        {
            if (userid == dto.UploadedBy.Id)
                return true;
            else
                return dto.Privacy.CanSee(userid);
        }
    }
}
