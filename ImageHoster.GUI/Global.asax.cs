using ImageHoster.CQRS.Commands;
using ImageHoster.CQRS.Domain;
using ImageHoster.CQRS.Events;
using ImageHoster.CQRS.ReadModel;
using ImageHoster.CQRS.ReadModel.EFEventhandlers;
using ImageHoster.GUI.Files;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using Sogyo.CQRS.Library.Infrastructure.Bus.Messages;
using Sogyo.CQRS.Library.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ImageHoster.GUI
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Stopwatch s = Stopwatch.StartNew();

            var bus = new FakeBus();
            var eventStore = new RavenEventStore(bus);
            var userRepo = new Repository<User>(eventStore);
            var photoRepo = new Repository<Photo>(eventStore);
            var albumRepo = new Repository<Album>(eventStore);
            var groupRepo = new Repository<Group>(eventStore);

            var userCommands = new UserCommandHandler(userRepo);
            RegisterHandlers(bus, userCommands);

            var photoCommands = new PhotoCommandHandler(photoRepo);
            RegisterHandlers(bus, photoCommands);

            var albumCommands = new AlbumCommandHandler(albumRepo, photoRepo);
            RegisterHandlers(bus, albumCommands);

            var groupCommands = new GroupCommandHandler(groupRepo);
            RegisterHandlers(bus, groupCommands);

            string connectionstring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            var userEvents = new UserEventHandler(connectionstring);
            RegisterHandlers(bus, userEvents);

            var photoEvents = new PhotoEventHandler(connectionstring);
            RegisterHandlers(bus, photoEvents);

            var albumEvents = new AlbumEventHandler(connectionstring);
            RegisterHandlers(bus, albumEvents);

            var groupEvents = new GroupEventHandler(connectionstring);
            RegisterHandlers(bus, groupEvents);

            ServiceLocator.Bus = bus;

#if DEBUG
            ServiceLocator.Files = new LocalFileStorage(Server.MapPath("~/App_Data/Images"));
#else
            ServiceLocator.Files = new LocalFileStorage(Server.MapPath("~/App_Data/Images"));
#endif
            ServiceLocator.Files.Setup();
        }

        private void RegisterHandlers(IBus bus, object obj)
        {
            var interfaces = obj.GetType().GetInterfaces().SelectMany(i => i.GetGenericArguments());

            foreach (var i in interfaces)
            {
                var handleMethod = obj.GetType().GetMethod("Handle", new Type[] { i });
                var del = Delegate.CreateDelegate(Expression.GetActionType(i), obj, handleMethod);

                MethodInfo register = bus.GetType().GetMethod("RegisterHandler").MakeGenericMethod(i);
                register.Invoke(bus, new object[] { del });
            }
        }
    }
}   