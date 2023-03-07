using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace TQM.Model
{
    public class YCStrengthTestModel
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        public long testID { get; set; }

        [ForeignKey(typeof(UserModel))]
        public Guid userID { get; set; }

        public string userName { get; set; }

        [ForeignKey(typeof(MachineModel))]
        public Guid machineID { get; set; }

        public string machineCategory { get; set; }
        public string machineName { get; set; }

        public string shift { get; set; }

        public string process { get; set; }
        public string countsysname { get; set; }

        public string yarnlenunit { get; set; }

        public string yarnstrengthunit { get; set; }

        public decimal yarnlength { get; set; }

        public int totaltestcount { get; set; }

        public int testcount { get; set; }

        public decimal yarnweight { get; set; }

        public decimal yccalcval { get; set; }

        public int standardCSP { get; set; }

        public decimal yarnstrength { get; set; }

        public int CSP { get; set; }

        public DateTime createdate { get; set; }

        public bool dataSyncStatus { get; set; } = false;


    }
}
