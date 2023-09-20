using System;
namespace TQM.ModelView
{
	public class MaintenanceReportMV
	{
        public string serialNo { get; set; }
        public string date { get; set; }
        public string machineName { get; set; }
        public string speed { get; set; }
        public string p1 { get; set; }
        public string p1_bgcolor { get; set; } = "green";
        public string p1_textcolor { get; set; } = "black";
        public string p2 { get; set; }
        public string p2_bgcolor { get; set; } = "green";
        public string p2_textcolor { get; set; } = "black";
        public string n1 { get; set; }
        public string n1_bgcolor { get; set; } = "green";
        public string n1_textcolor { get; set; } = "black";
        public string remark { get; set; }

    }
}

