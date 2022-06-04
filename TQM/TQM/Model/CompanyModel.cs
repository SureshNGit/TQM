using SQLite;
using System;

namespace TQM.Model
{
    public class CompanyModel
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        public DateTime createdate { get; set; }
    }
}
