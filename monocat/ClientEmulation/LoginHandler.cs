using System;
using System.Collections.Generic;
using System.Text;
using monocat;

namespace client_emulation
{
    public class LoginHandler : ClientHandler
    {
        private int m_loginNumber = 1;

        public override void Run()
        {
            LoginRequest request = new LoginRequest();

            Console.Write("login name:");
            request.username = Console.ReadLine();
            Console.Write("login password:");
            request.password = Console.ReadLine();

            Console.WriteLine("login({0})...", m_loginNumber);

            byte[] bs = JsonHelper.SerializeToBytes(request);

            for (int i = 0; i < m_loginNumber; i++) // 发送N个登陆请求
            {
                User user = new User();
                WWWManager.Create<LoginResponse>("http://localhost:10001/login/", request, user, true, (LoginResponse r) =>
                    {
                        if (r != null)
                        {
                            if (r.status == LoginResponse.Status.OK)
                            {
                                user.username = request.username;
                                user.password = request.password;
                                user.id = r.user_id;
                                user.loginsession = request.session;

                                UserManager.Get.AddNewUser(user);
                            }

                            Console.WriteLine("new user:" + LitJson.JsonMapper.ToJson(user));
                        }
                    });
            }
        }
    }
}
