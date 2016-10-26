using AutoPrintr.Core.Models;
using AutoPrintr.Helpers;
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

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var listView = (ListView)sender;
            listView.SelectionChanged -= ListView_SelectionChanged;

            foreach (var item in ViewModel.SelectedLocations)
                listView.SelectedItems.Add(item);

            listView.SelectionChanged += ListView_SelectionChanged;
        }

        private void Slider_LostFocus(object sender, RoutedEventArgs e)
        {
            var slider = (Slider)sender;
            var parent = slider.FindParent<ItemsControl>();
            var printer = (Printer)parent.DataContext;

            ViewModel.UpdatePrinterCommand.Execute(printer);
        }

        private void CheckBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var printer = (Printer)checkBox.DataContext;

            ViewModel.UpdatePrinterCommand.Execute(printer);
        }

        private void ComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var printer = (Printer)comboBox.DataContext;

            ViewModel.UpdatePrinterCommand.Execute(printer);
        }
    }
}