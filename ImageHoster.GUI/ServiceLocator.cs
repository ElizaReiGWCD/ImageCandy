using ImageHoster.GUI.Files;
using Microsoft.WindowsAzure.Storage.Blob;
using Sogyo.CQRS.Library.Infrastructure.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImageHoster.GUI
{
    public static class ServiceLocator
    {
        public static IBus Bus { get; set; }
        public static IFileService Files { get; set; }
    }
}