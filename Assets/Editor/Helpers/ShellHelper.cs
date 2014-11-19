using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Diagnostics;

namespace ProjectTools {
	public class ShellHelper {
		public static Process shellp(string filename, string arguments)  {
			var p = new Process();
			p.StartInfo.Arguments = arguments;
			p.StartInfo.CreateNoWindow = true;
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardInput = true;
	        p.StartInfo.RedirectStandardError = true;
			p.StartInfo.FileName = filename;
			p.Start();
			return p;
		}
		
		public static string shell( string filename, string arguments = "") {
			var p = shellp(filename, arguments);
			var output = p.StandardOutput.ReadToEnd();
			p.WaitForExit();
			return output;
		}

	    public static void FilteredDebugLog(string line) {
	        if (!String.IsNullOrEmpty(line)) {
	            if (line.Contains("fatal") && line.Contains("error"))
					line = line.Insert(0, "<color=red>Fatal error:</color> ");
				UnityEngine.Debug.Log(line);
			}
	    }

		public static void Rmrf(string target_dir)
		{
			string[] files = Directory.GetFiles(target_dir);
			string[] dirs = Directory.GetDirectories(target_dir);
			
			foreach (string file in files)
			{
				File.SetAttributes(file, FileAttributes.Normal);
				File.Delete(file);
			}
			
			foreach (string dir in dirs)
			{
				Rmrf(dir);
			}
			
			Directory.Delete(target_dir, false);
		}
	}
}