using System;
using System.Collections.Generic;

namespace TQM.ModelView
{
	//public class MissingDrumReportModelView: List<DrumDetailsModelView>
    public class MissingDrumReportModelView
    {
        public string machineCategory { get; set; }
        public string machineName { get; set; }
        public int sectionNumber { get; set; }
        public string totalDrumNumbers { get; set; }
        public string testCompletedDrums { get; set; }
        public string pendingTestDrums { get; set; }
        public DateTime scheduledStartDate { get; set; }
        public DateTime scheduledEndDate { get; set; }
        public DateTime settingsUpdatedDate { get; set; }
        public string remark { get; set; }
        //public List<DrumDetailsModelView> drumDetailsListView => this;
    }
}

