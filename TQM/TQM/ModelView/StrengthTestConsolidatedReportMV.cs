using System;

namespace TQM.ModelView
{
    public class StrengthTestConsolidatedReportMV
    {
        public string serialNo { get; set; }
        public string testID { get; set; }
        public string testDate { get; set; }
        public string machineName { get; set; }
        public string shift { get; set; }
        public string drumNumber { get; set; }
        public string drumSelectionMethod { get; set; }
        public string belowLimit { get; set; }
        public string totalTestCount { get; set; }
        public string qualifiedTestCount { get; set; }
        public string standardValue { get; set; }
        public string deviation { get; set; }
        public string actualStrength { get; set; }
        public string strength { get; set; }
        public string testDuration { get; set; }
        public string remarks { get; set; }
        public string strengthColor { get; set; } = "green";
        public string strengthColor_text { get; set; } = "black";
        public string testIDColor { get; set; } = "green";
        public string testIDColor_text { get; set; } = "black";
    }
}
