
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace monocat
{
	/// <summary>
	/// TCP Client to Server peer
	/// </summary>
    public class TCPPeer
    {
		private bool m_isServer = false;
		/// <summary>
		/// 是否是服务器
		/// </summary>
		public bool isServer { get { return m_isServer; } }

		/// <summary>
		/// 服务器socket
		/// </summary>
		//private Socket m_socket;
		//public Socket socket { get { return m_socket; }}

		private MySocket m_mySocket;
		public MySocket mySocket { get { return m_mySocket; } }

		#region callback(通常仅用于客户端)
		public void ClearCallback() { if (m_mySocket != null) {
				m_mySocket.ClearCallback ();
			}}
		public bool HasCallback(){
			if (m_mySocket == null || m_mySocket.callback == null)
				return false;
			else
				return true;
		}
		#endregion

		private string m_address = string.Empty;
		public string address { get { return m_address; } }
		private int m_port = 0;
		public int port { get { return m_port; } }

		#region timer (仅用于客户端)
		/// <summary>
		/// 客户端连接计时器
		/// </summary>
		private System.DateTime m_connectTimer;
		public System.DateTime connectTimer { get { return m_connectTimer; } }
		/// <summary>
		/// 连接倒计时，时间为0时连接超时
		/// </summary>
		public double connectTimeRemain { get { return m_connectTimer.Subtract(System.DateTime.Now).TotalSeconds; } }
		/// <summary>
		/// 连接超时时间
		/// </summary>
		public double connectTimeout = 30; 

		/// <summary>
		/// 是否连接中
		/// </summary>
		private bool m_connecting = false;
		public bool connecting { get { return m_connecting; } }
		#endregion

		/// <summary>
		/// 是否处于连接状态
		/// </summary>
		public bool IsConnected { get { 
				if (m_mySocket == null)
					return false;
				return m_mySocket.socket == null ? false : m_mySocket.socket.Connected; } 
		}

        // 网络管理器
		private EventLoop m_networkMgr;

		// 可以使用一个独立线程
		private System.Threading.Thread myThread = null;

		public TCPPeer ( EventLoop netMgr )
        {
			m_networkMgr = netMgr;
			m_connectTimer = System.DateTime.Now;
			m_connecting = false;
        }

		/// <summary>
		/// 停止线程
		/// </summary>
		public void StopThread()
		{
			if (myThread != null && myThread.IsAlive) {
				myThread.Join ();
				myThread.Abort ();
			}
		}

		#region 作为服务器监听

        // 作为服务器，开始监听
		public void Listen( string ip, int port, int backlog )
        {
			m_isServer = true;

            // ip地址
			IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), port);

			m_mySocket = new MySocket ();
            // 创建socket
			m_mySocket.socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                // 将socket绑定到地址上
				m_mySocket.socket.Bind(ipe);
                // 开始监听
				m_mySocket.socket.Listen(backlog);
                // 开始异步接受连接
				m_mySocket.socket.BeginAccept(new System.AsyncCallback(ListenCallback), m_mySocket);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("TCPPeer.Listen exception:" + ex.Message);
            }
        }

        // 异步接受一个新的连接
        void ListenCallback(System.IAsyncResult ar)
        {
            // 取得服务器socket
			MySocket listener = (MySocket)ar.AsyncState;
            try
            {
                // 获得客户端的socket
				MySocket myClient = new MySocket();
				myClient.socket = listener.socket.EndAccept(ar);

                // 通知服务器接受一个新的连接
				AddInternalPacket("OnAccepted", myClient);

                // 创建接收数据的数据包
                NetPacket packet = new NetPacket();
				packet.mySocket = myClient;
                // 开始接收从来自客户端的数据
				myClient.socket.BeginReceive(packet.bytes, 0, NetPacket.headerLength, SocketFlags.None, new System.AsyncCallback(ReceiveHeader), packet);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

            // 继续接受其它连接
			listener.socket.BeginAccept(new System.AsyncCallback(ListenCallback), listener);
        }

		#endregion

		#region 作为客户端连接服务器
		// 使用一个独立线程连接服务器
		public void ConnectWithThread( string ip, int port )
		{
			m_address = ip;
			m_port = port;

			myThread = new System.Threading.Thread(new System.Threading.ThreadStart(ConnectServer));
			myThread.Start();
		}

		private void ConnectServer()
		{
			Connect (m_address, m_port);
		}

        // 作为客户端，开始连接服务器
        public void Connect( string ip, int port )
        {
			m_isServer = false;
			m_connectTimer = System.DateTime.Now.AddSeconds( connectTimeout );
			m_connecting = true;
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), port);
            try
            {
				m_mySocket = new MySocket ();
				m_mySocket.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // 开始连接
				m_mySocket.socket.BeginConnect(ipe, new System.AsyncCallback(ConnectionCallback), m_mySocket);


            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
				AddInternalPacket("OnConnectFailed", m_mySocket);
				m_connecting = false;
            }
        }

        // 异步连接回调
        void ConnectionCallback(System.IAsyncResult ar)
        {
			m_connecting = false;
			MySocket client = (MySocket)ar.AsyncState;
            try
            {
                // 与服务器取得连接
				client.socket.EndConnect(ar);

                // 通知已经成功连接到服务器
				AddInternalPacket("OnConnected", client);

                // 开始异步接收服务器信息
                NetPacket packet = new NetPacket();
				packet.mySocket = client;
				client.socket.BeginReceive(packet.bytes, 0, NetPacket.headerLength, SocketFlags.None, new System.AsyncCallback(ReceiveHeader), packet);

            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
				AddInternalPacket("OnConnectFailed", client);

            }
        }

		#endregion

        // 接收消息头
        void ReceiveHeader(System.IAsyncResult ar)
        {
			NetPacket packet = (NetPacket)ar.AsyncState;
			if ( packet.mySocket==null || packet.mySocket.socket == null || !packet.mySocket.socket.Connected )
			{
				// 通知丢失连接
				AddInternalPacket("OnLost", packet.mySocket);
				return;
			}

            try
            {
				packet.mySocket.liveTimer.Reset();

                // 返回网络上接收的数据长度
				int read = packet.mySocket.socket.EndReceive(ar);
                // 已断开连接
                if (read < 1)
                {
                    // 通知丢失连接
					AddInternalPacket("OnLost", packet.mySocket);
                    return;
                }

                packet.readLength += read;
                // 消息头必须读满4个字节
                if (packet.readLength < NetPacket.headerLength)
                {
					System.Console.WriteLine("read header:" + packet.readLength  );
					packet.mySocket.socket.BeginReceive(packet.bytes,
                        packet.readLength,                          // 存储偏移已读入的长度
                        NetPacket.headerLength - packet.readLength, // 只读入剩余的数据
                        SocketFlags.None,
                        new System.AsyncCallback(ReceiveHeader),
                        packet);
                }
                else
                {
                    // 获得消息体长度
                    packet.DecodeHeader();
					//Console.WriteLine("decodeheader:" + packet.bodyLenght );
                    packet.readLength = 0;
                    // 开始读取消息体
					packet.mySocket.socket.BeginReceive(packet.bytes,
                        NetPacket.headerLength,
                        packet.bodyLenght,
                        SocketFlags.None,
                        new System.AsyncCallback(ReceiveBody),
                        packet);
                }

            }
            catch (System.Exception ex)
            {
				ShutDown (packet.mySocket);
				AddInternalPacket("OnLost", packet.mySocket);
				System.Console.WriteLine("ReceiveHeader:" + ex.Message + "\n" + ex.StackTrace);
            }
        }

        // 接收体消息
        void ReceiveBody(System.IAsyncResult ar)
        {
            NetPacket packet = (NetPacket)ar.AsyncState;
			if ( packet.mySocket.socket == null || !packet.mySocket.socket.Connected )
			{
				// 通知丢失连接
				AddInternalPacket("OnLost", packet.mySocket);
				return;
			}

            try
            {

                // 返回网络上接收的数据长度
				int read = packet.mySocket.socket.EndReceive(ar);
                // 已断开连接
                if (read < 1)
                {
                    // 通知丢失连接
					AddInternalPacket("OnLost", packet.mySocket);
                    return;
                }
                packet.readLength += read;

                // 消息体必须读满指定的长度
                if ( packet.readLength < packet.bodyLenght )
                {
					packet.mySocket.socket.BeginReceive(packet.bytes,
                        NetPacket.headerLength + packet.readLength,
                        packet.bodyLenght - packet.readLength,
                        SocketFlags.None,
                        new System.AsyncCallback(ReceiveBody),
                        packet);
                }
                else
                {
                    // 将消息传入到逻辑处理队列
					NetPacket newpacket = new NetPacket(packet);
					m_networkMgr.AddPacket(newpacket);

                    // 下一个读取
					packet.Reset();
					packet.mySocket.socket.BeginReceive(packet.bytes,
                        0,
                        NetPacket.headerLength,
                        SocketFlags.None,
                        new System.AsyncCallback(ReceiveHeader),
                        packet);
                }
            }
            catch (System.Exception ex)
            {
				ShutDown (packet.mySocket);
				AddInternalPacket("OnLost", packet.mySocket);
				System.Console.WriteLine("ReceiveBody:" + ex.Message + "\n" + ex.StackTrace);
            }
        }

		/// <summary>
		/// 关闭网络连接
		/// </summary>
		public void ShutDown()
		{
			ShutDown (m_mySocket);
		}

		/// <summary>
		/// 向远程发送消息
		/// </summary>
		public void Send( NetPacket packet )
		{
			m_mySocket.SetCallback( packet.msgid, packet.callback );
			Send( m_mySocket, packet );
		}
			
		/// <summary>
		/// 向远程发送消息
		/// </summary>
		public static void Send(MySocket mysk, NetPacket packet)
        {
			if (mysk == null || mysk.socket==null || !mysk.socket.Connected)
                return;
			mysk.SetCallback( packet.msgid, packet.callback );
			mysk.liveTimer.Reset ();

            NetworkStream ns;
			lock (mysk.socket)
            {
                try
                {
					ns = new NetworkStream(mysk.socket);
                    if (ns.CanWrite)
                    {
                        try
                        {
                            ns.BeginWrite(packet.bytes, 0, packet.Length, new System.AsyncCallback(SendCallback), ns);
                        }
                        catch (System.Exception ex)
                        {
							System.Console.WriteLine("TCPPeer:Send:Exception:" + ex.Message + "\n" + ex.StackTrace);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine("TCPPeer:Send:Exception:" + ex.Message + "\n" + ex.StackTrace);
                }
            }
        }
			
		/// <summary>
		/// 发送消息回调
		/// </summary>
        private static void SendCallback(System.IAsyncResult ar)
        {
            NetworkStream ns = (NetworkStream)ar.AsyncState;
            try
            {
                ns.EndWrite(ar);
                ns.Flush();
                ns.Close();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

		/// <summary>
		/// 关闭网络连接
		/// </summary>
		public static void ShutDown(MySocket mysk)
        {
			if (mysk == null || mysk.socket == null)
                return;
			if (mysk.socket.Connected)
            {
				mysk.socket.Shutdown(SocketShutdown.Both);
                
			}
			mysk.socket.Close();
			//mysk.socket = null;
        }


        // 添加内部消息
		private void AddInternalPacket( string msg, MySocket mysk )
        {
            NetPacket p = new NetPacket();
			p.mySocket = mysk;
            p.BeginWrite(msg);
            p.EncodeHeader();
			m_networkMgr.AddPacket(p);
        }
    }
}
