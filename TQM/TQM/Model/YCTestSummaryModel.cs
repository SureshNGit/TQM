using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace TQM.Model
{
    public class YCTestSummaryModel
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        [ForeignKey(typeof(YCTestModel))]
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

        public decimal yarnlength { get; set; }

        public int totaltestcount { get; set; }

        public decimal testaverage { get; set; }

        public decimal testsd { get; set; }

        public decimal testcv { get; set; }

        public DateTime createdate { get; set; }
    }
}
