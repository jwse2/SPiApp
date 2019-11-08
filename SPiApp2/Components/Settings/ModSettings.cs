using SPiApp2.Components.Common;
using System;

namespace SPiApp2.Components.Settings
{
    public class ModSettings : SettingsFile
    {
        private static ModSettings _instance = null;

        public static ModSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ModSettings();
                }

                return _instance;
            }
        }

        private const string MOD_TYPE = "modType";

        private ModSettings() :
            base(false)
        {
            Bind(MOD_TYPE, "Singleplayer");
        }

        protected override string GetPath()
        {
            return string.Format("{0}{1}settings{1}mods{1}{2}.settings",
                Environment.CurrentDirectory, System.IO.Path.DirectorySeparatorChar, UserData.SelectedMod);
        }

        protected override string GetAltPath()
        {
            return null;
        }

        public string ModType
        {
            get
            {
                return GetString(MOD_TYPE);
            }
            set
            {
                SetString(MOD_TYPE, value);
            }
        }
    }
}
