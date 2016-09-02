using AutoPrintr.Models;
using AutoPrintr.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AutoPrintr.Views
{
    internal partial class SettingsWindow : Window
    {
        private SettingsViewModel ViewModel => (SettingsViewModel)DataContext;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (Location newItem in e.AddedItems)
                ViewModel.SelectedLocations.Add(newItem);

            foreach (Location oldtem in e.RemovedItems)
                ViewModel.SelectedLocations.Remove(oldtem);
        }

        private void ListView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var listView = (ListView)sender;
            listView.SelectionChanged -= ListView_SelectionChanged;

            foreach (var item in ViewModel.SelectedLocations)
                listView.SelectedItems.Add(item);

            listView.SelectionChanged += ListView_SelectionChanged;
        }
    }
}