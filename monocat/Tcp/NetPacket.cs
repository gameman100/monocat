using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace monocat
{
	public class NetPacket
    {
        /// <summary>
        /// int32 占4个字节	
        /// </summary>
        public const int INT32_LEN = 4;

        /// <summary>
        /// 头 占4个字节
        /// </summary>
        public const int headerLength = 4;
        
        /// <summary>
        /// 身体 最大30kb
        /// </summary>
		public const int max_body_length = 1024 * 30; // 30kb
        
        /// <summary>
        /// 总长
        /// </summary>
        public int Length
        {
            get { return headerLength + bodyLenght; }
        }
        
        /// <summary>
        /// 当前数据体长
        /// </summary>
		public int bodyLenght;

        /// <summary>
        /// byte数组
        /// </summary>
		public byte[] bytes;

        /// <summary>
        /// 发送这个数据包的socket
        /// </summary>
		public MySocket mySocket;

        /// <summary>
        /// 从网络上读到的数据长度
        /// </summary>
        public int readLength;
       
        /// <summary>
        /// msg id
        /// </summary>
		public string msgid = string.Empty;

		/// <summary>
		/// 回调，它将传递给mysocket，只能返回最后一个请求的回调,
		/// </summary>
		public System.Action<NetPacket> callback = null;

        // 构造函数
        public NetPacket()
        {
            readLength = 0;
            bodyLenght = 0;
            bytes = new byte[headerLength + max_body_length];
			msgid = string.Empty;
			callback = null;
        }

		public NetPacket ( NetPacket packet )
		{
			bodyLenght = packet.bodyLenght;
			bytes = new byte[packet.bytes.Length];
			packet.bytes.CopyTo (bytes, 0);
			mySocket = packet.mySocket;
			readLength = packet.readLength;
			msgid = packet.msgid;
			callback = packet.callback;
		}

        public void Reset()
        {
            readLength = 0;
            bodyLenght = 0;
			callback = null;
        }

        // 开始写入数据
        public void BeginWrite( string msg )
        {
            // 初始化体长为0
            bodyLenght = 0;
            WriteString(msg);
			msgid = msg;
        }

        // 写整型
        public void WriteInt(int number)
        {
            if (bodyLenght + INT32_LEN > max_body_length)
                return;

            byte[] bs = System.BitConverter.GetBytes(number);

            bs.CopyTo(bytes, headerLength + bodyLenght);

            bodyLenght += INT32_LEN;
        }

        // 写字符串
        public void WriteString(string str)
        {
            int len = System.Text.Encoding.UTF8.GetByteCount(str);
            this.WriteInt(len);
            if (bodyLenght + len > max_body_length)
                return;

            System.Text.Encoding.UTF8.GetBytes(str, 0, str.Length, bytes, headerLength + bodyLenght);

            bodyLenght += len;
        }

           /// <summary>
		/// 写入byte数组
	    /// </summary>
	    public void WriteStream(byte[] bs)
	    {
	        WriteInt(bs.Length);
			if (bodyLenght + bs.Length > max_body_length)
                return;

	        // 压入数据流
			bs.CopyTo(bytes, headerLength + bodyLenght);

			bodyLenght += bs.Length;
	    }

        // 直接写入一个序列化的对象
        public void WriteObject<T>(T t)
        {
            byte[] bs = Serialize<T>(t);
            WriteStream(bs);
        }

        // 直接写入一个序列化的对象
        //public virtual void WriteProtobufObject<T>(T t)
        //{
        //    byte[] bs = SerializerProtobuf<T>(t);
        //    WriteStream(bs);
        //}

		// 使用end write一定要使用endread
		public void EndWrite()
		{
			if (bytes == null || bytes.Length <= 4) {
				return;
			}
			if (bytes.Length < Length)
				return;

			try{
				for (int i = headerLength; i < bodyLenght-1; i+=2) {
					byte temp = bytes [i];
					bytes[i] = bytes[i+1];
					bytes[i+1] = temp;
				}
			}
			catch( System.Exception e ) {

				System.Console.WriteLine (e.Message + "\n" + e.StackTrace);
			}
		}

        // 开始读取
        public void BeginRead(out string msg)
        {
			msg = "";
            bodyLenght = 0;
			ReadString(out msg);
			msgid = msg;
        }

        // 读 int
        public void ReadInt(out int number)
        {
            number = 0;
            if (bodyLenght + INT32_LEN > max_body_length)
                return;

            number = System.BitConverter.ToInt32(bytes, headerLength + bodyLenght);
            bodyLenght += INT32_LEN;
        }

        // 读取一个字符串
        public void ReadString(out string str)
        {
            str = "";
            int len = 0;
            ReadInt(out len);

			if (bodyLenght + len > max_body_length || len==0)
                return;

			try{
            	str = Encoding.UTF8.GetString(bytes, headerLength + bodyLenght, (int)len);
			}
			catch( System.Exception e ) {
				System.Console.WriteLine (e.Message + "\n" + e.StackTrace);
			}

            bodyLenght += len;
        }

        // 读取byte数组
        public byte[] ReadStream()
	    {
	        int size = 0;
	        ReadInt(out size);

			if (bodyLenght + size > max_body_length) {
                System.Console.WriteLine("NetPacket:ReadStream:stream length is too big:" + size);
				return null;
			}
			// 对于protobuf,长度为0的byte[]是合法的
	        byte[] bs = new byte[size];
	        for (int i = 0; i < size; i++)
	        {
	            bs[i] = bytes[headerLength + bodyLenght + i];
	        }

	        bodyLenght += size;
	        return bs;
	    }

        // 直接将读取的byte数组反序列化
        public T ReadObject<T>()
        {
            byte[] bs = ReadStream();
            if (bs == null)
            {
                return default(T);
            }
            return Deserialize<T>(bs);
        }


        // 直接将读取的byte数组反序列化
        //public virtual T ReadProtobufObject<T>()
        //{
        //    byte[] bs = ReadStream();
        //    if (bs == null)
        //    {
        //        return default(T);
        //    }
        //    return DeserializeProtobuf<T>(bs);
        //}


        // 将数据长度转为4个字节存到byte数组的最前面
        public void EncodeHeader()
        {
            byte[] bs = System.BitConverter.GetBytes(bodyLenght);

            bs.CopyTo(bytes, 0);
        }

		/// <summary>
		/// 反
		/// </summary>
		public void EndRead()
		{
			if (bytes == null || bytes.Length <= 4) {
				return;
			}
			if (bytes.Length < Length)
				return;

			try{
				for (int i = headerLength; i < bodyLenght-1; i+=2) {
					byte temp = bytes [i];
					bytes[i] = bytes[i+1];
					bytes[i+1] = temp;
				}
			}
			catch( System.Exception e ) {

                System.Console.WriteLine(e.Message + "\n" + e.StackTrace);
			}
		}

        // 由byte数组最前面4个字节得到数据的长度
        public void DecodeHeader()
        {
            bodyLenght = System.BitConverter.ToInt32(bytes, 0);
        }

		#region 序列化C#对象（只能用于服务器与客户端均使用C#)

		/// <summary>
		/// 序列化对象
		/// </summary>
        public static byte[] Serialize<T>(T t)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                try
                {
                    // 创建序列化类
                    BinaryFormatter bf = new BinaryFormatter();
                    //序列化到stream中
                    bf.Serialize(stream, t);
                    stream.Seek(0, SeekOrigin.Begin);
                    return stream.ToArray();
                }
                catch (System.Exception ex )
                {
                    System.Console.WriteLine(ex.Message);
                    return null;
                }
            }
        }

        // 反序列化对象
        public static T Deserialize<T>(byte[] bs)
        {
            using (MemoryStream stream = new MemoryStream(bs))
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    //stream.Seek(0, SeekOrigin.Begin);
                    T t = (T)bf.Deserialize(stream);
                    return t;
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine("Deserialize:"+ex.Message);
                    return default(T);
                }
            }
        }

		#endregion


		#region 序列化Protobuf对象（该方法不适合直接在手机上使用)
        /// <summary>
        /// Serialize Proto
        /// </summary>
        //public static byte[] SerializerProtobuf<T>(T t)
        //{
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        try
        //        {
        //            ProtoBuf.Serializer.Serialize<T>(stream, t);
        //        }
        //        catch (Exception e)
        //        {

        //            Console.WriteLine("ProtoParser.Serializer:" + e.Message);
        //        }

        //        return stream.ToArray();
        //    }
        //}

        ///// <summary>
        ///// Deserialize Proto
        ///// </summary>
        //public static T DeserializeProtobuf<T>(byte[] bs)
        //{
        //    T t = default(T);
        //    using (MemoryStream stream = new MemoryStream(bs))
        //    {
        //        try
        //        {
        //            t = ProtoBuf.Serializer.Deserialize<T>(stream);
        //        }
        //        catch (Exception e)
        //        {

        //            Console.WriteLine("ProtoParser.Deserialize:" + e.Message);
        //        }
        //    }

        //    return t;
        //}



		#endregion

    }
}
