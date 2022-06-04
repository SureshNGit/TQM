using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace TQM.Model
{
    public class UserModel
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string userId { get; set; }
        public bool isAdmin { get; set; }
        public string password { get; set; }
        public bool isActive { get; set; }
        public DateTime createdate { get; set; }
        public DateTime lastLogin { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<CompanyModel> companies { get; set; }

    }
}
