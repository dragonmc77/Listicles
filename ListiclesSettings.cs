using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Listicles
{
    public class ListiclesSettings : ObservableObject
    {
        private string option1 = string.Empty;
        private bool numberedGameLists = false;
        private bool confirmListicleDelete = false;
        private bool showSaveConfirmation = false;
        private bool saveOnExit = false;
        private bool doubleClickPlay = false;
        private bool doubleClickShow = true;
        private string copyFromFolder = string.Empty;
        private string copyToFolder = string.Empty;
        private bool optionThatWontBeSaved = false;

        public string Option1 { get => option1; set => SetValue(ref option1, value); }
        public bool NumberedGameLists { get => numberedGameLists; set => SetValue(ref numberedGameLists, value); }
        public bool ConfirmListicleDelete { get => confirmListicleDelete; set => SetValue(ref confirmListicleDelete, value); }
        public bool ShowSaveConfirmation { get => showSaveConfirmation; set => SetValue(ref showSaveConfirmation, value); }
        public bool SaveOnExit { get => saveOnExit; set => SetValue(ref saveOnExit, value); }
        public bool DoubleClickPlay { get => doubleClickPlay; set => SetValue(ref doubleClickPlay, value); }
        public bool DoubleClickShow { get => doubleClickShow; set => SetValue(ref doubleClickShow, value); }
        public string CopyFromFolder { get => copyFromFolder; set => SetValue(ref copyFromFolder, value); }
        public string CopyToFolder { get => copyToFolder; set => SetValue(ref copyToFolder, value); }
        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonDontSerialize` ignore attribute.
        [DontSerialize]
        public bool OptionThatWontBeSaved { get => optionThatWontBeSaved; set => SetValue(ref optionThatWontBeSaved, value); }
    }

    public class ListiclesSettingsViewModel : ObservableObject, ISettings
    {
        private readonly Listicles plugin;
        private ListiclesSettings editingClone { get; set; }

        private ListiclesSettings settings;
        public ListiclesSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public ListiclesSettingsViewModel(Listicles plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<ListiclesSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new ListiclesSettings();
            }
        }

        public void BeginEdit()
        {
            // Code executed when settings view is opened and user starts editing values.
            editingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
            Settings = editingClone;
        }

        public void EndEdit()
        {
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            if (Settings.CopyFromFolder != String.Empty && !System.IO.Directory.Exists(Settings.CopyFromFolder)) {
                errors.Add("Invalid path specified in CopyFromFolder");
            }
            if (Settings.CopyToFolder != String.Empty && !System.IO.Directory.Exists(Settings.CopyToFolder))
            {
                errors.Add("Invalid path specified in CopyToFolder");
            }
            return errors.Count == 0;
        }
    }
}