using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace TQM.Model
{
    public class TestResumeCheck
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        public long testID { get; set; }

        public int testcount { get; set; }

        public string testType { get; set; }

        [ForeignKey(typeof(UserModel))]
        public Guid userID { get; set; }

        public string userName { get; set; }

        public DateTime createdate { get; set; }

        public bool dataSyncStatus { get; set; } = false;


    }
}
