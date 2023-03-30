using System;
using System.Collections.Generic;
using TQM.Model;

namespace TQM.ModelView
{
    public class OverallReportModelView : List<YCTestModel>
    {
        public long testID { get; set; }
        public string userName { get; set; }
        public string machineCategory { get; set; }
        public string machineName { get; set; }
        public string shift { get; set; }
        public string process { get; set; }
        public string countsysname { get; set; }
        public string yarnlenunit { get; set; }
        public decimal yarnlength { get; set; }
        public int totaltestcount { get; set; }
        public decimal testaverage { get; set; }
        public decimal standardHank { get; set; }
        public decimal testsd { get; set; }
        public decimal testcv { get; set; }
        public string testRemark { get; set; }
        public bool isSpinning { get; set; } = false;
        public bool otherThanSpinning { get; set; } = false;
        public bool isIndividualReport { get; set; } = true;
        public bool isConsolidatedReport { get; set; } = false;
        public string testDuration { get; set; }
        public string deviationPercent { get; set; }
        public string hankColor { get; set; } = "Green";
        public string hankColorGg { get; set; } = "White";
        public DateTime createdate { get; set; }
        public List<YCTestModel> yctestlist => this;
    }


}
