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

        public string yarnStrengthUnit { get; set; }

        public int yarnLength { get; set; }

        public int testcount { get; set; }

        public decimal standardHank { get; set; }

        public decimal standardCSP { get; set; }

        public int shiftCount { get; set; }

        public string shift1time { get; set; }

        public string shift2time { get; set; }

        public string shift3time { get; set; }

        public bool dataSyncStatus { get; set; } = false;


    }
}
