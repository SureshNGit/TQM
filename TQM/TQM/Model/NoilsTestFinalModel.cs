using SQLite;
using System;

namespace TQM.Model
{
    public class NoilsTestFinalModel
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        public long testID { get; set; }

        public int testcount { get; set; }

        public decimal weigth_sliver { get; set; }

        public decimal weigth_noils { get; set; }

        public decimal noils { get; set; }

        public bool status { get; set; }

        public DateTime createdate { get; set; }


    }
}
