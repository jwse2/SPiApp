using SPiApp2.Components.Settings;
using System.IO;

namespace SPiApp2.Components
{
    public static class DynamicZone
    {
        public static void CopyModContents()
        {
            char sep = Path.DirectorySeparatorChar;
            char rep = (sep == '\\') ? '/' : '\\';
            string installPath = Preferences.InstallPath;
            string selectedMod = UserData.SelectedMod;

            string path = string.Format("{0}{1}mods{1}{2}{1}mod.csv", installPath, sep, selectedMod);
            if (File.Exists(path))
            {
                string srcDirectory = string.Format("{0}{1}mods{1}{2}{1}", installPath, sep, selectedMod);
                string dstDirectory = string.Format("{0}{1}raw{1}", installPath, sep);

                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines)
                {
                    string[] tokens = line.Split(',');

                    if (tokens.Length >= 2)
                    {
                        if (tokens[0] == "rawfile")
                        {
                            string relPath = tokens[1].Replace(rep, sep);

                            string srcFile = string.Format("{0}{1}", srcDirectory, relPath);
                            string dstFile = string.Format("{0}{1}", dstDirectory, relPath);

                            if (File.Exists(srcFile))
                            {
                                string prefix = Path.GetDirectoryName(dstFile);
                                if (!Directory.Exists(prefix))
                                {
                                    Directory.CreateDirectory(prefix);
                                }

                                File.Copy(srcFile, dstFile, true);
                            }
                        }
                    }
                }
            }
        }
    }
}
