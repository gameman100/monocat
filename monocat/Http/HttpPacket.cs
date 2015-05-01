using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
//using System.Collections.Specialized.NameValueCollection;

namespace monocat
{
    /// <summary>
    /// HTTP包
    /// </summary>
    public class HttpPacket
    {
        /// <summary>
        /// HTTP请求上下文
        /// </summary>
        public HttpListenerContext context;
        /// <summary>
        /// 读取的字符数量
        /// </summary>
        public int readLength = 0;
        /// <summary>
        /// Http请求bytes
        /// </summary>
        public byte[] bytes = null;
        /// <summary>
        /// Http请求Text
        /// </summary>
        public string text = string.Empty;

        public HttpPacket()
        {
        }

        public void Decode()
        {
            if (bytes == null)
                return;
            // 先解密json
            this.bytes = JsonHelper.DecodeBytes(this.bytes);
            // 转为text
            this.text = UTF8Encoding.UTF8.GetString(bytes);
        }

        private void DecodeJsonBytes()
        {
            if (bytes == null)
                return;
            this.bytes = JsonHelper.DecodeBytes(this.bytes);
        }

        private void DecodeText()
        {
            if (bytes == null)
                return;

            text = UTF8Encoding.UTF8.GetString(bytes);
        }
    }
}
