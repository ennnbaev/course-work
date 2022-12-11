using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
enum operation { Add, Delete, Update, AddFolder };

namespace WindowsFormsApp1
{
  class Log
  {
    DateTime _date;
    public operation _op { get; set; }
    public string _adress { get; set; }

    public Log(operation op, string adress)
    {
      _date = DateTime.Now;
      _op = op;
      _adress = adress;
    }

    public override string ToString()
    {
      return (_date.ToString() + "; " + _adress + "; " + _op);
    }
  }
}

