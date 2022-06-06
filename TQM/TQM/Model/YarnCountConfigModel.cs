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

        public int yarnlength { get; set; }

        public int testcount { get; set; }


    }
}
