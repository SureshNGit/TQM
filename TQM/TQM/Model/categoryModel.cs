using SQLite;
using System;

namespace TQM.Model
{
    public class CategoryModel
    {
        [PrimaryKey]
        public Guid ID { get; set; }
        [MaxLength(50)]
        public string category { get; set; }
        public DateTime createdate { get; set; }

        public bool dataSyncStatus { get; set; } = false;
    }
}
