using System;

namespace TQM.ModelView
{
    public class UserModelView
    {
        public Guid ID { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string displayname { get; set; }
        public string userId { get; set; }
        public bool isAdmin { get; set; }
        public string password { get; set; }
        public bool isActive { get; set; }
        public Guid companyID { get; set; }
    }
}
