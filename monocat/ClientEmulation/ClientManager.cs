using System;
using System.Collections.Generic;
using System.Text;
using monocat;

namespace client_emulation
{
    class ClientManager : EventLoop
    {
        WWWObserver m_wwwmager;

        private Dictionary<string, ClientHandler> m_callback = new Dictionary<string, ClientHandler>();

        public ClientManager()
        {
            m_wwwmager = new WWWObserver();
            m_callback.Add( "login", new HandlerLogin() );
            m_callback.Add( "signup", new HandlerSignup());
        }

        protected override void UpdateMain()
        {
            m_wwwmager.DoUpdate();
       
            Input();
        }

        protected void Input()
        {
            if (m_wwwmager.request_cout > 0)
                return;

            Console.Write(">>");
            string input = Console.ReadLine();

            if (m_callback.ContainsKey(input))
            {
                ClientHandler h;
                m_callback.TryGetValue(input, out h);
                h.Run();
            }
            else
            {
                Console.WriteLine("wrong input");
            }
        }


    }
}
