using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace monocat
{
    public class WWWRequest
    {
        /// <summary>
        /// WWW
        /// </summary>
        private WWWClient m_client = null;
        public WWWClient client
        {
            get{return m_client;}
        }
        public string error { get { return m_client.error; } }

        public string URL { set; get; }

        /// <summary>
        /// 更新类
        /// </summary>
        public object updateObject;

        /// <summary>
        /// 状态
        /// </summary>
        public enum State
        {
            Waiting, // 等待网络反馈
            Error,   // 出现错误
            TimeOut, // 超时
            Done     // 完成
        }
        protected State m_state = State.Waiting;


        /// <summary>
        /// 超时时间(秒)
        /// </summary>
        public float Timeout = 10;

        /// <summary>
        /// 返回WWW状态 Read only
        /// </summary>
        public State state
        {
            get
            {
                if (m_state != State.Waiting)
                    return m_state;

                bool timeout = false;
                System.TimeSpan span = System.DateTime.Now.Subtract(m_waittime);
                if (span.Seconds > Timeout)
                {
                    timeout = true;
                }

                if (timeout)
                {
                    m_state = State.TimeOut;
                    m_client.error = "time out";
                }
                //else if (!string.IsNullOrEmpty(m_client.error))
                //{
                //    m_state = State.Error;
                //}
                //else if (m_client.isDone)
                //{
                //    m_state = State.Done;
                //}
                else
                {
                    m_state = State.Waiting;
                }
                return m_state;
            }
        }


        /// <summary>
        /// 时间戳
        /// </summary>
        private System.DateTime m_waittime = new System.DateTime();

        /// <summary>
        /// 回调
        /// </summary>
        public System.Action<string> m_callback = null;

        /// <summary>
        /// www data
        /// </summary>
        protected byte[] m_sendStream = null;
        public byte[] sendStream { get { return m_sendStream; } }

        protected byte[] m_revStream = null;

        protected NameValueCollection m_sendForm = null;
        public NameValueCollection sendForm { get { return m_sendForm; } }

        internal static WWWRequest Create<T>(string url, NameValueCollection form, System.Action<T> callback)
        {
            WWWRequest wwwrequest = new WWWRequest(url, form);

            wwwrequest.SendFormData( (string json)=>
            {
                T response = default(T);
                if (!string.IsNullOrEmpty(json))
                {
                    response = JsonHelper.Deserialize<T>(json);
                    if (response == null)
                    {
                        throw new Exception(string.Format("WWWRequest.Create<{0}> callback error", typeof(T)));
                    }
                    else
                        callback(response);
                }
               
            });

            return wwwrequest;
        }

        internal static WWWRequest Create<T>(string url, Object request, bool encodebyte, bool md5, System.Action<T> callback )
        {
            byte[] bs =JsonHelper.SerializeToBytes(request);
            if (encodebyte)
            {
                JsonHelper.EncodeBytes(ref bs);
            }

            WWWRequest wwwrequest = new WWWRequest(url, bs);

            wwwrequest.SendStreamData( (string json) =>
            {
                T response = default(T);
                if (!string.IsNullOrEmpty(json))
                {
                    response = JsonHelper.Deserialize<T>(json);
                    if (response == null)
                    {
                        throw new Exception(string.Format("WWWRequest.Create<{0}> callback error", typeof(T)));
                    }
                    else
                        callback(response);
                }
            });

            return wwwrequest;
        }

        /// <summary>
        /// WWW请求
        /// </summary>
        public WWWRequest( string url, NameValueCollection form  )
        {
            //记录时间
            m_waittime = System.DateTime.Now;
            m_client = new WWWClient();
            m_state = State.Waiting;
            URL = url;
            m_sendForm = form;

            WWWClient client = new WWWClient();
        }


        /// <summary>
        /// WWW请求
        /// </summary>
        public WWWRequest(string url, byte[] bs)
        {
            //记录时间
            m_waittime = System.DateTime.Now;
            m_client = new WWWClient();
            m_state = State.Waiting;
            URL = url;
            m_sendStream = bs;

            WWWClient client = new WWWClient();
        }

        public void SendFormData(System.Action<string> callback)
        {
            m_callback = callback;
            m_client.SendPostAsync(URL, m_sendForm, (byte[] bs) =>
            {
                m_revStream = bs;

                if (m_state != State.TimeOut)
                {
                    if (!string.IsNullOrEmpty(m_client.error))
                        m_state = State.Error;
                    else if (bs == null || bs.Length == 0)
                    {
                        m_state = State.Error;
                        m_client.error = "empty reply";
                    }
                    else
                        m_state = State.Done;
                }
            });
        }

        public void SendStreamData( System.Action<string> callback )
        {
            m_callback = callback;
            m_client.SendDataAsync(URL, m_sendStream, (byte[] bs) =>
            {
                m_revStream = bs;
              
                if (m_state != State.TimeOut)
                {
                    if (!string.IsNullOrEmpty(m_client.error))
                        m_state = State.Error;
                    else if (bs == null || bs.Length == 0)
                    {
                        m_state = State.Error;
                        m_client.error = "empty reply";
                    }
                    else
                        m_state = State.Done;
                }
            });
        }

        /// <summary>
        /// callback[在WWWManager中主动调用]
        /// </summary>
        /// <param name="response"></param>
        public string DoCallback()
        {
            //m_state == State.Done [assert]

            string errorcode = string.Empty;

            if (m_revStream == null)
            {
                // callback null
                HandleCallback(string.Empty);
            }
            else
            {  
                Dictionary<string, string> output = null;
                JsonHelper.ToDictionary(m_revStream, out output);

                if (output == null)
                {
                    errorcode = string.Format("WWWRequest.DoCallback: decode stream error from [url:{0}]",  URL);
                }
                else
                {
                    foreach (string key in output.Keys)
                    {
                        if (key.CompareTo(MsgType.exception) == 0)
                        {
                            // 显示返回值
                            errorcode = string.Format("WWWRequest.DoCallback: exception[{0}] from [url:{1}]",  output[MsgType.exception], URL);
                        }
                        else if (key.CompareTo(MsgType.update) == 0)
                        {
                            errorcode = HandleUpdate( output[key] );
                        }
                    }
                }

                if (string.IsNullOrEmpty(errorcode))
                {
                    if (!output.ContainsKey(MsgType.current))
                    {
                        errorcode = string.Format("WWWRequest.DoCallback: no current error from [url:{0}]",  URL);
                    }
                    else
                        errorcode = HandleCallback(output[MsgType.current]);

                }
               
            }

            if ( !string.IsNullOrEmpty(errorcode ))
                this.m_client.error = errorcode;
            return errorcode;
        }

        protected virtual string HandleCallback(string response)
        {
            if (m_callback != null)
            {
                try
                {
                    m_callback(response);
                }
                catch (Exception e)
                {
                    return string.Format("WWWRequest.HandleCallback: exception[{0}] from [url:{1}]", e.Message, URL);
                }
            }

            return string.Empty;
        }

        protected virtual string HandleUpdate(string jsontxt)
        {
            if (updateObject == null)
            {
                return "update object is null";
            }

            try
            {
                // 1. 解析为协议 object
                //UpdateObjectData update = JsonHelper.Deserialize<UpdateObjectData>(jsontxt);
                Dictionary<string, string> update = null;
                JsonHelper.ToDictionary(jsontxt, out update);
                if (update == null || update.Count == 0)
                {
                    return "empty update";
                }

                // 2. 获得object.path和object.value
                foreach ( string key in update.Keys )
                {
                    // 3. 更新JsonHelper.UpdateObject( path, value );
                    JsonHelper.UpdateObject(updateObject, key, update[key]);
                }
                
            }
            catch (Exception e)
            {
                return ( string.Format( "WWWRequest.HandleUpdate:Exception[{0}]", e.Message ) );
            }

            return string.Empty;
        }
    }
}
