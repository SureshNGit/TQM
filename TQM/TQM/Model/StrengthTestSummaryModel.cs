using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace TQM.Model
{
    public class StrengthTestSummaryModel
    {
        [PrimaryKey]
        public Guid ID { get; set; }
        [ForeignKey(typeof(StrengthTestModel))]
        public long testID { get; set; }
        [ForeignKey(typeof(UserModel))]
        public Guid userID { get; set; }
        public string userName { get; set; }
        [ForeignKey(typeof(MachineModel))]
        public Guid machineID { get; set; }
        public string machineCategory { get; set; }
        public string machineName { get; set; }
        public int speed { get; set; }
        public decimal p1 { get; set; }
        public decimal p1Deviation { get; set; }
        public decimal p2 { get; set; }
        public decimal p2Deviation { get; set; }
        public decimal n1 { get; set; }
        public decimal n1Deviation { get; set; }
        public int sectionNumber { get; set; }
        public string totalDrumNumbers { get; set; }
        public int drumNumber { get; set; }
        public decimal standardStrength { get; set; }
        public decimal strengthDeviation { get; set; }
        public int belowLimit { get; set; }
        public int qualifiedTestCount { get; set; }
        public int totalTestCount { get; set; }
        public string drumSelectionMethod { get; set; }
        public decimal yarnStrength { get; set; }
        public int maxRollingCount { get; set; }
        public string materialCount { get; set; }
        public string shift { get; set; }
        public string testRemark { get; set; }
        public string testDuration { get; set; }
        public string uf_value_1 { get; set; }
        public string uf_value_2 { get; set; }
        public string uf_value_3 { get; set; }
        public string uf_value_4 { get; set; }
        public DateTime createdate { get; set; }
        public DateTime scheduledStartDate { get; set; }
        public DateTime scheduledEndDate { get; set; }
        public DateTime settingsUpdatedDate { get; set; }
        public bool dataSyncStatus { get; set; } = false;
    }
}
