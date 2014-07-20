using ImageHoster.CQRS;
using ImageHoster.CQRS.Commands;
using ImageHoster.CQRS.Domain;
using ImageHoster.CQRS.ReadModel;
using ImageHoster.CQRS.ReadModel.Dto;
using ImageHoster.GUI.Attributes;
using ImageHoster.GUI.Models.Album;
using ImageHoster.GUI.Models.Photo;
using ImageHoster.GUI.Models.User;
using PagedList;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ImageHoster.GUI.Controllers
{
    public class UserController : Controller
    {
        IReadModel readModel;
        IBus bus;

        public UserController()
        {
            readModel = new EFReadModel(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            bus = ServiceLocator.Bus;
        }

        [GetUserId]
        public ActionResult Index(string username)
        {
            UserDto user = readModel.GetUsers().AsParallel().FirstOrDefault(u => u.Username.ToLower() == username.ToLower());

            if (user != null)
            {
                IEnumerable<GroupDto> groups = user.Groups.Take(4);
                var photos = user.Photos
                    .Where(p => p.Privacy.Level != PrivacyLevel.Hidden ? p.Privacy.CanSee((Guid)HttpContext.Items["userid"]) : p.UploadedBy.Id == (Guid)HttpContext.Items["userid"]).Take(4);
                var albums = user.Albums
                    .Where(a => a.Privacy.Level != PrivacyLevel.Hidden ? a.Privacy.CanSee((Guid)HttpContext.Items["userid"]) : a.Owner.Id == (Guid)HttpContext.Items["userid"]).Take(4);

                UserDetailViewModel model = new UserDetailViewModel() { User = user, Groups = groups, Albums = albums, Photos = photos };

                return View(model);
            }
            else
                return new HttpNotFoundResult();
        }

        [GetUserId]
        public ActionResult Photos(string username)
        {
            UserDto user = readModel.GetUsers().AsParallel().FirstOrDefault(u => u.Username.ToLower() == username.ToLower());

            if (user == null)
                return new HttpNotFoundResult();

            PhotoListViewModel model = new PhotoListViewModel()
            {
                Photos = user.Photos
                    .Where(p => CanSeePhoto(p, (Guid)HttpContext.Items["userid"])).ToList(),
                Username = username
            };

            return View(model);
        }

        [GetUserId]
        public ActionResult Albums(string username)
        {
            UserDto user = readModel.GetUsers().AsParallel().FirstOrDefault(u => u.Username.ToLower() == username.ToLower());

            if (user == null)
                return new HttpNotFoundResult();

            AlbumListViewModel model = new AlbumListViewModel()
            {
                Albums = user.Albums
                    .Where(a => CanSeePhoto(a, (Guid)HttpContext.Items["userid"])).ToList(),
                Username = username
            };

            return View(model);
        }

        public ActionResult Groups(string username)
        {
            UserDto user = readModel.GetUsers().AsParallel().FirstOrDefault(u => u.Username.ToLower() == username.ToLower());

            if (user == null)
                return new HttpNotFoundResult();

            IEnumerable<Tuple<GroupDto, string>> groups = user.Groups
                .Select(group =>
                {
                    var photo = group.Photos.FirstOrDefault();

                    if (photo == null)
                        return new Tuple<GroupDto, string>(group, "");

                    var file = photo.FileName;

                    return new Tuple<GroupDto, string>(group, file);
                });

            ViewBag.Username = user.Username;

            return View(groups);
        }

        [IfLoggedIn]
        public ActionResult News(string username)
        {
            UserDto user = (UserDto)Session["user"];
            var model = new NewsViewModel();

            model.Announcements = user.Groups
                .SelectMany(g => g.Announcements)
                .OrderByDescending(g => g.Time)
                .ToList();

            bus.Send(new ViewNewsPage() { UserId = user.Id });

            return View(model);
        }

        [IfLoggedIn]
        [GetUserId]
        public ActionResult GroupInvites(string username, int? page)
        {
            UserDto user = readModel.GetUsers().AsParallel().FirstOrDefault(u => u.Username.ToLower() == username.ToLower());

            if (user == null)
                return new HttpNotFoundResult();
            if (user.Id != (Guid)HttpContext.Items["userid"])
            {
                return new HttpUnauthorizedResult();
            }

            var groups = user.GroupInvites;

            var list = new PagedList<GroupDto>(groups, page ?? 1, 10);

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult AcceptInvite([System.Web.Http.FromBody]Guid groupid)
        {
            UserDto user = (UserDto)Session["user"];

            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == groupid);

            if (group != null)
                bus.Send(new UserAcceptsInvite() { Bus = this.bus, GroupId = groupid, UserId = user.Id });

            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult RejectInvite([System.Web.Http.FromBody]Guid groupid)
        {
            UserDto user = (UserDto)Session["user"];

            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == groupid);

            if (group != null)
                bus.Send(new UserRejectsInvite() { GroupId = groupid, UserId = user.Id });

            return new HttpStatusCodeResult(200);
        }

        private bool CanSeePhoto(AlbumDto dto, Guid userid)
        {
            if (userid == dto.Owner.Id)
                return true;
            else
                return dto.Privacy.CanSee(userid);
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
