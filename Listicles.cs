using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using static Listicles.Listicles;

namespace Listicles
{
    public class Listicles : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private ListiclesSettingsViewModel settings { get; set; }
        public ListiclesViewModel ListiclesViewModel { get; set; }
        private ListiclesView listiclesView { get; set; }
        private AddToListicle addToListicle { get; set; }
        public ListiclesItem SelectedListicle { get; set; }
        public override Guid Id { get; } = Guid.Parse("f353f85c-2d42-4cd2-8ed6-0583c221d16c");
        
        private const string listiclesPath = "listicles.xml";
        private string stubId = "00000000-0000-0000-0000-000000000000";
        public ListiclesSettings Settings;

        public Listicles(IPlayniteAPI api) : base(api)
        {
            settings = new ListiclesSettingsViewModel(this);
            Settings = settings.Settings;
            ListiclesViewModel = new ListiclesViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }
        public class ListiclesGame : Game
        {
            private bool isSelected;

            public bool IsSelected
            {
                get { return isSelected; }
                set { IsSelected = value; }
            }
        }
        public class ListiclesItem : INotifyPropertyChanged
        {
            private FontFamily selectedListicleHighlight = new FontFamily("Segoe UI Light");
            private int gameCount;
            private int stubCount;
            private int selectedGameIndex;
            private string title;
            [XmlAttribute("id")]
            public Guid Id { get; set; }

            [XmlAttribute("index")]
            public int Index { get; set; }

            [XmlElement("title")]
            public string Title
            {
                get
                {
                    return title;
                }
                set
                {
                    title = value;
                    OnPropertyChanged("Title");
                }
            }

            [XmlElement("link")]
            public string Link { get; set; }
            
            [XmlIgnore]
            public FontFamily SelectedListicleHighlight
            {
                get
                {
                    return selectedListicleHighlight;
                }
                set
                {
                    selectedListicleHighlight = value;
                    OnPropertyChanged("SelectedListicleHighlight");
                }
            }
            [XmlIgnore]
            public int SelectedGameIndex
            {
                get
                {
                    return selectedGameIndex;
                }
                set
                {
                    selectedGameIndex = value;
                    OnPropertyChanged("SelectedGameIndex");
                }
            }
            [XmlIgnore]
            public int GameCount
            {
                get { return gameCount; }
                set { gameCount = value; OnPropertyChanged("GameCount"); }
            }
            [XmlIgnore]
            public int StubCount
            {
                get { return stubCount; }
                set { stubCount = value; OnPropertyChanged("StubCount"); }
            }
            [XmlIgnore]
            public ObservableCollection<Game> Games { get; set; }

            public ListiclesItem() : this(Guid.NewGuid().ToString())
            {
                //Id = Guid.NewGuid();
            }

            public ListiclesItem(string id)
            {
                Id = Guid.Parse(id);
                string stubId = "00000000-0000-0000-0000-000000000000";
                Games = new ObservableCollection<Game>();
                Games.CollectionChanged += (sender, changedArgs) =>
                {
                    if (changedArgs.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove | 
                        changedArgs.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add )
                    {
                        this.GameCount = Games.Count;
                        this.StubCount = Games.Where(x => x.Notes == $"Stub{{{stubId}}}").ToList().Count;
                    }
                };
            }

