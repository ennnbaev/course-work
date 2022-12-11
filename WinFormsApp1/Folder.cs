using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Diagnostics;

namespace WindowsFormsApp1
{
  class Folder
  {
    public string path { get; set; }
    List<Item> items;
    public bool isChecked { get; set; }

    private List<Log> logs = new List<Log>();

    public Folder(string _path)
    {
      path = _path;
      items = new List<Item>();
      isChecked = false;
      AddRange(new DirectoryInfo(path).EnumerateFileSystemInfos("*", SearchOption.AllDirectories).OrderBy(x => x.FullName));
    }

    public void AddRange(IEnumerable<FileSystemInfo> dirs)
    {
      items.AddRange(dirs.Select(x => new Item(x, path)));
    }

    public void CopyFrom(Folder _baseFolder)
    {
      string[] dirs = Directory.GetDirectories(this.path);
      foreach (var v in dirs)
      {
        try
        {
          Directory.Delete(v, true);
        }
        catch { break; }
      }
      string[] files = Directory.GetFiles(this.path);
      foreach (var v in files)
      {
        try
        {
          File.Delete(v);
        }
        catch { break; }
      }

      foreach (var items in _baseFolder.items)
      {
        items.CopyTo(path);
      }
    }

    public void Update()
    {
      items.Clear();
      items.Capacity = 0;
      AddRange(new DirectoryInfo(path).EnumerateFileSystemInfos("*", SearchOption.AllDirectories).OrderBy(x => x.FullName));
    }

    public void FirstSync(Folder _baseFolder)
    {
      foreach (var items in this.items)
      {
        int dateCheck = 0;
        foreach (var items2 in _baseFolder.items)
        {
          if (items.CompareName(items2) && (items.dateModified < items2.dateModified)) dateCheck++;
        }
        if (dateCheck == 0)
        {
          items.CopyTo(_baseFolder.path);
          _baseFolder.Update();
        }
      }
      logs.Add(new Log(operation.AddFolder, this.path));
      isChecked = true;
    }

    public void SyncAdd(Folder _baseFolder)
    {
      foreach (var items1 in this.items)
      {
        int checkAdd = 0;
        foreach (var items2 in _baseFolder.items)
        {
          if (!items1.CompareName(items2)) checkAdd++;
        }
        if (checkAdd == _baseFolder.items.Count)

        {
          items1.CopyTo(_baseFolder.path);
          logs.Add(new Log(operation.Add, items1.path));
        }
      }
    }

    public void SyncDel(Folder _baseFolder)
    {
      foreach (var items1 in _baseFolder.items)
      {
        int checkDel = 0;
        foreach (var items2 in this.items)
        {
          if (items1.CompareName(items2)) checkDel++;
        }
        if (checkDel == 0)
        {
          items1.Delete();
          logs.Add(new Log(operation.Delete, this.path + items1.relDir));
        }
      }

    }

    public void SyncUpdate(Folder _baseFolder)
    {
      foreach (var items1 in this.items)
      {
        int checkUpdate = 0;
        foreach (var items2 in _baseFolder.items)
        {
          if (items1.CompareName(items2) && (items1.dateModified > items2.dateModified) && items1.isFile) checkUpdate++;
        }
        if (checkUpdate != 0)
        {
          items1.CopyTo(_baseFolder.path);
          logs.Add(new Log(operation.Update, items1.path));
        }
      }
    }

    public void LogByAdress(string _name)
    {
      using (StreamWriter sw = new StreamWriter("log.txt", true, System.Text.Encoding.Default))
      {
        foreach (var v in logs)
        {
          if (v._adress.StartsWith(_name))
          {
            sw.WriteLine(v.ToString());
          }
        }
      }
    }

    public void LogByOp(operation _op)
    {
      using (StreamWriter sw = new StreamWriter("log.txt", true, System.Text.Encoding.Default))
      {
        foreach (var v in logs)
        {
          if (v._op == _op)
          {
            sw.WriteLine(v.ToString());
          }
        }
        sw.WriteLine();
      }
    }
  }
}
