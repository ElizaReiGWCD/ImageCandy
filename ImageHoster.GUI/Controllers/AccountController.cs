using ImageHoster.CQRS.Commands;
using ImageHoster.CQRS.ReadModel;
using ImageHoster.GUI.Models.User;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using ImageHoster.CQRS.ReadModel.Dto;
using System.Globalization;
using System.Threading;
using ImageHoster.GUI.Attributes;
using System.Configuration;

namespace ImageHoster.GUI.Controllers
{
    public class AccountController : Controller
    {
        IReadModel readModel;
        IBus bus;

        public AccountController()
        {
            readModel = new EFReadModel(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            bus = ServiceLocator.Bus;
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            //Check if user exists
            UserDto user = readModel.GetUsers().FirstOrDefault(u => u.Username.ToLower() == model.Username.ToLower());

            if (user != null)
            {
                //Check if pw is correct
                string pw = Encryption.ComputeHash(model.Password, user.Salt);

                if (user.Password == pw)
                {
                    this.Session.Add("username", user.Username);
                    this.Session.Add("password", pw);
                    this.Session.Add("user", user);
                    return RedirectToAction("Index", "Home");
                }
            }

            //If user doesnt exist or pw is incorrect, add error
            ModelState.AddModelError("Username", "Username or password is incorrect");

            return View(model);
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            //If there are errors, return immediately
            if (ModelState.Values.SelectMany(v => v.Errors).Count() > 0)
                return View(model);

            string salt = Encryption.GetSalt();
            model.Password = Encryption.ComputeHash(model.Password, salt);

            Guid userid = Guid.NewGuid();
            bus.Send(new RegisterUser(userid,
                model.Username,
                model.Password,
                salt,
                model.Email,
                model.FirstName,
                model.LastName,
                model.Sex,
                model.About,
                readModel.GetUsers().Select(u => u.Username)
                ));

            this.Session.Add("username", model.Username);
            this.Session.Add("password", model.Password);

            UserDto user = null;

            while (user == null)
            {
                Thread.Sleep(25);
                user = readModel.GetUsers().FirstOrDefault(u => u.Id == userid);
            }

            this.Session.Add("user", user);

            return RedirectToAction("Index", "Home");
        }

        [IfLoggedIn]
        public ActionResult Manage()
        {
            UserDto user = (UserDto)Session["user"];

            ManageViewModel vm = new ManageViewModel();
            vm.About = user.About;
            vm.FirstName = user.FirstName;
            vm.LastName = user.LastName;
            vm.Sex = user.Sex;
            vm.Id = user.Id;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult Manage(ManageViewModel model)
        {
            if (ModelState.Values.SelectMany(v => v.Errors).Count() > 0)
                return View(model);

            UserDto user = (UserDto)Session["user"];

            string pw = Encryption.ComputeHash(model.OldPassword, user.Salt);

            if (user.Password == pw)
            {
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    model.NewPassword = Encryption.ComputeHash(model.NewPassword, user.Salt);
                }

                bus.Send(new ChangeUserInfo(model.Id, model.NewPassword, model.FirstName, model.LastName, model.Sex, model.About));
            }
            else
            {
                ModelState.AddModelError("OldPassword", "The password is incorrect");
                return View(model);
            }

            return RedirectToAction("Index", "User", new { username = user.Username });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [IfLoggedIn]
        public ActionResult LogOff()
        {
            Session.Remove("username");
            Session.Remove("password");
            Session.Remove("user");
            return RedirectToAction("Index", "Home");
        }
    }
}