            public event PropertyChangedEventHandler PropertyChanged = delegate { };
            private void OnPropertyChanged(string propertyName)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        [XmlRoot("playnite_listicles", IsNullable = false)]
        public class XmlListicles
        {
            [XmlArray("listicles", IsNullable = false)]
            [XmlArrayItem("listicle", IsNullable = false)]
            public XmlListiclesItem[] XmlListiclesItem;
        }
        public class XmlListiclesItem : ListiclesItem
        {
            [XmlArray("games")]
            [XmlArrayItem("game")]
            public XmlGame[] XmlGames { get; set; }
        }
        public class XmlGame
        {
            [XmlAttribute("id")]
            public string Id;
            [XmlAttribute("index")]
            public string Index;
            [XmlText]
            public string Name;
        }
        public IEnumerable<ListiclesItem> LoadListiclesData()
        {
            string path = System.IO.Path.Combine(GetPluginUserDataPath(), listiclesPath);
            if (File.Exists(path))
            {
                XmlDocument xmlListicles = new XmlDocument();
                xmlListicles.Load(path);

                foreach (XmlElement xmlListicle in xmlListicles.GetElementsByTagName("listicle"))
                {
                    // bring the values into our class from the xml data
                    ListiclesItem listicle = new ListiclesItem
                    {
                        Id = Guid.Parse(xmlListicle.GetAttribute("id")),
                        Index = int.Parse(xmlListicle.GetAttribute("index")),
                        Title = xmlListicle.GetElementsByTagName("title").Item(0).InnerText,
                        Link = xmlListicle.GetElementsByTagName("link").Item(0).InnerText,
                        //Games = new ObservableCollection<Game>()
                    };

                    /* in order to support preserving the order that the user specifies for games in
                     * each listicle, we can't just bring in the games directly from the xml because they
                     * will not necessarily be in the correct order. we need to bring in the games in the
                     * order specified by the index xml attribute of each game, so the interim
                     * step will be to store the gameid's in a SortedDictionary hashtable which automatically
                     * sorts them by key (index), then interate through that. */
                    SortedDictionary<int, string> gameIds = new SortedDictionary<int, string>();
                    foreach (XmlElement xmlGame in xmlListicle.GetElementsByTagName("game"))
                    {
                        gameIds.Add(int.Parse(xmlGame.GetAttribute("index")), xmlGame.GetAttribute("id"));
                    }

                    /* enumerating through gameIds (rather than the xml data itself) will return
                     * the sorted list of games so they can now be added in the correct order */
                    foreach (var gameId in gameIds)
                    {
                        Game game = PlayniteApi.Database.Games.Get(Guid.Parse(gameId.Value));
                        if (game != null)
                        {
                            listicle.Games.Add(game);
                        }
                        /* Check if the game is a stub. Stubs are manual entries that are not in the
                         * user's library. We handle this by adding an empty game object to the list */
                        else if (gameId.Value == stubId)
                        {
                            string name = xmlListicle.SelectSingleNode($"games/game[@index='{gameId.Key}']").InnerText;
                            Game newGame = new Game(name);
                            newGame.Id = Guid.NewGuid();
                            newGame.Notes = $"Stub{{{stubId}}}";
                            newGame.Icon = "icon_stub.png";
                            listicle.Games.Add(newGame);
                        }
                        else
                        {
                            PlayniteApi.Notifications.Add($"{gameId}-LoadListiclesFile", $"Game not found: {gameId}", NotificationType.Error);
                        }
                    }

                    yield return listicle;
                }
            }
            else
            {
                PlayniteApi.Notifications.Add($"{Id}-LoadListiclesFile", $"Listicles file not found: {path}", NotificationType.Error);
            }
        }

        /// <summary>
        /// Saves all Listicles data to disk.
        /// </summary>
        /// <remarks>
        /// The method used to write the data back to the XML file simply uses the .NET
        /// serializer. The data is copied to "XML friendly" objects (tagged with XML
        /// attributes) after which the .Serialize method is called to write the XML to file.
        /// </remarks>
        public bool UpdateListiclesData()
        {
            XmlListicles xmlListicles = new XmlListicles();
            xmlListicles.XmlListiclesItem = new XmlListiclesItem[ListiclesViewModel.Listicles.Count];

            foreach (ListiclesItem listicle in ListiclesViewModel.Listicles)
            {
                XmlListiclesItem item = new XmlListiclesItem
                {
                    Id = listicle.Id,
                    Index = ListiclesViewModel.Listicles.IndexOf(listicle),
                    Title = listicle.Title,
                    Link = listicle.Link,
                    XmlGames = new XmlGame[listicle.Games.Count]
                };

                int index = 0;
                foreach (Game game in listicle.Games)
                {
                    XmlGame xmlGame = new XmlGame();
                    xmlGame.Id = game.Notes?.Contains($"Stub{{{stubId}}}") == true ? stubId : game.Id.ToString();
                    xmlGame.Name = game.Notes?.Contains($"Stub{{{stubId}}}") == true ? game.Name : String.Empty;
                    xmlGame.Index = index.ToString();
                    item.XmlGames[index] = xmlGame;
                    index++;
                }

                xmlListicles.XmlListiclesItem[listicle.Index] = item;
            }
            string path = System.IO.Path.Combine(GetPluginUserDataPath(), listiclesPath);
            XmlSerializer serializer = new XmlSerializer(typeof(XmlListicles));
            TextWriter writer = new StreamWriter($"{path}");
            serializer.Serialize(writer, xmlListicles);
            writer.Close();
            return true;
        }
        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            yield return new GameMenuItem
            {
                Description = "Add to Listicle",
                Icon = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "listicles.png"),
                Action = (itemArgs) =>
                {
                    var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
                    {
                        ShowMinimizeButton = false
                    });
                    addToListicle = new AddToListicle(ListiclesViewModel, window, PlayniteApi.MainView.SelectedGames.ToList());
                    window.Content = addToListicle;
                    window.SizeToContent = SizeToContent.WidthAndHeight;
                    window.ResizeMode = ResizeMode.NoResize;
                    window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.Title = "Add to Listicle";
                    window.ShowDialog();

