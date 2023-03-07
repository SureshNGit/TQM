using System;
using System.Collections.Generic;

namespace TQM.ModelView
{
    public class OverallReportModelView : List<YCTestReportModelView>
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
        public decimal testsd { get; set; }
        public decimal testcv { get; set; }
        public decimal minTest { get; set; }
        public decimal maxTest { get; set; }
        public decimal rangeTest { get; set; }
        public decimal avgWeight { get; set; }
        public decimal sdWeight { get; set; }
        public decimal cvWeight { get; set; }
        public decimal minWeight { get; set; }
        public decimal maxWeight { get; set; }
        public decimal rangeWeight { get; set; }
        public decimal standardCount { get; set; }
        public string testRemark { get; set; }
        public DateTime createdate { get; set; }
        public List<YCTestReportModelView> yctestlist => this;
    }


}
