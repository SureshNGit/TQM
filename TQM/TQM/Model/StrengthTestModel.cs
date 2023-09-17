using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace TQM.Model
{
    public class StrengthTestModel
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
        public int sectionNumber { get; set; }
        public string totalDrumNumbers { get; set; }
        public int drumNumber { get; set; }
        public decimal standardStrength { get; set; }
        public decimal strengthDeviation { get; set; }
        public int belowLimit { get; set; }
        public int totalTestCount { get; set; }
        public string drumSelectionMethod { get; set; }
        public int sampleNo { get; set; }
        public int sampleStrengthCount { get; set; }
        public string isQualified { get; set; }
        public string shift { get; set; }
        public DateTime createdate { get; set; }
        public DateTime scheduledStartDate { get; set; }
        public DateTime scheduledEndDate{ get; set; }
        public DateTime settingsUpdatedDate { get; set; }
        public bool dataSyncStatus { get; set; } = false;
    }
}
