using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace ProjectTools {
	public class SubprojectManagerWindow : EditorWindow {

		private static Dictionary<string, bool> subprojects = new Dictionary<string, bool>() {
			{"ios", false},
			{"android", false},
			{"wp8", false},
			{"desktop", false},
		};

		private Vector2 _scrollPosition;

		private static string new_subp = ""; 

		[MenuItem("Project/Subproject Manager")]
		private static void showSubprojectManagerWindow() {
			refreshSubProjectsList();
			EditorWindow.GetWindow<SubprojectManagerWindow>(true, "Subproject Manager");
		}

		[MenuItem("Project/Subproject Manager", true)]
		private static bool showProjectManagerWindowvalidator() {
			return true;	
		}

		private static void refreshSubProjectsList() {

			List<string> keys = new List<string>(subprojects.Keys);
			foreach(string key in keys)
			{
				subprojects[key] = false;
			}
			
			var info = new DirectoryInfo(Application.dataPath+"/../..");
			var dirInfo = info.GetDirectories();
			foreach(var dir in dirInfo) {
				if (!dir.Name.StartsWith(".")) {
					var valid = 0;
					foreach(var subdir in dir.GetDirectories()) {
						if (subdir.Name == "Assets") valid++;
						else if (subdir.Name == "ProjectSettings") valid++;
					}
					if (valid == 2)
						subprojects[dir.Name] = true;
				}
			}
		}

		private static bool isIosSubProject() {
			var folders = Application.dataPath.Split('/');
			return folders[folders.Length-2]=="ios";
		}

		private static bool isHardlinkInstalled() {
			return System.IO.File.Exists("/usr/local/bin/hardlink");
		}

		private void createSubProject(string name) {
			ShellHelper.shell ("mkdir", "../"+name);
			ShellHelper.shell ("/usr/local/bin/hardlink", "./Assets ../"+name+"/Assets");
			ShellHelper.shell ("/usr/local/bin/hardlink", "./ProjectSettings ../"+name+"/ProjectSettings");
		}

		private void unlinkSubProject(string name) {
			ShellHelper.shell ("/usr/local/bin/hardlink", "-u ../"+name+"/Assets");
			ShellHelper.shell ("/usr/local/bin/hardlink", "-u ../"+name+"/ProjectSettings");
			ShellHelper.shell ("rm", "-rf ../"+name);
		}

		private void installHardlink() {
			Debug.Log(ShellHelper.shell ("/bin/sh", Application.dataPath+"/Plugins/hardlink/install_hardlink.sh \"" + Application.dataPath+"/Plugins/hardlink\""));
		}


		void OnGUI() {
			maxSize = new Vector2(430, 300);
			minSize = maxSize;

			refreshSubProjectsList();
			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

			bool toDisable = false;

			/* invalid project structure, the project must be located in a folder names ios */
			if (!isIosSubProject()) {
				GUI.color = Color.yellow;
				var tmpStyle = new GUIStyle(GUI.skin.box);
				tmpStyle.normal.textColor = Color.yellow;
				GUILayout.Box("The subproject manager is only usable from the ios project.", tmpStyle ,new GUILayoutOption[]{GUILayout.ExpandWidth(true)});
				GUI.enabled = false;
				toDisable = true;
			}

			/* hardlink not installed on your system */
			if (!isHardlinkInstalled()) {
				GUI.color = Color.green;
				var tmpStyle = new GUIStyle(GUI.skin.box);
				tmpStyle.normal.textColor = Color.green;
				GUILayout.Box("\"hardlink\" is not installed on your system. \nTo be able to install it, you need to thave the \"Command line tools\" provided by Xcode installed on your system.", tmpStyle, new GUILayoutOption[]{GUILayout.ExpandWidth(true)});
				if (System.IO.File.Exists("/usr/bin/gcc")) {
					GUI.enabled = true;
					if (GUILayout.Button("Install hardlink")) {
						installHardlink();
					}
					GUI.enabled = false;
				}
				toDisable = true;
			}

			if (isIosSubProject() && isHardlinkInstalled()) {
				GUI.color = Color.cyan;
				var tmpStyle = new GUIStyle(GUI.skin.box);
				tmpStyle.normal.textColor = Color.cyan;
				GUILayout.Box("The subproject manager is ready.", tmpStyle ,new GUILayoutOption[]{GUILayout.ExpandWidth(true)});
			}

			GUI.color = Color.white;

			foreach(var subp in subprojects) {
				if (!toDisable) GUI.enabled = true;
				if (subp.Value) {
					GUI.enabled = false;
				}
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(subp.Key);
				if (subp.Key == "ios") {
					GUILayout.Button("Default \"subproject\"");
				} else {
					if (GUILayout.Button(subp.Value ? "\"subproject\" exists" : "Create \"subproject\"")) {
						createSubProject(subp.Key);
					}
					if (!toDisable) GUI.enabled = !GUI.enabled;
					if (GUILayout.Button("Unlink this \"subproject\"")) {
						unlinkSubProject(subp.Key);
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			if (isIosSubProject()) GUI.enabled = true; else GUI.enabled = false;
			EditorGUILayout.BeginHorizontal();
			new_subp = EditorGUILayout.TextField("Custom \"subproject\" name:", new_subp);
			if (GUILayout.Button("Create \"subproject\"")) {
				if (new_subp != "" && !new_subp.Contains(" ") && !new_subp.Contains("\\") && !new_subp.Contains("/")) {
					Debug.Log("New subproject name is valid");
					createSubProject(new_subp);
				}
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndScrollView();
		}
	}
}
