using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace monocat
{
	/// <summary>
	/// 网络管理器
	/// </summary>
    public class EventLoop
    {
		// 逻辑处理可以使用一个独立线程，与网络的线程分开
		private System.Threading.Thread myThread;

		/// <summary>
		/// 更新消息队列的时间间隔
		/// </summary>
		public int update_sleep = 30;

        /// <summary>
        /// 代理回调函数
        /// </summary>
        public delegate void OnReceive( NetPacket packet );

        // 每个消息对应一个OnReceive函数
		private Dictionary<string, OnReceive> m_callbacks;

        // 每个消息对应一个handler对象
		private Dictionary<string, NetworkHandler> m_handlers;

		// 存储消息的队列
        private Queue Packets = new System.Collections.Queue();
		/// <summary>
		/// frame count
		/// </summary>
		private int m_frameCount = 0;
		/// <summary>
		/// 多少帧清理一次垃圾, 0 表示不清理
		/// </summary>
		protected int m_GCFrameCount = 30;

		public EventLoop()
        {
            m_callbacks = new Dictionary<string, OnReceive>();
            m_handlers = new Dictionary<string, NetworkHandler>();

            // 注册连接成功，丢失连接消息
            AddCallback("OnAccepted", OnAccepted);
            AddCallback("OnConnected", OnConnected);
            AddCallback("OnConnectFailed", OnConnectFailed);
            AddCallback("OnLost", OnLost);
        }

        // 注册消息
        public void AddCallback(string msgid, OnReceive handler)
        {
            if (m_callbacks.ContainsKey(msgid))
                return;
            m_callbacks.Add(msgid, handler);
        }

        public void AddHandler(string msgid, NetworkHandler handler)
        {
            if (m_handlers.ContainsKey(msgid))
                return;
            m_handlers.Add(msgid, handler);
        }

		protected virtual void UpdateMain()
		{
		}

        // 数据包入队
        public void AddPacket( NetPacket packet )
        {
            lock (Packets)
            {
                Packets.Enqueue(packet);
            }
        }

        // 数据包出队
        public NetPacket GetPacket()
        {
            lock (Packets)
            {
                if (Packets.Count == 0)
                    return null;
                return (NetPacket)Packets.Dequeue();
            }
        }

        // 开始执行另一个线程处理逻辑
        public void StartThreadUpdate()
        {
            // 为逻辑部分建立新的线程
            myThread = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadUpdate));
            myThread.Start();
        }

		/// <summary>
		/// 在另一个线程执行Update，更新消息队列
		/// </summary>
        protected void ThreadUpdate()
        {
              while (true)
              {
                  // 为了节约cpu, 每次循环暂停30帧
				  System.Threading.Thread.Sleep(update_sleep);

				  // 更新消息队列
				  this.UpdatePacket();

                  // 更新
                  this.UpdateMain();

              }
        }

		/// <summary>
		/// 更新队列消息（在服务端，通常在另一个线程中执行，在Unity中直接在主线程中执行）
		/// </summary>
		public void UpdatePacket()
        {
            NetPacket packet = null;
            for (packet = GetPacket(); packet != null; )
            {
                string msg = "";
                // 获得消息标识符
                packet.BeginRead(out msg);

                OnReceive callback = null;
				NetworkHandler handler = null;

                if (m_callbacks.TryGetValue(msg, out callback))
                {
                    if (callback != null)
                    {
                        callback(packet);
                    }
                }
				else if (m_handlers.TryGetValue(msg, out handler))
                {
                    if (handler != null)
                    {
                        handler.OnNetworkEvent(packet);
                    }
                }

				// 客户端回调
				if (packet.mySocket != null && packet.mySocket.callback != null
					&& packet.mySocket.msgid.CompareTo(msg)==0) {

					packet.mySocket.callback (packet);
					packet.mySocket.ClearCallback ();
				}

				packet = GetPacket ();
            }

			//this.GC();
        }

        /// <summary>
        /// 强制垃圾回收
        /// </summary>
		private void GC()
		{
			if (m_GCFrameCount == 0)
				return;

			m_frameCount++;
			if (m_frameCount % m_GCFrameCount == 0)
			{
				System.GC.Collect();
				m_frameCount = 0;
			}
		}

        // 处理服务器接受客户端的连接
        public virtual void OnAccepted(NetPacket packet)
        {

        }

        // 处理客户端取得与服务器的连接
        public virtual void OnConnected(NetPacket packet)
        {

        }

        /// <summary>
        /// 处理客户端取得与服务器连接失败
        /// </summary>
        public virtual void OnConnectFailed(NetPacket packet)
        {

        }

        /// <summary>
        /// 连接丢失
        /// </summary>
        public virtual void OnLost(NetPacket packet)
        {

        }
    }
}
