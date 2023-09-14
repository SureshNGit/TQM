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
        public List<StrengthTestModel> yctestlist => this;
    }


}
