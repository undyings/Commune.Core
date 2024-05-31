using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Diagnostics;
using Serilog;

namespace Commune.Basis
{
  public class ApplicationHlp
  {
    public static Process? ProcessHideStart(string processPath)
    {
      return ProcessHideStart(processPath, "");
    }

    public static Process? ProcessHideStart(string processPath, string args)
    {
      ProcessStartInfo info = new ProcessStartInfo(processPath);
      if (!StringHlp.IsEmpty(args))
        info.Arguments = args;
      info.CreateNoWindow = true;
      info.UseShellExecute = false;
      return Process.Start(info);
    }

    public static string CheckAndCreateFolderPath(string root, params string[] folders)
    {
      string path = root;

      foreach (string folder in folders)
      {
				if (StringHlp.IsEmpty(folder))
					continue;

        path = Path.Combine(path, folder);
        if (!Directory.Exists(path))
          Directory.CreateDirectory(path);
      }
      return path;
    }

    public static void RemoveOldFiles(string dir, string searchPattern, int maxFiles)
    {
      string[] files = Directory.GetFiles(dir, searchPattern);
      if (files.Length > maxFiles)
      {
        Array.Sort(files);

        for (int i = 0; i < files.Length - maxFiles; i++)
        {
          try { File.Delete(files[i]); }
          catch (Exception exc) { Log.Error(exc, ""); }
        }
      }
    }
  }
}
