using System;
using System.Collections.Generic;

namespace TQM.ModelView
{
    public class OverallNoilsReportModelView : List<NoilsReportModelView>
    {
        public long testID { get; set; }
        public string userName { get; set; }
        public string machineCategory { get; set; }
        public string machineName { get; set; }
        public string shift { get; set; }
        public string process { get; set; }
        public string testType { get; set; }
        public string countsysname { get; set; }
        public string yarnlenunit { get; set; }
        public decimal yarnlength { get; set; }
        public int totaltestcount { get; set; }
        public decimal standardNoils { get; set; }
        public decimal noilsRange { get; set; }
        public bool isGREEN { get; set; } = true;

        public bool isRED { get; set; } = false;
        public decimal average_wt_sliverwt { get; set; }

        public decimal max_sliverwt { get; set; }

        public decimal min_sliverwt { get; set; }

        public decimal range_sliverwt { get; set; }

        public decimal testaverage_sliverwt { get; set; }

        public decimal testsd_sliverwt { get; set; }

        public decimal testcv_sliverwt { get; set; }

        public decimal average_wt_noilswt { get; set; }

        public decimal max_noilswt { get; set; }

        public decimal min_noilswt { get; set; }

        public decimal range_noilswt { get; set; }

        public decimal testaverage_noilswt { get; set; }

        public decimal testsd_noilswt { get; set; }

        public decimal testcv_noilswt { get; set; }

        public decimal average_wt_noils { get; set; }

        public decimal max_noils { get; set; }

        public decimal min_noils { get; set; }

        public decimal range_noils { get; set; }

        //public decimal testaverage_noils { get; set; }

        public decimal testsd_noils { get; set; }

        public decimal testcv_noils { get; set; }

        public string testRemark { get; set; }

        public DateTime createdate { get; set; }
        public List<NoilsReportModelView> noilsTestList => this;
    }


}
