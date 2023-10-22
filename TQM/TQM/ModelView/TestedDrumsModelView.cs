using System;
using System.Collections.Generic;
using TQM.Model;

namespace TQM.ModelView
{
	public class TestedDrumsModelView
	{
        public Guid machineID { get; set; }
        //public string machineCategory { get; set; }
        //public string machineName { get; set; }
        public int overallDrumCount { get; set; }
        public int overallSections { get; set; }
        public string scheduledStartDate { get; set; }
        public string scheduledEndDate { get; set; }
        //public TestConfigModel tcm { get; set; }
    } 
}