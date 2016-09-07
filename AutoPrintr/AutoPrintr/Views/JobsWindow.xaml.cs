using AutoPrintr.ViewModels;
using System.Windows;

namespace AutoPrintr.Views
{
    internal partial class JobsWindow : Window
    {
        public JobsWindow()
        {
            InitializeComponent();
            Closing += JobsWindow_Closing;
        }

        private void JobsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= JobsWindow_Closing;
            ((JobsViewModel)DataContext).NavigatedFrom();
        }
    }
}