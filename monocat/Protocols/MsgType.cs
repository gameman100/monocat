using System.Collections.Generic;
using System.Text;

namespace monocat
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public class MsgType
    {
        /// <summary>
        /// 当前协议
        /// </summary>
        public const string current = "current";
        /// <summary>
        /// 出现错误
        /// </summary>
        public const string exception = "exception";
        /// <summary>
        /// 重新登陆
        /// </summary>
        public const string relogin = "relogin";
        /// <summary>
        /// 更新数据
        /// </summary>
        public const string update = "update";

    }
}
