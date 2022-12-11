using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Diagnostics;

namespace WindowsFormsApp1
{
  class Item
  {
    public string path { get; set; }
    public bool isFile { get; set; }
    public string baseDir { get; set; }
    public DateTime dateModified { get; set; }
    public string relDir { get; set; }

    public Item(FileSystemInfo _dir, string _basedir)
    {
      isFile = _dir is FileInfo;
      path = _dir.FullName;
      baseDir = _basedir;
      relDir = getDiff(_basedir, path);
      dateModified = _dir.LastWriteTime;
    }

    public void CopyTo(string _path)
    {
      try
      {
        if (isFile)
        {
          File.Copy(path, _path + relDir, true);
        }
        else
        {
          Directory.CreateDirectory(_path + relDir);
        }
      }
      catch
      {
        return;
      }
    }

    public void Delete()
    {
      try
      {
        if ((isFile) && (File.Exists(path))) File.Delete(path);
        else if (Directory.Exists(path)) Directory.Delete(path, true);
      }
      catch
      {
        return;
      }
    }

    public bool CompareName(Item _item2)
    {
      if (Path.GetFileName(this.relDir) == Path.GetFileName(_item2.relDir))
        return true;
      return false;
    }

    public static string getDiff(string str1, string str2)
    {
      int i = str2.LastIndexOf(str1);
      int a = 0;
      string str = "";
      try
      {

        while (str1[a] == str2[i])
        {
          i++;
          a++;
        }
      }
      catch (System.IndexOutOfRangeException)
      {
        for (; i < str2.Length; i++)
          str += str2[i];
        return str;
      }
      return str;
    }
  }
}
