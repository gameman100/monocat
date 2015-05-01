using System;
using System.Collections.Generic;
using System.Text;

namespace monocat
{
    public class SignupRequest : BaseRequest
    {
        public string username = string.Empty;
        public string password = string.Empty;
        /// <summary>
        /// 是否是取机器名注册
        /// </summary>
        public int machine_code = 0;
    }
}
