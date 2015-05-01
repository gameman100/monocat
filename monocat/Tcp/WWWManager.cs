using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;

namespace monocat
{
    /// <summary>
    /// 使用时需要继承
    /// </summary>
    public class WWWManager
    {

        protected static WWWManager m_instance = null;
        public static WWWManager Get { get { return m_instance; }}

        // Callback list
        protected Queue<WWWRequest> m_requests = new Queue<WWWRequest>();
        public int request_cout { get { return m_requests.Count; } }


        public static WWWRequest Create<T>(string url, NameValueCollection form, object updateobject, System.Action<T> callback)
        {
             WWWRequest r =  WWWRequest.Create<T>(url, form, callback);
             r.updateObject = updateobject;
             WWWManager.Get.m_requests.Enqueue(r);

             return r;
        }

        public static WWWRequest Create<T>(string url, object request, object updateobject, bool encodebyte, System.Action<T> callback )
        {
            WWWRequest r = WWWRequest.Create<T>(url, request, encodebyte, false, callback );
            r.updateObject = updateobject;
            WWWManager.Get.m_requests.Enqueue(r);

            return r;
        }


        public WWWManager()
        {
            m_instance = this;
        }

        /// <summary>
        /// 放到主循环中更新
        /// </summary>
        public void DoUpdate()
        {
            if (m_requests.Count == 0)
                return;

            WWWRequest request = m_requests.Peek();
            if (request.state == WWWRequest.State.Waiting)
                return;
            m_requests.Dequeue();
            switch (request.state)
            {
                case WWWRequest.State.Waiting:
                    break;
                case WWWRequest.State.TimeOut: // 连接超时
                    {
                        HandleError(request);
                        break;
                    }
                case WWWRequest.State.Error:    // 连接不到网络
                    {
                        HandleError(request);
                        break;
                    }
                case WWWRequest.State.Done: //返回数据
                    {
                        HandleDone(request);
                        break;
                    }
            }
        }


        /// <summary>
        /// 重新再试 或退出
        /// </summary>
        protected virtual void HandleError(WWWRequest request)
        {
        }

        /// <summary>
        /// 退出
        /// </summary>
        protected virtual void HandleDone(WWWRequest request)
        {
            string error = request.DoCallback();
            if (!string.IsNullOrEmpty(error))
            {
                HandleError(request);
            }
        }
    }
}
