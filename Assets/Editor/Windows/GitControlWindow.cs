using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace ProjectTools {
	enum FoldType {
	    NONE = 0,
	    ADDING_BRANCH,
	    ADDING_REMOTE,
	    ADDING_SUBMODULE,
		MERGING,
		DISPLAYING_SUBMODULE,
        SUBD_TO_SUBMODULE
	}

	public class GitControlWindow : EditorWindow {

		// icons
		static private Dictionary<string, Texture> icon_map;

	    // styles
		static GUIStyle redBoxStyle		= null;
		static GUIStyle outStyle		= null;

	    // default commit string prepended to commit messages made from the tool
		const string git_commit_prefix  = "🔧";
		const string git_update_commit  = "🔁";
	    const string git_first_commit   = "✅";

		static bool gitInstalled 		= false;
		static bool initialized 		= false;
		static bool gitignore 			= false;
		static bool submodule			= false;
		static string status 		    = "";
		static string latest_commit_message = "";

		static string remote 		    = "";
		static string branch 			= "";
	    static List<string> branch_list = null;
	    static List<string> remote_list = null;
	    static List<string> submod_list = null;

		static Dictionary<string, bool> submod_folds = null;
		static Dictionary<string, string> submod_statuses = null;

		// foldout fields values
	    private string commit_message   = "";
	    
		static string newRemoteName 	= "";
		static string newRemoteUrl 		= "";

	    static string newBranchName 	= "";
	    static string newSubmoduleUrl 	= "";
	    static string newSubmodulePath 	= "";

		static int merge_selection = 0;

		private Vector2 _scrollPosition;
		bool showStatus					= false;

		FoldType addType = FoldType.NONE;

		// Button validators
		private GUIHelper.Validator unStagedUnCommited = () => { return hasUnstagedChanges() || hasUncommittedChanges(); };
		private GUIHelper.Validator stagedAndCommited = () => { return !hasUnstagedChanges() && !hasUnstagedChanges(); };

		[MenuItem("Project/Git Window")]
		private static void showGitWindow() {
			EditorWindow.GetWindow<GitControlWindow>("Git", true, typeof(EditorWindow));
		}
		
		[MenuItem("Project/Git Window", true)]
		private static bool showGitWindowValidator() {
			return true;
		}
	    
		public static void refresh() {
			if (!gitInstalled)
				gitInstalled = GitHelper.usage().Contains("usage:");

			initialized			= System.IO.Directory.Exists(".git");

			if(gitInstalled && initialized)
				status = GitHelper.status();

			gitignore			= System.IO.File.Exists(".gitignore");
			branch				= ShellHelper.shell("git","rev-parse --abbrev-ref HEAD").Replace("\n","").Replace("\r","");
			latest_commit_message = ShellHelper.shell("git", "log --oneline").Split('\n')[0];
			submodule 			= GitHelper.submodule() != "";

	        // feeding the branch list
			if (branch_list == null) 
				branch_list = new List<String>();
			else 
				branch_list.Clear();
			foreach(var s in GitHelper.branch().Split('\n')) {
				String str = s;
				str = str.Replace("* ","");
				str = str.Trim();
				if (!String.IsNullOrEmpty(str))
					branch_list.Add(str);
			}

	        // feeding the remote list
			if (remote_list == null) 
				remote_list = new List<string>();
			else 
				remote_list.Clear();
			foreach(var s in GitHelper.remote().Split('\n')) {
				string str = s;
				str = str.Trim();
				if (!string.IsNullOrEmpty(str))
					remote_list.Add(str);
			}

			if (remote == "" && remote_list.Count != 0) {
				remote = remote_list[0];
			}
	        
	        // feeding the submodules list
			if (submod_folds == null)
				submod_folds = new Dictionary<string, bool>();
			else submod_folds.Clear();

			if (submod_statuses == null)
				submod_statuses = new Dictionary<string, string>();
			else submod_statuses.Clear();

			if (submod_list == null) 
				submod_list = new List<string>();
			else submod_list.Clear();
			foreach(var s in GitHelper.submodule().Split('\n')) {
				string str = s;
				str = str.Trim();
				if (!string.IsNullOrEmpty(str)) {
					submod_list.Add(str);
					submod_folds.Add(str, false);
					submod_statuses.Add(str, ShellHelper.shell("git", "--git-dir=" + str.Split(' ')[1] + "/.git status"));
				}
			}
		}

		static List<string> getAssetsSubdirectoriesList() {
            var tmp = Directory.GetDirectories("Assets");
			List<string> ret = new  List<string>();
            foreach (var line in tmp) {
                if (!Directory.Exists(line + "/.git"))
                    if (Directory.GetDirectories(line).Length != 0 || Directory.GetFiles(line).Length != 0) 
					    ret.Add(line);
            }
            return ret;

        }

		static bool hasUnstagedChanges() {
			return status.Contains("not staged") || status.Contains("Untracked");
		}

	    static bool hasUncommittedChanges() {
	        return status.Contains("Changes to be committed");
	    }

	    static bool hasDeletedFilesChanges() {
	        return status.Contains("deleted:");
	    }

	    static void nextBranch() {
			var index = branch_list.IndexOf(branch);
			if (index+1 > branch_list.Count-1) GitHelper.switchToBranch(branch_list[0]);
	        else GitHelper.switchToBranch(branch_list[index+1]);
	        refresh(); 
	    }

	    static void previousBranch() {
			var index = branch_list.IndexOf(branch);
	        if (index == 0) GitHelper.switchToBranch(branch_list[branch_list.Count-1]);
	        else GitHelper.switchToBranch(branch_list[index-1]);
	        refresh(); 
	    }

		static void nextRemote() {
			var index = remote_list.IndexOf(remote);
			if (index+1 > remote_list.Count-1) remote = remote_list[0];
			else remote = remote_list[index+1];
			refresh(); 
		}

		static void previousRemote() {
			var index = remote_list.IndexOf(remote);
			if (index == 0) remote = remote_list[remote_list.Count-1];
			else remote = remote_list[index-1];
			refresh(); 
		}

	    void OnEnable() {
			refresh();
			Repaint();
	    }

		void OnGUI() {
			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

			if (icon_map == null) {
				icon_map = new Dictionary<string, Texture>();
				var info = new DirectoryInfo(Application.dataPath + "/Editor Default Resources/GitControl/");
				var fileInfo = info.GetFiles();
				foreach (var f in fileInfo) {
					if (!f.FullName.Replace(f.DirectoryName,"").Contains(".meta")) {
						var p = "GitControl" + f.FullName.Replace(f.DirectoryName,"").Replace("\\","/");
						#if UNITY_EDITOR_OSX
						var n = f.FullName.Replace(f.DirectoryName,"").Replace(".png","").Replace("/","");
						#elif UNITY_EDITOR_WIN
						var n = f.FullName.Replace(f.DirectoryName,"").Replace(".png","").Replace("\\","");
						#endif

						if (EditorGUIUtility.isProSkin) {
							// Using proskin
						} else {
							// Not using proskin
						}
						icon_map.Add (n, EditorGUIUtility.Load(p) as Texture);
					}
				}
	        }

			if (outStyle == null) {
				outStyle = new GUIStyle(GUI.skin.box);
				outStyle.normal.textColor = Color.white;
				outStyle.alignment = TextAnchor.MiddleLeft;
			}

	        if (!gitInstalled) {
				GUI.color = Color.red;
				GUILayout.Box("\"git\" doesn't seem to be installed.\n\"git\" is distributed as part of the Xcode command line tools on Mac OS X.", redBoxStyle ,new GUILayoutOption[]{GUILayout.ExpandWidth(true)});
	            return;
	        } else {
				if (!initialized && !gitignore) {
					EditorGUILayout.BeginHorizontal ();
			        if (initialized) GUI.enabled = false;
			        if (GUILayout.Button ("init", EditorStyles.miniButtonLeft)) {
			            GitHelper.init(git_first_commit);
			            refresh();
			        }
			        GUI.enabled = true;
			        if (gitignore) GUI.enabled = false;
			        if (GUILayout.Button ("add .gitignore", EditorStyles.miniButtonRight)) {
			            GitHelper.createGitignore(git_commit_prefix + " Adding .gitignore"); 
			            refresh();
			        }
			        GUI.enabled = true;
					EditorGUILayout.EndHorizontal ();
				}
		        
				// branch row
				EditorGUILayout.BeginHorizontal ();
				if (GUIHelper.Button(icon_map["branch"], "merge the current branch with another one", EditorStyles.miniButtonMid, () => {return branch_list.Count > 1;}, GUILayout.ExpandWidth(false))) {
					addType = FoldType.MERGING;
				}
				if (GUIHelper.Button(icon_map["left"], "previous branch", EditorStyles.miniButtonMid, () => {return branch_list.Count > 1 && !hasUncommittedChanges() && !hasUnstagedChanges();}, GUILayout.ExpandWidth(false))) {
					previousBranch();
				}
				GUILayout.Button (branch, EditorStyles.miniButtonMid, GUILayout.Height(20));
				if (GUIHelper.Button(icon_map["right"], "next branch", EditorStyles.miniButtonMid, () => {return branch_list.Count > 1 && !hasUncommittedChanges() && !hasUnstagedChanges();}, GUILayout.ExpandWidth(false))) {
					nextBranch();
				}
				if (GUIHelper.Button(icon_map["minus"], "remove current branch", EditorStyles.miniButtonMid, () => {return branch != "master" && !hasUncommittedChanges() && !hasUnstagedChanges();}, GUILayout.ExpandWidth(false))) {
					if (branch != "master") {
						GitHelper.removeBranch(branch);
						refresh();
					}
				}
                // TODO : add validator for the new branch creation
				if (GUIHelper.Button (icon_map["plus"], "create new branch", EditorStyles.miniButtonMid, () => {return true;}, GUILayout.ExpandWidth(false))) {
					addType = FoldType.ADDING_BRANCH;
		        }
				EditorGUILayout.EndHorizontal ();

				if (addType == FoldType.MERGING) {
					merge_selection = EditorGUILayout.Popup("merge current branch with ", 0, branch_list.ToArray());
					EditorGUILayout.BeginHorizontal ();
					if (GUILayout.Button("cancel")) {
						addType = FoldType.NONE;
					}
					if (GUILayout.Button("merge")) {
						GitHelper.merge(branch_list[merge_selection]);
						addType = FoldType.NONE;
						refresh();
					}
					EditorGUILayout.EndHorizontal ();
				}
				
				if (addType == FoldType.ADDING_BRANCH) {
					newBranchName = EditorGUILayout.TextField("New branch name :", newBranchName);
				    EditorGUILayout.BeginHorizontal ();
		            if (GUILayout.Button("cancel")) {
						addType = FoldType.NONE;
		            }
		            if (GUILayout.Button("add")) {
		                GitHelper.createBranch(newBranchName);
						addType = FoldType.NONE;
		                refresh();
		            }
				    EditorGUILayout.EndHorizontal ();
		        }
				if (hasUnstagedChanges()) GUI.color = Color.red;
				else if (hasUncommittedChanges()) GUI.color = Color.yellow;
				showStatus = EditorGUILayout.Foldout(showStatus, "git status");
				if(showStatus) {
					GUILayout.Box(status, outStyle ,new GUILayoutOption[]{GUILayout.ExpandWidth(true)});
				}
		        GUI.color = Color.white;

				EditorGUILayout.BeginHorizontal ();
				if (GUIHelper.Button(icon_map["add"], "git add", EditorStyles.miniButtonLeft, () => {return hasUnstagedChanges();})) {
					GitHelper.add(); 
					refresh();
				}
				if (GUIHelper.Button(icon_map["remove"], "git rm", EditorStyles.miniButtonMid, () => {return hasDeletedFilesChanges();})) {
					GitHelper.remove(); 
					refresh();
				}
				if (GUIHelper.Button(icon_map["commit"], "git commit", EditorStyles.miniButtonRight, () => {return hasUncommittedChanges();})) {
					GUIHelper.OpenCommitWindow((string message) => {GitHelper.commit(git_commit_prefix + " " + message);GitControlWindow.refresh();});
				}
				EditorGUILayout.EndHorizontal ();

		        // remotes/commit row
				EditorGUILayout.BeginHorizontal ();
				if (GUIHelper.Button(icon_map["pull"], "pull from remote", EditorStyles.miniButtonMid, () => {return !hasUnstagedChanges() && !hasUnstagedChanges() && remote != "";} , GUILayout.ExpandWidth(false))) {
					GitHelper.pull();
					AssetDatabase.Refresh();
					refresh();
				}
				if (GUIHelper.Button(icon_map["push"], "push to remote", EditorStyles.miniButtonMid, () => {return !hasUnstagedChanges() && !hasUnstagedChanges() && remote != "";} , GUILayout.ExpandWidth(false))) {
					GitHelper.push(remote);
					AssetDatabase.Refresh();
					refresh();
				}
				if (GUIHelper.Button(icon_map["left"], "previous remote", EditorStyles.miniButtonMid, () => {return remote_list.Count > 1;}, GUILayout.ExpandWidth(false))) {
					previousRemote();
					refresh();
				}
				GUILayout.Button (remote , EditorStyles.miniButtonMid, GUILayout.Height(20));
				if (GUIHelper.Button(icon_map["right"], "next remote", EditorStyles.miniButtonMid, () => {return remote_list.Count > 1;}, GUILayout.ExpandWidth(false))) {
					nextRemote();
					refresh();
				}
				if (GUIHelper.Button(icon_map["minus"], "remove current remote", EditorStyles.miniButtonMid, () => {return remote != "" && !remote.Contains("origin");}, GUILayout.ExpandWidth(false))) {
					GitHelper.removeRemote(remote);
					refresh();
					if (remote_list.Count == 0) remote = "";
					else remote = remote_list[0];
				}

				if (GUIHelper.Button (icon_map["plus"], "add remote", EditorStyles.miniButtonMid, () => {return true;}, GUILayout.ExpandWidth(false))) {
					addType = FoldType.ADDING_REMOTE;
					if (remote_list.Count == 0)
						newRemoteName = "origin";
		        }
				EditorGUILayout.EndHorizontal ();

				if (addType == FoldType.ADDING_REMOTE) {
					newRemoteName = EditorGUILayout.TextField("Remote name :", newRemoteName);
					newRemoteUrl = EditorGUILayout.TextField("Remote url :", newRemoteUrl);
				    EditorGUILayout.BeginHorizontal ();
		            if (GUILayout.Button("cancel")) {
						addType = FoldType.NONE;
		            }
		            if (GUILayout.Button("add")) {
						GitHelper.remote_add_origin(newRemoteUrl, newRemoteName);
		                newRemoteName = "";
		                newRemoteUrl = "";
						addType = FoldType.NONE;
		                refresh();
		            }
				    EditorGUILayout.EndHorizontal ();
		        }

		        // submodules row
				EditorGUILayout.BeginHorizontal ();
				if (GUIHelper.Button(icon_map["pull_subm"], "update submodules", EditorStyles.miniButtonMid, () => {return submodule;}, GUILayout.ExpandWidth(false))) {
					GitHelper.submodule_update();
					AssetDatabase.Refresh();
					refresh();
				}
				if (GUIHelper.Button(icon_map["push_subm"], "push submodules", EditorStyles.miniButtonMid, () => {return submodule;}, GUILayout.ExpandWidth(false))) {
					GitHelper.commitToSubModules(submod_list);
					refresh();
				}

				string view_subm = "▶ view submodules";
				if (addType == FoldType.DISPLAYING_SUBMODULE) view_subm = "▼ view submodules";

				if (GUILayout.Button (view_subm, EditorStyles.miniButtonMid, GUILayout.Height(20))) {
					if (addType == FoldType.DISPLAYING_SUBMODULE)
						addType = FoldType.NONE;
					else
						addType = FoldType.DISPLAYING_SUBMODULE;
		        }
				if (GUIHelper.Button (icon_map["folder_to_subm"], "turn an existing Assets subdirectory into a submodule", EditorStyles.miniButtonMid, () => {return true;}, GUILayout.ExpandWidth(false))) {
                    addType = FoldType.SUBD_TO_SUBMODULE;
                }
				if (GUIHelper.Button (icon_map["plus"], "add existing submodule", EditorStyles.miniButtonMid, () => {return true;}, GUILayout.ExpandWidth(false))) {
					addType = FoldType.ADDING_SUBMODULE; 
		        }
				EditorGUILayout.EndHorizontal ();

				if (addType == FoldType.DISPLAYING_SUBMODULE) {
					foreach(var s in submod_list) {
						EditorGUILayout.BeginHorizontal();
						var short_path = s.Split(' ')[1];
						bool unadded = submod_statuses[s].Contains("not staged") || submod_statuses[s].Contains("Untracked");
						bool unremoved = submod_statuses[s].Contains("deleted:");
						bool uncommited = submod_statuses[s].Contains("Changes to be committed");
						
						GUILayout.Label(short_path);
                        EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginHorizontal();

						if (GUIHelper.Button(icon_map["minus"], "remove this submodule", EditorStyles.miniButtonLeft, () => {return true;}, GUILayout.ExpandWidth(false))) {
							GitHelper.submodule_remove(short_path);
							AssetDatabase.Refresh();
							refresh();
						}

						GUI.enabled = false;
						GUILayout.Button (" ", EditorStyles.miniButtonMid, GUILayout.ExpandWidth(false), GUILayout.Height(20));
						GUI.enabled = true;

                        if (GUIHelper.Button(icon_map["branch"], "merge the current branch with another one", EditorStyles.miniButtonMid, () => {return false;}, GUILayout.ExpandWidth(false))) {}
						if (GUIHelper.Button(icon_map["left"], "previous branch", EditorStyles.miniButtonMid, () => {return false;}, GUILayout.ExpandWidth(false))) {}
                        GUILayout.Button (branch, EditorStyles.miniButtonMid, GUILayout.Height(20));
						if (GUIHelper.Button(icon_map["right"], "next branch", EditorStyles.miniButtonMid, () => {return false;}, GUILayout.ExpandWidth(false))) {}
                        if (GUIHelper.Button(icon_map["minus"], "remove current branch", EditorStyles.miniButtonMid, () => {return false;}, GUILayout.ExpandWidth(false))) {}
						if (GUIHelper.Button(icon_map["plus"], "remove current branch", EditorStyles.miniButtonMid, () => {return false;}, GUILayout.ExpandWidth(false))) {}

						GUI.enabled = false;
						GUILayout.Button (" ", EditorStyles.miniButtonMid, GUILayout.ExpandWidth(false), GUILayout.Height(20));
						GUI.enabled = true;

						if (GUIHelper.Button(icon_map["pull_individual_subm"], "pull from submodule origin remote", EditorStyles.miniButtonMid, () => {return !unadded || !unremoved || !uncommited;}, GUILayout.ExpandWidth(false))) {
							var str = "--git-dir=" + ShellHelper.shell("pwd").Replace("\n","").Replace("\r","") + "/" + short_path + "/.git pull origin master";
							ShellHelper.FilteredDebugLog( ShellHelper.shell("git", str));
							refresh();
							this.Repaint();
						}
						if (GUIHelper.Button(icon_map["push_individual_subm"], "pull from submodule origin remote", EditorStyles.miniButtonMid, () => {return !unadded || !unremoved || !uncommited;}, GUILayout.ExpandWidth(false))) {
							var str = "--git-dir=" + ShellHelper.shell("pwd").Replace("\n","").Replace("\r","") + "/" + short_path + "/.git push origin master";
							ShellHelper.FilteredDebugLog( ShellHelper.shell("git", str));
							refresh();
							this.Repaint();
						}
						GUI.enabled = false;
						GUILayout.Button (" ", EditorStyles.miniButtonMid, GUILayout.ExpandWidth(false), GUILayout.Height(20));
						GUI.enabled = true;
						
						if (GUIHelper.Button(icon_map["add"], "add untracked changes to the staging area of the submodule", EditorStyles.miniButtonMid, () => {return unadded;}, GUILayout.ExpandWidth(false))) {
							var str = "--git-dir=" + ShellHelper.shell("pwd").Replace("\n","").Replace("\r","") + "/" + short_path + "/.git add .";
							ShellHelper.FilteredDebugLog( ShellHelper.shell("git", str));
							refresh();
							this.Repaint();
						}
						if (GUIHelper.Button(icon_map["remove"], "remove deleted files from the submodule", EditorStyles.miniButtonMid, () => {return unremoved;}, GUILayout.ExpandWidth(false))) {
							var str = "--git-dir=" + ShellHelper.shell("pwd").Replace("\n","").Replace("\r","") + "/" + short_path + "/.git add -u .";
							ShellHelper.FilteredDebugLog( ShellHelper.shell("git", str));
							refresh();
							this.Repaint();
						}
						if (GUIHelper.Button(icon_map["commit"], "commit changes to this submodule", EditorStyles.miniButtonRight, () => {return uncommited;}, GUILayout.ExpandWidth(false))) {
							GUIHelper.OpenCommitWindow((string message) => {
								var str = "--git-dir=" + short_path + "/.git commit -m \"" + git_commit_prefix + " " + message + "\"";
								ShellHelper.shell("git", str);
								GitControlWindow.refresh();});
							this.Repaint();
						}
						EditorGUILayout.EndHorizontal ();
						if (unadded) GUI.color = Color.red;
						else if (uncommited) GUI.color = Color.yellow;
						submod_folds[s] = EditorGUILayout.Foldout(submod_folds[s], "submodule git status");
						if(submod_folds[s]) {
							GUILayout.Box(submod_statuses[s], outStyle ,new GUILayoutOption[]{GUILayout.ExpandWidth(true)});
						}
						GUI.color = Color.white;
					} 
				} else if (addType == FoldType.SUBD_TO_SUBMODULE) {
                    GUILayout.BeginVertical();
                    foreach(var str in getAssetsSubdirectoriesList()) {
                        GUILayout.BeginHorizontal();
						GUILayout.Label(str);
						if (GUIHelper.Button(icon_map["folder_subm"], "turn this folder into a submodule", EditorStyles.miniButton, () => {return true;}, GUILayout.ExpandWidth(false))) {
                            GUIHelper.OpenToSubmoduleWindow(str, 
                                    (string old_path, string new_path, string url) => {
                                        GitHelper.assetsSubdirectoryToSubModule(git_commit_prefix, old_path, url);
                                    });
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                }

				if (addType == FoldType.ADDING_SUBMODULE) {
		            newSubmoduleUrl = EditorGUILayout.TextField("Submodule url :", newSubmoduleUrl);
				    newSubmodulePath = EditorGUILayout.TextField("Submodule path :", newSubmodulePath);
				    EditorGUILayout.BeginHorizontal ();
		            if (GUILayout.Button("cancel")) {
						addType = FoldType.NONE;
		            }
		            if (GUILayout.Button("add")) {
		                GitHelper.submodule_add(newSubmoduleUrl, "./Assets/" + newSubmodulePath);
		                newSubmoduleUrl = "";
		                newSubmodulePath = "";
						addType = FoldType.NONE;
						AssetDatabase.Refresh();
		                refresh();
		            }
				    EditorGUILayout.EndHorizontal ();
		        }

				EditorGUILayout.EndScrollView();

				EditorGUILayout.BeginHorizontal ();
				if (hasUnstagedChanges() || hasUncommittedChanges()) GUI.enabled = false;  else GUI.enabled = true;
				if (GUILayout.Button (icon_map["pull_all"], EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(false))) {
					GitHelper.submodule_update();
                    GitHelper.pull();
                    AssetDatabase.Refresh();
					refresh();
				}
				if (hasUnstagedChanges() || hasUncommittedChanges()) GUI.enabled = true;  else GUI.enabled = false;
				if (GUILayout.Button (icon_map["revert"], EditorStyles.miniButtonMid, GUILayout.ExpandWidth(false))) {
		            GitHelper.stashClear();
		            refresh();
		        }
				if (GUILayout.Button (icon_map["update_db"], EditorStyles.miniButtonMid, GUILayout.ExpandWidth(false))) {
		            GitHelper.commitAsDbUpdate(git_update_commit);
		            refresh();
		        }
				GUI.enabled = false;
				GUILayout.Button ("", EditorStyles.miniButtonMid, GUILayout.Height(20));
				GUI.enabled = true;

				if (GUIHelper.Button(icon_map["nuke"], "nuke the local repository", EditorStyles.miniButtonMid, () => {return true;}, GUILayout.ExpandWidth(false))) {
					GUIHelper.OpenNukeConfirmWindow();
				}
				if (GUIHelper.Button(icon_map["term"], "open the terminal in the current project directory", EditorStyles.miniButtonMid, () => {return true;}, GUILayout.ExpandWidth(false))) {
					ShellHelper.OpenTerminal(Application.dataPath + "/.. ; git status");
				}
				if (GUIHelper.Button(icon_map["config"], "open the configuration window", EditorStyles.miniButtonMid, () => {return true;}, GUILayout.ExpandWidth(false))) {
					GUIHelper.OpenGitSettingsWindow();	
				}
				if (GUIHelper.Button(icon_map["update"], "update the window content", EditorStyles.miniButtonRight, () => {return true;}, GUILayout.ExpandWidth(false))) {
					AssetDatabase.Refresh();
					refresh();
					remote = "";
					branch = "";
					status = "";
				}
				EditorGUILayout.EndHorizontal ();
			}
		}
	}
}
