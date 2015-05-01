using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using MySql.Data.MySqlClient;
using monocat;

namespace AuthServer
{
    class HandlerServerInfo : HttpHandler
    {
        public override void Run(HttpPacket packet)
        {
            ResponseUtil util = new ResponseUtil();

            // 收到request
            ServerInfoRequest request = JsonHelper.Deserialize<ServerInfoRequest>(packet.text);
            if (request == null)
            {
               
                util.AddContent(MsgType.exception, "HandlerServerInfo.Run:decode error");
                Response(util, packet.context);
            }
            else
            {
                // 获得服务器列表
                // util.AddContent( MsgType.current, "" );
                
                // 响应客户端
                Response(util, packet.context);
            }

        }//end Run()
    }
}
