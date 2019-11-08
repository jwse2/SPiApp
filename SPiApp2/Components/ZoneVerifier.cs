using SPiApp2.Components.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPiApp2.Components
{
    public static class ZoneVerifier
    {
        /// <summary>
        /// Validate the zone file data.
        /// </summary>
        /// <param name="data">The zone file data.</param>
        /// <returns>true if valid; otherwise, false.</returns>
        public static bool Validate(string data)
        {
            return Validate(data.Split(/* '\r', */ '\n'));
        }

        /// <summary>
        /// Validate the text lines of a zone file.
        /// </summary>
        /// <param name="data">The zone file's lines of text.</param>
        /// <returns>true if valid; otherwise, false.</returns>
        public static bool Validate(string[] lines)
        {
            int numLine = 0;

            foreach (string line in lines)
            {
                numLine++;

                // Empty lines and comments are allowed, therefor check these first
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                {
                    continue;
                }

                string[] tokens = line.Split(',');
                if (tokens.Length < 2)
                {
                    AppDialogMessage.Show(string.Format("Invalid number of tokens on line {0} of the zone file.", numLine),
                        "Invalid zone file", MessageButtons.OK, MessageIcon.Error);
                    return false;
                }

                for (int i = 0; i < tokens.Length; i++)
                {
                    tokens[i] = tokens[i].Trim();

                    if (string.IsNullOrWhiteSpace(tokens[i]))
                    {
                        AppDialogMessage.Show(string.Format("One of more fields at line {0} are empty.", numLine),
                            "Invalid field", MessageButtons.OK, MessageIcon.Warning);
                    }
                }

                string keyword = tokens[0];
                bool resolved = false;

                switch (keyword)
                {
                    // ignore,<zone_file>
                    // include,<zone_file>
                    // [cod4]\zone_source\<zone_file>.csv
                    case "ignore":
                    case "include":
                        resolved = Validate_ExternalZone(ref tokens);
                        break;

                    // rawfile,<rawfile_name>
                    // [cod4]\raw\<rawfile_name>
                    case "rawfile":
                        resolved = Validate_RawFile(ref tokens);
                        break;

                    // menufile,<menufile_name>
                    // [cod4]\raw\<menufile_name>
                    case "menufile":
                        resolved = Validate_MenuFile(ref tokens);
                        break;

                    // localize,<localized_file>
                    // [cod4]\raw\<language>\localizedstrings\<localized_file>.str
                    case "localize":
                        resolved = Validate_Localize(ref tokens);
                        break;

                    // xanim,<xanim_name>
                    // [cod4]\raw\xanims\<xanim_name>
                    case "xanim":
                        resolved = Validate_Animation(ref tokens);
                        break;

                    // xmodel,<xmodel_name>
                    // [cod4]\raw\xmodels\<xmodel_name>
                    case "xmodel":
                        resolved = Validate_Model(ref tokens);
                        break;

                    // weapon,<weapon_file>
                    // [cod4]\raw\weapons\<weapon_file>
                    case "weapon":
                        resolved = Validate_Weapon(ref tokens);
                        break;

                    // col_map_**,<bsp_file>
                    // [cod4]\raw\<bsp_file>
                    case "col_map_sp":
                    case "col_map_mp":
                        resolved = Validate_CollisionMap(ref tokens);
                        break;

                    // fx,<filename>
                    // [cod4]\raw\fx\<filename>.efx
                    case "fx":
                        resolved = Validate_Effect(ref tokens);
                        break;

                    // sound,<soundalias_name>,<map_name>,<modifier>
                    // [cod4]\raw\soundaliases\<soundalias_name>.csv
                    case "sound":
                        resolved = Validate_Sound(ref tokens);
                        break;

                    // material,<material_name>
                    // [cod4]\raw\materials\<material_name>
                    case "material":
                        resolved = Validate_Material(ref tokens);
                        break;

                    // ui_map,<filename>
                    // [cod4]\raw\<filename>
                    case "ui_map":
                        resolved = Validate_InterfaceMap(ref tokens);
                        break;

                    // impactfx,<map_name>
                    // [cod4]\raw\fx\maps\<map_name>\iw_impacts.csv
                    case "impactfx":
                        resolved = Validate_ImpactEffects(ref tokens);
                        break;

                    default:
                        AppDialogMessage.Show(string.Format("Unknown keyword '{0}' found. Skipping validation of line {1}.", tokens[0], numLine),
                            "Unknown keyword", MessageButtons.OK, MessageIcon.Warning);
                        continue;
                }

                if (!resolved)
                {
                    AppDialogMessage.Show(string.Format("Could not resolve {0} at line {1}.", keyword, numLine),
                        "Unresolved zone file", MessageButtons.OK, MessageIcon.Warning);
                }
            }

            return true;
        }

        private static bool Validate_TwoTokenFile(string format, ref string[] tokens)
        {
            if (tokens.Length == 2)
            {
                string path = string.Format(format, Preferences.InstallPath, Path.DirectorySeparatorChar, tokens[1]);
                return File.Exists(path);
            }

            return false;
        }

        private static bool Validate_ExternalZone(ref string[] tokens)
        {
            return Validate_TwoTokenFile("{0}{1}zone_source{1}{2}.csv", ref tokens);
        }

        private static bool Validate_RawFile(ref string[] tokens)
        {
            return Validate_TwoTokenFile("{0}{1}raw{1}{2}", ref tokens);
        }

        private static bool Validate_MenuFile(ref string[] tokens)
        {
            return Validate_TwoTokenFile("{0}{1}raw{1}{2}", ref tokens);
        }

        private static bool Validate_Localize(ref string[] tokens)
        {
            Debug.WriteLine(string.Format("\tkeyword: {0}", tokens[0]));
            return false;
        }

        private static bool Validate_Animation(ref string[] tokens)
        {
            return Validate_TwoTokenFile("{0}{1}raw{1}xanim{1}{2}", ref tokens);
        }

        private static bool Validate_Model(ref string[] tokens)
        {
            return Validate_TwoTokenFile("{0}{1}raw{1}xmodel{1}{2}", ref tokens);
        }

        private static bool Validate_Weapon(ref string[] tokens)
        {
            return Validate_TwoTokenFile("{0}{1}raw{1}weapons{1}{2}", ref tokens);
        }

        private static bool Validate_CollisionMap(ref string[] tokens)
        {
            return Validate_TwoTokenFile("{0}{1}raw{1}{2}", ref tokens);
        }

        private static bool Validate_Effect(ref string[] tokens)
        {
            return Validate_TwoTokenFile("{0}{1}raw{1}fx{1}{2}", ref tokens);
        }

        private static bool Validate_Sound(ref string[] tokens)
        {
            if (tokens.Length == 4)
            {
                string aliases_name = tokens[1];
                string keyword = tokens[2];
                string modifier = tokens[3];           

                string path = string.Format("{0}{1}raw{1}soundaliases{1}{2}.csv", Preferences.InstallPath, Path.DirectorySeparatorChar, tokens[1]);
                return File.Exists(path);
            }

            return false;
        }

        private static bool Validate_Material(ref string[] tokens)
        {
            return Validate_TwoTokenFile("{0}{1}raw{1}materials{1}{2}", ref tokens);
        }

        private static bool Validate_InterfaceMap(ref string[] tokens)
        {
            return Validate_TwoTokenFile("{0}{1}raw{1}{2}", ref tokens);
        }

        private static bool Validate_ImpactEffects(ref string[] tokens)
        {
            Debug.WriteLine(string.Format("\tkeyword: {0}", tokens[0]));
            return false;
        }

        private static bool Validate_(ref string[] tokens)
        {
            Debug.WriteLine(string.Format("\tkeyword: {0}", tokens[0]));
            return false;
        }
    }
}
