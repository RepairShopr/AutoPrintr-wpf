using AutoPrintr.Helpers;
using AutoPrintr.IServices;
using AutoPrintr.Models;
using GalaSoft.MvvmLight.Views;
using System;
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
        private readonly IPrinterService _printerService;

        public User User { get; private set; }
        public ObservableCollection<Location> SelectedLocations { get; private set; }
        public IEnumerable<string> DocumentTypes { get; private set; }
        public IEnumerable<Printer> Printers { get; private set; }

        public override ViewType Type => ViewType.Settings;
        #endregion

        #region Constructors
        public SettingsViewModel(INavigationService navigationService,
            ISettingsService settingsService,
            IPrinterService printerService)
            : base(navigationService)
        {
            _settingsService = settingsService;
            _printerService = printerService;

            SelectedLocations = new ObservableCollection<Location>();
            DocumentTypes = Enum.GetValues(typeof(DocumentType))
                .OfType<DocumentType>()
                .Select(x => Document.GetTitle(x))
                .OrderBy(x => x)
                .ToList();

            MessengerInstance.Register<User>(this, OnUserChanged);
        }
        #endregion

        #region Methods
        public override void NavigatedTo(object parameter = null)
        {
            base.NavigatedTo(parameter);

            InitializeLocations();
            InitializePrinters();
        }

        private void OnUserChanged(User obj)
        {
            User = obj;
            RaisePropertyChanged(nameof(User));
        }

        private void InitializeLocations()
        {
            SelectedLocations.CollectionChanged -= SelectedLocations_CollectionChanged;
            SelectedLocations.Clear();

            foreach (var location in User.Locations)
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

        private async void InitializePrinters()
        {
            Printers = (await _printerService.GetPrintersAsync()).ToList();
            var documentTypes = Enum.GetValues(typeof(DocumentType)).OfType<DocumentType>().ToList();

            foreach (var printer in Printers)
            {
                var documentTypesToAdd = documentTypes
                    .Where(x => !printer.DocumentTypes.Any(p => p.DocumentType == x))
                    .Select(x => new DocumentTypeSettings { DocumentType = x })
                    .ToList();

                printer.DocumentTypes = printer.DocumentTypes
                    .Union(documentTypesToAdd)
                    .OrderBy(x => Document.GetTitle(x.DocumentType))
                    .ToList();
            }
            RaisePropertyChanged(nameof(Printers));
        }
        #endregion
    }
}