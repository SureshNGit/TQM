using System;
using System.Collections.Generic;
using TQM.Model;

namespace TQM.ModelView
{
    public class OverallReportModelView : List<StrengthTestModel>
    {
        public long testID { get; set; }
        public string userName { get; set; }
        public string machineCategory { get; set; }
        public string machineName { get; set; }
        public int drumNumber { get; set; }
        public decimal standardStrength { get; set; }
        public decimal strengthDeviation { get; set; }
        public int belowLimit { get; set; }
        public int maxRollingCount { get; set; }
        public int totalTestCount { get; set; }
        public string drumSelectionMethod { get; set; }
        public decimal yarnStrength { get; set; }
        public string shift { get; set; }
        public string testRemark { get; set; }
        public bool isIndividualReport { get; set; } = true;
        public bool isConsolidatedReport { get; set; } = false;
        public string testDuration { get; set; }
        public string deviationPercent { get; set; }
        public string testResult_BG_Color { get; set; } = "white";
        public string testResultColor { get; set; } = "green";
        public bool DispUF_1 { get; set; } = false;
        public bool DispUF_2 { get; set; } = false;
        public bool DispUF_2_Col1 { get; set; } = false;
        public bool DispUF_3 { get; set; } = false;
        public bool DispUF_4 { get; set; } = false;
        public bool DispUF_4_Col1 { get; set; } = false;
        public string uf_name_1 { get; set; } = null;
        public string uf_name_2 { get; set; } = null;
        public string uf_name_3 { get; set; } = null;
        public string uf_name_4 { get; set; } = null;
        public string uf_value_1 { get; set; } = null;
        public string uf_value_2 { get; set; } = null;
        public string uf_value_3 { get; set; } = null;
        public string uf_value_4 { get; set; } = null;
        public bool remark_1 { get; set; } = false;
        public bool remark_2 { get; set; } = false;
        public bool remark_3 { get; set; } = false;
        public DateTime createdate { get; set; }
        public string scheduledStartDate { get; set; }
        public string scheduledEndDate { get; set; }
        public DateTime settingsUpdatedDate { get; set; }
        public List<StrengthTestModel> strengthtestlist => this;
    }


}
