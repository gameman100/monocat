using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using monocat;

namespace client_emulation
{
    class ClientEmulation
    {
        static void Create(string[] args)
        {
            ClientManager client = new ClientManager();
            client.StartThreadUpdate();

        }
    }
}
