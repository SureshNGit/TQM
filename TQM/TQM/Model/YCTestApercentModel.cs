using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace TQM.Model
{
    public class YCTestApercentModel
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        public long testID { get; set; }

        [ForeignKey(typeof(UserModel))]
        public Guid userID { get; set; }

        public string userName { get; set; }

        public string countsysname { get; set; }

        public string yarnlenunit { get; set; }

        public decimal yarnlength { get; set; }

        public string shift { get; set; }

        public string process { get; set; }

        public string testType { get; set; }

        public int totaltestcount { get; set; }

        public int testcount { get; set; }

        public decimal yarnweight { get; set; }

        public decimal yccalcval { get; set; }

        public bool status { get; set; }

        public DateTime createdate { get; set; }


    }
}
