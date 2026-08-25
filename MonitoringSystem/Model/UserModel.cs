using MonitoringSystem.Base;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringSystem.Model
{
    public class UserModel : NotifyPropertyBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        // public string Password { get; set; }
        private string _password;

        public string Password
        {
            get { return _password; }
            set { _password = value;RaisePropertyChanged(); }
        }


        //  public string UserName { get; set; }

        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set { _userName = value;RaisePropertyChanged(); }
        }

        public bool Status { get; set; }

        public bool Sex { get; set; }

        public string CreateTime { get; set; }

        public string UpdateTime { get; set; }
    }
}
