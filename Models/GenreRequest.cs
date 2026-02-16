using System;

namespace Models
{
    public class GenreRequest
    {
        public int requestid { get; set; }
        public int userid { get; set; }
        public string genre_name { get; set; }
        public DateTime requested_date { get; set; }
        public string status { get; set; }
        public int? reviewed_by { get; set; }
        public DateTime? reviewed_date { get; set; }

        public GenreRequest()
        {
            status = "pending";
            requested_date = DateTime.Now;
        }
    }
}