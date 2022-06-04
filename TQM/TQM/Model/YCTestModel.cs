using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace TQM.Model
{
    public class YCTestModel
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        public string testID { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<UserModel> users { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<MachineModel> machines { get; set; }

        public string countsysname { get; set; }

        public int yarnlenunit { get; set; }

        public int yarnlength { get; set; }

        public int totaltestcount { get; set; }

        public int testcount { get; set; }

        public decimal yarnweight { get; set; }

        public decimal yccalcval { get; set; }

        public DateTime createdate { get; set; }


    }
}
