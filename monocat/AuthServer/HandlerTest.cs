using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using monocat;

namespace AuthServer
{
    /// <summary>
    /// 仅用于测试
    /// </summary>
    class HandlerTest : HttpHandler
    {
        public override void Run(HttpPacket packet)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append( "<H1>Hello, world</H1>" );
            sb.Append( "<H2>收到:").Append( packet.text).Append("</H2>");
            byte[] bs = UTF8Encoding.UTF8.GetBytes(sb.ToString());
            Response(bs, bs.Length, packet.context);
        }
    }
}
