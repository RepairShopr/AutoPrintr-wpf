using AutoPrintr.Helpers;
using AutoPrintr.ViewModels;
using AutoPrintr.Views;
using GalaSoft.MvvmLight.Views;
using System;
using System.Windows;

namespace AutoPrintr.Services
{
    internal class NavigationService : INavigationService
    {
        private Window _currentWindow;

        public string CurrentPageKey { get; private set; }

        public void GoBack()
        {
            throw new NotImplementedException();
        }

        public void NavigateTo(string pageKey)
        {
            _currentWindow?.Close();
            _currentWindow = GetView(pageKey);
            _currentWindow.Show();

            ((BaseViewModel)_currentWindow.DataContext).NavigatedTo();
        }

        public void NavigateTo(string pageKey, object parameter)
        {
            _currentWindow?.Close();
            _currentWindow = GetView(pageKey);
            _currentWindow.Show();

            ((BaseViewModel)_currentWindow.DataContext).NavigatedTo(parameter);
        }

        private Window GetView(string pageKey)
        {
            var viewType = (ViewType)Enum.Parse(typeof(ViewType), pageKey);

            CurrentPageKey = pageKey;

            Window window = null;
            switch (viewType)
            {
                case ViewType.Login:
                    window = new LoginWindow(); break;
                case ViewType.Settings:
                    window = new SettingsWindow(); break;
                default: throw new NotImplementedException();
            }
            window.DataContext = App.GetDataContext(viewType);
            return window;
        }
    }
}