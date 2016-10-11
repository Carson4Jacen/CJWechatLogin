using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
using System.Net;
using System.Web.Security;
using LitJson;
using WeChatAPI;

namespace WechatUserInfo
{
    public class WechatUserContext
    {
        /// <summary>
        /// 上下文
        /// </summary>
        private HttpContext currentHttpContext { get; set; }

        private string state { get; set; }
        /// <summary>
        /// openid
        /// </summary>
        public string openid { get; set; }

        /// <summary>
        /// access_token
        /// </summary>
        public string access_token { get; set; }

        //新增参数
        public string nickname { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int sex { get; set; }

        public string province { get; set; }

        public string city { get; set; }

        public string country { get; set; }

        public string headimgurl { get; set; }

        public string unionid { get; set; }

        public WechatUserContext(HttpContext currentHttpContext, string state)
        {
            this.currentHttpContext = currentHttpContext;
            this.state = state;
        }

        /**
        * 
        * 网页授权获取用户基本信息的全部过程
        * 详情请参看网页授权获取用户基本信息：http://mp.weixin.qq.com/wiki/17/c0f37d5704f0b64713d5d2c37b468d75.html
        * 第一步：利用url跳转获取code
        * 第二步：利用code去获取openid和access_token
        * 
        */
        public void GetOpenidAndAccessToken()
        {
            if (!string.IsNullOrEmpty(currentHttpContext.Request.QueryString["code"]))
            {
                //获取code码，以获取openid和access_token
                string code = currentHttpContext.Request.QueryString["code"];
                GetOpenidAndAccessTokenFromCode(code);
            }
            else
            {
                //构造网页授权获取code的URL

                string host = currentHttpContext.Request.Url.Host;
                string path = currentHttpContext.Request.Path;
                string redirect_uri = HttpUtility.UrlEncode("http://" + host + path);
                WeChatData data = new WeChatData();
                data.SetValue("appid", WeChatConfig.APPID);
                data.SetValue("redirect_uri", redirect_uri);
                data.SetValue("response_type", "code");
                data.SetValue("scope", "snsapi_base");
                data.SetValue("state", state + "#wechat_redirect");//授权登录的改造
                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?" + data.ToUrl();
                try
                {
                    //触发微信返回code码
                    currentHttpContext.Response.Redirect(url);//Redirect函数会抛出ThreadAbortException异常，不用处理这个异常
                }
                catch (System.Threading.ThreadAbortException ex)
                {
                }
            }
        }
        
        /**
        * 
        * 通过code换取网页授权access_token和openid的返回数据，正确时返回的JSON数据包如下：
        * {
        *  "access_token":"ACCESS_TOKEN",
        *  "expires_in":7200,
        *  "refresh_token":"REFRESH_TOKEN",
        *  "openid":"OPENID",
        *  "scope":"SCOPE",
        *  "unionid": "o6_bmasdasdsad6_2sgVt7hMZOPfL"
        * }
        * 其中access_token可用于获取共享收货地址
        * openid是微信支付jsapi支付接口统一下单时必须的参数
        * 更详细的说明请参考网页授权获取用户基本信息：http://mp.weixin.qq.com/wiki/17/c0f37d5704f0b64713d5d2c37b468d75.html
        * @失败时抛异常WxPayException
        */
        public void GetOpenidAndAccessTokenFromCode(string code)
        {
            try
            {
                //构造获取openid及access_token的url
                WeChatData data = new WeChatData();
                data.SetValue("appid", WeChatConfig.APPID);
                data.SetValue("secret", WeChatConfig.APPSECRET);
                data.SetValue("code", code);
                data.SetValue("grant_type", "authorization_code");
                string url = "https://api.weixin.qq.com/sns/oauth2/access_token?" + data.ToUrl();

                //请求url以获取数据
                string result = HttpService.Get(url);

                //Log.Debug(this.GetType().ToString(), "GetOpenidAndAccessTokenFromCode response : " + result);
                WriteContent(result);
                //保存access_token，用于收货地址获取
                JsonData jd = JsonMapper.ToObject(result);

                access_token = (string)jd["access_token"];

                //获取用户openid
                openid = (string)jd["openid"];
            }
            catch (Exception ex)
            {
                throw new WeChatException(ex.ToString());
            }
        }

        public void GetUserInfo()
        {
            if (!string.IsNullOrEmpty(currentHttpContext.Request.QueryString["code"]))
            {
                //获取code码，以获取openid和access_token
                string code = currentHttpContext.Request.QueryString["code"];
                GetOpenidAndAccessTokenFromCode(code);

                //拉取用户基本信息
                string user_info_api = "https://api.weixin.qq.com/sns/userinfo?access_token=" + this.access_token + "&openid=" + openid + "&lang=zh_CN";
                string result = HttpService.Get(user_info_api);

                WriteContent(result);

                JsonData jd = JsonMapper.ToObject(result);
                nickname = (string)jd["nickname"];
                sex = (int)jd["sex"];
                province = (string)jd["province"];
                city = (string)jd["city"];
                country = (string)jd["country"];
                headimgurl = (string)jd["headimgurl"];
                //unionid = (string)jd["unionid"];
            }
            else
            {
                //构造网页授权获取code的URL

                string host = currentHttpContext.Request.Url.Host;
                string path = currentHttpContext.Request.Path;
                string redirect_uri = HttpUtility.UrlEncode("http://" + host + path);
                WeChatData data = new WeChatData();
                data.SetValue("appid", WeChatConfig.APPID);
                data.SetValue("redirect_uri", redirect_uri);
                data.SetValue("response_type", "code");
                data.SetValue("scope", "snsapi_userinfo");
                data.SetValue("state", state + "#wechat_redirect");//授权登录的改造

                string url = "https://open.weixin.qq.com/connect/oauth2/authorize?" + data.ToUrl();
                try
                {
                    //触发微信返回code码
                    currentHttpContext.Response.Redirect(url);//Redirect函数会抛出ThreadAbortException异常，不用处理这个异常
                }
                catch (System.Threading.ThreadAbortException ex)
                {
                }
            }          
        }

        public void WriteContent(string content)
        {
            string path = System.Web.HttpContext.Current.Server.MapPath("~/log.txt");
            StreamWriter mySw = System.IO.File.AppendText(path);
            mySw.WriteLine(content);
            mySw.Close();
        }

    }
}