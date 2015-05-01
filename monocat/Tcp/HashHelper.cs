
using System.Security.Cryptography;

namespace monocat
{
    /// <summary>
    /// 用于MD5加密解密
    /// </summary>
	public class HashHelper
	{
		private static MD5CryptoServiceProvider md = new MD5CryptoServiceProvider();

        /// <summary>
        /// 获得MD5 byte数组
        /// </summary>
		public static byte[] MD5( byte[] data, byte[] key )
		{
			//byte[] bs = System.Text.UTF8Encoding.UTF8.GetBytes (key);

			int data_length = data.Length;
			int key_length = key.Length;

			byte[] buff = new byte[data_length + key_length];
			data.CopyTo (buff, 0);
			key.CopyTo (buff, data_length);

			byte[] bs = md.ComputeHash (buff);

			return bs;
		}

        public static string MD5(string content, string password)
        {
            byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(content);
            byte[] key =  System.Text.UTF8Encoding.UTF8.GetBytes(password);

            byte[] bs = MD5(data, key);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                string t = System.Convert.ToString(b, 16);
                t = t.PadLeft(2, '0');
                sb.Append (t);
            }
            string result = sb.ToString();
            result = result.PadLeft(32, '0');

            return result;
        }

        public static string MD5(byte[] content)
        {
            byte[] bs = md.ComputeHash(content);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                string t = System.Convert.ToString(b, 16);
                t = t.PadLeft(2, '0');
                sb.Append(t);
            }
            string result = sb.ToString();
            result = result.PadLeft(32, '0');

            return result;
        }

        public static string MD5(string content)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
            byte[] bs = md.ComputeHash(bytes);

            return MD5(bs);
        }

        /// <summary>
        /// 比较两段byte是否一致
        /// </summary>
		public static bool CompareBytes( byte[] a, byte[] b )
		{
			if (a == null || b == null || a.Length != b.Length)
				return false;

			for (int i = 0; i < a.Length; i++) {

				if (a [i] != b [i])
					return false;
			}

			return true;
		}
	}
}

