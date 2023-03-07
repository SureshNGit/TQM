using System;
using System.Collections.Generic;

namespace TQM.ModelView
{
    public class OverallCountStrengthReportModelView : List<YCStrengthTestReportModelView>
    {
        public long testID { get; set; }
        public string userName { get; set; }
        public string machineCategory { get; set; }
        public string machineName { get; set; }
        public string shift { get; set; }
        public string process { get; set; }
        public string countsysname { get; set; }
        public string yarnlenunit { get; set; }
        public string yarnstrengthunit { get; set; }
        public decimal yarnlength { get; set; }
        public int totaltestcount { get; set; }
        public decimal testaverage { get; set; }
        public int standardCSP { get; set; }
        public decimal testsd { get; set; }
        public decimal testcv { get; set; }
        public string testRemark { get; set; }
        public decimal avgCSP { get; set; }
        public decimal sdCSP { get; set; }
        public decimal cvCSP { get; set; }
        public DateTime createdate { get; set; }
        public List<YCStrengthTestReportModelView> ycStrengthTestlist => this;
    }


}
