using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExtremeMirror;
using System.IO;
using System.Diagnostics;

namespace WindowsFormsApp1
{
  public partial class SyncProgram : Form
  {
    AddComputer newComp = new AddComputer();
    AddCustomFolder newFolder = new AddCustomFolder();
    List<Computer> computers = new List<Computer>();
    private BindingSource _computersBinding;
    private BindingSource _foldersBinding;

    Folder baseFolder;

    private List<Folder> syncFolders = new List<Folder>();

    public SyncProgram()
    {

      InitializeComponent();

      if (Directory.Exists("Base")) Directory.Delete("Base", true);
      Directory.CreateDirectory("Base");
      baseFolder = new Folder("Base");

      timer1.Interval = 60000;
      timer1.Start();

      ReadComputersFromFile();
      _computersBinding = new BindingSource();
      _computersBinding.DataSource = computers;
      dataGridView1.DataSource = _computersBinding;
      dataGridView1.RowHeaderMouseDoubleClick += dataGridView1_RowHeaderMouseDoubleClick;

      _foldersBinding = new BindingSource();

      this.Closing += Syncronization_program_Closing;
    }
    private void ReadComputersFromFile()
    {
      using (StreamReader sr = new StreamReader("computers.txt", System.Text.Encoding.Default))
      {
        string line;
        while ((line = sr.ReadLine()) != null)
        {
          GetNetShares shares = new GetNetShares();
          GetNetShares.SHARE_INFO_1[] st = shares.EnumNetShares(line);
          if (st[0].shi1_type == 10) return;
          else computers.Add(new Computer(line));
        }
      }
    }

    private void dataGridView1_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
    {
      listBox1.DataSource = _foldersBinding;
      _foldersBinding.DataSource = computers[e.RowIndex].getFolders();
      _foldersBinding.ResetBindings(true);
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (newComp.IsDisposed)
        newComp = new AddComputer();
      if (!newComp.Visible)
        newComp.ShowDialog();
      else
        newComp.Activate();

      foreach (var v in computers)
        if (v.name == newComp.name)
        {
          MessageBox.Show("Уже существует");
          return;
        }
      string s = PinvokeWindowsNetworking.connectToRemote(newComp.name, "", "", true);
      if (s != null)
      {
        MessageBox.Show(s);
        return;
      }
      else
      {
        computers.Add(new Computer(newComp.name));
      }
      _computersBinding.ResetBindings(true);
    }

    private void button2_Click(object sender, EventArgs e)
    {
      if (dataGridView1.CurrentCell == null)
      {
        MessageBox.Show("Еще не был добавлен ни один компьютер!");
        return;
      }
      SetClose(dataGridView1.CurrentCell.RowIndex);
      computers.RemoveAt(dataGridView1.CurrentCell.RowIndex);
      dataGridView1.CurrentCell.Dispose();
      _foldersBinding.DataSource = null;
      _computersBinding.ResetBindings(true);
      _foldersBinding.ResetBindings(true);
    }

