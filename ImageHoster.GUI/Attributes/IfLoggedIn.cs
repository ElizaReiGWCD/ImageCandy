using ImageHoster.CQRS.ReadModel;
using ImageHoster.CQRS.ReadModel.Dto;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ImageHoster.GUI.Attributes
{
    public class IfLoggedIn : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext.Session["user"] != null)
            {
                string connection = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                var user = new EFReadModel(connection).GetUsers().FirstOrDefault(u => u.Id == ((UserDto)httpContext.Session["user"]).Id);
                httpContext.Session["user"] = user;

                return true;
            }
            else
                return false;
        }
    }
}