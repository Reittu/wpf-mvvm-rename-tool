using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSDesktopUI.Models
{
    public class ShellModel
    {
        public class File
        {
            public string OldName { get; set; }
            public string NewName { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}
