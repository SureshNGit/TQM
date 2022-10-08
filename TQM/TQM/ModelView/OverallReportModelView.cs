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
        public decimal testsd { get; set; }
        public decimal testcv { get; set; }
        public DateTime createdate { get; set; }
        public List<YCTestModel> yctestlist => this;
    }


}
