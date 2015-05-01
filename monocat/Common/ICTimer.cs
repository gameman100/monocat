
namespace monocat
{
	/// <summary>
	/// 正计时计时器
	/// </summary>
	public class ICTimer
	{
		/// <summary>
		/// 记录时间
		/// </summary>
        private System.DateTime m_set_time;

        /// <summary>
        /// 过去的时间
        /// </summary>
		public int Seconds 
        {
			get
            {
                int temp = (int)(System.DateTime.UtcNow - m_set_time).TotalSeconds;
                return System.Math.Max(temp, 0);
			}
		}
		
        /// <summary>
        /// Constructor
        /// </summary>
		public ICTimer ()
		{
            m_set_time = System.DateTime.UtcNow;
		}

        /// <summary>
        /// 重置时间
        /// </summary>
		public void Reset ()
		{
            m_set_time = System.DateTime.UtcNow;
		}

        /// <summary>
        /// 覆盖默认的ToString方法，以小时为单位返回时间文字
        /// </summary>
		public override string ToString ()
		{
			return ToHourString (Seconds);
		}

        /// <summary>
        /// 以分钟为单位返回时间文字
        /// </summary>
		public static string ToMinuteString( int seconds )
		{
			int min = seconds / 60;
			int sec = seconds % 60;

			return string.Format ("{0:D2}:{1:D2}", min, sec);
		}

        /// <summary>
        /// 以小时为单位返回时间文字
        /// </summary>
		public static string ToHourString( int seconds )
		{
			int hour = seconds / 3600;
			int min = (seconds % 3600) / 60;
			int sec = seconds % 60;

			return string.Format ("{0:D2}:{1:D2}:{2:D2}", hour, min, sec);
		}

	}
}

