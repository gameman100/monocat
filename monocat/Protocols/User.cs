using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace monocat
{
    /// <summary>
    /// 用户数据[与数据库的table结构，字段名一致】
    /// </summary>
    public class User
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int id = 0;
        /// <summary>
        /// 用户名
        /// </summary>
        public string username = string.Empty;
        /// <summary>
        /// 密码
        /// </summary>
        public string password = string.Empty;
        /// <summary>
        /// 邮件
        /// </summary>
        public string email = string.Empty;
        /// <summary>
        /// 会话[不需要存入数据库]
        /// </summary>
        public string session = string.Empty;
        /// <summary>
        /// 注册时间
        /// </summary>
        public string signdate = string.Empty;
        /// <summary>
        /// 最后登陆的时间
        /// </summary>
        public string lastlogin = string.Empty;

        /// <summary>
        /// 登陆计时
        /// </summary>
        public CDTimer logintimer = new CDTimer();

        public string loginsession = string.Empty;
    }
}
