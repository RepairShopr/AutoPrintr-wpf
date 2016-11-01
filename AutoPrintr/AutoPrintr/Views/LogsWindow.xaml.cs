using System;
using System.Windows;
using System.Windows.Controls;

namespace AutoPrintr.Views
{
    internal partial class LogsWindow : Window
    {
        public LogsWindow()
        {
            InitializeComponent();
        }

        private void DatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            var datePicker = (DatePicker)sender;
            datePicker.BlackoutDates.Add(new CalendarDateRange { Start = DateTime.Today.AddDays(1) });
        }
    }
}