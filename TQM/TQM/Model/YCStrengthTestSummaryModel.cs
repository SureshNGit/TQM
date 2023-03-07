using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace TQM.Model
{
    public class YCStrengthTestSummaryModel
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

        public string yarnstrengthunit { get; set; }

        public decimal yarnlength { get; set; }

        public int totaltestcount { get; set; }

        public decimal testaverage { get; set; }

        public decimal testsd { get; set; }

        public decimal testcv { get; set; }

        public decimal testMax { get; set; }

        public decimal testMin { get; set; }

        public decimal testRange { get; set; }

        public int standardCSP { get; set; }

        public decimal avgStrength { get; set; }

        public decimal sdStrength { get; set; }

        public decimal cvStrength { get; set; }

        public decimal StrengthMax { get; set; }

        public decimal StrengthMin { get; set; }

        public decimal StrengthRange { get; set; }

        public decimal avgCSP { get; set; }

        public decimal sdCSP { get; set; }

        public decimal cvCSP { get; set; }

        public decimal CSPMax { get; set; }

        public decimal CSPMin { get; set; }

        public decimal CSPRange { get; set; }

        public string testRemark { get; set; }

        public DateTime createdate { get; set; }

        public bool dataSyncStatus { get; set; } = false;
    }
}
