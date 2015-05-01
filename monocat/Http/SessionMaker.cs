using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace monocat
{
    /// <summary>
    /// 通过用户ID和区号生成Session
    /// </summary>
    public static class SessionMaker
    {
        static MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

        /// <summary>
        /// 输出session
        /// </summary>
        /// <param name="user_id">用户id,4个字节</param>
        /// <param name="area_id">分区id,4个字节</param>
        /// <param name="code">密钥</param>
        /// <returns>返回Session string</returns>
        public static string Create(int user_id, int area_id, string code)
        {
            System.Text.StringBuilder sb = new StringBuilder();
            sb.Append(user_id.ToString());
            sb.Append(area_id.ToString());
            sb.Append(System.DateTime.Now.ToString());
            sb.Append(code);

            return HashHelper.MD5(sb.ToString());
        }
    }
}
