using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace monocat
{
    public class ServerInfo
    {
        /// <summary>
        /// 服务器ID
        /// </summary>
        public int id = 0;
        /// <summary>
        /// 服务器名称
        /// </summary>
        public string name = string.Empty;
        /// <summary>
        /// 服务器状态
        /// </summary>
        public string status = string.Empty;
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string addr = "localhost";
        /// <summary>
        /// 服务器端口
        /// </summary>
        public int port = 0;
    }
}
