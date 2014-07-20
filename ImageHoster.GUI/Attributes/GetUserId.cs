using ImageHoster.CQRS.ReadModel.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ImageHoster.GUI.Attributes
{
    public class GetUserId : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.HttpContext.Items["userid"] = filterContext.HttpContext.Session["user"] != null ? ((UserDto)filterContext.HttpContext.Session["user"]).Id : Guid.Empty;

            base.OnActionExecuting(filterContext);
        }
    }
}