using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace TQM.Model
{
    public class UserModel
    {
        [PrimaryKey]
        public Guid ID { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string userId { get; set; }
        public bool isAdmin { get; set; }
        public string password { get; set; }
        public bool isActive { get; set; }
        public bool isloggedIn { get; set; }
        public DateTime createdate { get; set; }
        public DateTime lastLogin { get; set; }

        [ForeignKey(typeof(CompanyModel))]
        public Guid companyID { get; set; }

        public bool dataSyncStatus { get; set; } = false;


    }
}
