using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using MySql.Data.MySqlClient;
using monocat;

namespace AuthServer
{
    /// <summary>
    /// 注册
    /// </summary>
    class HandlerSignup : HttpHandler
    {
        public override void Run(HttpPacket packet)
        {
            packet.Decode();

            ResponseUtil util = new ResponseUtil();

            // 收到request
            SignupRequest request = JsonHelper.Deserialize<SignupRequest>(packet.text);
            if (request == null)
            {
                util.AddContent(MsgType.exception, "HandlerSignup.Run:decode error");
                Response(util, packet.context);
            }
            else
            {
                //[sync]到数据库查询结果
                int user_id = LoginServer.Get.database.SignUp(request.username, request.password );
                try
                {
                    SignupResponse response = new SignupResponse();

                    if (user_id > 0) // 注册成功
                    {
                        User user = new User();
                        user.id = user_id;
                        user.username = request.username;
                        user.password = request.password;
                        //user.logintimer.Seconds = LoginServer.session_timelife;

                        response.status = SignupResponse.Status.OK;
                        response.user_id = user_id;
                        // 这一步生成session
                        response.loginsession = LoginServer.Get.database.CreateSession(user);

                    
                    }
                    else // 没有
                    {
                        response.status = SignupResponse.Status.Fail;
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
    }
}
