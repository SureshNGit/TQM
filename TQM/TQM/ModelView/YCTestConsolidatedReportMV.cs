namespace TQM.ModelView
{
    public class YCTestConsolidatedReportMV
    {
        public string serialNo { get; set; }
        public string testID { get; set; }
        public string machineName { get; set; }
        public string testDate { get; set; }
        public string shift { get; set; }
        public string standardValue { get; set; }
        public string testAverage { get; set; }
        public string standardDeviation { get; set; }
        public string CoEfficientOfVariation { get; set; }
        public string testDuration { get; set; }
        public string remarks { get; set; }
        public bool isWhite { get; set; } = true;
        public bool isRed { get; set; } = false;
    }
}
