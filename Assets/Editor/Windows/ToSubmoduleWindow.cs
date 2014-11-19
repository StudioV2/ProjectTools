using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ProjectTools {
	public class ToSubmoduleWindow : EditorWindow {
        public delegate void Func(string old_path, string new_path, string url);

        public Func Fun = null;
        public string OldPath = "";
        public string NewPath = "";
        public string RepoUrl = "";

        public ToSubmoduleWindow() {
			minSize = new Vector2(321,64);
			maxSize = new Vector2(321,64);
        }

        void OnGUI() {
            EditorGUILayout.BeginVertical();

            RepoUrl = EditorGUILayout.TextField("repo url", RepoUrl);
            NewPath = EditorGUILayout.TextField("name", NewPath);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("cancel")) {
                this.Close();
            }
            if (GUILayout.Button("add")) {
                if (Fun != null) {
                    Fun(OldPath, NewPath, RepoUrl);
                    this.Close();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }
}
