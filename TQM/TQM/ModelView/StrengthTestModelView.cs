using System;

namespace TQM.ModelView
{
    public class StrengthTestModelView
    {
        public long testID { get; set; }
        public Guid userID { get; set; }
        public string userName { get; set; }
        public Guid categoryID { get; set; }
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
        public int overallDrumCount { get; set; }
        public int overallSections { get; set; }
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
        public int maxRollingCount { get; set; }
        public string materialCount { get; set; }
        public string shift { get; set; }
        public DateTime scheduledStartDate { get; set; }
        public DateTime scheduledEndDate { get; set; }
    }
}
