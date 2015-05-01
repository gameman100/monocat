using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace monocat
{
    /// <summary>
    /// 所有处理Http事件的类均继承此类
    /// </summary>
    public abstract class HttpHandler
    {
        //protected HttpListenerRequest m_request;
        //protected HttpListenerResponse m_response;

        /// <summary>
        /// 处理请求
        /// </summary>
        internal void HandleRequest(HttpPacket packet)
        {
            try
            {
                Run(packet);
            }
            catch (Exception e)
            {
                ResponseUtil util = new ResponseUtil();
                util.response.Add( MsgType.exception, string.Format("HttpHandler.HandleRequest:{0}", e.Message ) );
                Response(util, packet.context);
            }
        }

        /// <summary>
        /// 处理客户端请求
        /// </summary>
        /// <param name="packet"></param>
        public virtual void Run( HttpPacket packet )
        {

        }

        /* 不用的代码
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
                Console.WriteLine(e.Message + " " + e.StackTrace);
            }
        }

        private void HandleReadRequest(IAsyncResult result)
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
                packet.text = UTF8Encoding.UTF8.GetString(packet.bytes);
                Run(packet);
            }

        }
        
        */

        /* 不用的代码

        /// <summary>
        /// offset default
        /// </summary>
        /// <param name="bs"></param>
        /// <param name="offset">default set it to 0</param>
        /// <param name="context"></param>
        private void ReadRequestHead(HttpPacket packet)
        {
            try
            {
                packet.context.Request.InputStream.BeginRead(packet.bytes, packet.readLength, NetPacket.headerLength - packet.readLength, new AsyncCallback(HandleRequestHead), packet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
            }
        }

        private void HandleRequestHead(IAsyncResult result)
        {
            HttpPacket packet = (HttpPacket)result.AsyncState;
            packet.readLength += packet.context.Request.InputStream.EndRead(result);

            if (packet.readLength < NetPacket.headerLength)
            {
                
                ReadRequestHead(packet);
            }
            else
            {
                packet.Reset(); // 将readlength = 0
                packet.DecodeHeader();
                ReadRequestBody(packet);

            }
          
        }

        private void ReadRequestBody(HttpPacket packet)
        {
            try
            {
                packet.context.Request.InputStream.BeginRead(packet.bytes, NetPacket.headerLength + packet.readLength, packet.bodyLenght - packet.readLength, new AsyncCallback(HandleRequestBody), packet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
            }
        }

        private void HandleRequestBody(IAsyncResult result)
        {

            HttpPacket packet = (HttpPacket)result.AsyncState;
            packet.readLength += packet.context.Request.InputStream.EndRead(result);

            if (packet.readLength < packet.bodyLenght)
            {
                ReadRequestBody(packet);
            }
            else
            {
                packet.context.Request.InputStream.Close();
                Run(packet);
            }

        }
        */

        /// <summary>
        /// 响应客户端
        /// </summary>
        public void Response(byte[] bs, int length, HttpListenerContext context)
        {
            context.Response.OutputStream.BeginWrite(bs, 0, length, new AsyncCallback(HandleWrite), context);
        }

        /// <summary>
        /// 响应客户端
        /// </summary>
        public void Response(ResponseUtil util, HttpListenerContext context)
        {
            byte[] bs = util.ToStream();
            context.Response.OutputStream.BeginWrite(bs, 0, bs.Length, new AsyncCallback(HandleWrite), context);
        }

        private void HandleWrite(IAsyncResult result)
        {
            HttpListenerContext context = (HttpListenerContext)result.AsyncState;
            if (context != null)
            {
                context.Response.OutputStream.EndWrite(result);
                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Close();
            }
        }
    }
}
