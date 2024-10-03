using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Installer.Forge;
using CmlLib.Core.ModLoaders.FabricMC;
using CmlLib.Core.ProcessBuilder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TestDownloadPanel
{
    public partial class Page1 : Page
    {
        private const string ProfilesFolderPath = "Profiles";
        private List<Profile> _profiles = new List<Profile>();

        public Page1()
        {
            InitializeComponent();
            LoadProfiles();
            ProfileComboBox.SelectedIndex = 0;
        }

        private void AddProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileInputGrid.Visibility = Visibility.Visible;
        }

        private void CreateProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var profileName = ProfileNameTextBox.Text;
            var loaderType = (LoaderTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            //Создание профиля
            if (!string.IsNullOrWhiteSpace(profileName) && !string.IsNullOrWhiteSpace(loaderType))
            {
                var profile = new Profile { Name = profileName, LoaderType = loaderType };
                _profiles.Add(profile);
                Directory.CreateDirectory(Path.Combine(ProfilesFolderPath, profileName));
                ProfileComboBox.Items.Add($"{profile.Name} ({profile.LoaderType})");
                SaveProfiles();

                ProfileInputGrid.Visibility = Visibility.Collapsed;
                ProfileNameTextBox.Clear();
                LoaderTypeComboBox.SelectedIndex = -1;
            }
        }

        private void CancelProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileInputGrid.Visibility = Visibility.Collapsed;
            ProfileNameTextBox.Clear();
            LoaderTypeComboBox.SelectedIndex = -1;
        }

        private void LoadProfiles()
        {
            try
            {
                if (File.Exists(Path.Combine(ProfilesFolderPath, "profiles.json")))
                {
                    var json = File.ReadAllText(Path.Combine(ProfilesFolderPath, "profiles.json"));
                    _profiles = JsonConvert.DeserializeObject<List<Profile>>(json) ?? new List<Profile>();

                    foreach (var profile in _profiles)
                    {
                        ProfileComboBox.Items.Add($"{profile.Name} ({profile.LoaderType})");
                    }
                }
                else if (Directory.Exists(ProfilesFolderPath))
                {
                    var profileDirs = Directory.GetDirectories(ProfilesFolderPath);
                    foreach (var dir in profileDirs)
                    {
                        var profileName = Path.GetFileName(dir);
                        _profiles.Add(new Profile { Name = profileName });
                        ProfileComboBox.Items.Add(profileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить профили. {ex.Message}");
            }
        }

        private void SaveProfiles()
        {
            var json = JsonConvert.SerializeObject(_profiles, Formatting.Indented);
            File.WriteAllText(Path.Combine(ProfilesFolderPath, "profiles.json"), json);
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedProfileName = ProfileComboBox.SelectedItem?.ToString();
                if (string.IsNullOrWhiteSpace(selectedProfileName))
                {
                    return;
                }

                var profile = _profiles.Find(p => p.Name == selectedProfileName);
                if (profile == null)
                {
                    return;
                }

                var profilePath = Path.Combine(ProfilesFolderPath, selectedProfileName);
                var minecraftPath = new MinecraftPath();
                var launcher = new MinecraftLauncher(minecraftPath);


                var launchOption = new MLaunchOption
                {
                    Session = MSession.CreateOfflineSession("Laun4er"), // Заменить мой ник на локальное значение из бдшки
                    MaximumRamMb = 2048 // Внести локальное значение ОЗУ из бдшки
                };

                switch (profile.LoaderType)
                {
                    case "Vanilla":
                        await UpdateProgressAsync(async () =>
                        {
                            
                            var process = await launcher.CreateProcessAsync("1.19.4", launchOption);
                            process.Start();
                        });
                        break;
                    case "Forge":
                        await UpdateProgressAsync(async () =>
                        {
                            var forgeInstaller = new ForgeInstaller(launcher);
                            var verNameForge = await forgeInstaller.Install("1.19.4");
                            var forgeProcess = await launcher.CreateProcessAsync(verNameForge, launchOption);
                            forgeProcess.Start();
                        });
                        break;
                    case "Fabric":
                        await UpdateProgressAsync(async () =>
                        {
                            var fabricInstaller = new FabricInstaller(new HttpClient());
                            var verName = await fabricInstaller.Install("1.19.4", minecraftPath);
                            var fabricProcess = await launcher.CreateProcessAsync(verName, launchOption);
                            fabricProcess.Start();
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось запустить игру {ex.Message}");
            }
        }

        private async Task UpdateProgressAsync(Func<Task> action)
        {
            for (int i = 0; i <= 100; i += 20)
            {
                await Task.Delay(100); //ОБЯЗАТЕЛЬНО СДЕЛАТЬ НОРМАЛЬНЫЙ ПРОГРЕСС БАР
                ProgressBar.Value = i;
            }
            await action();
            ProgressBar.Value = 0;
            ProgressBar.Visibility = Visibility.Collapsed;
        }

        private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Обработчик комбо за 159 рублей
        }
    }

    public class Profile
    {
        public string Name { get; set; }
        public string LoaderType { get; set; }
    }
}


