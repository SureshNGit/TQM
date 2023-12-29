using System;
namespace TQM.ModelView
{
	public class IndividualTestMVReport
    {
        public bool isHeader { get; set; } = false;
        public bool isBody { get; set; } = false;
        public string Header { get; set; }
        public string T1 { get; set; }
        public string T2 { get; set; }
        public string T3 { get; set; }
        public string T4 { get; set; }
        public string T5 { get; set; }
        public string T6 { get; set; }
        public string T7 { get; set; }
        public string T8 { get; set; }
        public string T9 { get; set; }
        public string T10 { get; set; }
        public string QT { get; set; }
        public string ST { get; set; }
        public string DrumNumber { get; set; }
        public string Speed { get; set; }
        public string P1 { get; set; }
        public string P2 { get; set; }
        public string N1 { get; set; }
        public string Mat_Count { get; set; }
        public string ST_BG_Color { get; set; } = "red";
    }
}

