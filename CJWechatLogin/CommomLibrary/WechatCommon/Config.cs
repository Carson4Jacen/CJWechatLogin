using System;
using System.Collections.Generic;
using System.Web;

namespace WeChatAPI
{
    /**
    * 	配置账号信息
    */
    public class WeChatConfig
    {
        //=======【基本信息设置】=====================================

        //获取用户信息基本配置参数
        public const string APPID = "";
        public const string KEY = "";//需要微信签名的功能模块才用到(签名方法:Data.cs MakeSign )
        public const string APPSECRET = "";

    }
}