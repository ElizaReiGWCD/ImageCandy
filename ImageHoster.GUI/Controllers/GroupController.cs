using ImageHoster.CQRS.Commands;
using ImageHoster.CQRS.ReadModel;
using ImageHoster.CQRS.ReadModel.Dto;
using ImageHoster.GUI.Models.Group;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using ImageHoster.GUI.Attributes;
using System.Configuration;

namespace ImageHoster.GUI.Controllers
{
    public class GroupController : Controller
    {
        IReadModel readModel;
        IBus bus;

        Random random = new Random();

        public GroupController()
        {
            readModel = new EFReadModel(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            bus = ServiceLocator.Bus;
        }

        public ActionResult Newest(int? page)
        {
            IEnumerable<Tuple<GroupDto, string>> groups = GetGroups();

            var pagedList = new PagedList<Tuple<GroupDto, string>>(groups, page ?? 1, 16);

            return View(pagedList);
        }

        public ActionResult Random()
        {
            IEnumerable<Tuple<GroupDto, string>> groups = GetGroups().OrderBy(g => random.Next());
            var pagedList = new PagedList<Tuple<GroupDto, string>>(groups, 1, 16);

            return View(pagedList);
        }

        public ActionResult MostPopular(int? page)
        {
            IEnumerable<Tuple<GroupDto, string>> groups = GetGroups().OrderByDescending(t => t.Item1.Members.Count);
            var pagedList = new PagedList<Tuple<GroupDto, string>>(groups, page ?? 1, 16);

            return View(pagedList);
        }

        private IEnumerable<Tuple<GroupDto, string>> GetGroups()
        {
            IEnumerable<Tuple<GroupDto, string>> groups = readModel.GetGroups().AsParallel()
                .Select(group =>
                {
                    var photo = group.Photos.FirstOrDefault();

                    if (photo == null)
                        return new Tuple<GroupDto, string>(group, "");

                    var file = photo.FileName;

                    return new Tuple<GroupDto, string>(group, file);
                });
            return groups;
        }

        [GetUserId]
        public ActionResult Index(Guid id)
        {
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == id);

            if (group != null)
            {
                GroupDetailViewModel model = new GroupDetailViewModel();
                model.Name = group.Name;
                model.Id = group.Id;
                model.Description = group.Description;
                model.Users = group.Members;
                model.Moderators = group.Moderators;

                model.Photos = group.Photos;

                model.Albums = group.Albums;

                model.Privacy = group.Privacy;

                model.LatestAnnouncement = group.Announcements.OrderByDescending(g => g.Time).FirstOrDefault();

                ViewBag.LoggedIn = Session["user"] != null;
                ViewBag.InGroup = ViewBag.LoggedIn ? model.Users.Any(u => u.Username == ((UserDto)Session["user"]).Username) : false;
                ViewBag.IsModerator = ViewBag.InGroup ? group.Moderators.Contains(((UserDto)Session["user"])) : false;
                ViewBag.IsAdmin = ViewBag.InGroup ? group.Admin.Id == ((UserDto)Session["user"]).Id : false;
                ViewBag.RequestPending = ViewBag.LoggedIn ? group.JoinRequests.Contains(((UserDto)Session["user"])) : false;

                return View(model);
            }
            else
                return new HttpNotFoundResult();

        }

        [GetUserId]
        public ActionResult Photos(Guid id, int? page)
        {
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == id);

            if (group != null)
            {
                var users = group.Members;
                var photos = users
                    .SelectMany(u => u.Photos)
                    .Where(p => p.Privacy.Level == CQRS.PrivacyLevel.Group
                        && ((GroupPrivacy)p.Privacy).GroupsEligible.FirstOrDefault(g => g != null && g.Id == group.Id) != null
                        && p.Privacy.CanSee((Guid)HttpContext.Items["userid"]));

                ViewBag.GroupName = group.Name;
                PagedList<PhotoDto> model = new PagedList<PhotoDto>(photos, page ?? 1, 16);

                return View(model);
            }
            else
                return new HttpNotFoundResult();
        }

