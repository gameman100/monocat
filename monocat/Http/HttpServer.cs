using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace monocat
{
	/// <summary>
	/// Http server.
	/// </summary>
	public class HttpServer
    {
		/// <summary>
		/// server listener
		/// </summary>
        protected HttpListener m_listener;

        string baseURL = "http://localhost:8000/";
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string BaseURL { get { return baseURL; } }
        
        protected int port = 0;
		/// <summary>
		/// 服务器端口
		/// </summary>
		public int Port { get { return port; } }

        /// <summary>
		/// 线程
        /// </summary>
		System.Threading.Thread contextThread;

		/// <summary>
		/// 线程休息时间
		/// </summary>
		int sleepTime = 30;

        /// <summary>
        /// Http handlers
        /// </summary>
        Dictionary< string, HttpHandler> handlers;

		/// <summary>
		/// HttpListener contexts
		/// </summary>
		Queue<HttpPacket> contextQueue;
		
		/// <summary>
		/// ctor example: http://localhost:8000/
		/// </summary>
		public HttpServer( string addr, int serverport, int sleeptm = 30 )
        {
            m_listener = new HttpListener();

            baseURL = addr + ":" + serverport.ToString() + "/";
			port = serverport;
			sleepTime = sleeptm;
            
            handlers = new Dictionary<string, HttpHandler>();
			contextQueue = new Queue<HttpPacket> ();
        }

		/// <summary>
		/// ctor example: http://localhost:8000/
		/// </summary>
		public HttpServer( string addr_with_port, int sleeptm = 30 )
		{
            m_listener = new HttpListener();
			baseURL = addr_with_port;
			sleepTime = sleeptm;

			try 
            {
                // 获取服务器端口
                int startindex = addr_with_port.LastIndexOf(":");
                int endindex = addr_with_port.LastIndexOf("/");

				string portstr = addr_with_port.Substring (startindex + 1, endindex - startindex - 1);
				port = int.Parse (portstr);
                handlers = new Dictionary<string, HttpHandler>();
                contextQueue = new Queue<HttpPacket>();
			}
			catch( Exception e ) 
            {
				Console.WriteLine ("Excpetion in HttpServer.HttpServer(), " + e.Message);
			}

		
		}

        /// <summary>
		/// 添加Http响应Handler,这个操作必须在StartServer之前完成
        /// </summary>
        public void CreateContext( string addr, HttpHandler handler )
        {
            try
            {
                m_listener.Prefixes.Add(baseURL + addr);
                handlers.Add(baseURL + addr, handler);
            }
            catch (Exception e)
            {
                Console.WriteLine( e.Message + " " + e.StackTrace);
            }
        }

        /// <summary>
        /// 启动服务器，并开始在一个独立线程中更新逻辑
        /// </summary>
        protected void RunServer()
        {
            try
            {
                m_listener.Start();

                // 在一个独立线程中处理逻辑事件
                contextThread = new System.Threading.Thread(new System.Threading.ThreadStart(Update));
                contextThread.Start();

                Console.WriteLine("Start Http Server");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
            }
        }

        /// <summary>
        /// 关闭服务器
        /// </summary>
        public void StopServer()
        {
            try
            {
                contextThread.Abort();
                m_listener.Stop();
                contextQueue.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
            }
          
        }

        /// <summary>
        /// Enqueues the context.
        /// </summary>
        void EnqueueContext(HttpPacket context)
        {
            lock (contextQueue)
            {
                contextQueue.Enqueue(context);
            }
        }

        /// <summary>
        /// Dequeues the context.
        /// </summary>
        HttpPacket DequeueContext()
        {
            lock (contextQueue)
            {
                return contextQueue.Dequeue();
            }
        }

        /// <summary>
        /// 在一个独立线程中处理逻辑事件
        /// </summary>
        protected void Update()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(sleepTime);

                while (contextQueue.Count > 0) // 收到来自client的request
                {
                    HttpPacket packet = DequeueContext();
                    HttpListenerContext context = packet.context;
                    string url = context.Request.Url.AbsoluteUri;
                    if (handlers.ContainsKey(url))
                    {
                        HttpHandler handler;
                        handlers.TryGetValue(url, out handler);
                        if (handler != null)
                        {
                            handler.HandleRequest(packet);
                        }
                    }
                }
            }
        }


        #region 同步服务器

        /// <summary>
        /// Start Http Listen
        /// </summary>
        public void StartServer()
        {
            RunServer();

            // 主线程同步IO
            while (true)
            {
                try
                {
                    HttpListenerContext context = m_listener.GetContext(); // IO block，等待连接
                    HttpPacket packet = new HttpPacket();
                    packet.context = context;
                    packet.readLength = 0;
                    if (context.Request.HasEntityBody)
                    {
                        packet.bytes = new byte[context.Request.ContentLength64];
                        ReadRequest(packet);
                    }
                    else
                    {
                        EnqueueContext(packet);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("HttpServer.AsyncListenCallback:" + e.Message);
                }
            }
        }

        #endregion


        #region 异步服务器

        /// <summary>
        /// 开启异步服务器
        /// </summary>
        public void StartServerAsync()
        {
            RunServer();

            try
            {
                // 主线程异步IO
                m_listener.BeginGetContext(new AsyncCallback(AsyncListenCallback), m_listener);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "/" + e.StackTrace);
            }
        }

        /// <summary>
        /// Async Listen callback
        /// </summary>
        private void AsyncListenCallback(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            try
            {
                HttpListenerContext context = listener.EndGetContext(result);

                HttpPacket packet = new HttpPacket();
                packet.context = context;
                packet.readLength = 0;
                if (context.Request.HasEntityBody)
                {
                    packet.bytes = new byte[context.Request.ContentLength64];
                    ReadRequest(packet);
                }
                else
                {
                    EnqueueContext(packet);
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine("HttpServer.AsyncListenCallback:" + e.Message);
            }
            // start another listen
            listener.BeginGetContext(new AsyncCallback(AsyncListenCallback), listener);
        }


        /// <summary>
        /// offset default
        /// </summary>
        /// <param name="bs"></param>
        /// <param name="offset">default set it to 0</param>
        /// <param name="context"></param>
        private void ReadRequest(HttpPacket packet)
        {
            try
            {
                HttpListenerRequest request = packet.context.Request;
                packet.context.Request.InputStream.BeginRead(packet.bytes, packet.readLength, (int)request.ContentLength64 - packet.readLength, new AsyncCallback(HandleReadRequest), packet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void HandleReadRequest(IAsyncResult result)
        {
            try
            {
                HttpPacket packet = (HttpPacket)result.AsyncState;
                HttpListenerRequest request = packet.context.Request;

                packet.readLength += packet.context.Request.InputStream.EndRead(result);

                if (packet.readLength < request.ContentLength64)
                {
                    ReadRequest(packet);
                }
                else
                {
                    packet.context.Request.InputStream.Close();

                    // packet.text = UTF8Encoding.UTF8.GetString(packet.bytes);

                    EnqueueContext(packet);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #endregion

	
    }
}
