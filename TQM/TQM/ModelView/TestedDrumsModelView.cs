using System;
using System.Collections.Generic;
using TQM.Model;

namespace TQM.ModelView
{
	public class TestedDrumsModelView
	{
        public Guid machineID { get; set; }
        public int totalDrumCount { get; set; }
        public int totalSections { get; set; }
        public string scheduledStartDate { get; set; }
        public string scheduledEndDate { get; set; }
        public TestConfigModel tcm { get; set; }
    } 
}