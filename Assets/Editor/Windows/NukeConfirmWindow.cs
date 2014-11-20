using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ProjectTools {
	public class NukeConfirmWindow : EditorWindow {
		void OnGUI() {
			minSize = new Vector2(321,64);
			maxSize = new Vector2(321,64);

			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Do you want to delete the versionning of the current project ?");
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("cancel")) {
				this.Close ();
			}
			if (GUILayout.Button("yes")) {
				ShellHelper.shell("rm", "-rf .git");
				ShellHelper.shell("rm", "-rf .gitignore");
				this.Close ();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();
		}
	}
}
