using System.Collections.Generic;
using System.Text;

namespace monocat
{
    /// <summary>
    /// 服务端响应客户端请求工具类
    /// </summary>
    public class ResponseUtil
    {
        public Dictionary<string, string> response = new Dictionary<string,string>();
        private Dictionary<string, string> update = null;

        public void AddContent(string key, string content)
        {
            response.Add(key, content);
        }

        public string ToJson()
        {
            return JsonHelper.Serialize(response);
        }

        public byte[] ToStream()
        {
            string json = JsonHelper.Serialize(response);

            return System.Text.UTF8Encoding.UTF8.GetBytes( json );
        }

        public void Clear()
        {
            response.Clear();
            if (update != null)
                update.Clear();
        }

        public void BuildValueUpdate(string path, string value)
        {
            if (update == null)
                update = new Dictionary<string, string>();

            update.Add(path, value);
        }

        /// <summary>
        /// 更新或删除
        /// </summary>
        public void BuildObjectUpdate(string path,  object obj)
        {
            if (update == null)
                update = new Dictionary<string, string>();

            update.Add(path, JsonHelper.Serialize(obj));
        }

        /// <summary>
        /// 更新或删除
        /// </summary>
        public void BuildItemUpdate(string path, int id, object item)
        {
            if (update == null)
                update = new Dictionary<string, string>();

            update.Add( string.Format("{0}.{1}",path, id), JsonHelper.Serialize(item));
        }

        public void BuildRemoveItem(string path, int id)
        {
            if (update == null)
                update = new Dictionary<string, string>();

            update.Add(string.Format("{0}.{1}", path, id), "null");
        }

        public void GetUpdate(ref string jsonstr)
        {
            JsonHelper.ToJson(update, ref jsonstr);
        }
    }
}
