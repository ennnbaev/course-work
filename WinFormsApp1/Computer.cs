using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
enum check { active, close };

namespace WindowsFormsApp1
{
  class Computer
  {
    public string name { get; set; }
    public check status { get; set; }
    List<String> folders = new List<String>();

    public List<String> getFolders()
    {
      if (status == check.active)
        return folders;
      return null;
    }

    public Computer(string _name)
    {
      name = _name;
      status = check.active;
      UpdateFolders();
    }

    public void UpdateFolders()
    {
      GetNetShares shares = new GetNetShares();
      GetNetShares.SHARE_INFO_1[] st = shares.EnumNetShares(name);
      foreach (var n in st)
        if (n.shi1_type == 0) folders.Add(n.shi1_netname);
    }
  }
}