    private void button6_Click(object sender, EventArgs e)
    {
      if ((dataGridView1.CurrentCell == null) || (listBox1.SelectedItem == null))
      {
        MessageBox.Show("Выберите компьютер и папку");
        return;
      }
      for (int i = 0; i < syncFolders.Count(); i++)
        if (syncFolders[i].path == @computers[dataGridView1.CurrentCell.RowIndex].name + @"\" + listBox1.SelectedItem)
        {
          MessageBox.Show("Папка уже добавлена");
          return;
        }
      syncFolders.Add(new Folder(@computers[dataGridView1.CurrentCell.RowIndex].name + @"\" + listBox1.SelectedItem));
    }

    private void Syncronization_program_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      LogComputers();
    }

    private void LogComputers()
    {
      using (StreamWriter sw = new StreamWriter("computers.txt", false, System.Text.Encoding.Default))
      {
        foreach (var v in computers)
          sw.WriteLine(v.name);
      }
    }

    private void button5_Click(object sender, EventArgs e)
    {
      if (newFolder.IsDisposed)
        newFolder = new AddCustomFolder();
      if (!newFolder.Visible)
        newFolder.ShowDialog();
      else
        newFolder.Activate();

      for (int i = 0; i < syncFolders.Count(); i++)
        if (syncFolders[i].path == newFolder.name)
        {
          MessageBox.Show("Папка уже добавлена");
          return;
        }
      if (Directory.Exists(newFolder.name)) syncFolders.Add(new Folder(newFolder.name));
      else
      {
        MessageBox.Show("Такой папки не существует");
        return;
      }
    }

    private void button4_Click(object sender, EventArgs e)
    {
      foreach (var v in syncFolders)
        if (v.path != listBox1.SelectedItem)
        {
          syncFolders.RemoveAt(listBox1.SelectedIndex);
          _foldersBinding.ResetBindings(true);
          return;
        }
      MessageBox.Show("Выберите папку");
    }

    private void button3_Click(object sender, EventArgs e)
    {
      listBox1.DataSource = _foldersBinding;
      _foldersBinding.DataSource = syncFolders;
      listBox1.DisplayMember = "Path";
      _foldersBinding.ResetBindings(true);
    }

    private void Ping()
    {
      for (int i = 0; i < computers.Count; i++)
      {
        GetNetShares shares = new GetNetShares();
        GetNetShares.SHARE_INFO_1[] st = shares.EnumNetShares(computers[i].name);
        if (st[0].shi1_type == 10) SetClose(i);
        else computers[i].status = check.active;
      }
      _computersBinding.ResetBindings(true);
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      Ping();
      Synchronization();
    }

    private void Synchronization()
    {
      if ((syncFolders.Count == 1) || (syncFolders.Count == 0)) return;
      foreach (var v in syncFolders)
      {
        v.Update();
      }
      foreach (var v in syncFolders)
      {
        if (v.isChecked == false) v.FirstSync(baseFolder);
        else
        {
          v.SyncAdd(baseFolder);
          v.SyncUpdate(baseFolder);
          v.SyncDel(baseFolder);
        }
      }

      baseFolder.Update();

      foreach (var v in syncFolders)
        v.CopyFrom(baseFolder);
    }

    private void SetClose(int index)
    {
      for (int i = 0; i < syncFolders.Count; i++)
      {
        if (syncFolders[i].path.StartsWith(computers[index].name)) syncFolders.RemoveAt(i);
      }
      computers[index].status = check.close;
    }

    private void logAddToolMainMenuItem_Click(object sender, EventArgs e)
    {
      if (File.Exists("log.txt")) File.Delete("log.txt");
      foreach (var v in syncFolders)
        v.LogByOp(operation.Add);
      Process.Start("C:\\Windows\\System32\\notepad.exe", "log.txt");
    }

    private void logDelToolMainMenuItem_Click(object sender, EventArgs e)
    {
      if (File.Exists("log.txt")) File.Delete("log.txt");
      foreach (var v in syncFolders)
        v.LogByOp(operation.Delete);
      Process.Start("C:\\Windows\\System32\\notepad.exe", "log.txt");
    }

    private void logChangeToolMainMenuItem_Click(object sender, EventArgs e)
    {
      if (File.Exists("log.txt")) File.Delete("log.txt");
      foreach (var v in syncFolders)
        v.LogByOp(operation.Update);
      Process.Start("C:\\Windows\\System32\\notepad.exe", "log.txt");
    }

    private void logByAdressToolMainMenuItem_Click(object sender, EventArgs e)
    {
      if (File.Exists("log.txt")) File.Delete("log.txt");
      foreach (var v in syncFolders)
        v.LogByAdress(computers[dataGridView1.CurrentCell.RowIndex].name);
      Process.Start("C:\\Windows\\System32\\notepad.exe", "log.txt");
    }

    private void Syncronization_program_Load(object sender, EventArgs e)
    {
      this.Icon = new Icon(@"D:\Курсовой проект\WindowsFormsApp1\sync.ico");
      ShowIcon = true;
    }
  }
}
