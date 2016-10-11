using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CJWechatLogin.Models;

namespace CJWechatLogin.Controllers
{
    public class BaseController : Controller
    {

        protected PCWeChatLoginEntities db = new PCWeChatLoginEntities();

    }
}
