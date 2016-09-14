using AutoPrintr.Core.Models;
using AutoPrintr.Helpers;
using AutoPrintr.ViewModels;
using System.Linq;
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

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var printer = (Printer)textBox.DataContext;

            if (string.IsNullOrEmpty(textBox.Text))
                printer.Register = null;

            ViewModel.UpdatePrinterCommand.Execute(printer);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (textBox.Text.Any(x => !char.IsDigit(x)))
            {
                textBox.Text = new string(textBox.Text.Where(x => char.IsDigit(x)).ToArray());
                textBox.Select(textBox.Text.Count(), 0);
            }
        }
    }
}