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

		public static void OpenTerminal(string startup_string = "") {
			#if UNITY_EDITOR_OSX
			ShellHelper.shell("osascript", "-e 'tell app \"Terminal\" to do script \"" + startup_string + "\" activate'");
			#elif UNITY_EDITOR_WIN

			#endif
		}
	}
}