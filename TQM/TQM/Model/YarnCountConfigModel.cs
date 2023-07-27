using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace TQM.Model
{
    public class YarnCountConfigModel
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        [ForeignKey(typeof(MachineModel))]
        public Guid machineID { get; set; }

        public string machineCategory { get; set; }
        public string machineName { get; set; }

        public string countsysname { get; set; }

        public string yarnlenunit { get; set; }

        public int lealength { get; set; }
        public int sliverlength { get; set; }

        public int rovinglength { get; set; }

        public int testcount { get; set; }

        public decimal standardHank { get; set; }

        public decimal deviationPercent { get; set; }

        public int testcountApercent { get; set; }
        public decimal standardApercent { get; set; }
        public int testcountStretch { get; set; }
        public decimal standardStretch { get; set; }
        public decimal stretchDeviation { get; set; }
        public int testcountNoils { get; set; }
        public decimal standardNoils { get; set; }
        public decimal noilsRange { get; set; }
        public int shiftCount { get; set; }

        public string shift1time { get; set; }

        public string shift2time { get; set; }

        public string shift3time { get; set; }

        public string uf_name_1 { get; set; }

        public string uf_value_1 { get; set; }

        public string uf_name_2 { get; set; }

        public string uf_value_2 { get; set; }

        public string uf_name_3 { get; set; }

        public string uf_value_3 { get; set; }

        public string uf_name_4 { get; set; }

        public string uf_value_4 { get; set; }

        public DateTime createdate { get; set; } = DateTime.Now;
        public bool dataSyncStatus { get; set; } = false;


    }
}
