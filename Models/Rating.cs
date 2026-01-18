using System;

namespace Models
{
    public class Rating
    {
        public int ratingid { get; set; }
        public int userid { get; set; }
        public int songid { get; set; }
        public int rating { get; set; } // 1 - 5
        public DateTime? daterated { get; set; }

        public Rating()
        {
        }
    }
}
