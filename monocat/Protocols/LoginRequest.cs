using System.Collections.Generic;
using System.Text;

namespace monocat
{
    public class LoginRequest : BaseRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string username = string.Empty;
        /// <summary>
        /// 密码
        /// </summary>
        public string password = string.Empty;
    }
}
