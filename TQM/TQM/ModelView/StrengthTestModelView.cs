using System;

namespace TQM.ModelView
{
    public class StrengthTestModelView
    {
        public long testID { get; set; }
        public Guid userID { get; set; }
        public string userName { get; set; }
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
        public DateTime scheduledStartDate { get; set; }
        public DateTime scheduledEndDate { get; set; }
    }
}
