using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using monocat;

namespace GameServer
{
    /// <summary>
    /// 响应用户登陆
    /// </summary>
    class HandlerLogin : HttpHandler
    {
        public override void Run(HttpPacket packet)
        {
            // decode packet byte and text
            packet.Decode();

            // 收到request
            LoginRequest request = JsonHelper.Deserialize<LoginRequest>(packet.text);
            if (request == null)
            {
                ResponseUtil util = new ResponseUtil();
                util.AddContent(MsgType.exception, "HandlerLogin.Run:decode error");

                Response(util, packet.context);
            }
            else
            {
                //验证hash

                ResponseUtil util = new ResponseUtil();
                
                //[sync]到数据库查询结果
                int id = LoginServer.Get.database.VerifyUserInDatabase(request.username, request.password,
                    packet.context.Request.UserHostAddress);
                
                try
                {
                   
                    LoginResponse response = new LoginResponse();

                    if (id > 0) // 找到该用户
                    {
                        // 这一步生成用户和session
                        User user = new User();
                        user.id = id;
                        user.username = request.username;
                        user.password = request.password;
                        //user.logintimer.Seconds = LoginServer.session_timelife;// N秒后session失效

                        response.status = LoginResponse.Status.OK;
                        response.user_id = id;
                        response.loginsession = LoginServer.Get.database.CreateSession(user);

                        ///添加更新内容
                        //util.BuildValueUpdate(user.get_id, user.id.ToString());
                        //util.BuildValueUpdate(user.get_username, user.username);
                        //string update = string.Empty;
                        //util.GetUpdate(ref update);
                        //util.response.Add(MsgType.update, update);
                    }
                    else // 没有
                    {
                        response.status = LoginResponse.Status.Fail;
                        response.user_id = 0;
                        response.loginsession = string.Empty;
                    }

                    util.AddContent(MsgType.current, JsonHelper.Serialize(response));                    
                }
                catch (System.Exception e)
                {
                    util.AddContent(MsgType.exception, e.Message);
                }

                // 响应客户端
                Response(util, packet.context);
            }

        }//end Run()

    }// end class
}
