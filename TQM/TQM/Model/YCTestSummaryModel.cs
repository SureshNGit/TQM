using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace TQM.Model
{
    public class YCTestSummaryModel
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string testID { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<UserModel> users { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<MachineModel> machines { get; set; }

        public string countsysname { get; set; }

        public int yarnlenunit { get; set; }

        public int yarnlength { get; set; }

        public int totaltestcount { get; set; }

        public decimal testaverage { get; set; }

        public decimal testsd { get; set; }

        public decimal testcv { get; set; }

        public DateTime createdate { get; set; }
    }
}
