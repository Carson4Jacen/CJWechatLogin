using System;
using System.Linq;
using System.Web.Mvc;
using CJWechatLogin.Models;
using System.Web.Security;
using WechatUserInfo;

namespace CJWechatLogin.Controllers
{
    public class HomeController : BaseController
    {
        //PC端授权登录DEMO

        #region 01.首页 - Index
        [Authorize]
        public ActionResult Index()
        {
            string user_name = System.Web.HttpContext.Current.User.Identity.Name;
            WX_UserRecord user = db.WX_UserRecord.Where(u => u.OpenId == user_name).FirstOrDefault();
            if (null == user)
                return HttpNotFound();
            return View(user);
        } 
        #endregion

        #region 02.扫码登录页 - Login

        public ActionResult Login()
        {
            //如果已登录,直接跳转到首页
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            string url = Request.Url.Host;
            string uuid = Guid.NewGuid().ToString();
            ViewBag.url = "http://" + url + "/home/loginfor?uuid=" + uuid;//构造授权链接
            ViewBag.uuid = uuid;//保存 uuid
            return View();
        } 
        #endregion

        #region 03.微信端授权页面 - LoginFor(string uuid)
        public ActionResult LoginFor(string uuid)
        {
            #region snsapi_base
            //JsApiPay jsApiPay = new JsApiPay(System.Web.HttpContext.Current, uuid);
            //jsApiPay.GetOpenidAndAccessToken();
            //if (!string.IsNullOrEmpty(jsApiPay.openid))
            //{
            //    uuid = Request["state"];
            //    //判断数据库是否存在
            //    WX_UserRecord user = db.WX_UserRecord.Where(u => u.OpenId == jsApiPay.openid).FirstOrDefault();
            //    if (user == null)
            //    {
            //        user = new WX_UserRecord();
            //        user.OpenId = jsApiPay.openid;
            //        user.uuId = uuid;
            //        db.WX_UserRecord.Add(user);
            //    }
            //    user.uuId = uuid;
            //    db.SaveChanges();
            //}
            //return View(); 
            #endregion

            #region 获取基本信息 - snsapi_userinfo-这里的授权可以写到过滤器上去变成通用的授权登录
            WechatUserContext wxcontext = new WechatUserContext(System.Web.HttpContext.Current, uuid);
            wxcontext.GetUserInfo();

            if (!string.IsNullOrEmpty(wxcontext.openid))
            {
                uuid = Request["state"];
                //判断数据库是否存在
                WX_UserRecord user = db.WX_UserRecord.Where(u => u.OpenId == wxcontext.openid).FirstOrDefault();
                if (user == null)
                {
                    user = new WX_UserRecord();
                    user.OpenId = wxcontext.openid;
                    user.City = wxcontext.city;
                    user.Country = wxcontext.country;
                    user.CreateTime = DateTime.Now;
                    user.HeadImgUrl = wxcontext.headimgurl;
                    user.Nickname = wxcontext.nickname;
                    user.Province = wxcontext.province;
                    user.Sex = wxcontext.sex;
                    user.Unionid = wxcontext.unionid;
                    user.uuId = uuid;
                    db.WX_UserRecord.Add(user);
                }
                user.uuId = uuid;
                db.SaveChanges();
            }
            #endregion

            return View();
        } 
        #endregion

        #region 04. 异步轮询API- UserLogin(string uuid)

        public string UserIsLogin(string uuid)
        {
            //验证参数是否合法
            if (string.IsNullOrEmpty(uuid))
                return "param_error";

            WX_UserRecord user = db.WX_UserRecord.Where(u => u.uuId == uuid).FirstOrDefault();
            if (user == null)
                return "not_authcode";

            //写入cookie
            FormsAuthentication.SetAuthCookie(user.OpenId, false);

            //清空uuid
            user.uuId = null;
            db.SaveChanges();

            return "success";
        } 
        #endregion

    }
}
