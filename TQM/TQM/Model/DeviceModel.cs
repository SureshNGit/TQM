using SQLite;

namespace TQM.Model
{
    public class deviceModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [MaxLength(50)]
        public string deviceName { get; set; }
    }
}
