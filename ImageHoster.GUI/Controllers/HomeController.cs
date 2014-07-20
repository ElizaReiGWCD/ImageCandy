using ImageHoster.CQRS.Commands;
using ImageHoster.CQRS.ReadModel;
using ImageHoster.CQRS.ReadModel.Dto;
using ImageHoster.GUI.Attributes;
using ImageHoster.GUI.Models;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using System.Net.Mail;

namespace ImageHoster.GUI.Controllers
{

    public class LogEntry
    {
        public string Revision { get; set; }
        public string Author { get; set; }
        public string Date { get; set; }
        public string Message { get; set; }
    }

    public class HomeController : Controller
    {
        IReadModel readModel;
        IBus bus;
        Random random = new Random();

        public HomeController()
        {
            readModel = new EFReadModel(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            bus = ServiceLocator.Bus;
        }

        // GET: /Home/
        [GetUserId]
        public ActionResult Index()
        {
            var photos = readModel.GetPhotos()
                .Where(p => p.Privacy.Publicized && p.Privacy.CanSee((Guid)HttpContext.Items["userid"]))
                .OrderBy(p => random.Next())
                .Take(16);

            return View(photos);
        }

        public ActionResult Changelog()
        {
            string path = Server.MapPath("~/Content/output.xml");

            var stream = new StreamReader(path);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(stream.ReadToEnd());

            var root = doc.SelectSingleNode("log");
            var entries = root.SelectNodes("logentry");

            List<LogEntry> objects = new List<LogEntry>();

            foreach (var entry in entries)
            {
                var node = (XmlNode)entry;
                var revision = node.Attributes["revision"].Value;
                var author = node.SelectSingleNode("author").InnerText;
                var date = node.SelectSingleNode("date").InnerText;
                var message = node.SelectSingleNode("msg").InnerText;
                objects.Add(new LogEntry() { Author = author, Date = date, Message = message, Revision = revision });
            }

            stream.Close();

            return View(objects);
        }

        public ActionResult FeatureRequest()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FeatureRequest(FeatureRequestViewModel model)
        {
            MailMessage message = new MailMessage();

            SmtpClient client = new SmtpClient("smtp-mail.outlook.com", 25);
            client.Credentials = new System.Net.NetworkCredential("elizarei@outlook.com", "dimitrie8");
            client.EnableSsl = true;

            message.From = new MailAddress(model.Email, "Feature Requests Leeward Candy");
            message.To.Add("elizarei@outlook.com");
            message.Subject = "Feature Request: " + model.Title;
            message.Body = model.Description;

            client.Send(message);

            return new HttpStatusCodeResult(200);
        }
    }
}
