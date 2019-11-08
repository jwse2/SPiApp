﻿using SPiApp2.Components.Common;
using SPiApp2.Components.Settings;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;
using ComTemplate = SPiApp2.Components.Common.Template;

namespace SPiApp2
{
    /// <summary>
    /// Interaction logic for WinCreateMod.xaml
    /// </summary>
    public partial class WinCreateMod : SPiApp2.Controls.CompilerWindow
    {
        /// <summary>
        /// Get the name of the selected mod.
        /// </summary>
        public string SelectedMod
        {
            get;
            private set;
        }

        /// <summary>
        /// Called when the Create Map dialog is created.
        /// </summary>
        public WinCreateMod()
        {
            InitializeComponent();
            CenterOnParent();
            Loaded += OnLoaded;
        }

        public override void GetWindowChrome(out WindowChrome chrome)
        {
            chrome = this.chrome;
        }

        /// <summary>
        /// Called when the window has completed initialization.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            List<string> templates = new List<string>();
            ComTemplate.GetTemplates(ref templates, TemplateType.Mods);

            ctrlTemplates.Items.Clear();
            foreach (string template in templates)
            {
                ctrlTemplates.Items.Add(template);
            }

            Utility.SelectValue(ref ctrlTemplates, UserData.SelectedModTemplate);
            Utility.SelectValue(ref ctrlTypes, UserData.SelectedModType);
        }

        /// <summary>
        /// Called when the ENTER key is pressed when typing the name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyDown_CreateMod(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Click_CreateMod(null, null);
            }
        }

        /// <summary>
        /// Saves the mod's type into its persistence file.
        /// </summary>
        /// <param name="name">The name of the mod.</param>
        /// <param name="type">Teh type of the mod.</param>
        private void SaveModType(string name, string type)
        {
            ModSettings settings = ModSettings.Instance;
            settings.ModType = type;
            settings.Save();
        }

        /// <summary>
        /// Called when the user wishes to create the mod.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Click_CreateMod(object sender, RoutedEventArgs e)
        {
            // Save the selected template
            string template = ctrlTemplates.SelectedItem as string;
            if (!string.IsNullOrEmpty(template))
            {
                UserData.SelectedModTemplate = template;
            }

            // Save the selected type
            ComboBoxItem item = ctrlTypes.SelectedItem as ComboBoxItem;
            Debug.Assert(item != null);
            string type = item.Content as string;
            Debug.Assert(type != null);
            UserData.SelectedModType = type;

            // Save the data
            UserData.Instance.Save();

            // Get the mod name
            string modName = ctrlModName.Text;
            if (!Utility.IsValid(modName))
            {
                AppDialogMessage.Show(string.Format("The given mod name of '{0}' is not valid.", modName),
                    "Invalid mod name", MessageButtons.OK, MessageIcon.Warning);
            }
            else
            {
                SelectedMod = modName;
                ComTemplate.Install(TemplateType.Mods, template, modName);
                SaveModType(modName, type);
                DialogResult = true;
            }
        }
    }
}
