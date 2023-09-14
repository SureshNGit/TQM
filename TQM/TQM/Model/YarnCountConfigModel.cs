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

        public int totalDrumCount { get; set; }

        public int totalSections { get; set; }

        public decimal stdRollingStrength_s1 { get; set; }

        public decimal strengthDeviation_s1 { get; set; }

        public int belowLimit_s1 { get; set; }

        public int totalSamples_s1 { get; set; }

        public string drumNumbers_s1 { get; set; }

        public string drumSelectionMethod_s1 { get; set; }

        public int scheduledDayLimit_s1 { get; set; }

        public DateTime scheduledDayLimitDate_s1 { get; set; }

        public decimal stdRollingStrength_s2 { get; set; }

        public decimal strengthDeviation_s2 { get; set; }

        public int belowLimit_s2 { get; set; }

        public int totalSamples_s2 { get; set; }

        public string drumNumbers_s2 { get; set; }

        public string drumSelectionMethod_s2 { get; set; }

        public int scheduledDayLimit_s2 { get; set; }

        public DateTime scheduledDayLimitDate_s2 { get; set; }

        public decimal stdRollingStrength_s3 { get; set; }

        public decimal strengthDeviation_s3 { get; set; }

        public int belowLimit_s3 { get; set; }

        public int totalSamples_s3 { get; set; }

        public string drumNumbers_s3 { get; set; }

        public string drumSelectionMethod_s3 { get; set; }

        public int scheduledDayLimit_s3 { get; set; }

        public DateTime scheduledDayLimitDate_s3 { get; set; }

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
        public DateTime updateddate { get; set; } 
        public DateTime createdate { get; set; } 
        public bool dataSyncStatus { get; set; } = false;


    }
}
