
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LitJson;

namespace monocat
{
    /// <summary>
    /// Json工具 www.json.org
    /// </summary>
    public class JsonHelper
    {

        /// <summary>
        /// 序列化
        /// </summary>
        public static string Serialize ( System.Object obj )
        {
            string jsondata = LitJson.JsonMapper.ToJson(obj);

            return jsondata;
        }

        /// <summary>
        /// 序列化
        /// </summary>
        public static byte[] SerializeToBytes(System.Object obj)
        {
            string jsondata = LitJson.JsonMapper.ToJson(obj);

            byte[] bs = UTF8Encoding.UTF8.GetBytes(jsondata);

            return bs;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public static T Deserialize<T>(string jsondata)
        {
            T t = LitJson.JsonMapper.ToObject<T>(jsondata);

            return t;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public static T Deserialize<T>(byte[] bs)
        {
            if (bs == null || bs.Length == 0)
                return default(T);

            string jsondata = UTF8Encoding.UTF8.GetString(bs);
            T t = LitJson.JsonMapper.ToObject<T>(jsondata);

            return t;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public static T Deserialize<T>(Dictionary<string, string> dic)
        {

            string json = LitJson.JsonMapper.ToJson(dic);
            T t = LitJson.JsonMapper.ToObject<T>(json);

            return t;
        }

        /// <summary>
        /// Dictionary to json format string
        /// </summary>
        public static void ToJson(Dictionary<string,string> dic, ref string json)
        {
            json = LitJson.JsonMapper.ToJson(dic);
        }

        /// <summary>
        /// Json string to Dictionary
        /// </summary>
        /// <param name="jsoninput">json string</param>
        /// <param name="output">Dictionary</param>
        public static void ToDictionary(string jsoninput, out Dictionary<string, string> output)
        {
            output = new Dictionary<string, string>();

            if ( string.IsNullOrEmpty( jsoninput ) )
                return;

            output = LitJson.JsonMapper.ToObject<Dictionary<string, string>>(jsoninput);
        }

        /// <summary>
        /// Json string to Dictionary
        /// </summary>
        /// <param name="jsoninput">json string</param>
        /// <param name="output">Dictionary</param>
        public static void ToDictionary(byte[] jsoninput, out Dictionary<string, string> output)
        {
            output = new Dictionary<string, string>();
            if (jsoninput == null || jsoninput.Length == 0)
                return;

            string jsontxt = UTF8Encoding.UTF8.GetString(jsoninput);

            ToDictionary(jsontxt, out output);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        public static bool UpdateObject( object instance, string path, string json )
        {
            return LitJson.JsonMapper.SetObjectValue(instance, path, json);
        }

        #region 加密

        internal static void EncodeBytes( ref byte[] json )
        {
            if (json == null || json.Length == 0 )
                return;

            // 倒序
            for (int i = 0; i < json.Length/2; i++)
            {
                byte temp = json[i];

                json[i] = json[json.Length-1 - i];

                json[json.Length - 1 - i] = temp;
            }

            // 前后错位
            for (int i = 0; i < json.Length-1; i+=2)
            {
                byte temp = json[i];
                json[i] = json[i + 1];
                json[i + 1] = temp;
            }
        }

        public static byte[] DecodeBytes(byte[] json)
        {
            if (json == null || json.Length == 0)
                return null;
            byte[] bs = new byte[json.Length];

            // 反前后错位
            //for (int i = 0; i < json.Length; i+=2)
            //{
            //    if (i + 1 < json.Length)
            //    {
            //        bs[i] = json[i + 1];
            //        bs[i + 1] = json[i];
            //    }
            //    else
            //        bs[i] = json[i];    
            //}

            // 前后错位
            for (int i = 0; i < json.Length - 1; i += 2)
            {
                byte temp = json[i];
                json[i] = json[i + 1];
                json[i + 1] = temp;
            }

            // 反倒序
            for (int i = 0; i < json.Length; i++)
            {
                bs[i] = json[json.Length - 1 - i];
            }

            return bs;
        }

        #endregion
    }
}
