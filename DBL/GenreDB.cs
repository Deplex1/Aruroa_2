using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBL
{
    public class GenreDB : BaseDB<Genre>
    {
        protected override string GetTableName()
        {
            return "genres";
        }

        protected override string GetPrimaryKeyName()
        {
            return "genreid";
        }

        protected async override Task<Genre> CreateModelAsync(object[] row)
        {
            Genre g = new Genre();
            g.genreid = int.Parse(row[0].ToString());
            g.name = row[1].ToString();
            return g;
        }

        public async Task<List<Genre>> SelectAllGenresAsync()
        {
            return await SelectAllAsync();
        }

        public async Task<int> AddGenreAsync(string name)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            values.Add("name", name);

            int rows = await InsertAsync(values);

            return rows;
        }
    }
}
