using Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBL
{
    public class UserDB : BaseDB<User>
    {
        protected override string GetTableName()
        {
            return "users";
        }

        protected override string GetPrimaryKeyName()
        {
            return "userid";
        }

        protected async override Task<User> CreateModelAsync(object[] row)
        {
            User u = new User();
            u.userid = int.Parse(row[0].ToString());
            u.username = row[1].ToString();
            u.password = row[2].ToString();
            u.email = row[3].ToString();
            u.profilepicture = row[4] as byte[];
            u.IsAdmin = int.Parse(row[5].ToString());
            u.ResetCode = row[6]?.ToString();
            return u;
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            string sql = "SELECT * FROM users WHERE username=@username AND password=@password";
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("username", username);
            p.Add("password", password);

            List<User> list = await SelectAllAsync(sql, p);
            if (list.Count == 1)
                return list[0];

            return null;
        }

        public async Task<bool> RegisterAsync(string username, string password, string email)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            values.Add("username", username);
            values.Add("password", password);
            values.Add("email", email);
            values.Add("IsAdmin", 0);

            int rows = await InsertAsync(values);
            return rows == 1;
        }
    }
}
