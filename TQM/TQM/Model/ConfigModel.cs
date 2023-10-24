using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace TQM.Model
{
    public class ConfigModel
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        [ForeignKey(typeof(CategoryModel))]
        public Guid categoryID { get; set; }

        [ForeignKey(typeof(MachineModel))]
        public Guid machineID { get; set; }

        public string machineCategory { get; set; }

        public string machineName { get; set; }

        public int speed { get; set; }

        public decimal p1 { get; set; }

        public decimal p1Deviation { get; set; }

        public decimal p2 { get; set; }

        public decimal p2Deviation { get; set; }

        public decimal n1 { get; set; }

        public decimal n1Deviation { get; set; }

        public int totalDrumCount { get; set; }

        public int totalSections { get; set; }

        public decimal stdRollingStrength { get; set; }

        public decimal strengthDeviation { get; set; }

        public int belowLimit { get; set; }

        public int maxLimit { get; set; }

        public int totalSamples { get; set; }

        public DateTime scheduledStartDate { get; set; }

        public DateTime scheduledEndDate { get; set; }

        public string materialCount { get; set; }

        public string drumNumbers_s1 { get; set; }

        public string drumNumbers_s2 { get; set; }

        public string drumNumbers_s3 { get; set; }

        public string drumNumbers_s4 { get; set; }

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
