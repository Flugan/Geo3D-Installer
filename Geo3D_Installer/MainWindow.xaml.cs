using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace Geo3D_Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Game> gameList = new List<Game>();
        private List<Game> gameGeo3D = new List<Game>();

        static public string[] specialCases = { };

        public string reduceSlashes(string input)
        {
            string libSlashless = "";
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\\')
                {
                    i++;
                    libSlashless += input[i];
                }
                else if (input[i] == '/')
                {
                    libSlashless += '\\';
                }
                else
                {
                    libSlashless += input[i];
                }
            }
            return libSlashless;
        }

        public MainWindow()
        {
            InitializeComponent();

            if (System.IO.File.Exists("data\\special.txt"))
            {
                specialCases = System.IO.File.ReadAllLines("data\\special.txt");
            }
        }

        private void filterGames()
        {
            gameListBox.Items.Clear();
            foreach (var game in gameList)
            {
                if (System.IO.File.Exists(game.path + "\\Geo3D.txt"))
                    continue;
                
                if (Search.Text == "")
                {
                    gameListBox.Items.Add(game);
                    continue;
                }
                string[] searchTerms = Search.Text.ToLower().Split(' ');
                string gameText = game.name.ToLower();
                int matched = 0;
                foreach (var term in searchTerms)
                {
                    if (gameText.Contains(term))
                    {
                        matched++;
                    }
                    else
                    {
                        break;
                    }
                }
                if (searchTerms.Length == matched)
                {
                    gameListBox.Items.Add(game);
                }
            }
            gameCount.Content = "Total Count: " + gameList.Count;
        }

        private void sortGames()
        {
            gameList.Sort((Game x, Game y) => String.Compare(x.name, y.name));
            filterGames();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            filterGames();
        }

        private void sortGeo3D()
        {
            gameGeo3D.Sort((Game x, Game y) => String.Compare(x.name, y.name));
        }

        private void updateGeo3D()
        {
            geo3DBox.Items.Clear();
            sortGeo3D();
            foreach (var item in gameGeo3D)
            {
                geo3DBox.Items.Add(item);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // for .NET Core you need to add UseShellExecute = true
            // see https://learn.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.useshellexecute#property-value
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        void installGame(Game currentGame, int dxVersion)
        {
            if (currentGame == null)
                return;

            var installDir = System.IO.Path.GetDirectoryName(currentGame.path + "\\" + currentGame.exe);

            if (xSR.IsChecked == true)
            {
                if (currentGame.bits == "x86")
                {
                    MessageBox.Show("32-bit unsupported on Simulated Reality");
                    return;
                }
                else
                {
                    string folder;
                    bool found = false;
                    while (true)
                    {
                        folder = "C:\\Program Files\\Simulated Reality\\SR Platform\\bin";
                        if (Directory.Exists(folder))
                        {
                            found = true; break;
                        }
                        folder = "C:\\Program Files\\Simulated Reality\\SDK\\bin";
                        if (Directory.Exists(folder))
                        {
                            found = true; break;
                        }
                        folder = "C:\\Program Files\\Acer\\SpatialLabs\\SR Platform\\bin";
                        if (Directory.Exists(folder))
                        {
                            found = true; break;
                        }
                        break;
                    }
                    if (found)
                    {
                        System.IO.File.Copy("srReshade Alpha\\srReshade_v0.1.0.addon64", installDir + "\\srReshade_v0.1.0.addon64", true);
                        System.IO.File.Copy(folder + "\\DimencoWeaving.dll", installDir + "\\DimencoWeaving.dll", true);
                        System.IO.File.Copy(folder + "\\Glog.dll", installDir + "\\Glog.dll", true);
                        System.IO.File.Copy(folder + "\\Opencv_world343.dll", installDir + "\\Opencv_world343.dll", true);
                        System.IO.File.Copy(folder + "\\SimulatedReality.dll", installDir + "\\SimulatedReality.dll", true);
                        System.IO.File.Copy(folder + "\\SimulatedRealityCore.dll", installDir + "\\SimulatedRealityCore.dll", true);
                        System.IO.File.Copy(folder + "\\SimulatedRealityDirectX.dll", installDir + "\\SimulatedRealityDirectX.dll", true);
                        System.IO.File.Copy(folder + "\\SimulatedRealityDisplays.dll", installDir + "\\SimulatedRealityDisplays.dll", true);
                        System.IO.File.Copy(folder + "\\SimulatedRealityFacetrackers.dll", installDir + "\\SimulatedRealityFacetrackers.dll", true);
                    }
                    else
                    {
                        MessageBox.Show("No Simulated Reality display found!");
                        return;
                    }
                }
            }

            System.IO.File.WriteAllText(currentGame.path + "\\Geo3D.txt", "");

            if (dxVersion != 0)
            {
                System.IO.File.Copy("ReShade\\3DToElse.fx", installDir + "\\3DToElse.fx", true);
                System.IO.File.Copy("ReShade\\ReShadePreset.ini", installDir + "\\ReShadePreset.ini", true);
                if (!System.IO.File.Exists(installDir + "\\ReShade.ini"))
                    System.IO.File.Copy("ReShade\\ReShade.ini", installDir + "\\ReShade.ini", true);
                if (xVR.IsChecked == true)
                {
                    System.IO.File.Copy("VR\\VRExport\\3DToElse.fx", installDir + "\\3DToElse.fx", true);
                     if (currentGame.bits == "x86")
                    {
                        System.IO.File.Copy("VR\\VRExport\\VRExport.addon32", installDir + "\\VRExport.addon32", true);
                    }
                    else
                    {
                        System.IO.File.Copy("VR\\VRExport\\VRExport.addon64", installDir + "\\VRExport.addon64", true);
                    }
                }
            }

            if (currentGame.bits == "x86")
            {
                if (dxVersion == 9)
                {
                    System.IO.File.Copy("ReShade\\ReShade32.dll", installDir + "\\d3d9.dll", true);
                    System.IO.File.Delete(installDir + "\\dxgi.dll");
                }
                else if (dxVersion == 10)
                {
                    System.IO.File.Copy("ReShade\\ReShade32.dll", installDir + "\\dxgi.dll", true);
                    System.IO.File.Delete(installDir + "\\d3d9.dll");
                }
                else if (dxVersion == 0)
                {
                    try
                    {
                        if (System.IO.File.Exists(installDir + "\\dxgi.dll"))
                            System.IO.File.Copy("ReShade\\ReShade32.dll", installDir + "\\dxgi.dll", true);
                        if (System.IO.File.Exists(installDir + "\\d3d9.dll"))
                            System.IO.File.Copy("ReShade\\ReShade32.dll", installDir + "\\d3d9.dll", true);
                    }
                    catch { }
                }
                System.IO.File.Copy("Geo3D\\Geo3D.addon32", installDir + "\\Geo3D.addon32", true);
            }
            if (currentGame.bits == "x64")
            {
                if (dxVersion == 9)
                {
                    System.IO.File.Copy("ReShade\\ReShade64.dll", installDir + "\\d3d9.dll", true);
                    System.IO.File.Delete(installDir + "\\dxgi.dll");
                    System.IO.File.Delete(installDir + "\\d3d12.dll");
                }
                else if (dxVersion == 10)
                {
                    System.IO.File.Copy("ReShade\\ReShade64.dll", installDir + "\\dxgi.dll", true);
                    System.IO.File.Delete(installDir + "\\d3d9.dll");
                    System.IO.File.Delete(installDir + "\\d3d12.dll");
                }
                else if (dxVersion == 12)
                {
                    System.IO.File.Copy("ReShade\\ReShade64.dll", installDir + "\\d3d12.dll", true);
                    System.IO.File.Delete(installDir + "\\d3d9.dll");
                    System.IO.File.Delete(installDir + "\\dxgi.dll");
                }
                else if (dxVersion == 0)
                {
                    try
                    {
                        if (System.IO.File.Exists(installDir + "\\dxgi.dll"))
                            System.IO.File.Copy("ReShade\\ReShade64.dll", installDir + "\\dxgi.dll", true);
                        if (System.IO.File.Exists(installDir + "\\d3d9.dll"))
                            System.IO.File.Copy("ReShade\\ReShade64.dll", installDir + "\\d3d9.dll", true);
                        if (System.IO.File.Exists(installDir + "\\d3d12.dll"))
                            System.IO.File.Copy("ReShade\\ReShade64.dll", installDir + "\\d3d12.dll", true);
                    } catch { }
                }
                System.IO.File.Copy("Geo3D\\Geo3D.addon64", installDir + "\\Geo3D.addon64", true);
                if (!System.IO.File.Exists(installDir + "\\dxcompiler.dll"))
                    System.IO.File.Copy("DXIL\\dxcompiler.dll", installDir + "\\dxcompiler.dll", true);
                System.IO.File.Copy("DXIL\\dxil.dll", installDir + "\\dxil.dll", true);
            }

            gameGeo3D.Clear();
            foreach (var game in gameList)
            {
                if (System.IO.File.Exists(game.path + "\\Geo3D.txt"))
                {
                    gameGeo3D.Add(game);
                }
            }
            updateGeo3D();
            filterGames();
        }

        private void install_Click(object sender, RoutedEventArgs e)
        {
            var currentGame = (Game)gameListBox.SelectedItem;
            if (currentGame == null)
                return;

            currentGame.Expand();
            if (currentGame.bits=="x86")
            {
                installGame(currentGame, 9);
            }
            else
            {
                installGame(currentGame, 10);
            }
        }

        private void uninstall_Click(object sender, RoutedEventArgs e)
        {
            var currentGame = (Game)geo3DBox.SelectedItem;
            if (currentGame == null)
                return;

            System.IO.File.Delete(currentGame.path + "\\Geo3D.txt");
            string combinedPath = Directory.GetParent(currentGame.path + "\\" + currentGame.exe).ToString();
            System.IO.File.Delete(combinedPath + "\\3DToElse.fx");
            System.IO.File.Delete(combinedPath + "\\Geo3D.addon");
            System.IO.File.Delete(combinedPath + "\\Geo3D.addon32");
            System.IO.File.Delete(combinedPath + "\\Geo3D.addon64");

            System.IO.File.Delete(combinedPath + "\\d3d9.dll");
            System.IO.File.Delete(combinedPath + "\\d3d12.dll");
            System.IO.File.Delete(combinedPath + "\\dxgi.dll");

            System.IO.File.Delete(combinedPath + "\\ReShade.log");
            System.IO.File.Delete(combinedPath + "\\ReShade.log2");
            System.IO.File.Delete(combinedPath + "\\ReShadePreset.ini");

            var fi = new FileInfo(combinedPath + "\\dxcompiler.dll");
            if (fi.Exists && fi.Length == 17107352)
            {
                fi.Delete();
            }
            System.IO.File.Delete(combinedPath + "\\dxil.dll");
            System.IO.File.Delete(combinedPath + "\\VRExport.addon32");
            System.IO.File.Delete(combinedPath + "\\VRExport.addon64");
            System.IO.File.Delete(combinedPath + "\\srReshade_v0.0.0.addon64");
            System.IO.File.Delete(combinedPath + "\\srReshade_v0.1.0.addon64");
            System.IO.File.Delete(combinedPath + "\\srReshade_v0.2.0.addon64");
            System.IO.File.Delete(combinedPath + "\\DimencoWeaving.dll");
            System.IO.File.Delete(combinedPath + "\\Glog.dll");
            System.IO.File.Delete(combinedPath + "\\Opencv_world343.dll");
            System.IO.File.Delete(combinedPath + "\\SimulatedReality.dll");
            System.IO.File.Delete(combinedPath + "\\SimulatedRealityCore.dll");
            System.IO.File.Delete(combinedPath + "\\SimulatedRealityDirectX.dll");
            System.IO.File.Delete(combinedPath + "\\SimulatedRealityDisplays.dll");
            System.IO.File.Delete(combinedPath + "\\SimulatedRealityFacetrackers.dll");

            gameGeo3D.Clear();
            foreach (var game in gameList)
            {
                if (System.IO.File.Exists(game.path + "\\Geo3D.txt"))
                {
                    gameGeo3D.Add(game);
                }
            }
            updateGeo3D();
            filterGames();
        }

         private bool popupExplorer = false;

        private void gameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (popupExplorer)
            {
                Game g = (Game)gameListBox.SelectedItem;
                if (g != null)
                {
                    Process.Start(g.path);
                }
            }
        }

        private void popup_Click(object sender, RoutedEventArgs e)
        {
            if (popupExplorer)
            {
                gameListBox.Items.Clear();
                foreach (var game in gameList)
                {
                    gameListBox.Items.Add(game);
                }
                popup.Content = "Filter";
                popupExplorer = false;
            }
            else
            {
                gameListBox.Items.Clear();
                foreach (var game in gameList)
                {
                    if (game.exe == "")
                    {
                        gameListBox.Items.Add(game);
                    }
                }
                popup.Content = "Popup";
                popupExplorer = true;
            }
        }

        private void exeButton_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            foreach (var game in gameList)
            {
                if (game.exe == "")
                    game.Expand();
                gameCount.Content = "Total Count: " + (++i) + "/" + gameList.Count;
            }
            popup.Visibility = Visibility.Visible;
            if (gameList.Count() != 0) {
                bool exe = !gameList.First().displayExe;
                foreach (var game in gameList)
                {
                    game.displayExe = exe;
                }
            }
            gameListBox.Items.Clear();
            foreach (var game in gameList)
            {
                if (popupExplorer)
                {
                    if (game.exe == "")
                    {
                        gameListBox.Items.Add(game);
                    }
                }
                else
                {
                    gameListBox.Items.Add(game);
                }
            }
            filterGames();
        }

        private void pathButton_Click(object sender, RoutedEventArgs e)
        {
            if (gameList.Count() != 0)
            {
                bool path = !gameList.First().displayPath;
                foreach (var game in gameList)
                {
                    game.displayPath = path;
                }
            }
            gameListBox.Items.Clear();
            foreach (var game in gameList)
            {
                if (popupExplorer)
                {
                    if (game.exe == "")
                    {
                        gameListBox.Items.Add(game);
                    }
                }
                else
                {
                    gameListBox.Items.Add(game);
                }
            }
            filterGames();
        }

        void addGame(string name, string path)
        {
            Game g;
            if (name == "AFOP")
            {
                addGame("Avatar: Frontiers of Pandora", path);
            }
            else if (name == "World of Warcraft")
            {
                g = new Game(name, path + @"\_retail_");
                gameList.Add(g);
                addGame(name + ": Classic", path + @"\_classic_");
                addGame(name + ": Classic Era", path + @"\_classic_era_");
            }
            else if (name == "Homeworld Remastered Collection")
            {
                g = new Game(name, path + @"\HomeworldRM\Bin\Release");
                gameList.Add(g);
                addGame(name + ": 1 Classic", path + @"\Homeworld1Classic\exe");
                addGame(name + ": 2 Classic", path + @"\Homeworld2Classic\Bin\Release");
            }
            else if (name == "BioShock 2")
            {
                addGame(name + ": Online", path + @"\MP");
                addGame(name + ": Offline", path + @"\SP");
            }
            else if (name == "Batman™: Arkham Origins")
            {
                addGame(name + ": Offline", path + @"\SinglePlayer");
                addGame(name + ": Online", path + @"\Online");
            }
            else if (name == "Tom Clancy's Splinter Cell: Double Agent")
            {
                addGame(name + ": Offline", path + @"\SCDA-Offline");
                addGame(name + ": Online", path + @"\SCDA-Online");
            }
            else if (name == "Commandos: Beyond the Call of Duty")
            {
                addGame(name + ": Legacy", path + @"\Legacy");
                g = new Game(name, path);
                gameList.Add(g);
            }
            else if (name == "Little Nightmares II")
            {
                addGame(name + ": Enhanced Editon", path + @"\EnhancedEdition");
                g = new Game(name, path);
                gameList.Add(g);
            }
            else if (name == "Shenmue I & II")
            {
                addGame("Shenmue I", path + @"\sm1");
                addGame("Shenmue II", path + @"\sm2");
            }
            else if (name == "The Witcher 3: Wild Hunt")
            {
                addGame(name + " dx11", path + @"\bin\x64");
                addGame(name + " dx12", path + @"\bin\x64_dx12");
            }
            else if (name == "Mass Effect Legendary Edition")
            {
                addGame("Mass Effect Legendary", path + @"\Game\ME1");
                addGame("Mass Effect 2 Legendary", path + @"\Game\ME2");
                addGame("Mass Effect 3 Legendary", path + @"\Game\ME3");
            }
            else if (name == "Command and Conquer 3 TW and KW")
            {
                addGame("Command Conquer 3 Tiberium Wars", path + @"\Command Conquer 3 Tiberium Wars");
                addGame("Command Conquer 3 Kanes Wrath", path + @"\Command Conquer 3 Kanes Wrath");
            }
            else if (name == "Command and Conquer Red Alert 3")
            {
                g = new Game("Command and Conquer Red Alert 3", path + @"\Red Alert 3");
                gameList.Add(g);
                addGame("Command and Conquer Red Alert 3: Uprising", path + @"\Red Alert 3 Uprising");
            }
            else if (name == "Command and Conquer Generals Zero Hour")
            {
                addGame("Command and Conquer Generals", path + @"\Command and Conquer Generals");
                g = new Game("Command and Conquer Generals Zero Hour", path + @"\Command and Conquer Generals Zero Hour");
                gameList.Add(g);
            }
            else if (name == "AWayOut")
            {
                addGame("A Way Out", path);
            }
            else if (name == "Apex")
            {
                addGame("Apex  Legend", path);
            }
            else if (name == "BFH")
            {
                addGame("BattleField Hardline", path);
            }
            else if (name == "BurnoutPR")
            {
                addGame("Burnout Paradise Remastered", path);
            }
            else if (name == "CnCRemastered")
            {
                addGame("Command and Conquer Remastered", path);
            }
            else if (name == "GridLegends")
            {
                addGame("Grid Legends", path);
            }
            else if (name == "ItTakesTwo")
            {
                addGame("It Takes Two", path);
            }
            else if (name == "RocketArena")
            {
                addGame("Rocket Arena", path);
            }
            else if (name == "SeaOfSolitude")
            {
                addGame("Sea Of Solitude", path);
            }
            else if (name == "SMB3")
            {
                addGame("Super Mega Baseball 3", path);
            }
            else if (name == "Titanfall2")
            {
                addGame("Titanfall 2", path);
            }
            else if (name == "UnravelTwo")
            {
                addGame("Unravel Two", path);
            }
            else if (name == "Assassin’s Creed Chronicles Russia")
            {
                addGame("Assassin's Creed Chronicles Russia", path);
            }
            else if (name == "Brothers in Arms Road to Hill 30")
            {
                addGame("Brothers in Arms: Road to Hill 30", path);
            }
            else if (name == "BiA EiB")
            {
                addGame("Brothers in Arms: Earned in Blood", path);
            }
            else if (name == "BiAHH")
            {
                addGame("Brothers in Arms: Hell's Highway", path);
            }
            else if (name == "ForHonor")
            {
                addGame("For Honor", path);
            }
            else if (name == "LegendOfKeepers")
            {
                addGame("Legend of Keepers", path);
            }
            else if (name == "MMChessRoyale")
            {
                addGame("Might & Magic - Chess Royale", path);
            }
            else if (name == "RidersRepublic")
            {
                addGame("Riders Republic", path);
            }
            else if (name == "The Crew(Worldwide)")
            {
                addGame("The Crew", path);
            }
            else if (name == "thesettlers")
            {
                addGame("The Settlers - History Edition", path);
            }
            else if (name == "thesettlers2")
            {
                addGame("The Settlers 2 - History Edition", path);
            }
            else if (name == "thesettlers3")
            {
                addGame("The Settlers 3 - History Edition", path);
            }
            else if (name == "thesettlers4")
            {
                addGame("The Settlers 4 - History Edition", path);
            }
            else if (name == "thesettlers5")
            {
                addGame("The Settlers 5 - History Edition", path);
            }
            else if (name == "thesettlers6")
            {
                addGame("The Settlers 6 - History Edition", path);
            }
            else if (name == "thesettlers7")
            {
                addGame("The Settlers 7 - History Edition", path);
            }
            else if (name == "Ghost Recon Breakpoint")
            {
                addGame("Tom Clancy's Ghost Reacon Breakpoint", path);
            }
            else if (name == "Tom Clancy's Rainbow 6 VEGAS 2")
            {
                addGame("Tom Clancy's Rainbow Six VEGAS 2", path);
            }
            else if (name == "Tom Clancy’s Rainbow Six Extraction")
            {
                addGame("Tom Clancy's Rainbow Six Extraction", path);
            }
            else if (name == "Rainbow Six Lockdown")
            {
                addGame("Tom Clancy's Rainbow Six Lockdown", path);
            }
            else if (name == "Rainbow Six 3 Gold")
            {
                addGame("Tom Clancy's Rainbow Six 3 Gold", path);
            }
            else if (name == "WATCH_DOGS2")
            {
                addGame("WATCH_DOGS 2", path);
            }
            else
            {
                g = new Game(name, path);
                gameList.Add(g);
            }
        }

        void size_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> libs = new List<string>();

            try
            {
                var key = (string)Registry.GetValue(@"HKEY_CLASSES_ROOT\steam\Shell\Open\Command", "", null);
                var steamFolder = key.Substring(1, key.Length - 19) + @"steamapps\libraryfolders.vdf";
                var libsLines = System.IO.File.ReadAllLines(steamFolder);

                foreach (var library in libsLines)
                {
                    if (library.Contains("\"path\""))
                    {
                        var steamLib = library.Substring(11, library.Length - 12);
                        libs.Add(reduceSlashes(steamLib));
                    }
                }
            }
            catch { }
            var ignores = new string[0];
            if (System.IO.File.Exists("data\\ignore.txt"))
            {
                ignores = System.IO.File.ReadAllLines("data\\ignore.txt");
            }

            foreach (var lib in libs)
            {
                if (Directory.Exists(lib + @"\SteamApps"))
                {
                    var acfs = Directory.EnumerateFiles(lib + @"\SteamApps", "*.acf");
                    foreach (var acf in acfs)
                    {
                        var lines = System.IO.File.ReadAllLines(acf);
                        var name = "";
                        var path = "";
                        foreach (string line in lines)
                        {
                            if (line.Contains("\"name\""))
                            {
                                name = line.Substring(10, line.Length - 11);
                            }
                            if (line.Contains("\"installdir\""))
                            {
                                var installdir = line.Substring(16, line.Length - 17);
                                path = lib + @"\SteamApps\Common\" + installdir;
                            }
                        }
                        if (Directory.Exists(path))
                        {
                            if (!ignores.Contains(name))
                            {
                                (sender as BackgroundWorker).ReportProgress(0);
                                addGame(name, path);
                            }
                        }
                    }
                }
            }
            if (Directory.Exists(@"C:\ProgramData\Epic\EpicGamesLauncher\Data\Manifests"))
            {
                var items = Directory.EnumerateFiles(@"C:\ProgramData\Epic\EpicGamesLauncher\Data\Manifests", "*.item");
                List<string> paths = new List<string>();
                foreach (var item in items)
                {
                    var lines = System.IO.File.ReadAllLines(item);
                    var name = "";
                    var path = "";
                    foreach (string line in lines)
                    {
                        if (line.Contains("\"LaunchExecutable\""))
                        {
                            var executable = line.Substring(22);
                            executable = executable.Substring(0, executable.Length - 2);
                            if (executable == "")
                                break;
                        }

                        if (line.Contains("\"DisplayName\""))
                        {
                            name = line.Substring(17);
                            name = name.Substring(0, name.Length - 2);
                            name = reduceSlashes(name);
                        }

                        if (line.Contains("\"InstallLocation\""))
                        {
                            path = line.Substring(21);
                            path = path.Substring(0, path.Length - 2);
                            path = reduceSlashes(path);
                            if (!paths.Contains(path))
                            {
                                paths.Add(path);
                                if (!ignores.Contains(name))
                                {
                                    (sender as BackgroundWorker).ReportProgress(0);
                                    if (Directory.Exists(path))
                                    {
                                        addGame(name, path);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (System.IO.File.Exists("PATHs.txt"))
            {
                var dirs = System.IO.File.ReadAllLines("PATHs.txt");
                foreach (var dir in dirs)
                {
                    if (Directory.Exists(dir))
                    {
                        var gamePaths = Directory.EnumerateDirectories(dir);
                        foreach (var path in gamePaths)
                        {
                            var name = System.IO.Path.GetFileName(path);
                            if (!ignores.Contains(name))
                            {
                                (sender as BackgroundWorker).ReportProgress(0);
                                addGame(name, path);
                            }
                        }
                    }
                }
            }

            if (System.IO.File.Exists("FOLDERs.txt"))
            {
                var paths2 = System.IO.File.ReadAllLines("FOLDERs.txt");
                foreach (var path in paths2)
                {
                    if (Directory.Exists(path))
                    {
                        var path2 = path;
                        if (path.Last() == '\\')
                        {
                            path2 = path.Substring(0, path.Length- 1);
                        }
                        var name = System.IO.Path.GetFileName(path2);
                        if (!ignores.Contains(name))
                        {
                            (sender as BackgroundWorker).ReportProgress(0);
                            addGame(name, path);
                        }
                    }
                }
            }
        }

        void size_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            gameCount.Content = "Total Count: " + gameList.Count;
        }

        void size_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            sortGames();
            foreach (var game in gameList)
            {
                if (System.IO.File.Exists(game.path + "\\Geo3D.txt"))
                {
                    game.Expand();
                    gameGeo3D.Add(game);
                    // Update game
                    installGame(game, 0);
                }
            }
            updateGeo3D();
            gameCount.Content = "Total Count: " + gameList.Count;
        }

        private void Grid_Initialized(object sender, EventArgs e)
        {
            BackgroundWorker sizeWorker = new BackgroundWorker();
            sizeWorker.WorkerReportsProgress = true;
            sizeWorker.DoWork += size_DoWork;
            sizeWorker.ProgressChanged += size_ProgressChanged;
            sizeWorker.RunWorkerCompleted += size_RunWorkerCompleted;
            sizeWorker.RunWorkerAsync();
        }
        
        private void DX9_Click(object sender, RoutedEventArgs e)
        {
            var currentGame = (Game)geo3DBox.SelectedItem;
            if (currentGame != null)
            {
                currentGame.Expand();
                installGame(currentGame, 9);
            }
        }

        private void DX10_Click(object sender, RoutedEventArgs e)
        {
            var currentGame = (Game)geo3DBox.SelectedItem;
            if (currentGame != null)
            {
                currentGame.Expand();
                installGame(currentGame, 10);
            }
        }

        private void DX12_Click(object sender, RoutedEventArgs e)
        {
            var currentGame = (Game)geo3DBox.SelectedItem;
            if (currentGame != null)
            {
                currentGame.Expand();
                installGame(currentGame, 12);
            }           
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            var test = gameGeo3D.ToArray<Game>();
            foreach (Game game in test)
            {
                // Update game
                installGame(game, 0);
            }
        }
    }

    public class Game
    {
        public bool displayPath = false;
        public bool displayExe = false;
        public int dxVersion = 10;

        public override string ToString()
        {
            var installDir = System.IO.Path.GetDirectoryName(path + "\\" + exe);
            var display = name;
            if (System.IO.File.Exists(installDir + "\\d3d9.dll"))
                display += " (DX9)";
            else if (System.IO.File.Exists(installDir + "\\d3d12.dll"))
                display += " (DX12)";

            if (displayPath)
                display += " (" + path.Replace(@"\SteamApps\Common\", @"\") + ")";
            if (displayExe)
                display += " (" + exe + ")";
            return display;
        }

        public string Pick(string[] files)
        {
            string[] badWords = { "\\game.exe", "crash", "launcher", "setting", "restarthelper", " vr", "dumper", "install", "bugreport", "gta2 manager",
                                "register", "djinni!", "testapp2", "configtool", "update.exe", "editor.exe", "nox.exe", "dbxMotionPanel", "dw15.exe", "legobatman\\legobatman.exe",
                                "binkplay", "creation kit", "archive.exe", "bssndrpt", "worldbuilder", "cwsdpmi", "config", "dowser", "afscmd", "post mortem",
                                "glvanhelsing_x86.exe", "vanhelsing.exe", "mgsvmgo.exe", "launchpad.exe", "autorun.exe", "Activator.exe", "syberia", "LuaCompiler",
                                "procdump.exe", "BmStartApp.exe", "cef_process.exe", "patch", "NvProfileFixer.exe", "dedicatedserver", "stilllife", "NVShaderPerf",
                                "Registration", "uploader", "dotnetfx.exe", "SplashScreen.exe", "EACLaunch.exe", "GDFTool", "firewall", "ZISupportTool", "fxc.exe",
                                "helper", "GPUBurner", "7za.exe", "shadercompile", "bootstrapper.exe", "clokspl.exe", "crs-handler.exe", "vpk", "berkelium", "\\gu.exe",
                                "eddos.exe", "edwin.exe", "\\ra.exe", "benchmark", "BlizzardError", "BNUpdate", "ChromiumRenderer", "eac.exe",  "control.exe", "control_dx11.exe",
                                "gtaEncoder.exe", "DumpTool.exe", "ar405.exe", "vcredist", "ExtractTextures.exe", "setup", "ActivationUI.exe", "barony-16-player", "BugSplatHD.exe",
                                "bspzip.exe", "dxsupportclean.exe", "modelbrowser.exe", "ndsrv.exe", "decoda.exe", "CompileModel.exe", "Builder.exe", "oalinst.exe", "venice\\testapp",
                                "ffedit.exe", "system\\fforce.exe", "BugSplatHD64.exe", "editor_initialize", "pariseditor.vshost.exe", "arcadeeditor64.exe", "directx",
                                "mmh7editor-win64-shipping.exe", "goblineditorapp.exe", "missioneditor2.exe", "starcraft ii editor_x64.exe", "CPUInfo.exe", "dig\\testapp",
                                "CheckApplication.exe", "testapp_mp.exe", "testapp_sp.exe", "patriots.exe", "TagesClient.exe", "Chernobyl\\bin\\testapp.exe", "Readme.exe",
                                "AskFiles", "decode", "dedicatedserver", "depend", "ecc", "makefont", "rcon", "modeler", "editor32", "-dedicated.exe", "LuaPlus.exe", "ChaosRising.e.e",
                                "cpp.exe", "\\re.exe", "scriptdev.exe", "ConvertData", " dev.exe", "rgb2theora.exe", "ClientSdkMDNSHost.exe", "Subnautica32.exe", "Detection.exe",
                                "System\\testapp.exe", "3DS2E", "BSP", "CSGMERGE", "DROMED.exe", "GOLDSKIP", "runme.exe", "cmark.exe", "awesomium_process.exe", "awesomiumprocess.exe",
                                "DevicePicker.exe", "mplaynow.exe", "R6VegasServer.exe", "DumpLog.exe", "ModManager.exe", "AppData.exe", "dotnetfx", "startup.exe", "UCC.exe" };

            bool vvisClean = true;
            for (int i = 0; i < files.Length; i++)
            {
                var file = System.IO.Path.GetFileName(files[i]).ToLower();
                if (file == "vvis.exe")
                    vvisClean = false;
            }

            string gameExe = "";
            for (int i = 0; i < files.Length; i++)
            {
                var file = System.IO.Path.GetFileName(files[i]).ToLower();
                if (file == "game.exe")
                {
                    gameExe = files[i];
                }
                bool clean = true;
                for (int j = 0; j < badWords.Length; j++)
                {
                    if (files[i].ToLower().Contains(badWords[j].ToLower()))
                    {
                        clean = false;
                        break;
                    }
                }
                if (clean && vvisClean)
                {
                    return files[i];
                }
            }
            if (gameExe != "")
                return gameExe;
            if (files[0].Contains("Crash Bandicoot"))
                return files[0];
            if (files[0].Contains("Crashlands"))
                return files[0];
            if (files[0].Contains("CastleCrashers"))
                return files[0];
            return exePath;
        }

        public string name;
        public string path;
        public string exePath;
        public string exe;
        public string bits;

        public void Scan(string ext)
        {
            if (Directory.Exists(path + ext))
            {
                var exe = Directory.EnumerateFiles(path + ext, "*.exe");
                if (exe.Count() > 0)
                {
                    exePath = Pick(exe.ToArray());
                }
            }
        }

        public Game(string name, string path)
        {
            if (path.ToLower().Contains("xbox"))
            {
                var directory = Directory.EnumerateDirectories(path);
                var exeC = Directory.EnumerateFiles(path, "*.exe");
                if (exeC.Count() == 0 && directory.Count() == 1 && System.IO.Path.GetFileName(directory.First()) == "Content")
                {
                    path = directory.First();
                }
            }
            this.name = name;
            this.path = path;
            this.bits = "x64";
            this.exe = "";
            this.exePath = path + "\\";
        }

        public void Expand()
        {
            Scan(@"");
            if (Directory.Exists(path + @"\Binaries"))
            {
                var exe = Directory.EnumerateFiles(path + @"\Binaries", "*.exe");
                if (exe.Count() > 0)
                {
                    exePath = Pick(exe.ToArray());
                }
                var shipping = Directory.EnumerateDirectories(path + @"\Binaries");
                if (shipping.Count() > 0)
                {
                    var exeFolder = shipping.Last();
                    var folderName = System.IO.Path.GetFileName(exeFolder);
                    if (folderName == "Windows")
                        exeFolder = shipping.ToArray()[shipping.Count() - 2];
                    exe = Directory.EnumerateFiles(exeFolder, "*.exe");
                    if (exe.Count() > 0)
                    {
                        exePath = Pick(exe.ToArray());
                    }
                }
            }

            if (Directory.Exists(path + @"\Bin"))
            {
                var exe = Directory.EnumerateFiles(path + @"\Bin", "*.exe");
                if (exe.Count() > 0)
                {
                    exePath = Pick(exe.ToArray());
                }
                var shipping = Directory.EnumerateDirectories(path + @"\Bin");
                if (shipping.Count() > 0)
                {
                    var exeFolder = shipping.Last();
                    if (System.IO.Path.GetFileName(exeFolder) == "Win64Shared")
                    {
                        exeFolder = shipping.ElementAt(shipping.Count() - 2);
                    }
                    if (exeFolder.ToLower().Contains("\\win") ||
                        exeFolder.ToLower().Contains("\\x64") ||
                        exeFolder.ToLower().Contains("\\release"))
                    {
                        exe = Directory.EnumerateFiles(exeFolder, "*.exe");
                        if (exe.Count() > 0)
                        {
                            exePath = Pick(exe.ToArray());
                        }
                    }
                }
            }

            Scan(@"\Bin_plus");

            var dirs = Directory.EnumerateDirectories(path);
            foreach (var dir in dirs)
            {
                if (dir.Substring(dir.Length - 7) == "\\Engine")
                    continue;

                if (Directory.Exists(dir + @"\Binaries"))
                {
                    var shipping = Directory.EnumerateDirectories(dir + @"\Binaries");
                    if (shipping.Count() > 0)
                    {
                        var exeFolder = shipping.Last();
                        var exe = Directory.EnumerateFiles(exeFolder, "*.exe");
                        if (exe.Count() > 0)
                        {
                            exePath = Pick(exe.ToArray());
                            break;
                        }
                    }
                }
            }

            this.exe = exePath.Replace(path + "\\", "");

            foreach (var Case in MainWindow.specialCases)
            {
                if (System.IO.File.Exists(path + "\\" + Case))
                {
                    this.exe = Case;
                }
            }

            string combinedPath = path + "\\" + this.exe;
            try
            {
                var fStream = System.IO.File.OpenRead(combinedPath);
                byte[] buffer = new byte[5000];
                int read = fStream.Read(buffer, 0, 5000);
                for (int i = 0; i < read - 6; i++)
                {
                    if (buffer[i] == 'P' &&
                        buffer[i + 1] == 'E' &&
                        buffer[i + 2] == 0 &&
                        buffer[i + 3] == 0)
                    {
                        if (buffer[i + 4] == 0x64 && buffer[i + 5] == 0x86)
                        {
                            break;
                        }
                        else if (buffer[i + 4] == 0x4C && buffer[i + 5] == 1)
                        {
                            this.bits = "x86";
                            break;
                        }
                    }
                }
                fStream.Close();
            }
            catch { }
        }
    }
}