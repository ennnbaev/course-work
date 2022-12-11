using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
  public partial class AddCustomFolder : Form
  {
    public string name;

    public AddCustomFolder()
    {
      InitializeComponent();
    }

    private void Form2_Load(object sender, EventArgs e)
    {
      this.Icon = new Icon(@"D:\Курсовой проект\WindowsFormsApp1\sync.ico");
      ShowIcon = true;
    }

    private void deleteButton_Click(object sender, EventArgs e)
    {
      name = @textBox1.Text;
      Close();
    }
  }
}
