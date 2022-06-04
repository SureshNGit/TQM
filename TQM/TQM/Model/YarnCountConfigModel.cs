using SQLite;

namespace TQM.Model
{
    public class YarnCountConfigModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string countsysname { get; set; }

        public int yarnlenunit { get; set; }

        public int yarnlength { get; set; }

        public int testcount { get; set; }


    }
}