        [GetUserId]
        public ActionResult Albums(Guid id, int? page)
        {
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == id);

            if (group != null)
            {
                var users = group.Members;
                var albums = users
                    .SelectMany(u => u.Albums)
                    .Where(a => a.Privacy.Level == CQRS.PrivacyLevel.Group
                        && ((GroupPrivacy)a.Privacy).GroupsEligible.FirstOrDefault(g => g != null && g.Id == group.Id) != null
                        && a.Privacy.CanSee((Guid)HttpContext.Items["userid"]));

                ViewBag.GroupName = group.Name;
                PagedList<AlbumDto> model = new PagedList<AlbumDto>(albums, page ?? 1, 16);

                return View(model);
            }
            else
                return new HttpNotFoundResult();
        }

        public ActionResult Members(Guid id, int? page)
        {
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == id);

            if (group != null)
            {
                var model = new PagedList<UserDto>(group.Members, page ?? 1, 32);

                ViewBag.GroupName = group.Name;

                return View(model);
            }
            else
                return new HttpNotFoundResult();
        }

        public ActionResult Announcements(Guid id)
        {
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == id);

            if (group == null)
                return new HttpNotFoundResult();

            return View(group.Announcements.OrderByDescending(g => g.Time).ToList());
        }

        [IfLoggedIn]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult Create(CreateGroupViewModel model)
        {
            UserDto user = (UserDto)Session["user"];

            bus.Send(new CreateGroup()
            {
                Id = Guid.NewGuid(),
                Description = model.Description,
                Name = model.Name,
                UserId = user.Id,
                AlreadyExistingGroups = readModel.GetGroups().Select(g => g.Name),
                Privacy = new CQRS.Domain.GroupPrivacy()
                {
                    Level = model.Level,
                    MembersVisibleToOutsiders = model.MembersVisibleToOutsiders,
                    PhotosVisibleToOutsiders = model.PhotosVisibleToOutsiders,
                    GroupVisibleToOutsiders = model.GroupVisibleToOutsiders,
                    PhotosAddedAfterAccepting = model.AcceptPhotos
                }
            });

            return RedirectToAction("Groups", "User", new { username = user.Username });
        }

        [IfLoggedIn]
        public ActionResult Edit(Guid Id)
        {
            EditGroupViewModel model = new EditGroupViewModel();
            GroupDto group = readModel.GetGroups().AsParallel().FirstOrDefault(g => g.Id == Id);

            if (group != null)
            {
                if (group.Admin.Id == ((UserDto)Session["user"]).Id)
                {
                    model.Id = Id;
                    model.Name = group.Name;
                    model.Description = group.Description;
                    model.GroupVisibleToOutsiders = group.Privacy.GroupVisibleToOutsiders;
                    model.MembersVisibleToOutsiders = group.Privacy.MembersVisibleToOutsiders;
                    model.PhotosVisibleToOutsiders = group.Privacy.PhotosVisibleToOutsiders;
                    model.AcceptPhotos = group.Privacy.AddPhotosAfterAccepting;
                    model.Level = group.Privacy.Level;

                    return View(model);
                }
                else
                    return new HttpUnauthorizedResult();
            }
            else
                return new HttpNotFoundResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult Edit(EditGroupViewModel model)
        {
            GroupDto group = readModel.GetGroups().AsParallel().FirstOrDefault(g => g.Id == model.Id);
            UserDto user = (UserDto)Session["user"];

            if (group != null)
            {
                bus.Send(new EditGroupInfo()
                {
                    Id = Guid.NewGuid(),
                    Description = model.Description,
                    UserId = user.Id,
                    GroupId = model.Id,
                    Name = model.Name,
                    Privacy = new CQRS.Domain.GroupPrivacy()
                    {
                        Level = model.Level,
                        GroupVisibleToOutsiders = model.GroupVisibleToOutsiders,
                        MembersVisibleToOutsiders = model.MembersVisibleToOutsiders,
                        PhotosVisibleToOutsiders = model.PhotosVisibleToOutsiders,
                        PhotosAddedAfterAccepting = model.AcceptPhotos
                    },
                    Bus = this.bus
                });
                return RedirectToAction("Index", new { id = model.Id });
            }
            else
                return new HttpNotFoundResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult Delete(Guid id)
        {
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == id);

            if (group != null)
            {
                var user = (UserDto)Session["user"];
                bus.Send(new DeleteGroup() { Id = Guid.NewGuid(), UserId = user.Id, GroupId = id });
                return RedirectToAction("Groups", "User", new { username = user.Username });
            }
            else
                return new HttpNotFoundResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult AddMember([System.Web.Http.FromBody]Guid id)
        {
            UserDto user = (UserDto)Session["user"];
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == id);

            if (group != null)
                bus.Send(new AddMemberToGroup() { Id = Guid.NewGuid(), GroupId = id, UserId = user.Id });

            return RedirectToAction("Index", new { id = id });
        }

        [IfLoggedIn]
        public ActionResult ViewJoinRequests(Guid id, int? page)
        {
            UserDto user = (UserDto)Session["user"];
            var group = readModel.GetGroups().FirstOrDefault(g => g.Id == id);

            if (group == null)
                return new HttpNotFoundResult();
            if (!group.Moderators.Contains(user))
                return new HttpUnauthorizedResult();

            var users = group.JoinRequests;

            var list = new PagedList<UserDto>(users, page ?? 1, 10);
            ViewBag.Name = group.Name;
            ViewBag.GroupId = group.Id;

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult SubmitJoinRequest([System.Web.Http.FromBody]Guid id)
        {
            UserDto user = (UserDto)Session["user"];
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == id);

            if (group != null)
                bus.Send(new SubmitJoinRequest() { UserId = user.Id, GroupId = id });

            return RedirectToAction("Index", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult AcceptRequest([System.Web.Http.FromBody]Guid groupid, [System.Web.Http.FromBody]Guid userid)
        {
            UserDto user = (UserDto)Session["user"];

            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == groupid);

            if (group != null)
                bus.Send(new AcceptJoinRequest() { ActingUser = user.Id, UserId = userid, GroupId = groupid });

            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult RejectRequest([System.Web.Http.FromBody]Guid groupid, [System.Web.Http.FromBody]Guid userid)
        {
            UserDto user = (UserDto)Session["user"];

            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == groupid);

            if (group != null)
                bus.Send(new RejectJoinRequest() { ActingUser = user.Id, UserId = userid, GroupId = groupid });

            return new HttpStatusCodeResult(200);
        }

        [IfLoggedIn]
        public ActionResult InviteUsers(Guid id, int? page)
        {
            UserDto user = (UserDto)Session["user"];
            var group = readModel.GetGroups().FirstOrDefault(g => g.Id == id);

            if (group == null)
                return new HttpNotFoundResult();
            if (!group.Moderators.Contains(user))
                return new HttpUnauthorizedResult();

            var users = readModel.GetUsers().Where(u => !group.Members.Contains(u) && !u.GroupInvites.Contains(group)).OrderBy(u => u.Username);

            var list = new PagedList<UserDto>(users, page ?? 1, 24);
            ViewBag.Name = group.Name;
            ViewBag.GroupId = group.Id;

            return View(list);
        }

        [HttpPost]
        [IfLoggedIn]
        [ValidateAntiForgeryToken]
        public ActionResult InviteUsers([System.Web.Http.FromBody]Guid groupId, [System.Web.Http.FromBody]IEnumerable<Guid> ids)
        {
            var user = (UserDto)Session["user"];

            foreach (var id in ids)
            {
                bus.Send(new InviteUserToGroup() { ActingUser = user.Id, UserId = id, Bus = this.bus, GroupId = groupId });
            }

            return new HttpStatusCodeResult(200);
        }

        [IfLoggedIn]
        public ActionResult GetUsers(Guid id, int? page)
        {
            UserDto user = (UserDto)Session["user"];
            var group = readModel.GetGroups().FirstOrDefault(g => g.Id == id);

            if (group == null)
                return new HttpNotFoundResult();
            if (!group.Moderators.Contains(user))
                return new HttpUnauthorizedResult();

            var users = readModel.GetUsers().Where(u => !group.Members.Contains(u) || !u.GroupInvites.Contains(group)).OrderBy(u => u.Username);

            var list = new PagedList<UserDto>(users, page ?? 1, 24);

            return PartialView(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult RemoveMember([System.Web.Http.FromBody]Guid id)
        {
            UserDto user = (UserDto)Session["user"];

            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == id);

            if (group != null)
                bus.Send(new RemoveMemberFromGroup() { Id = Guid.NewGuid(), GroupId = id, UserId = user.Id });

            return RedirectToAction("Index", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult BanUser([System.Web.Http.FromBody]Guid groupid, [System.Web.Http.FromBody]Guid userid)
        {
            UserDto user = (UserDto)Session["user"];

            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == groupid);

            if (group != null)
                bus.Send(new BanUserFromGroup() { GroupId = groupid, ActingUser = user.Id, UserToBan = userid });

            return RedirectToAction("Index", new { id = groupid });
        }

        [IfLoggedIn]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddModerator([System.Web.Http.FromBody]Guid userid, [System.Web.Http.FromBody]Guid groupid)
        {
            UserDto user = (UserDto)Session["user"];
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == groupid);

            if (group != null)
                bus.Send(new AddModeratorToGroup() { GroupId = groupid, UserToAdd = userid, UserPerforming = user.Id });

            return RedirectToAction("Index", new { id = groupid });
        }

        [IfLoggedIn]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult RemoveModerator([System.Web.Http.FromBody]Guid userid, [System.Web.Http.FromBody]Guid groupid)
        {
            UserDto user = (UserDto)Session["user"];
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == groupid);

            if (group != null)
                bus.Send(new RemoveModeratorFromGroup() { GroupId = groupid, UserToAdd = userid, UserPerforming = user.Id });

            return RedirectToAction("Index", new { id = groupid });
        }

        [IfLoggedIn]
        public ActionResult MakeAnnouncement(Guid id)
        {
            ViewBag.Id = id;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult MakeAnnouncement(MakeAnnouncementViewModel model)
        {
            UserDto user = (UserDto)Session["user"];
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == model.GroupId);

            if (group != null)
                bus.Send(new ImageHoster.CQRS.Commands.MakeAnnouncement() { GroupId = model.GroupId, Announcement = model.Announcement, AnnouncerId = user.Id, Title = model.Title, TimeOfAnnouncement = DateTime.UtcNow });
            else
                return new HttpNotFoundResult();

            return RedirectToAction("Index", new { id = model.GroupId });
        }

        [IfLoggedIn]
        public ActionResult BanPhotos(Guid id)
        {
            UserDto user = (UserDto)Session["user"];
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == id);

            if (group == null)
                return new HttpNotFoundResult();
            if (!group.Moderators.Contains(user))
                return new HttpUnauthorizedResult();

            ViewBag.GroupId = group.Id;

            return View(group.Photos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult BanPhotos([System.Web.Http.FromBody]Guid GroupId, [System.Web.Http.FromBody]List<Guid> ids)
        {
            UserDto user = (UserDto)Session["user"];
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == GroupId);

            if (group == null)
                return new HttpNotFoundResult();

            foreach (var id in ids)
            {
                var photo = readModel.GetPhotos().FirstOrDefault(p => p.Id == id);

                if (photo != null)
                    bus.Send(new BanPhotoFromGroup() { PhotoId = photo.Id, GroupId = GroupId, UserId = user.Id });
            }

            return new HttpStatusCodeResult(200);
        }

        [IfLoggedIn]
        public ActionResult BanAlbums(Guid id)
        {
            UserDto user = (UserDto)Session["user"];
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == id);

            if (group == null)
                return new HttpNotFoundResult();
            if (!group.Moderators.Contains(user))
                return new HttpUnauthorizedResult();

            ViewBag.GroupId = group.Id;

            return View(group.Albums);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult BanAlbums([System.Web.Http.FromBody]Guid GroupId, [System.Web.Http.FromBody]List<Guid> ids)
        {
            UserDto user = (UserDto)Session["user"];
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == GroupId);

            if (group == null)
                return new HttpNotFoundResult();

            foreach (var id in ids)
            {
                var album = readModel.GetAlbums().FirstOrDefault(a => a.Id == id);

                if (album != null)
                    bus.Send(new BanAlbumFromGroup() { AlbumId = album.Id, GroupId = GroupId, UserId = user.Id, Photos = album.Photos.Select(p => p.Id).ToList() });
            }

            return new HttpStatusCodeResult(200);
        }

        [IfLoggedIn]
        public ActionResult ViewPendingAlbums(Guid id)
        {
            UserDto user = (UserDto)Session["user"];
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == id);

            if (group == null)
                return new HttpNotFoundResult();
            if (!group.Moderators.Contains(user))
                return new HttpUnauthorizedResult();

            ViewBag.GroupId = id;

            return View(group.PendingAlbums);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult RejectAlbum([System.Web.Http.FromBody]Guid GroupId, [System.Web.Http.FromBody]Guid albumid)
        {
            UserDto user = (UserDto)Session["user"];
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == GroupId);

            if (group == null)
                return new HttpNotFoundResult();

            var album = readModel.GetAlbums().FirstOrDefault(a => a.Id == albumid);

            if (album != null)
                bus.Send(new RejectAlbumGroup() { AlbumId = album.Id, GroupId = GroupId, ActingUser = user.Id, Photos = album.Photos.Select(p => p.Id).ToList() });

            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult AcceptAlbum([System.Web.Http.FromBody]Guid GroupId, [System.Web.Http.FromBody]Guid albumid)
        {
            UserDto user = (UserDto)Session["user"];
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == GroupId);

            if (group == null)
                return new HttpNotFoundResult();

            var album = readModel.GetAlbums().FirstOrDefault(a => a.Id == albumid);

            if (album != null)
                bus.Send(new AcceptAlbumGroup() { AlbumId = album.Id, GroupId = GroupId, ActingUser = user.Id, Photos = album.Photos.Select(p => p.Id).ToList() });

            return new HttpStatusCodeResult(200);
        }

        [IfLoggedIn]
        public ActionResult ViewPendingPhotos(Guid id)
        {
            UserDto user = (UserDto)Session["user"];
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == id);

            if (group == null)
                return new HttpNotFoundResult();
            if (!group.Moderators.Contains(user))
                return new HttpUnauthorizedResult();

            ViewBag.GroupId = id;

            return View(group.PendingPhotos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult RejectPhoto([System.Web.Http.FromBody]Guid GroupId, [System.Web.Http.FromBody]Guid photoid)
        {
            UserDto user = (UserDto)Session["user"];
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == GroupId);

            if (group == null)
                return new HttpNotFoundResult();

            var photo = readModel.GetPhotos().FirstOrDefault(a => a.Id == photoid);

            if (photo != null)
                bus.Send(new RejectPhotoGroup() { PhotoId = photo.Id, GroupId = GroupId, ActingUser = user.Id });

            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult AcceptPhoto([System.Web.Http.FromBody]Guid GroupId, [System.Web.Http.FromBody]Guid photoid)
        {
            UserDto user = (UserDto)Session["user"];
            GroupDto group = readModel.GetGroups().FirstOrDefault(g => g.Id == GroupId);

            if (group == null)
                return new HttpNotFoundResult();

            var photo = readModel.GetPhotos().FirstOrDefault(a => a.Id == photoid);

            if (photo != null)
                bus.Send(new AcceptPhotoGroup() { PhotoId = photo.Id, GroupId = GroupId, ActingUser = user.Id });

            return new HttpStatusCodeResult(200);
        }
    }
}
