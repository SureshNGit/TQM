using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace TQM.Model
{
    public class MachineModel
    {
        [PrimaryKey]
        public Guid ID { get; set; }
        [ForeignKey(typeof(CategoryModel))]
        public Guid categoryID { get; set; }
        [MaxLength(50)]
        public string machineCategory { get; set; }
        public string machineName { get; set; }
        public string macSerialNo { get; set; }
        public DateTime createdate { get; set; }

        public bool dataSyncStatus { get; set; } = false;
    }
}
