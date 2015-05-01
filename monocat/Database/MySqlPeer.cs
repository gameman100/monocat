using System;
using System.Collections.Generic;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace monocat
{
	/// <summary>
	/// mysql database peer
	/// </summary>
	public class MySqlPeer
	{
        
        /// <summary>
        /// 只合用一个连接
        /// </summary>
		private MySqlConnection m_connection;
        public MySqlConnection connection { get { return m_connection; } }

		private MySqlDataReader m_dataReader;
        public MySqlDataReader dataReader { get { return m_dataReader; } }

        /// <summary>
        /// 执行SQL语句, 只适合同步使用
        /// </summary>
        private MySqlCommand m_command;
        public MySqlCommand command { get { return m_command; } }

        /// <summary>
        /// 异步请求列表
        /// </summary>
        private Queue<string> m_asyncNonQueryList;
        /// <summary>
        /// 异步请求回调[即使是异步，也不可能是并发]
        /// </summary>
        private Queue<System.IAsyncResult> m_asyncNonQueryCallbacks;

        private System.Threading.Thread m_asyncThread;

		public MySqlPeer ()
		{
            m_asyncNonQueryList = new Queue<string>();
            m_asyncNonQueryCallbacks = new Queue<System.IAsyncResult>();
		}

        /// <summary>
        /// 连接数据库 eg: localhost, 3306, root, ******, "somedatabase"
        /// </summary>
        /// <returns>0 false, 1 ok</returns>
        public int Connect(string host, int port, string user, string password, string database)
		{
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("server=").Append(host).Append(";");
            sb.Append("user=").Append(user).Append(";");
            sb.Append("database=").Append(database).Append(";");
            sb.Append("port=").Append(port).Append(";");
            sb.Append("password=").Append(password).Append(";");
            string connStr = sb.ToString();
            m_connection = new MySqlConnection(connStr);
			try
			{
				Console.WriteLine("Connecting to MySQL...");
                m_connection.Open();
				// Perform database operations
                m_command = new MySqlCommand();
                m_command.Connection = m_connection;
                return 1;
			}
            catch (MySqlException ex)
			{
                Console.WriteLine(string.Format("MySqlPeer.Connect:[exception:{0}]", ex.Message));
                return 0;
			}

		}

		/// <summary>
		/// 断开与数据库的连接
		/// </summary>
		public void Close()
		{
            m_asyncThread.Join();
            m_connection.Close();
		}

        public void StartThreadUpdate()
        {
            m_asyncThread = new Thread(UpdateThread);
            m_asyncThread.Start();
        }

        private void UpdateThread()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(30);

                // 更新NonQuery
                if (m_asyncNonQueryCallbacks.Count > 0) // 如果有Nonquery请求任务
                {
                    IAsyncResult r = m_asyncNonQueryCallbacks.Peek(); // 获得异步状态
                    if (r.IsCompleted)
                    {
                        try
                        {
                            //Console.WriteLine(m_command.CommandText);
                            m_command.EndExecuteNonQuery(r);
                            m_command.CommandText = string.Empty;
                            m_asyncNonQueryCallbacks.Dequeue();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }

                if (m_asyncNonQueryList.Count > 0 && string.IsNullOrEmpty(m_command.CommandText))
                {
                    m_command.CommandText = m_asyncNonQueryList.Dequeue();
                    try
                    {
                        IAsyncResult r = m_command.BeginExecuteNonQuery();
                        m_asyncNonQueryCallbacks.Enqueue(r);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                }
            }
        }

        #region 同步实现

        /// <summary>
		/// 从数据库读取数据, eg: "SELECT Name, HeadOfState FROM Country WHERE Continent='Oceania'"
		/// </summary>
		public void Read( string sql )
		{
			try
			{
                m_command.CommandText = sql;
                m_dataReader = m_command.ExecuteReader();

                /*
                while (m_dataReader.Read())
                {
                    Console.WriteLine(m_dataReader[0]+" -- "+m_dataReader[1] + ","+m_dataReader.GetFieldType(0) +","+m_dataReader.GetName(0));
                }
                */
            }
            catch (MySqlException ex)
			{
                Console.WriteLine(string.Format("MySqlPeer.Read:[exception:{0}]", ex.Message));
			}
            finally
            {
                if (m_dataReader != null) m_dataReader.Close();
            }
		}

		public void CloseReader()
		{
            m_dataReader.Close();
		}

        public bool HasRows()
        {
            return m_dataReader.HasRows;
        }

        public bool ReadRow(out Dictionary<string, string> dic)
        {
            return ReadRow(m_dataReader, out dic);
        }

        public bool ReadRow(MySqlDataReader reader, out Dictionary<string, string> dic)
        {
            dic = new Dictionary<string, string>();
            if (!reader.HasRows)
                return false;
            try
            {
                if (!reader.Read())
                    return false;

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    dic.Add(reader.GetName(i), reader[i].ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("MySqlPeer.ReadRow:[exception:{0}]", e.Message));
            }
            finally
            {
                if (reader != null) reader.Close();
            }

            return true;
        }

		/// <summary>
		/// 添加或删除记录 insert update or delete , eg: "INSERT INTO Country (Name, HeadOfState, Continent) VALUES ('Disneyland','Mickey Mouse', 'North America')"
		/// </summary>
		public int NonQuery( string sql )
		{
            int result = 0;
			try
			{
                m_command.CommandText = sql;
                //MySqlCommand cmd = new MySqlCommand(sql, m_connection);
                result = m_command.ExecuteNonQuery();
			}
            catch (MySqlException ex)
			{
                Console.WriteLine(string.Format("MySqlPeer.NonQuery:[exception:{0}]", ex.Message));
                result = -2;
			}

            return result;
		}

        /// <summary>
        /// 返回一个数据 eg: "SELECT COUNT(*) FROM Country"
        /// </summary>
        public void Scalar(string sql, ref object result)
		{
			try
			{
                MySqlCommand cmd = new MySqlCommand(sql, m_connection);
                result = cmd.ExecuteScalar();

                //m_command.CommandText = sql;
                //result = m_command.ExecuteScalar();
			}
            catch (MySqlException ex)
			{
                Console.WriteLine(string.Format("MySqlPeer.Scalar:[exception:{0}]", ex.Message));
			}
		}

        /// <summary>
        /// 
        /// </summary>
        [System.Obsolete("返回一个最后插入数据的ID eg: SELECT LAST_INSERT_ID()")]
        private int LastInsertID()
        {
            int id = 0;
            try
            {
                //MySqlCommand cmd = new MySqlCommand(sql, m_connection);
                m_command.CommandText = "SELECT LAST_INSERT_ID()";
                object obj = m_command.ExecuteScalar();
                id = int.Parse((string)obj);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(string.Format("MySqlPeer.LastInsertID:[exception:{0}]", ex.Message));
            }

            return id;
        }

        #endregion

        #region 异步实现

        /// <summary>
        /// 从数据库读取数据, eg: "SELECT Name, HeadOfState FROM Country WHERE Continent='Oceania'"
        /// </summary>
        private System.IAsyncResult BeginReadAsync(string sql)
        {
            IAsyncResult result = null;
            try
            {
                MySqlCommand cmd = new MySqlCommand(sql);
                result = cmd.BeginExecuteReader();
               
            }
            catch (MySqlException ex)
            {
                System.Console.WriteLine(string.Format("MySqlPeer.BeginReadAsync:[exception:{0}]", ex.Message));
            }

            return result;
        }

        private MySqlDataReader EndReadAsync(System.IAsyncResult result)
        {
            MySqlCommand cmd = (MySqlCommand)result.AsyncState;
            return cmd.EndExecuteReader(result);
        }

        /// <summary>
        /// [Ob]异步添加，更新或删除记录 insert or delete , eg1: INSERT INTO Country (Name, HeadOfState, Continent) VALUES ('Disneyland','Mickey Mouse', 'North America'); eg2: update table set xx=xx, xx=xx where xx=xx; eg3:delete from xx where xx=xx
        /// </summary>
        private void NonQueryAsyncQuickTest(string sql)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand(sql, m_connection);
                cmd.BeginExecuteNonQuery(NonQueryCallback, cmd);
            }
            catch (MySqlException e)
            {
                System.Console.WriteLine(string.Format("MySqlPeer.NonQueryAsync:[exception:{0}]", e.Message));
            }
        }

        private void NonQueryCallback(System.IAsyncResult result)
        {
            MySqlCommand cmd = (MySqlCommand)result.AsyncState;
            try
            {
                cmd.EndExecuteNonQuery(result);
            }
            catch (MySqlException e)
            {
                System.Console.WriteLine(string.Format("MySqlPeer.NonQueryCallback:[exception:{0}]", e.Message));
            }
        }

        /// <summary>
        /// 异步NonQuery
        /// </summary>
        /// <param name="sql"></param>
        public void NonQueryAsync(string sql)
        {
            this.m_asyncNonQueryList.Enqueue(sql);
        }

        #endregion

        #region 生成SQL语句

        /// <summary>
        /// 创建sql insert语句
        /// eg: string sql = MySqlPeer.SQLInsert("player", new string[]{"name", "age", "sex"} , new string[]{"player", "18", "1"});
        /// </summary>
        public static string SQLInsert(string tablename, string[] fields, string[] values)
        {
            string sql = string.Empty;

            if (fields == null || values == null || fields.Length != values.Length)
            {
                Console.WriteLine("error, field length is not the same as values");
                return sql;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("INSERT INTO ");
            sb.Append(tablename);
            sb.Append(" (");
            for (int i = 0; i < fields.Length; i++)
            {
                sb.Append(fields[i]);
                if (i < values.Length - 1)
                    sb.Append(",");

            }
            //sb.Append(fields);
            sb.Append(")");
            sb.Append(" VALUES (");
            for (int i = 0; i < values.Length; i++)
            {
                //sb.Append("'");
                sb.Append(MySqlHelper.EscapeString(values[i]));
                //sb.Append("'");
                if (i < values.Length - 1)
                    sb.Append(",");
            }

            sb.Append(")");
            sql = sb.ToString();

            return sql;
        }

        public static string SQLInsert(string tablename, string setvalue)
        {
            string sql = string.Empty;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("INSERT INTO ");
            sb.Append(tablename);
            sb.Append(" SET ");
            sb.Append(setvalue);
            sql = sb.ToString();
            
            return sql;
        }

        /// <summary>
        /// 创建sql delete语句(eg: delete from xx where xx=xx)
        /// </summary>
        public static string SQLDelete(string tablename, string where)
        {
            string sql = string.Empty;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("DELETE FROM ");
            sb.Append(tablename);
            sb.Append(" WHERE ");
            sb.Append(where);

            sql = sb.ToString();
            sql = MySqlHelper.EscapeString(sql);
            return sql;
        }

        /// <summary>
        /// 创建select语句
        /// eg: string sql2 = MySqlPeer.SQLSelect("player", new string[]{"*","id","name"}, new string[]{"id"}, new string[]{"22"});
        /// </summary>
        /// <param name="tablename">table名称</param>
        /// <param name="selectfields">选择的字段名称</param>
        /// <param name="keyname">键名称</param>
        /// <param name="keyvalue">值名称</param>
        /// <returns>返回sql string</returns>
        public static string SQLSelect( string tablename, string[] selectfields, string[] keyname, string[] keyvalue)
        {
            // eg: "SELECT Name, HeadOfState FROM Country WHERE Continent='Oceania'"

            string sql = string.Empty;
            if (selectfields == null )
            {
                Console.WriteLine("error, select fields is empty");
                return sql;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("SELECT ");
            for (int i = 0; i < selectfields.Length; i++)
            {
                sb.Append(selectfields[i]);
                if (i < selectfields.Length - 1)
                    sb.Append(",");
            }

            sb.Append(" from ");
            sb.Append(tablename);

            if (keyname != null && keyname.Length > 0)
            {
                sb.Append(" WHERE ");

                int len = keyname.Length <= keyvalue.Length ? keyname.Length : keyvalue.Length;
                for (int i = 0; i < len; i++)
                {
                    if (!string.IsNullOrEmpty(keyname[i]) && !string.IsNullOrEmpty(keyvalue[i]))
                    {
                        sb.Append(keyname[i]);
                        sb.Append("='");
                        sb.Append(keyvalue[i]);
                        sb.Append("'");

                        if (i < len - 1)
                        {
                            sb.Append(" and ");
                        }
                    }
                }
            }

            sql = sb.ToString();
            return sql;
        }

        /// <summary>
        /// 创建select语句
        /// eg: string sql2 = MySqlPeer.SQLSelect("player", new string[]{"*","id","name"}, new string[]{"id"}, new string[]{"22"});
        /// </summary>
        /// <param name="tablename">table名称</param>
        /// <param name="selectfields">选择的字段名称</param>
        /// <param name="keyname">键名称</param>
        /// <param name="keyvalue">值名称</param>
        /// <returns>返回sql string</returns>
        public static string SQLSelect(string tablename, string[] selectfields, string keyname, string keyvalue)
        {
            // eg: "SELECT Name, HeadOfState FROM Country WHERE Continent='Oceania'"

            string sql = string.Empty;
            if (selectfields == null)
            {
                Console.WriteLine("error, select fields is empty");
                return sql;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("SELECT ");
            for (int i = 0; i < selectfields.Length; i++)
            {
                sb.Append(selectfields[i]);
                if (i < selectfields.Length - 1)
                    sb.Append(",");
            }

            sb.Append(" from ");
            sb.Append(tablename);

            if (!string.IsNullOrEmpty(keyname) && !string.IsNullOrEmpty(keyvalue))
            {
                sb.Append(" WHERE ");
                sb.Append(keyname);
                sb.Append("=");
                sb.Append(keyvalue);
                //sb.Append("'");
            }

            sql = sb.ToString();
            sql = MySqlHelper.EscapeString(sql);
            return sql;
        }

     

        /// <summary>
        /// 创建sql update语句
        /// eg:  update table set xx=xx, xx=xx where xx=xx; 
        /// </summary>
        public static string SQLUpdate(string tablename, string[] fields, string[] values, string where)
        {
            string sql = string.Empty;

            if (fields == null || values == null || fields.Length != values.Length )
            {
                Console.WriteLine("error, field length is not the same as values");
                return sql;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("UPDATE ");
            sb.Append(tablename);
            sb.Append(" SET ");
            for (int i = 0; i < fields.Length; i++)
            {
                sb.Append(fields[i]);
                sb.Append("=");
                sb.Append(values[i]);
                //sb.Append("'");
                if (i < values.Length - 1)
                    sb.Append(",");
            }

            sb.Append(" WHERE ");
            sb.Append(where);
            sql = sb.ToString();
            sql = MySqlHelper.EscapeString(sql);
            return sql;
        }

        #endregion

    } // end class


} // end namespace

