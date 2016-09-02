using AutoPrintr.Helpers;
using AutoPrintr.IServices;
using AutoPrintr.Models;
using GalaSoft.MvvmLight.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace AutoPrintr.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        #region Properties
        private readonly ISettingsService _settingsService;

        public IEnumerable<Location> Locations { get; private set; }
        public ObservableCollection<Location> SelectedLocations { get; private set; }

        public override ViewType Type => ViewType.Settings;
        #endregion

        #region Constructors
        public SettingsViewModel(INavigationService navigationService,
            ISettingsService settingsService)
            : base(navigationService)
        {
            _settingsService = settingsService;

            SelectedLocations = new ObservableCollection<Location>();
        }
        #endregion

        #region Methods
        public override void NavigatedTo(object parameter = null)
        {
            base.NavigatedTo(parameter);

            Locations = _settingsService.Settings.User.Locations;
            RaisePropertyChanged(nameof(Locations));

            SelectedLocations.CollectionChanged -= SelectedLocations_CollectionChanged;
            SelectedLocations.Clear();

            foreach (var location in Locations)
                if (_settingsService.Settings.Locations.Any(l => l.Id == location.Id))
                    SelectedLocations.Add(location);

            SelectedLocations.CollectionChanged += SelectedLocations_CollectionChanged;
        }

        private async void SelectedLocations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SelectedLocations.CollectionChanged -= SelectedLocations_CollectionChanged;

            if (e.NewItems != null)
            {
                foreach (Location newItem in e.NewItems)
                {
                    SelectedLocations.Add(newItem);
                    await _settingsService.AddLocationAsync(newItem);
                }
            }

            if (e.OldItems != null)
            {
                foreach (Location oldtem in e.OldItems)
                {
                    SelectedLocations.Remove(oldtem);
                    await _settingsService.RemoveLocationAsync(oldtem);
                }
            }

            SelectedLocations.CollectionChanged += SelectedLocations_CollectionChanged;
        }
        #endregion
    }
}