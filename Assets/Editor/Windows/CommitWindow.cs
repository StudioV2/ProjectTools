using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ProjectTools {
	public class CommitWindow : EditorWindow {
		public delegate void Func(string message);
		
		public Func Fun;
		string commit_message = "";

		public CommitWindow(Func commit_action) {
            Fun = commit_action;
			minSize = new Vector2(300,105);
			maxSize = new Vector2(300,105);
		}

		void OnGUI() {
			EditorGUILayout.BeginVertical ();

			GUILayout.Label("Commit message :");
			EditorGUILayout.BeginHorizontal ();
			commit_message = EditorGUILayout.TextArea(commit_message, GUILayout.Height(62));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			if (GUIHelper.Button("cancel", "cancel", EditorStyles.miniButton, () => {return true;})) {
				this.Close();
			}
			if (GUIHelper.Button("commit", "commit", EditorStyles.miniButton, () => {return commit_message!="";})) {
				Fun(commit_message);
				GitControlWindow.refresh();
				this.Close();
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.EndVertical ();
		}
	}
}
