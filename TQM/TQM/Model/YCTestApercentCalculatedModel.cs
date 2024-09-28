using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace TQM.Model
{
    public class YCTestApercentCalculatedModel
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
        public decimal standardApercent { get; set; }

        public decimal avg_weight_nMinus1 { get; set; }

        public decimal testaverage_nMinus1 { get; set; }

        public decimal testsd_nMinus1 { get; set; }

        public decimal testcv_nMinus1 { get; set; }

        public decimal max_nMinus1 { get; set; }

        public decimal min_nMinus1 { get; set; }

        public decimal range_nMinus1 { get; set; }

        public decimal apercent_nMinus1 { get; set; }

        public decimal avg_weight_N { get; set; }

        public decimal testaverage_N { get; set; }

        public decimal testsd_N { get; set; }

        public decimal testcv_N { get; set; }

        public decimal max_N { get; set; }

        public decimal min_N { get; set; }

        public decimal range_N { get; set; }

        public decimal avg_weight_nPlus1 { get; set; }

        public decimal testaverage_nPlus1 { get; set; }

        public decimal testsd_nPlus1 { get; set; }

        public decimal testcv_nPlus1 { get; set; }

        public decimal max_nPlus1 { get; set; }

        public decimal min_nPlus1 { get; set; }

        public decimal range_nPlus1 { get; set; }

        public decimal apercent_nPlus1 { get; set; }

        public string testRemark { get; set; }

        public decimal standardCV { get; set; }

        public decimal CVDeviationPercent { get; set; }

        public string uf_value_1 { get; set; }
        public string uf_value_2 { get; set; }
        public string uf_value_3 { get; set; }
        public string uf_value_4 { get; set; }

        public bool status { get; set; }

        public DateTime createdate { get; set; }

        public bool dataSyncStatus { get; set; } = false;
    }
}
