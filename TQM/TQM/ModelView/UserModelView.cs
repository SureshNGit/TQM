using System.Collections.Generic;
using TQM.Model;

namespace TQM.ModelView
{
    public class UserModelView
    {
        public int id { get { return id; } set { this.id = value; } }
        public string firstname { get { return firstname; } set { this.firstname = value; } }
        public string lastname { get { return lastname; } set { this.lastname = value; } }
        public string displayname { get { return displayname; } set { this.displayname = value; } }
        public string userId { get { return userId; } set { this.userId = value; } }
        public bool isAdmin { get { return isAdmin; } set { this.isAdmin = value; } }
        public string password { get { return password; } set { this.password = value; } }
        public bool isActive { get { return isActive; } set { this.isActive = value; } }
        public List<CompanyModel> companies { get { return companies; } set { this.companies = value; } }
    }
}
