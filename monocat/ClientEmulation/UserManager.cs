using System;
using System.Collections.Generic;
using System.Text;
using monocat;

namespace client_emulation
{
    public class UserManager
    {
        protected static UserManager m_instance = null;
        public static UserManager Get { 
            get {
                if (m_instance == null)
                    m_instance = new UserManager();
                return m_instance;
            } 
        }
        public Dictionary<int, User> clientList = new Dictionary<int,User>();

        public UserManager()
        {
        }

        public void AddNewUser(User u)
        {
            if (u == null || u.id<=0 )
                return;

            if (clientList.ContainsKey(u.id))
                clientList.Remove(u.id);

            clientList.Add(u.id, u);
        }
    }
}
