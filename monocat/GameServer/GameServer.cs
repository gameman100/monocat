using System;
using System.Collections.Generic;
using System.Text;
using monocat;
// 如果

namespace GameServer
{
    class GameServer
    {
        // login, signup, delivery server info
        static void Create(string[] args)
        {
            // start login server
            LoginServer server = new LoginServer("http://localhost", 10001, 30);
           
            server.StartServer();
        }
    }
}
