using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    class Core
    {
        public static StreamingDBEntities Context = new StreamingDBEntities();
        public static Users LoggedUser { get; set; }
    }
}