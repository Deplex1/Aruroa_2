using System;

namespace Models
{
    public class User
    {
        public int userid { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public byte[] profilepicture { get; set; }
        public int IsAdmin { get; set; }
        public string ResetCode { get; set; }

        public User()
        {
        }
    }
}
