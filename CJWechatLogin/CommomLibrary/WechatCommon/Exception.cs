using System;
using System.Collections.Generic;
using System.Web;

namespace WeChatAPI
{
    public class WeChatException : Exception 
    {
        public WeChatException(string msg)
            : base(msg) 
        {

        }
     }
}