
using System.Net;
using System.Net.Sockets;

namespace monocat
{
	/// <summary>
	/// Socket Wrapper
	/// </summary>
	public class MySocket
	{
        /// <summary>
        /// socket
        /// </summary>
        public Socket socket;

        /// <summary>
        /// 用户ID
        /// </summary>
        public int id = 0;
		/// <summary>
		/// 选区ID
		/// </summary>
		public int area_id = 0;
        /// <summary>
        /// 是否授权
        /// </summary>
        public bool isAuth = false;
		/// <summary>
		/// 用户名
		/// </summary>
		public string userName = string.Empty;
		/// <summary>
		/// session id
		/// </summary>
		public string session = string.Empty;
        /// <summary>
        /// 回调名
        /// </summary>
        public string msgid = string.Empty;

		/// <summary>
		/// 回调（每次网络请求的回调)
		/// </summary>
		private System.Action<NetPacket> m_callback = null;
		public System.Action<NetPacket> callback { get { return m_callback; } }

		public void SetCallback( string msgid_, System.Action<NetPacket> callback_ )
		{
            msgid = msgid_;
            m_callback = callback_;
		}

		public void ClearCallback() {
			m_callback = null;
		}

		#region 认证计时器，如果在连接后一定时间未得到认证，则断开连接
		/// <summary>
		/// 倒计时计时器
		/// </summary>
		public ICTimer m_deadTimer;
		public ICTimer deadTimer { get { return m_deadTimer; } }

		#endregion

		#region 生存计时器 如果在一定时间未出现通讯，则可以断开连接

		/// <summary>
		/// 记录最后的通讯时间
		/// </summary>
		private ICTimer m_liveTimer;
		public ICTimer liveTimer { get { return m_liveTimer; } }

		#endregion

		/// <summary>
		/// 构造函数
		/// </summary>
		public MySocket ()
		{
			m_liveTimer = new ICTimer ();
			m_deadTimer = new ICTimer ();
		}
	}
}

