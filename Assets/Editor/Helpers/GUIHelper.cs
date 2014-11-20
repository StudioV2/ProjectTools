using UnityEngine;
using UnityEditor;
using System.Collections;

namespace ProjectTools {
	public class GUIHelper {
		public delegate bool Validator();

		public static bool Button(string str, string t, GUIStyle s, Validator v, params GUILayoutOption[] l) {
			GUIContent c = new GUIContent(str, t);
			return Button (c, s, v, l);
		}

		public static bool Button(Texture tex, string t, GUIStyle s, Validator v, params GUILayoutOption[] l) {
			GUIContent c = new GUIContent(tex, t);
			return Button (c, s, v, l);
		}

		private static bool Button(GUIContent c, GUIStyle s, Validator v, params GUILayoutOption[] l) {
			bool ret = false;

			if (v()) GUI.enabled = true; else GUI.enabled = false;
			if (l.Length == 0) {
				ret = GUILayout.Button(c, s);
			} else {
				ret = GUILayout.Button(c, s, l);
			}
			GUI.enabled = true;
			return ret;
		}

		public static void OpenCommitWindow(CommitWindow.Func f) {
			CommitWindow w = EditorWindow.GetWindow<CommitWindow>(true, "Git commit", true) as CommitWindow;
            w.Fun = f;
		}

        public static void OpenToSubmoduleWindow(string newpath, ToSubmoduleWindow.Func f) {
			ToSubmoduleWindow w = EditorWindow.GetWindow<ToSubmoduleWindow>(true, "Assets subdirectory to submodule", true) as ToSubmoduleWindow;
            w.Fun       = f;
            w.OldPath   = newpath;
            w.NewPath   = newpath;
        }

        public static void OpenGitSettingsWindow() {
            EditorWindow.GetWindow<GitSettingsWindow>("Git settings", true);
        }

		public static void OpenNukeConfirmWindow() {
			EditorWindow.GetWindow<NukeConfirmWindow>(true, "Nuke this project versionning", true);
		}
	}
}
