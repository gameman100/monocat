using System;
using System.Collections.Generic;
using System.Text;
using monocat;

namespace client_emulation
{
    /// <summary>
    /// 注册
    /// </summary>
    class SignupHandler : ClientHandler
    {
        private int m_signupNumber = 1;

        public override void Run()
        {
            SignupRequest request = new SignupRequest();

            Console.Write("input name:");
            request.username = Console.ReadLine();
            Console.Write("input password:");
            request.password = Console.ReadLine();

            Console.WriteLine("signup({0})...", m_signupNumber);

            byte[] bs = JsonHelper.SerializeToBytes(request);

            for (int i = 0; i < m_signupNumber; i++) // 发送N个登陆请求
            {
                User user = new User();
                WWWManager.Create<SignupResponse>("http://localhost:10001/signup/", request, user, true, (SignupResponse r) =>
                {
                    if (r != null)
                    {
                        if (r.status == SignupResponse.Status.OK)
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
