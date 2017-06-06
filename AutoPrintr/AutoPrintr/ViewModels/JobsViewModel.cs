using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using AutoPrintr.Helpers;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoPrintr.ViewModels
{
    public class JobsViewModel : BaseViewModel
    {
        #region Properties
        private readonly IWindowsServiceClient _windowsServiceClient;
        private readonly ILoggerService _loggingService;

        public ObservableCollection<Job> Jobs { get; private set; }

        public IEnumerable<KeyValuePair<JobState?, string>> JobStates { get; private set; }

        private JobState? _selectedJobState;
        public JobState? SelectedJobState
        {
            get { return _selectedJobState; }
            set { Set(ref _selectedJobState, value); LoadJobs(); }
        }

        public IEnumerable<KeyValuePair<DocumentType?, string>> DocumentTypes { get; private set; }

        private DocumentType? _selectedDocumentType;
        public DocumentType? SelectedDocumentType
        {
            get { return _selectedDocumentType; }
            set { Set(ref _selectedDocumentType, value); LoadJobs(); }
        }

        public override ViewType Type => ViewType.Jobs;

        public RelayCommand<Job> PrintCommand { get; private set; }
        public RelayCommand<Job> DeleteJobCommand { get; private set; }
        public RelayCommand<DeleteJobAmount> DeleteJobsCommand { get; private set; }
        #endregion

        #region Constructors
        public JobsViewModel(INavigationService navigationService,
            IWindowsServiceClient windowsServiceClient,
            ILoggerService loggingService)
            : base(navigationService)
        {
            _windowsServiceClient = windowsServiceClient;
            _loggingService = loggingService;

            JobStates = Enum.GetValues(typeof(JobState))
                .OfType<JobState?>()
                .Union(new[] { (JobState?)null })
                .Select(x => new KeyValuePair<JobState?, string>(x, x.HasValue ? x.ToString() : "All"))
                .ToList();
            DocumentTypes = Enum.GetValues(typeof(DocumentType))
                .OfType<DocumentType?>()
                .OrderBy(x => Document.GetTypeTitle(x.Value))
                .Union(new[] { (DocumentType?)null })
                .Select(x => new KeyValuePair<DocumentType?, string>(x, x.HasValue ? Document.GetTypeTitle(x.Value) : "All"))
                .ToList();

            PrintCommand = new RelayCommand<Job>(OnPrint);
            DeleteJobCommand = new RelayCommand<Job>(OnDeleteJob);
            DeleteJobsCommand = new RelayCommand<DeleteJobAmount>(OnDeleteJobs);
        }
        #endregion

        #region Methods
        public override void NavigatedTo(object parameter = null)
        {
            base.NavigatedTo(parameter);

            Jobs = new ObservableCollection<Job>();

            _selectedJobState = null;
            _selectedDocumentType = null;
            _windowsServiceClient.JobChangedAction = JobChanged;

            LoadJobs();
        }

        public void NavigatedFrom()
        {
            _windowsServiceClient.JobChangedAction = null;
            Jobs = null;
        }

        private async void LoadJobs()
        {
            try
            {
                ShowBusyControl();

                Jobs.Clear();

                var jobs = (await _windowsServiceClient.GetJobsAsync())?
                    .Where(x => x != null)
                    .Where(x => SelectedJobState.HasValue ? x.State == SelectedJobState.Value : true)
                    .Where(x => SelectedDocumentType.HasValue ? x.Document.Type == SelectedDocumentType.Value : true)
                    .OrderByDescending(x => x.UpdatedOn);

                if (jobs == null)
                {
                    ShowMessageControl("Jobs cannot be loaded, the AutoPrintr service is not available. Please run the service and try again");
                    HideBusyControl();
                    return;
                }

                foreach (var job in jobs)
                    Jobs.Add(job);
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

        private void JobChanged(Job job)
        {
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var localJob = Jobs.FirstOrDefault(x => x.Id == job.Id);
                if (localJob != null && Jobs.Contains(localJob))
                    Jobs.Remove(localJob);

                Jobs.Insert(0, job);
            }));
        }

        private async void OnPrint(Job obj)
        {
            var result = await _windowsServiceClient.PrintAsync(obj);
            if (!result)
                ShowMessageControl("Job cannot be printed, the AutoPrintr service is not available. Please run the service and try again");
        }

        private async void OnDeleteJob(Job obj)
        {
            var result = await _windowsServiceClient.DeleteJobsAsync(new[] { obj });
            if (!result)
            {
                ShowMessageControl("Job cannot be removed, the AutoPrintr service is not available. Please run the service and try again");
                return;
            }

            Jobs.Remove(obj);
        }

        private async void OnDeleteJobs(DeleteJobAmount obj)
        {
            ShowBusyControl();

            var startDate = new DateTime();
            var endDate = new DateTime();

            switch (obj)
            {
                case DeleteJobAmount.PreviousWeek:
                    startDate = GetPreviousMonday();
                    endDate = startDate.AddDays(7);
                    break;
                case DeleteJobAmount.PreviousMonth:
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1);
                    endDate = startDate.AddMonths(1);
                    break;
                case DeleteJobAmount.AllPast:
                    startDate = DateTime.MinValue;
                    endDate = DateTime.Now.Date;
                    break;
                default: break;
            }

            var jobsToRemove = Jobs.Where(x => x.CreatedOn >= startDate && x.CreatedOn < endDate).ToArray();
            var result = await _windowsServiceClient.DeleteJobsAsync(jobsToRemove);

            HideBusyControl();

            if (!result)
            {
                ShowMessageControl("Jobs cannot be removed, the AutoPrintr service is not available. Please run the service and try again");
                return;
            }

            LoadJobs();
        }

        private DateTime GetPreviousMonday()
        {
            var weekStart = DayOfWeek.Monday;
            var startingDate = DateTime.Today;

            while (startingDate.DayOfWeek != weekStart)
                startingDate = startingDate.AddDays(-1);

            return startingDate.AddDays(-7);
        }
        #endregion
    }
}