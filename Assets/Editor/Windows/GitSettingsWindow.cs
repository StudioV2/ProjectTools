using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ProjectTools {
	public class GitSettingsWindow : EditorWindow {

	    public static string GitLocalUsername { get; internal set; }
	    public static string GitLocalUserEmail { get; internal set; }
	    public static string GitExecutablePath { get; internal set; }

		[MenuItem("Project/Git Settings")]
		private static void showGitSettingsWindow() {
			EditorWindow.GetWindow<GitSettingsWindow>("Git Settings", false);
		}
		
		[MenuItem("Project/Git Settings", true)]
		private static bool showGitSettingsWindowValidator() {
			return true;
		}

	    void OnEnable() {
			GitLocalUsername = ShellHelper.shell("git", "config user.name");
			GitLocalUserEmail = ShellHelper.shell("git", "config user.email");
	    }

	    void OnGUI () {
	        EditorGUILayout.LabelField("Local git configuration", EditorStyles.boldLabel);

			EditorGUILayout.BeginVertical ();
			GitLocalUsername = EditorGUILayout.TextField("Local git username :", GitLocalUsername);
			GitLocalUserEmail = EditorGUILayout.TextField("Local git user email :", GitLocalUserEmail);
			EditorGUILayout.EndVertical ();

	        EditorGUILayout.LabelField("Git executable (Shitdowns only)", EditorStyles.boldLabel);
			EditorGUILayout.BeginVertical ();
	        GitExecutablePath = EditorGUILayout.TextField("Git executable :", GitExecutablePath);
			EditorGUILayout.EndVertical ();

			if (GUILayout.Button ("apply")) {
				ShellHelper.FilteredDebugLog(ShellHelper.shell ("git", "config user.name \"" + GitLocalUsername + "\""));
				ShellHelper.FilteredDebugLog(ShellHelper.shell ("git", "config user.email \"" + GitLocalUserEmail + "\""));
			}
	    }
	}
}
