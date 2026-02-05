using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBL
{
    public class RatingDB : BaseDB<Rating>
    {
        protected override string GetTableName()
        {
            return "ratings";
        }

        protected override string GetPrimaryKeyName()
        {
            return "ratingid";
        }

        protected async override Task<Rating> CreateModelAsync(object[] row)
        {
            Rating r = new Rating();
            r.ratingid = int.Parse(row[0].ToString());
            r.userid = int.Parse(row[1].ToString());
            r.songid = int.Parse(row[2].ToString());
            r.rating = int.Parse(row[3].ToString());
            r.daterated = row[4] == null ? null : (System.DateTime?)row[4];
            return r;
        }

        public async Task AddOrUpdateRatingAsync(int userId, int songId, int value)
        {
            string checkSql = "SELECT * FROM ratings WHERE userid=@u AND songid=@s";
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("u", userId);
            p.Add("s", songId);

            List<Rating> list = await SelectAllAsync(checkSql, p);

            if (list.Count == 0)
            {
                Dictionary<string, object> insert = new Dictionary<string, object>();
                insert.Add("userid", userId);
                insert.Add("songid", songId);
                insert.Add("rating", value);
                insert.Add("daterated", DateTime.Now);
                await InsertAsync(insert);
            }
            else
            {
                Dictionary<string, object> fields = new Dictionary<string, object>();
                fields.Add("rating", value);
                fields.Add("daterated", DateTime.Now);

                Dictionary<string, object> where = new Dictionary<string, object>();
                where.Add("ratingid", list[0].ratingid);

                await UpdateAsync(fields, where);
            }
        }

        // Save rating (alias for AddOrUpdateRatingAsync)
        public async Task SaveRatingAsync(int userId, int songId, int value)
        {
            await AddOrUpdateRatingAsync(userId, songId, value);
        }

        // Get all ratings for a specific user
        public async Task<List<Rating>> GetUserRatingsAsync(int userId)
        {
            string sql = "SELECT * FROM ratings WHERE userid=@u ORDER BY daterated DESC";
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("u", userId);

            return await SelectAllAsync(sql, p);
        }

        // Get all ratings for a specific song
        public async Task<List<Rating>> GetSongRatingsAsync(int songId)
        {
            string sql = "SELECT * FROM ratings WHERE songid=@s ORDER BY daterated DESC";
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("s", songId);

            return await SelectAllAsync(sql, p);
        }

        // Get user's rating for a specific song
        public async Task<Rating?> GetUserRatingForSongAsync(int userId, int songId)
        {
            string sql = "SELECT * FROM ratings WHERE userid=@u AND songid=@s";
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add("u", userId);
            p.Add("s", songId);

            var list = await SelectAllAsync(sql, p);
            return list.Count > 0 ? list[0] : null;
        }

        //public async Task<double> GetAverageRatingAsync(int songId)
        //{
        //    string sql = "SELECT AVG(rating) FROM ratings WHERE songid=@s";
        //    var p = new Dictionary<string, object> { 
        //        { "s", songId } 
        //    };

        //    List<object> list = await SelectAllAsync(sql, p);

        //    if (list != null && list.Count > 0)
        //    {
        //        object[] row = (object[])list[0];
        //        object val = row[0];

        //        // Check for both C# null and Database Null
        //        if (val != null && val != DBNull.Value)
        //        {
        //            // Direct conversion is safer and faster
        //            return Convert.ToDouble(val);
        //        }
        //    }

        //    return 0;
        //}
    }
}
