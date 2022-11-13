using SQLite;
using System;

namespace TQM.Model
{
    public class YarnCountConfigModel
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        public string countsysname { get; set; }

        public string yarnlenunit { get; set; }

        public int sliverlength { get; set; }

        public int rovinglength { get; set; }

        public int testcount { get; set; }

        public decimal standardHank { get; set; }

        public int testcountApercent { get; set; }

        public int testcountStretch { get; set; }

        public int testcountNoils { get; set; }

        public bool dataSyncStatus { get; set; } = false;


    }
}
