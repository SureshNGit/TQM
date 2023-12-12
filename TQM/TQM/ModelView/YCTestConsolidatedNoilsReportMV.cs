namespace TQM.ModelView
{
    public class YCTestConsolidatedNoilsReportMV
    {
        public string serialNo { get; set; }
        public string testID { get; set; }
        public string testNo { get; set; }
        public string machineName { get; set; }
        public string testDate { get; set; }
        public string shift { get; set; }
        public string standardValue { get; set; }
        public string noilsAvgWeight { get; set; }
        public string standardDeviation { get; set; }
        public string CoEfficientOfVariation { get; set; }
        public string testDuration { get; set; }
        public string remarks { get; set; }
        public bool isWhite { get; set; } = true;
        public bool isRed { get; set; } = false;
        public string uf_name_1 { get; set; } = null;
        public string uf_name_2 { get; set; } = null;
        public string uf_name_3 { get; set; } = null;
        public string uf_name_4 { get; set; } = null;
        public string uf_value_1 { get; set; } = null;
        public string uf_value_2 { get; set; } = null;
        public string uf_value_3 { get; set; } = null;
        public string uf_value_4 { get; set; } = null;
    }
}
