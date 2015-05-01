using System.Text;

namespace monocat
{
	/// <summary>
	/// 功能类似 System.Collections.Specialized.NameValueCollection
	/// </summary>
	public class WWWPost
	{
		StringBuilder m_postdata = null;

		public WWWPost ()
		{
		}

		public void AddData( string key, string data )
		{
			if (m_postdata == null) {
				m_postdata = new StringBuilder ();
				m_postdata.Append (key + "=" + data);
			}
			else
				m_postdata.Append ("&" + key + "=" + data);
		}

		public byte[] ToUTF8Bytes()
		{
			string str = m_postdata.ToString ();
			byte[] bs = System.Text.UTF8Encoding.UTF8.GetBytes (str);
			return bs;
		}
	}
}

