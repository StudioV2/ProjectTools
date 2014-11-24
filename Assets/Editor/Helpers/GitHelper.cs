using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace ProjectTools {
	public class GitHelper {
        public static string git_executable = "git";

		private const string gitignore_content = 
			    "# Text editor swap files\n" +
				"*.swp\n" +
				"\n" +
				"# Unity specific files\n" +
				"Temp\n" +
				"Library\n" +
				"*.sln\n" +
				"*.csproj\n" +
				"*.unityproj\n" +
				"*.userprefs\n" +
				"**CurrentLayout.dwlt\n" +
				"**CurrentMaximizeLayout.dwlt\n" +
				"**/ScriptAssemblies\n" +
				"\n" +
				"# OS generated files\n" +
				"**/.DS_Store\n" +
				"**/.DS_Store?\n" +
				"**/._*\n" +
				"**/.Spotlight-V100\n" +
				"**/.Trashes\n" +
				"**/ehthumbs.db\n" +
				"**/Thumbs.db\n";

        private const string submodule_gitignore_content =
			    "# Text editor swap files\n" +
				"*.swp\n" +
				"\n" +
				"# Unity specific files\n" +
                "*.meta\n" + 
				"\n" +
				"# OS generated files\n" +
				"**/.DS_Store\n" +
				"**/.DS_Store?\n" +
				"**/._*\n" +
				"**/.Spotlight-V100\n" +
				"**/.Trashes\n" +
				"**/ehthumbs.db\n" +
				"**/Thumbs.db\n";

        public static string git(string args, string submodule_path = "") {
			if (submodule_path != "")
				args = "--git-dir=" + submodule_path + "/.git " + args;
            return ShellHelper.shell(git_executable, args);
        }

		public static string usage() {
			return ShellHelper.shell("git");
		}

		public static void init(string message) {
	        //message = message.Replace("\"", "\\\"");
			ShellHelper.FilteredDebugLog(git("init"));
			System.IO.File.WriteAllText(".gitignore", gitignore_content);
			ShellHelper.FilteredDebugLog(git("add ."));
			ShellHelper.FilteredDebugLog(git("commit -m \"" + message + "\""));
   		}

		public static void createGitignore(string message) {
            //message = message.Replace("\"", "\\\"");
			System.IO.File.WriteAllText(".gitignore", gitignore_content);
			ShellHelper.FilteredDebugLog(git("add .gitignore"));
			ShellHelper.FilteredDebugLog(git("commit -m \"" + message + "\""));
		}

		public static void add(string target_submodule_path = "") {
			ShellHelper.FilteredDebugLog(git("add .", target_submodule_path));
            
		}
		
		public static void remove(string target_submodule_path = "") {
			ShellHelper.FilteredDebugLog(git("add -u .", target_submodule_path));
		}
		
		public static void push(string remote = "", string target_submodule_path = "") {
			ShellHelper.FilteredDebugLog(git("push " + remote, target_submodule_path));
		}
		
		public static void pull(string remote = "", string target_submodule_path = "") {
			ShellHelper.FilteredDebugLog(git("pull " + remote + " --rebase", target_submodule_path));
		}

		public static void commit(string message, string target_submodule_path = "") {
	        //message = message.Replace("\"", "\\\"");
            // TODO : find a way to have quotes in the commit message
			ShellHelper.FilteredDebugLog(git("commit -m \"" + message + "\"", target_submodule_path));
		}

		public static void merge(string branch, string target_submodule_path = "") {
			ShellHelper.FilteredDebugLog(git("merge " + branch, target_submodule_path));
		}

		public static string status(string target_submodule_path = "") {
			return git("status --ignore-submodules=dirty", target_submodule_path);
		}

		public static string branch(string target_submodule_path = "") {
			return git("branch", target_submodule_path);
		}
		
		public static string remote(string target_submodule_path = "") {
			return git("remote", target_submodule_path);
		}

		public static string submodule(string target_submodule_path = "") {
			return git("submodule", target_submodule_path);
		}

		public static void remote_add_origin(string e_url, string message, string target_submodule_path = "") {
            // TODO: separate distant repo initialization (push -u origin master) from the remote setting
	        message = message.Replace("\"", "\\\"");
			ShellHelper.FilteredDebugLog(git("remote add origin " + e_url, target_submodule_path));
			ShellHelper.FilteredDebugLog(git("add .", target_submodule_path));
			ShellHelper.FilteredDebugLog(git("commit -m \"" + message + " Commit for remote population\"", target_submodule_path));
			ShellHelper.FilteredDebugLog(git("push -u origin master", target_submodule_path));
		}

		public static void create_branch(string name, string target_submodule_path = "") {
			ShellHelper.FilteredDebugLog(git("branch " + name, target_submodule_path));
        }

		public static void remove_branch(string name, string target_submodule_path = "") {
			ShellHelper.FilteredDebugLog(git("checkout master", target_submodule_path));
			ShellHelper.FilteredDebugLog(git("branch -D " + name, target_submodule_path));
        }

        public static void checkout_branch(string name, string target_submodule_path = "") {
			ShellHelper.FilteredDebugLog(git("checkout ", target_submodule_path));
        }

		public static void remote_add(string remote_name, string remote_url, string target_submodule_path = "") {
			ShellHelper.FilteredDebugLog(git("remote add " + remote_name + " " + remote_url, target_submodule_path));
        }

		public static void remote_rm(string remote_name, string target_submodule_path = "") {
			ShellHelper.FilteredDebugLog(git("remote rm " + remote_name, target_submodule_path));
        }

		public static void submodule_add(string submodule_url, string submodule_path, string target_submodule_path = "") {
            //ShellHelper.FilteredDebugLog(git(

			var s = "submodule add " + submodule_url + " " + submodule_path + " -f";
			ShellHelper.FilteredDebugLog(git(s));
            add(target_submodule_path);
			commit("Adding new submodule", target_submodule_path);
        }

		public static void submodule_remove(string submodule_name, string target_submodule_path = "") {
			ShellHelper.FilteredDebugLog(git ("submodule deinit -f " + submodule_name, target_submodule_path));
			ShellHelper.FilteredDebugLog(git( "rm -f " + submodule_name, target_submodule_path));
			ShellHelper.FilteredDebugLog(git( "rm -f " + submodule_name + ".meta", target_submodule_path));
			ShellHelper.FilteredDebugLog(ShellHelper.shell("rm", "-rf " + submodule_name));
			ShellHelper.FilteredDebugLog(ShellHelper.shell("rm", "-rf " + submodule_name + ".meta"));
			/*
			ShellHelper.FilteredDebugLog(ShellHelper.shell(git_executable, "submodule deinit -f " + submodule_name));
			ShellHelper.FilteredDebugLog(ShellHelper.shell(git_executable, "rm -f " + submodule_name));
			ShellHelper.FilteredDebugLog(ShellHelper.shell(git_executable, "rm -f " + submodule_name + ".meta"));
			ShellHelper.FilteredDebugLog(ShellHelper.shell("rm", "-rf " + submodule_name));
			ShellHelper.FilteredDebugLog(ShellHelper.shell("rm", "-rf " + submodule_name + ".meta"));
			*/
        }

        public static void submodule_init(string target_submodule_path = "") {
            ShellHelper.FilteredDebugLog(git("submodule init .", target_submodule_path));
        }

        public static void submodule_update(string target_submodule_path = "") {
            submodule_init();
            ShellHelper.FilteredDebugLog(git("submodule update", target_submodule_path));
        }

        public static void stash(string target_submodule_path = "") {
            ShellHelper.FilteredDebugLog(git("stash", target_submodule_path));
        }

        public static void stash_clear(string target_submodule_path = "") {
            ShellHelper.FilteredDebugLog(git("stash clear", target_submodule_path));
        }

	    public static void createBranch(string name) {
	        ShellHelper.FilteredDebugLog(git("branch " + name));
	    }

	    public static void removeBranch(string name) {
	        Debug.Log(name);
	        ShellHelper.FilteredDebugLog(git("checkout master"));
	        ShellHelper.FilteredDebugLog(git("branch -D " + name));
	    }

	    public static void switchToBranch(string name) {
	        ShellHelper.FilteredDebugLog(git("checkout " + name));
	    }

	    public static void addRemote(string remote_name, string remote_url) {
			ShellHelper.FilteredDebugLog(git("remote add " + remote_name + " " + remote_url));
	    }

	    public static void removeRemote(string remote_name) {
			ShellHelper.FilteredDebugLog(git("remote rm " + remote_name));
	    }

		public static void commitToSubModules(List<string> submod_list) {
			foreach(var s in submod_list) {
				var ss = s.Split(' ')[1];
				Debug.Log(ss);
				ShellHelper.FilteredDebugLog(ShellHelper.shell(git_executable, "-rf " + ss));
			}
	    }

        public static void assetsSubdirectoryToSubModule(string commit_prefix, string pathToSubmodule, string remoteUrl, string target_submodule_path = "") {
			System.IO.File.WriteAllText(pathToSubmodule + "/.gitignore", submodule_gitignore_content);
			#if UNITY_EDITOR_OSX
			ShellHelper.shell("osascript", "-e 'tell app \"Terminal\" to do script \"" +
					"cd " + Application.dataPath.Replace("/Assets", "") + "/" + pathToSubmodule + " ; " +
					"git init ; git add . ; " +
					"git commit -m ✅ ; " +
					"git remote add origin " + remoteUrl + " ; " +
					"git push -u origin master ; " +
					"cd ../.. ; " +
			    	"rm -rf " + Application.dataPath.Replace("/Assets", "") + "/" + pathToSubmodule + " ; " +
			        "rm " + Application.dataPath.Replace("/Assets", "") + "/" + pathToSubmodule + ".meta ; " +
					"git add -u . ; " +
			        "git commit -m " + commit_prefix + " ; " +
			        "git submodule add " + remoteUrl + " " + pathToSubmodule + " -f ; " +
			        "git add . ; " + 
			        "git commit -m " + commit_prefix + " ; " +
			        "killall Terminal ;" +
				"\" activate'");
			#elif
			return;
			#endif
		}
		
		public static void commitAsDbUpdate(string message) {
	        message = message.Replace("\"", "\\\"");
			ShellHelper.FilteredDebugLog(ShellHelper.shell(git_executable, "add .."));
			ShellHelper.FilteredDebugLog(ShellHelper.shell(git_executable, "commit -m \"" + message + "\""));
	    }

	    public static void stashClear() {
			ShellHelper.FilteredDebugLog(ShellHelper.shell(git_executable, "stash"));
			ShellHelper.FilteredDebugLog(ShellHelper.shell(git_executable, "stash clear"));
	    }
	}
}
