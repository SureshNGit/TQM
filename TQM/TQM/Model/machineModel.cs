using SQLite;

namespace TQM.Model
{
    public class machineModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [MaxLength(50)]
        public string machineName { get; set; }
    }
}
