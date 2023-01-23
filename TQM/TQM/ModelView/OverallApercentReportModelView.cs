using System;
using System.Collections.Generic;

namespace TQM.ModelView
{
    public class OverallApercentReportModelView : List<ApercentReportModelView>
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

        public decimal testaverage_nMinus1 { get; set; }

        public decimal testsd_nMinus1 { get; set; }

        public decimal testcv_nMinus1 { get; set; }

        //public decimal max_nMinus1 { get; set; }

        //public decimal min_nMinus1 { get; set; }

        //public decimal range_nMinus1 { get; set; }

        public decimal apercent_nMinus1 { get; set; }

        public decimal testaverage_N { get; set; }

        public decimal testsd_N { get; set; }

        public decimal testcv_N { get; set; }

        //public decimal max_N { get; set; }

        //public decimal min_N { get; set; }

        //public decimal range_N { get; set; }

        public decimal testaverage_nPlus1 { get; set; }

        public decimal testsd_nPlus1 { get; set; }

        public decimal testcv_nPlus1 { get; set; }

        //public decimal max_nPlus1 { get; set; }

        //public decimal min_nPlus1 { get; set; }

        //public decimal range_nPlus1 { get; set; }

        public decimal apercent_nPlus1 { get; set; }

        public string testRemark { get; set; }
        public DateTime createdate { get; set; }
        public List<ApercentReportModelView> apercentReportMV => this;
    }


}
