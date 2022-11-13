using SQLite;
using System;

namespace TQM.Model
{
    public class MachineModel
    {
        [PrimaryKey]
        public Guid ID { get; set; }
        [MaxLength(50)]
        public string machineCategory { get; set; }
        public string machineName { get; set; }
        public DateTime createdate { get; set; }

        public bool dataSyncStatus { get; set; } = false;
    }
}
