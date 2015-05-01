using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace monocat
{
    /// <summary>
    /// 负责读入数据库数据，存储数据(运行在逻辑线程中)
    /// </summary>
    class DatabaseManager
    {
        /// <summary>
        /// 根据用户名索引[仅用于内存数据库,暂时不使用]
        /// </summary>
        private Dictionary<string, User> m_userAccount;

        #region Session
        /// <summary>
        /// session列表
        /// </summary>
        private Dictionary<string, User> m_sessions = new Dictionary<string, User>();
        public string CreateSession(User user)
        {
            if (user == null)
                return string.Empty;

            // 这一步生成session
            string s = SessionMaker.Create(user.id, 0, Sessions.session_key);
            if (m_sessions.ContainsKey(s))
                m_sessions.Remove(s);
            m_sessions.Add(s, user);

            return s;
        }
        /// <summary>
        /// 验证session合法性
        /// </summary>
        /// <param name="session"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool VerifySession(string session, int id)
        {
            if (m_sessions.ContainsKey(session) && m_sessions[session].id == id)
            {
                m_sessions.Remove(session);
                return true;
            }
            else
                return false;
        }

        #endregion

        #region 服务器信息

        private ServerInfoDatabase m_serverInfo = new ServerInfoDatabase();

        /// <summary>
        /// 读入服务器配置[原始配置保存在excel中，使用python导出json格式的配置文件]
        /// </summary>
        public void ReadServerInfo()
        {
        }

        #endregion

        /// <summary>
        /// 数据库连接
        /// </summary>
        private MySqlPeer m_sqlPeerSync;
        public MySqlPeer sqlPeerSync { get { return m_sqlPeerSync; } }

        private MySqlPeer m_sqlPeerAsync;
        public MySqlPeer sqlPeerAsync { get { return m_sqlPeerAsync; } }

        public DatabaseManager()
        {
            m_userAccount = new Dictionary<string, User>();
        }

        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <returns></returns>
        public int InitDatabase()
        {
            m_sqlPeerSync = new MySqlPeer();
            m_sqlPeerAsync = new MySqlPeer();
            int result1 = m_sqlPeerSync.Connect("localhost", 3306, "root", "123456", "accountdb");
            int result2 = m_sqlPeerAsync.Connect("localhost", 3306, "root", "123456", "accountdb");

            return result1 + result2;
        }

        public void StartThreadUpdate()
        {
            m_sqlPeerAsync.StartThreadUpdate();
        }

        #region 内存数据库

        /// <summary>
        /// 将用户数据全部读到内存中
        /// </summary>
        public void ReadDataIntoMemeory()
        {
            // query
            string sql = MySqlPeer.SQLSelect("user", new string[] { "*" }, "", "");
            Console.WriteLine("sql:" + sql);
            m_sqlPeerSync.Read(sql);

            while (true)
            {
                Dictionary<string, string> temp;
                if (!m_sqlPeerSync.ReadRow(out temp))
                {
                    break;
                }

                // 使用JSON序列化user
                string jsonstr = string.Empty;
                JsonHelper.ToJson(temp, ref jsonstr);
                User user = JsonHelper.Deserialize<User>(jsonstr);

                m_userAccount.Add(user.username, user);
            }

            m_sqlPeerSync.CloseReader();

            Console.WriteLine("read ({0}) user from database", m_userAccount.Count);
        }


        /// <summary>
        /// 验证用户信息
        /// </summary>
        public int VerifyUserInMemory(string username, string password)
        {
            if (m_userAccount.ContainsKey(username))
            {
                if (m_userAccount[username].password.CompareTo(password) == 0)
                    return m_userAccount[username].id;
            }

            return -1;
        }

        #endregion

        #region 物理数据库

        /// <summary>
        /// 验证用户信息,返回用户ID, -1找不到该用户
        /// </summary>
        public int VerifyUserInDatabase(string username, string password, string loginip)
        {
            //string sql = MySqlPeer.SQLSelect("user", new string[] { "id" }, new string[] { "username", "password" }, new string[] { username, password });
            string sql = string.Format("select id from user where username= '{0}' and password ='{1}'",
                MySqlHelper.EscapeString(username), MySqlHelper.EscapeString( password ) );

            object result = null;
            int id = -1;
            m_sqlPeerSync.Scalar(sql, ref result);
           
            if (result == null)
            {
                return -1;
            }
            try
            {
                int.TryParse(result.ToString(), out id);

                string ts = System.DateTime.Now.ToString();
                string updatesql = string.Format("update user set loginip = '{0}', lastlogin= '{1}' where id = {2}", loginip, ts, id);
                //string updatesql = MySqlPeer.SQLUpdate("user", new string[] { "lastlogin" }, new string[] { "CURRENT_TIMESTAMP" }, string.Format("id={0}", id));
                m_sqlPeerAsync.NonQueryAsync(updatesql);
               
                return id;
            }
            catch( Exception e )
            {
                Console.WriteLine("[Exception]DatabaseManager.VerifyUserInDatabase: " + e.Message);

                return -1;
            }
        }

        /// <summary>
        /// 注册新用户
        /// </summary>
        public int SignUp(string username, string password)
        {
            // 插入新用户，如果失败，则说明该用户已存在
            //string sql = string.Format("insert into user set username='{0}', password ='{1}'", username, password);
            string ts = System.DateTime.Now.ToString();
            string insert = string.Format("username='{0}', password ='{1}', lastlogin='{2}'",
                MySqlHelper.EscapeString(username), MySqlHelper.EscapeString(password), ts);
            string sql = MySqlPeer.SQLInsert("user", insert);

            int user_id = 0;
            int result = m_sqlPeerSync.NonQuery(sql);
          
            if (result < -1) // 出错，可能用户名已经被占用，无法插入
            {
                user_id = 0;
            }
            else // 注册成功
            {
                // 查找注册的用户ID
                user_id = (int)m_sqlPeerSync.command.LastInsertedId;
            }

            return user_id;
        }

        #endregion
    }
}