                    if (window.DialogResult == true)
                    {
                        PlayniteApi.Notifications.Add($"{Id}-AddToListicle", $"Listicle selected: {ListiclesViewModel.CurrentListicle.Title}", NotificationType.Info);
                    }
                    
                    foreach (Game game in args.Games)
                    {

                        //TODO: Prompt for Listicle to add games to
                    }
                }
            };
        }
        public override IEnumerable<SidebarItem> GetSidebarItems()
        {
            yield return new SidebarItem
            {
                Title = "Listicles",
                Type = SiderbarItemType.View,
                Icon = new TextBlock
                {
                    Text = char.ConvertFromUtf32(0xef74),
                    FontSize = 20,
                    FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily
                },
                Opened = () => {
                    if (ListiclesViewModel == null)
                    {
                        ListiclesViewModel = new ListiclesViewModel(this);
                        listiclesView = new ListiclesView(ListiclesViewModel);
                    }
                    listiclesView = new ListiclesView(ListiclesViewModel);
                    
                    return listiclesView;
                }
            };
        }
        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            // Add code to be executed when game is finished installing.
        }
        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            // Add code to be executed when game is started running.
        }
        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }
        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            // Add code to be executed when game is preparing to be started.
        }
        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            // Add code to be executed when game is uninstalled.
        }
        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            if (Settings.CopyFromFolder != String.Empty)
            {
                try
                {
                    string copyFrom = System.IO.Path.Combine(Settings.CopyFromFolder, listiclesPath);
                    string copyTo = System.IO.Path.Combine(GetPluginUserDataPath(), listiclesPath);
                    if (File.GetLastWriteTime(copyFrom) > File.GetLastWriteTime(copyTo))
                    {
                        File.Copy(copyTo, System.IO.Path.Combine(GetPluginUserDataPath(), $"{listiclesPath}.bak"), true);
                        File.Copy(copyFrom, copyTo, true);
                    }
                }
                catch ( Exception ex )
                {
                    logger.Error(ex, "Error copying Listicles data file in OnApplicationStarted");
                    PlayniteApi.Notifications.Add($"{Id}-CopyFile", $"Listicles plugin could not copy file: {ex.Message}", NotificationType.Error);
                }
            }
            // Add code to be executed when Playnite is initialized.
            try
            {
                // Initialization is done inside OnApplicationStarted, otherwise
                // LoadListiclesData runs too early in Playnite's startup and
                // cannot call PlayniteApi.Database.Games.Get()

                ListiclesViewModel.Listicles = new ObservableCollection<ListiclesItem>(LoadListiclesData().OrderBy(x => x.Index));
                ListiclesViewModel.Listicles.CollectionChanged += (sender, changedArgs) =>
                {
                    //UpdateListiclesData();
                };
                PlayniteApi.Database.Games.ItemCollectionChanged += (sender, changedArgs) =>
                {
                    foreach (Game playniteGame in changedArgs.RemovedItems)
                    {
                        foreach (ListiclesItem list in ListiclesViewModel.Listicles)
                        {
                            list.Games.Remove(playniteGame);
                        }
                    }
                };
            }
            catch (Exception e)
            {
                logger.Error(e, "Error loading Listicles data file in OnApplicationStarted");
                PlayniteApi.Notifications.Add($"{Id}-OnApplicationStarted", $"Listicles plugin could not load file: {e.Message}", NotificationType.Error);
            }
        }
        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            // Add code to be executed when Playnite is shutting down.
            if (Settings.SaveOnExit)
            {
                UpdateListiclesData();
            }
            
            if (Settings.CopyToFolder != String.Empty)
            {
                try
                {
                    string copyFrom = System.IO.Path.Combine(GetPluginUserDataPath(), listiclesPath);
                    string copyTo = System.IO.Path.Combine(Settings.CopyToFolder, listiclesPath);
                    if (File.GetLastWriteTime(copyFrom) > File.GetLastWriteTime(copyTo))
                    {
                        //File.Copy(copyTo, System.IO.Path.Combine(GetPluginUserDataPath(), $"{listiclesPath}.bak"), true);
                        File.Copy(copyFrom, copyTo, true);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error copying Listicles data file in OnApplicationStopped");
                    PlayniteApi.Notifications.Add($"{Id}-CopyFile", $"Listicles plugin could not copy file: {ex.Message}", NotificationType.Error);
                }
            }
        }
        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            // Add code to be executed when library is updated.
        }
        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }
        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new ListiclesSettingsView();
        }
    }

    public class ListiclesViewModel : ObservableObject
    {
        private readonly Listicles plugin;
        private Listicles editingClone { get; set; }
        private string stubId = "00000000-0000-0000-0000-000000000000";

        private ObservableCollection<ListiclesItem> listicles;

        public ListiclesItem CurrentListicle;
        public Game CurrentGame;
        public RelayCommand<Game> ShowGameInLibrary { get; }

        public bool ShowNumbers;
        public bool ConfirmListicleDelete;

        public void UpdateListiclesData()
        {
            plugin.UpdateListiclesData();
        }
        public ObservableCollection<ListiclesItem> Listicles
        {
            get => listicles;
            set
            {
                listicles = value;
                //OnPropertyChanged();
            }
        }
        public ListiclesViewModel(Listicles plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;
            var settings = plugin.GetSettings(false);
            
            // Load saved data
            //var listiclesData = plugin.LoadListiclesData().OrderBy(x => x.Index);

            // LoadPluginSettings returns null if no saved data is available.
            //if (listiclesData != null)
            //{
            //    Listicles = listiclesData.ToObservable();
            //}
            //else
            //{
            //    //Listicles = new IEnumerable<ListiclesItem>();
            //}
            ShowGameInLibrary = new RelayCommand<Game>((game) =>
            {
                if (game == null | game?.Notes?.Contains($"Stub{{{stubId}}}") == true)
                {
                    return;
                }
                // This does select the game, but doesn't always scroll it into view. seems spotty
                plugin.PlayniteApi.MainView.SelectGame(game.Id);
                plugin.PlayniteApi.MainView.SwitchToLibraryView();
            });
        }

        public Listicles Plugin
        {
            get { return plugin; }
        }

        public Game AddStub()
        {
            StringSelectionDialogResult input = plugin.PlayniteApi.Dialogs.SelectString("Enter the name of the game to add.", "Add Stub", "");
            if (input.Result & input.SelectedString != String.Empty)
            {
                Game newGame = new Game(input.SelectedString);
                newGame.Id = Guid.NewGuid();
                newGame.Notes = $"Stub{{{stubId}}}";
                newGame.Icon = "icon_stub.png";
                CurrentListicle.Games.Insert(CurrentListicle.SelectedGameIndex + 1, newGame);
                return newGame;
            }
            return null;
        }

        public void EditListicle(ListiclesItem selectedListicle)
        {
            var window = plugin.PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
            {
                ShowMinimizeButton = false
            });
            EditListicle editListicle = new EditListicle(window, selectedListicle);
            window.Content = editListicle;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.ResizeMode = ResizeMode.NoResize;
            window.Owner = plugin.PlayniteApi.Dialogs.GetCurrentAppWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Title = "Edit Listicle";
            window.ShowDialog();

            if (window.DialogResult == true)
            {
                if (editListicle.NewTitle != String.Empty)
                {
                    selectedListicle.Title = editListicle.NewTitle;
                }
                if (editListicle.NewLink != String.Empty)
                {
                    selectedListicle.Link = editListicle.NewLink;
                }
            }
            else
            {
                //plugin.PlayniteApi.Notifications.Add($"{plugin.Id}-AddToListicle", "Listicle selection cancelled", NotificationType.Info);
            }
        }
    }
}