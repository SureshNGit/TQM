using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace TQM.Model
{
    public class StretchTestCalculatedModel
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

        public string countsysname { get; set; }

        public string yarnlenunit { get; set; }

        public decimal yarnlength { get; set; }

        public string shift { get; set; }

        public string process { get; set; }

        public string testType { get; set; }

        public int totaltestcount { get; set; }

        public decimal standardStretch { get; set; }

        public decimal avg_weight_IB { get; set; }

        public decimal testaverage_IB { get; set; }

        public decimal testsd_IB { get; set; }

        public decimal testcv_IB { get; set; }

        public decimal max_IB { get; set; }

        public decimal min_IB { get; set; }

        public decimal range_IB { get; set; }

        public decimal avg_weight_FB { get; set; }

        public decimal testaverage_FB { get; set; }

        public decimal testsd_FB { get; set; }

        public decimal testcv_FB { get; set; }

        public decimal max_FB { get; set; }

        public decimal min_FB { get; set; }

        public decimal range_FB { get; set; }

        public decimal stretch { get; set; }

        public bool status { get; set; }

        public string testRemark { get; set; }

        public DateTime createdate { get; set; }

        public bool dataSyncStatus { get; set; } = false;
    }
}
