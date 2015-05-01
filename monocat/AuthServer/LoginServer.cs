using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using monocat;

namespace AuthServer
{
    /// <summary>
    /// 登陆服务器
    /// </summary>
    class LoginServer : EventLoop
    {
        private static LoginServer m_instance = null;
        public static LoginServer Get { get { return m_instance; } }

        /// <summary>
        /// HTTP server
        /// </summary>
        private HttpServer m_server;
        /// <summary>
        /// 数据库
        /// </summary>
        private DatabaseManager m_database;
        public DatabaseManager database { get { return m_database; } }

        #region 配置
     
        /// <summary>
        /// session失效时间
        /// </summary>
        public const int session_timelife = 60;

        #endregion

        public LoginServer( string addr, int port, int sleeptm )
        {
            m_instance = this;

            m_server = new HttpServer(addr, port, sleeptm);
            m_database = new DatabaseManager();

            // check timeout every minute
            update_sleep = 60;
        }

        public void AddHandler(string uri, HttpHandler handler)
        {
            m_server.CreateContext(uri, handler);
        }

        /// <summary>
        /// 添加句柄
        /// </summary>
        private void AddAllHandlers()
        {
            this.AddHandler("test/", new HandlerTest());
            this.AddHandler("serverinfo/", new HandlerServerInfo());
            this.AddHandler("login/", new HandlerLogin());
            this.AddHandler("signup/", new HandlerSignup());

        }

        public void StartServer()
        {
            AddAllHandlers();

            m_database = new DatabaseManager();

            int result = -1;
            Task task = new Task(()=>{
                // 异步读入数据库数据,等待结束后启动服务器
                result = m_database.InitDatabase();
                if (result > 0) // 连接数据库成功
                {
                    // test code
                    //System.DateTime now = System.DateTime.Now;
                    //int testnumber = 1000;
                    //for (int i = 0; i < testnumber; i++)
                    //{
                    //   //m_database.SignUp("test_" + i, "123");
                    //}
                    ////Console.WriteLine("ts:" + System.DateTime.Now.Subtract(now).Seconds);
                    //for (int i = 0; i < testnumber; i++)
                    //    m_database.VerifyUserInDatabase("test_" + i, "123", "localhost");
                    //Console.WriteLine("ts:" + System.DateTime.Now.Subtract(now).Seconds);
                    //test code end

                    m_database.StartThreadUpdate(); // this thread only update database
                    this.StartThreadUpdate();       // this thread is for logic
                    m_server.StartServerAsync();    // this thread is the main thread for http
                }
                else
                {
                    Console.WriteLine("[ERROR]:Connect database failed");
                    Console.ReadLine(); // quit program
                }
            });

            task.Start();
            task.Wait();
        }

        protected override void UpdateMain()
        {
            // update user session here
        }
    }
}
