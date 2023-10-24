using System;
using System.Collections.Generic;
using TQM.Model;

namespace TQM.ModelView
{
	//public class MissingDrumReportModelView: List<DrumDetailsModelView>
    public class MissingDrumReportModelView
    {
        public Guid categoryID { get; set; }
        //public string machineCategory { get; set; }
        public Guid machineID { get; set; }
        public string machineName { get; set; }
        public int totalDrumCount { get; set; }
        public int totalSections { get; set; }
        public int sectionNumber { get; set; }
        public string totalDrumNumbers { get; set; }
        public string testCompletedDrums { get; set; }
        public string pendingTestDrums { get; set; }
        public string pendingTestDrumsColor { get; set; } = "green";
        public string pendingTestDrumsTextColor { get; set; } = "black";
        public string scheduledStartDate { get; set; }
        public string scheduledEndDate { get; set; }
        public string settingsUpdatedDate { get; set; }
        public string remark { get; set; }
        //public List<DrumDetailsModelView> drumDetailsListView => this;
    }
}

