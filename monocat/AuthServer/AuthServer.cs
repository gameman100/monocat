using System;
using System.Collections.Generic;
using System.Text;
using monocat;

namespace AuthServer
{
    class AuthServer
    {
        // login, signup, delivery server info
        static void Create(string[] args)
        {
            if (args == null)
            {

            }
            else
            {

            }

            // start login server
            LoginServer server = new LoginServer("http://localhost", 10001, 30);
            server.StartServer();
        }
    }
}
