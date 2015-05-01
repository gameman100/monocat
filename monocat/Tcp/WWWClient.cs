using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.IO;

namespace monocat
{
	/* sample code

	NameValueCollection form = new NameValueCollection ();
	form.Add ("test1", "hello");
	form.Add ("test2", "world");

	WWWClient client = new WWWClient ();
	client.SendPostAsync( "http://192.168.0.210:800/Card2/testpost.php", form ,( byte[] bs )=>
		{
			if ( bs!=null )
			{
				string response = System.Text.UTF8Encoding.UTF8.GetString(bs);
				Console.WriteLine("response:\n" + response );
			}
			else
				Console.WriteLine("empty reply" );
		});
		
	*/
	/// <summary>
	/// Http client
	/// </summary>
	public class WWWClient
    {
		private WebClient m_web;
		private System.Action<byte[]> m_downloadAction;
		private System.Action<byte[]> m_uploadAction;
		private System.Action<byte[]> m_formAction;

        /// <summary>
        /// 错误消息[如果无错误则为空串]
        /// </summary>
        private string m_errorMsg = string.Empty;
        public string error { get { return m_errorMsg; } set { m_errorMsg = value; } }

        /// <summary>
        /// 是否完成请求
        /// </summary>
        private bool m_isDone = false;
        public bool isDone { get { return m_isDone; } }

        /// <summary>
        /// 通讯是否成功
        /// </summary>
        public bool isOk { get { return string.IsNullOrEmpty(m_errorMsg) ? true : false; } }
        
		public WWWClient()
        {
            m_isDone = false;
            m_errorMsg = string.Empty;

            m_web = new WebClient();
			m_web.Headers.Add("Content-Type","application/x-www-form-urlencoded");
			m_web.DownloadDataCompleted += new DownloadDataCompletedEventHandler(OnDownloadedFromServer);
			m_web.UploadDataCompleted += new UploadDataCompletedEventHandler (OnReplyFromServer);
			m_web.UploadValuesCompleted += new UploadValuesCompletedEventHandler (OnUploadValueCompleted);
        }

		/// <summary>
		/// 下载二进制数据
		/// </summary>
		public void DownloadDataAsync(string uri, System.Action<byte[]> downloadaction )
        {
            try
            {
                m_isDone = false;
                m_errorMsg = string.Empty;

				m_downloadAction = downloadaction;
                m_web.DownloadDataAsync(new System.Uri(uri));
            }
            catch (System.Exception e)
            {
				//Console.WriteLine("Exception:" + e.Message + "\n" + e.StackTrace);
                m_errorMsg = string.Format("WWWClient.DownloadDataAsync: {0}", e.Message);
            }
        }


		private void OnDownloadedFromServer(System.Object sender, DownloadDataCompletedEventArgs e)
        {
            m_isDone = true;
            try
            {
                byte[] bs = (byte[])e.Result;

                if (bs == null || bs.Length == 0)
                {
                    m_errorMsg = "WWWClient.OnDownloadedFromServer: empty reply";
                }
                else
				    m_downloadAction(bs);
            }
            catch (System.Exception ex)
            {
                m_errorMsg = string.Format("WWWClient.OnDownloadedFromServer: {0} ({1})", ex.Message, e.Error.Message);
				m_downloadAction (null);
				//Console.WriteLine("Exception:" + ex.Message + "\n" + ex.StackTrace);
            }

        }

        /// <summary>
		/// 上传二进制数据
		/// </summary>
		public void SendDataAsync(string uri, byte[] data, System.Action<byte[]> uploadAction)
		{
			try
			{
                m_isDone = false;
                m_errorMsg = string.Empty;

				m_uploadAction = uploadAction;
                m_web.UploadDataAsync(new System.Uri(uri), WebRequestMethods.Http.Post, data);
			}
			catch (System.Net.WebException e)
			{
                m_errorMsg = string.Format("WWWClient.SendDataAsync: {0}", e.Message);
				//Console.WriteLine("Exception:" + e.Message + "\n" + e.StackTrace);
			}

		}

        private void OnReplyFromServer(System.Object sender, UploadDataCompletedEventArgs e)
		{
            m_isDone = true;
			try
            {
                if (e.Result == null || e.Result.Length == 0)
                {
                    m_errorMsg = "WWWClient.OnDownloadedFromServer: empty reply";
                }
                else
                {
                    m_uploadAction(e.Result);
                }
			}
            catch (System.Exception ex)
			{
                m_errorMsg = string.Format("WWWClient.OnReplyFromServer: {0} ({1})", ex.Message, e.Error.Message);
				m_uploadAction (null);
				//Console.WriteLine("Exception:" + ex.Message + "\n" + ex.StackTrace);
			}

		}

        /// <summary>
		/// Post Form
		/// </summary>
		public void SendPostAsync(string uri, NameValueCollection myNameValueCollection, System.Action<byte[]> formAction)
		{
			try
			{
                m_isDone = false;
                m_errorMsg = string.Empty;

				m_formAction = formAction;
                m_web.UploadValuesAsync(new System.Uri(uri), WebRequestMethods.Http.Post, myNameValueCollection);
			}
            catch (System.Exception e)
			{
                m_errorMsg = string.Format("WWWClient.SendPostAsync: {0}", e.Message);
				//Console.WriteLine("Exception:" + e.Message + "\n" + e.StackTrace);
			}
		}


        private void OnUploadValueCompleted(System.Object sender, UploadValuesCompletedEventArgs e)
		{
            m_isDone = true;
			try
			{
               
				byte[] bs = (byte[])e.Result;
                if (bs == null || bs.Length == 0)
                {
                    m_errorMsg = "WWWClient.OnUploadValueCompleted: empty reply";
                }
                else
				    m_formAction(bs);
			}
            catch (System.Exception ex)
			{
                m_errorMsg = string.Format("WWWClient.OnUploadValueCompleted: {0} ({1})", ex.Message, e.Error.Message);
				m_formAction(null);
				//Console.WriteLine("Exception:" + ex.Message + "\n" + ex.StackTrace);
			}
		}
    }
}
