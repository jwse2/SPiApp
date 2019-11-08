using SPiApp2.Components.Common;
using SPiApp2.Components.Settings;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace SPiApp2.Components
{
    public static class Mod
    {
        public static bool CreateMod()
        {
            WinCreateMod dialog = new WinCreateMod();

            bool? result = dialog.ShowDialog();
            if (result != null && result.HasValue && result.Value == true)
            {
                RefreshList(true, dialog.SelectedMod);
                return true;
            }

            return false;
        }

        public static void BrowseFolder()
        {
            UserData.Instance.Save();

            string selectedMod = UserData.SelectedMod;
            string installPath = Preferences.InstallPath;
            char sep = System.IO.Path.DirectorySeparatorChar;

            if (!string.IsNullOrWhiteSpace(selectedMod))
            {
                string path = string.Format("{0}{1}mods{1}{2}", installPath, sep, selectedMod);
                SPiApp2.Components.Application.Browse(path);
            }
        }

        public static void BuildFastFile()
        {
            UserData.Instance.Save();

            // Copies the contents of the mod's zone source file
            DynamicZone.CopyModContents();

            SPiApp2.Components.Application.Launch(
                string.Format("{0}{1}bin{1}mod_build.bat", Environment.CurrentDirectory, System.IO.Path.DirectorySeparatorChar),
                Preferences.InstallPath,
                string.Format("\"{0}\" {1} {2}", Preferences.InstallPath, Preferences.Language.ToLower(), UserData.SelectedMod)
            );
        }

        public static void BuildIWD()
        {
            UserData.Instance.Save();

            char sep = System.IO.Path.DirectorySeparatorChar;
            string installPath = Preferences.InstallPath;
            string zipper = Preferences.Zipper;

            if (!File.Exists(zipper))
            {
                zipper = string.Format("{0}{1}{2}", installPath, sep, zipper);
                Debug.Assert(File.Exists(zipper));
            }

            SPiApp2.Components.Application.Launch(
                string.Format("{0}{1}bin{1}mod_iwd.bat", Environment.CurrentDirectory, sep),
                installPath,
                string.Format("\"{0}\" \"{1}\" {2}", installPath, zipper, UserData.SelectedMod)
            );
        }

        public static void UpdateZoneFile()
        {
            UserData.Instance.Save();
            (new WinZoneMod()).ShowDialog();
        }

        public static void RunSelectedMod()
        {
            // First save the settings
            UserData.Instance.Save();

            ModSettings settings = ModSettings.Instance;

            // Check to see if the settings file exists
            if (!settings.Exists())
            {
                AppDialogMessage dialog = new AppDialogMessage("No settings file has been found. Is this a SINGLEPLAYER mod?",
                    "Singleplayer mod?", MessageButtons.YesNo, MessageIcon.Question);

                bool? result = dialog.ShowDialog();
                if (result == null || result.HasValue == false || result.Value != true)
                {
                    // ABORT!
                    return;
                }

                if (dialog.Result == MessageResult.Yes)
                {
                    settings.ModType = "Singleplayer";
                }
                else
                {
                    settings.ModType = "Multiplayer";
                }

                settings.Save();
            }

            string installPath = Preferences.InstallPath;
            string optional = string.Empty;

            // Enable developer
            if (UserData.ModDeveloper)
                optional = string.Format("{0} +set developer 1", optional);

            // Enable developer_scipt
            if (UserData.ModDeveloperScript)
                optional = string.Format("{0} +set developer_script 1", optional);

            // Enable cheats
            if (UserData.ModCheats)
                optional = string.Format("{0} +set thereisacow 1337", optional);

            // Enable custom command line options
            if (UserData.ModCustom)
                optional = string.Format("{0} {1}", optional, UserData.ModOptions);

            SPiApp2.Components.Application.Launch(
                string.Format("{0}{1}bin{1}mod_run.bat", Environment.CurrentDirectory, System.IO.Path.DirectorySeparatorChar),
                installPath,
                string.Format("\"{0}\" {1} \"{2}\" {3} \"{4}\"", installPath, UserData.SelectedMod, Preferences.Executable, GetMultiplayerSign(), optional)
            );
        }

        public static void RefreshList(bool select, string target = null)
        {
            string directory = string.Format("{0}{1}mods", Preferences.InstallPath, System.IO.Path.DirectorySeparatorChar);
            if (Directory.Exists(directory))
            {
                Utility.GetWindow(out MainWindow window);
                ListBox list = window.ctrlModList;

                string[] files = Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly);

                list.SelectionChanged -= ChangedSelectedMod;
                list.Items.Clear();

                foreach (string file in files)
                {
                    int start = directory.Length + 1;
                    int length = file.Length - start;
                    string name = file.Substring(start, length);

                    if (!string.IsNullOrWhiteSpace(name) && Utility.IsValid(name))
                    {
                        list.Items.Add(name);
                    }
                }

                if (select)
                {
                    if (target == null)
                    {
                        target = UserData.SelectedMod;
                    }

                    Utility.SelectValue(ref list, target);

                    string selected = list.SelectedItem as string;
                    Debug.Assert(selected != null);
                    UserData.SelectedMod = selected;

                    ModSettings.Instance.Load();
                }

                list.SelectionChanged += ChangedSelectedMod;
            }
        }

        private static void ChangedSelectedMod(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                object item = e.AddedItems[0];

                if (item != null)
                {
                    string name = item as string;
                    Debug.Assert(name != null);

                    UserData.SelectedMod = name;
                    ModSettings.Instance.Load();
                }
            }
        }

        private static string GetMultiplayerSign()
        {
            string modType = ModSettings.Instance.ModType;

            if (modType.ToLower() != "singleplayer")
            {
                return "1";
            }
            else
            {
                return "0";
            }
        }
    }
}
