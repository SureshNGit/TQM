using System;
using System.Collections.Generic;

namespace TQM.ModelView
{
    public class OverallStretchReportModelView : List<StretchReportModelView>
    {
        public long testID { get; set; }
        public string userName { get; set; }

        public string machineCategory { get; set; }
        public string machineName { get; set; }
        public string countsysname { get; set; }

        public string yarnlenunit { get; set; }

        public decimal yarnlength { get; set; }

        public string shift { get; set; }

        public string process { get; set; }

        public string testType { get; set; }

        public int totaltestcount { get; set; }
        public decimal standardStretch { get; set; }
        public decimal stretchDeviation { get; set; }

        public bool isGREEN { get; set; } = true;

        public bool isRED { get; set; } = false;

        public decimal testaverage_IB { get; set; }

        public decimal testsd_IB { get; set; }

        public decimal testcv_IB { get; set; }

        public decimal testaverage_FB { get; set; }

        public decimal testsd_FB { get; set; }

        public decimal testcv_FB { get; set; }

        public decimal stretch { get; set; }

        public string testRemark { get; set; }
        public DateTime createdate { get; set; }
        public List<StretchReportModelView> stretchReportMV => this;
    }


}
