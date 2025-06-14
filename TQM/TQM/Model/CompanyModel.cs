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

        [MaxLength(1000)]
        public string ftpIpAddress { get; set; }

        [MaxLength(100)]
        public string username { get; set; }

        [MaxLength(100)]
        public string password { get; set; }

        public DateTime createdate { get; set; }

        public bool dataSyncStatus { get; set; } = false;
    }
}
