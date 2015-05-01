using System.Collections.Generic;
using System.Text;

namespace monocat
{
    public class LoginResponse
    {
        public enum Status
        {
            OK = 1,
            Fail = 2,
        }

        public Status status = Status.OK;

        /// <summary>
        /// 用户id
        /// </summary>
        public int user_id = 0;

        /// <summary>
        /// 登陆服loginsession
        /// </summary>
        public string loginsession = string.Empty;
    }
}
