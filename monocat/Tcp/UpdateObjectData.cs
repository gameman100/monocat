using System.Collections.Generic;
using System.Text;

namespace monocat
{
    /// <summary>
    /// 用于更新的协议
    /// </summary>
    public class UpdateObjectData
    {
        public List<UpdateItem> itemlist = new List<UpdateItem>();
    }

    public class UpdateItem
    {
        public string path = string.Empty;
        public string data = string.Empty;
    }
}
