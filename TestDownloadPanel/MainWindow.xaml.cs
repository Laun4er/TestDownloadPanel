﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TestDownloadPanel
{
    public partial class MainWindow : Window
    {
        private Dictionary<string, Page> pages = new Dictionary<string, Page>();
        private Dictionary<string, bool> downloadInProgress = new Dictionary<string, bool>();
        public MainWindow()
        {
            InitializeComponent();
            pages.Add("Main", new Page1(this));

            pageFrame.Content = pages["Main"];
        }
        
        public void ShowDownloadPanel()
        {
            if (DownloadGrid.Visibility == Visibility.Collapsed)
            {
                DownloadGrid.Visibility = Visibility.Visible;
            }
        }
        private void ShowDownloadsButton_Click(object sender, RoutedEventArgs e)
        {
            if(DownloadGrid.Visibility == Visibility.Visible)
            {
                DownloadGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                DownloadGrid.Visibility = Visibility.Visible;
            }
        }

        public void AddDownloadStatus(string profileName)
        {
            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var profileNameTextBlock = new TextBlock
            {
                Text = $"Профиль: {profileName}",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
            };
            var dockPanel = new DockPanel { Margin = new Thickness(0, 0, 0, 5) };
            var progressBar = new ProgressBar
            {
                Width = 200,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0)
            };
            var progressPercentageTextBlock = new TextBlock();

            DockPanel.SetDock(progressPercentageTextBlock, Dock.Right);
            DockPanel.SetDock(progressBar, Dock.Left);
            dockPanel.Children.Add(progressPercentageTextBlock);
            dockPanel.Children.Add(progressBar);

            stackPanel.Children.Add(profileNameTextBlock);
            stackPanel.Children.Add(dockPanel);

            DownloadStackPanel.Children.Add(stackPanel);

            var downloadStatus = new DownloadStatus
            {
                ProfileName = profileName,
                ProgressBar = progressBar,
                ProgressTextBlock = progressPercentageTextBlock
            };

            DownloadStatuses.Add(downloadStatus);
        }

        public void UpdateDownloadStatus(string profileName, double progressPercentage)
        {
            var downloadStatus = DownloadStatuses.FirstOrDefault(ds => ds.ProfileName == profileName);
            if (downloadStatus != null)
            {
                downloadStatus.ProgressBar.Value = progressPercentage;
                downloadStatus.ProgressTextBlock.Text = $"{Math.Round(progressPercentage)}%";
            }
        }

        public void RemoveDownloadStatus(string profileName)
        {
            var downloadStatus = DownloadStatuses.FirstOrDefault(ds => ds.ProfileName == profileName);
            if (downloadStatus != null)
            {
                DownloadStackPanel.Children.Remove(downloadStatus.ProgressBar.Parent as UIElement);
                DownloadStatuses.Remove(downloadStatus);
            }
        }

        private List<DownloadStatus> DownloadStatuses = new List<DownloadStatus>();

        public class DownloadStatus
        {
            public string ProfileName { get; set; }
            public ProgressBar ProgressBar { get; set; }
            public TextBlock ProgressTextBlock { get; set; }
        }
    }
}
