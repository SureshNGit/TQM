using SQLite;
using System;

namespace TQM.Model
{
    public class CompanyModel
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        public DateTime createdate { get; set; }

        public bool dataSyncStatus { get; set; } = false;
    }
}
