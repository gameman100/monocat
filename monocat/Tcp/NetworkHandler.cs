
using System.Collections.Generic;
using System.Text;

namespace monocat
{
	/// <summary>
	/// 消息队列回调对象（所有回调对象均需要继承自此类）
	/// </summary>
    public interface NetworkHandler
    {
        void OnNetworkEvent(NetPacket packet);
    }
}
