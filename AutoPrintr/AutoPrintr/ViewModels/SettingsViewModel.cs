using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using AutoPrintr.Helpers;
using GalaSoft.MvvmLight.Command;
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
        private readonly IWindowsServiceClient _windowsServiceClient;
        private readonly ILoggerService _loggingService;
        private volatile bool _isOpenedConnectionFailedMessage;

        public bool AddToStartup
        {
            get { return _settingsService.Settings.AddedToStartup; }
            set { OnAddToStartup(value); }
        }

        public bool InstallService
        {
            get { return _settingsService.Settings.InstalledService; }
            set { OnInstallService(value); }
        }

        public User User { get; private set; }
        public ObservableCollection<Location> SelectedLocations { get; private set; }
        public IEnumerable<string> DocumentTypes { get; private set; }
        public IEnumerable<Printer> Printers { get; private set; }
        public IEnumerable<Register> Registers { get; private set; }
        public IEnumerable<KeyValuePair<PrintMode, string>> PrintModes { get; private set; }

        public override ViewType Type => ViewType.Settings;

        public RelayCommand<Printer> UpdatePrinterCommand { get; private set; }
        public RelayCommand RestartServiceCommand { get; private set; }
        #endregion

        #region Constructors
        public SettingsViewModel(INavigationService navigationService,
            ISettingsService settingsService,
            IWindowsServiceClient windowsServiceClient,
            ILoggerService loggingService)
            : base(navigationService)
        {
            _settingsService = settingsService;
            _windowsServiceClient = windowsServiceClient;
            _loggingService = loggingService;

            SelectedLocations = new ObservableCollection<Location>();
            DocumentTypes = Enum.GetValues(typeof(DocumentType))
                .OfType<DocumentType>()
                .Select(x => Document.GetTypeTitle(x))
                .OrderBy(x => x)
                .ToList();
            PrintModes = Enum.GetValues(typeof(PrintMode))
                .OfType<PrintMode>()
                .Select(x => new KeyValuePair<PrintMode, string>(x, x.ToString()))
                .ToList();

            MessengerInstance.Register<User>(this, OnUserChanged);

            UpdatePrinterCommand = new RelayCommand<Printer>(OnUpdatePrinter);
            RestartServiceCommand = new RelayCommand(OnRestartService);
        }
        #endregion

        #region Methods
        public override void NavigatedTo(object parameter = null)
        {
            try
            {
                base.NavigatedTo(parameter);

                InitializeRegisters();
                InitializeLocations();
                InitializePrinters();
            }
            catch (Exception e)
            {
                _loggingService?.WriteError(e);
            }
        }

        private void OnUserChanged(User obj)
        {
            try
            {
                User = obj;
                RaisePropertyChanged(nameof(User));
            }
            catch (Exception e)
            {
                _loggingService?.WriteError(e);
            }
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
            try
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
            catch (Exception exeption)
            {
                _loggingService?.WriteError(exeption);
            }
        }

        private async void InitializePrinters()
        {
            try
            {
                ShowBusyControl();

                Printers = await _windowsServiceClient.GetPrintersAsync();
                if (Printers == null)
                {
                    ShowMessageControl(
                        "Printers cannot be loaded, the AutoPrintr service is not available. Please run the service and try again");
                    HideBusyControl();
                    return;
                }

                var documentTypes = Enum.GetValues(typeof(DocumentType)).OfType<DocumentType>().ToList();

                foreach (var printer in Printers)
                {
                    printer.DocumentTypes.ToList().ForEach(x => x.Enabled = true);

                    var documentTypesToAdd = documentTypes
                        .Where(x => !printer.DocumentTypes.Any(p => p.DocumentType == x))
                        .Select(x => new DocumentTypeSettings {DocumentType = x})
                        .ToList();

                    printer.DocumentTypes = printer.DocumentTypes
                        .Union(documentTypesToAdd)
                        .OrderBy(x => Document.GetTypeTitle(x.DocumentType))
                        .ToList();
                }

                RaisePropertyChanged(nameof(Printers));
            }
            catch (Exception e)
            {
                _loggingService?.WriteError(e);
            }
            finally
            {
                HideBusyControl();
            }
        }

        private void InitializeRegisters()
        {
            var registers = _settingsService.Settings.Channel.Registers?.ToList();
            if (registers == null)
                registers = new List<Register>();

            registers.Insert(0, new Register { Name = "None" });

            Registers = registers;
            RaisePropertyChanged(nameof(Registers));
        }

        private async void OnUpdatePrinter(Printer obj)
        {
            try
            {
                ShowBusyControl();

                if (!_settingsService.Settings.Printers.Any(x => string.Compare(x.Name, obj.Name) == 0) &&
                    obj.DocumentTypes.Any(x => x.Enabled))
                    await _settingsService.AddPrinterAsync(obj);
                else if (_settingsService.Settings.Printers.Any(x => string.Compare(x.Name, obj.Name) == 0) &&
                         !obj.DocumentTypes.Any(x => x.Enabled))
                    await _settingsService.RemovePrinterAsync(obj);
                else
                    await _settingsService.UpdatePrinterAsync(obj);
            }
            catch (Exception e)
            {
                _loggingService?.WriteError(e);
            }
            finally
            {
                HideBusyControl();
            }
        }

        private async void OnAddToStartup(bool value)
        {
            try
            {
                ShowBusyControl();
                await _settingsService.AddToStartup(value);
            }
            catch (Exception e)
            {
                _loggingService?.WriteError(e);
            }
            finally
            {
                HideBusyControl();
            }

            try
            {
                RaisePropertyChanged(nameof(AddToStartup));
            }
            catch (Exception e)
            {
                _loggingService?.WriteError(e);
            }
        }

        private async void OnInstallService(bool value)
        {
            try
            {
                ShowBusyControl();
                await _windowsServiceClient.DisconnectAsync();
                await _settingsService.InstallService(value);
                await _windowsServiceClient.ConnectAsync(ShowConnectionFailedMessage);
            }
            catch (Exception e)
            {
                _loggingService?.WriteError(e);
            }
            finally
            {
                HideBusyControl();
            }

            try
            {
                if (InstallService && Printers?.Any() != true)
                    InitializePrinters();

                RaisePropertyChanged(nameof(InstallService));
            }
            catch (Exception e)
            {
                _loggingService?.WriteError(e);
            }
        }

        private void OnRestartService()
        {
            OnInstallService(true);
        }

        public void ShowConnectionFailedMessage()
        {
            if (_isOpenedConnectionFailedMessage)
                return;

            try
            {
                _isOpenedConnectionFailedMessage = true;
                ShowWarningControl("You are either offline or AutoPrintr is being blocked from connecting to the internet. If you have a software firewall in place disable that or give AutoPrintr permission to the network", "You are either offline");
            }
            finally
            {
                _isOpenedConnectionFailedMessage = false;
            }
        }
        #endregion
    }
}