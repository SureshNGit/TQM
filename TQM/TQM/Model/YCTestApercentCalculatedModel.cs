using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace TQM.Model
{
    public class YCTestApercentCalculatedModel
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        [ForeignKey(typeof(YCTestModel))]
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

        public decimal testaverage { get; set; }

        public decimal testsd { get; set; }

        public decimal testcv { get; set; }

        public decimal testaverage_N { get; set; }

        public decimal testsd_N { get; set; }

        public decimal testcv_N { get; set; }

        public decimal apercent { get; set; }

        public bool status { get; set; }

        public DateTime createdate { get; set; }
    }
}
