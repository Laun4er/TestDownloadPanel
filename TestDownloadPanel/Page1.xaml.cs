using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.Forge;
using CmlLib.Core.ModLoaders.FabricMC;
using CmlLib.Core.ProcessBuilder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TestDownloadPanel
{
    public partial class Page1 : Page
    {
        private string profilesFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "profiles.json");
        public Page1()
        {
            InitializeComponent();
            LoadProfiles();
        }
        private void AddProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileGrid.Visibility = Visibility.Visible;
        }

        private void SaveProfileButton_Click(object sender, RoutedEventArgs e)
        {
            string profileName = ProfileNameTextBox.Text;
            string loader = (LoaderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            

            if (string.IsNullOrEmpty(profileName) || string.IsNullOrEmpty(loader))
            {
                MessageBox.Show("Пожалуйста, введите имя профиля и выберите загрузчик модов.");
                return;
            }

            var profile = new { Name = profileName, Loader = loader, Version = "1.19.4" };

            List<dynamic> profiles;
            if (File.Exists(profilesFilePath))
            {
                string json = File.ReadAllText(profilesFilePath);
                profiles = JsonConvert.DeserializeObject<List<dynamic>>(json) ?? new List<dynamic>();
            }
            else
            {
                profiles = new List<dynamic>();
            }

            profiles.Add(profile);
            
            File.WriteAllText(profilesFilePath, JsonConvert.SerializeObject(profiles, Formatting.Indented));

            ProfileGrid.Visibility = Visibility.Collapsed;
            LoadProfiles();
        }

        private void LoadProfiles()
        {
            if (File.Exists(profilesFilePath))
            {
                string json = File.ReadAllText(profilesFilePath);
                var profiles = JsonConvert.DeserializeObject<List<dynamic>>(json);
                ProfilesComboBox.ItemsSource = profiles.Select(p => $"{p.Name} ({p.Loader})").ToList();
            }
        }

        private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesComboBox.SelectedItem != null)
            {
                string selectedProfile = ProfilesComboBox.SelectedItem.ToString();

                string json = File.ReadAllText(profilesFilePath);
                var profiles = JsonConvert.DeserializeObject<List<dynamic>>(json);
                profiles.RemoveAll(p => p.Name == selectedProfile);
                File.WriteAllText(profilesFilePath, JsonConvert.SerializeObject(profiles, Formatting.Indented));

                string profilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "profiles", selectedProfile);
                if (Directory.Exists(profilePath))
                {
                    Directory.Delete(profilePath, true);
                }
                LoadProfiles();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите профиль для удаления.");
            }
        }

        private void OpenProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesComboBox.SelectedItem != null)
            {
                string selectedItem = ProfilesComboBox.SelectedItem.ToString();
                string selectedProfile = selectedItem.Split('(')[0].Trim(); 

                string profilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "profiles", selectedProfile);

                if (Directory.Exists(profilePath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = profilePath,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
                else
                {
                    MessageBox.Show("Папка профиля не найдена.");
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите профиль для открытия.");
            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesComboBox.SelectedItem != null)
            {
                string selectedItem = ProfilesComboBox.SelectedItem.ToString();
                string selectedProfile = selectedItem.Split('(')[0].Trim();
                string loaderType = selectedItem.Split('(', ')')[1].Trim();
        

                        string json = File.ReadAllText(profilesFilePath);
                var profiles = JsonConvert.DeserializeObject<List<dynamic>>(json);
                var profile = profiles.FirstOrDefault(p => p.Name == selectedProfile);

                if (profile != null)
                {
                    string version = profile.Vesrion;
                    string loader = profile.Loader;

                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "profiles", selectedProfile);

                    DownloadProgressBar.Visibility = Visibility.Visible;
                    var minecraftPath = new MinecraftPath(path);
                    var launcher = new MinecraftLauncher(minecraftPath);

                    launcher.ByteProgressChanged += (s, a) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            DownloadProgressBar.Value = a.ProgressedBytes;
                            DownloadProgressBar.Maximum = a.TotalBytes;
                        });
                    };

                    var launchOption = new MLaunchOption
                    {
                        Session = MSession.CreateOfflineSession("Laun4er"),
                        MaximumRamMb = 2048
                    };

                    if(loader == "Fabric")
                    {
                        var fabricInstaller = new FabricInstaller(new System.Net.Http.HttpClient());
                        var fabricVer = await fabricInstaller.Install("1.19.4", minecraftPath);
                        var fabricProcess = await launcher.CreateProcessAsync(fabricVer, launchOption);
                        fabricProcess.Start();
                    }
                    else if (loader == "Forge")
                    {
                        var forgeInstaller = new ForgeInstaller(launcher);
                        var forgeVer = await forgeInstaller.Install("1.19.4");

                        var forgeProcess = await launcher.CreateProcessAsync(forgeVer, launchOption);
                        forgeProcess.Start();
                        
                    }
                    else if (loader == "Vanilla")
                    {
                        await launcher.InstallAsync("1.19.4");
                        var vanillaProcess = await launcher.CreateProcessAsync("1.19.4", new MLaunchOption
                        {
                            VersionType = "release"
                        });
                        vanillaProcess.Start();
                    }

                    DownloadProgressBar.Value = 0;
                    DownloadProgressBar.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите профиль для игры.");
            }
        }
    }
}