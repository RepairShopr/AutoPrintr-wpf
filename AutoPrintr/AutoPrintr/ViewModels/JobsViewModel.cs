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
        private readonly IJobsService _jobsService;

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
            IJobsService jobsService)
            : base(navigationService)
        {
            _jobsService = jobsService;

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

            _jobsService.JobChangedEvent -= _jobsService_JobChangedEvent;
            _jobsService.JobChangedEvent += _jobsService_JobChangedEvent;

            _selectedJobState = null;
            _selectedDocumentType = null;

            LoadJobs();
        }

        public void NavigatedFrom()
        {
            _jobsService.JobChangedEvent -= _jobsService_JobChangedEvent;
            Jobs = null;
        }

        private void LoadJobs()
        {
            ShowBusyControl();

            Jobs.Clear();
            var jobs = _jobsService.Jobs
                .Where(x => SelectedJobState.HasValue ? x.State == SelectedJobState.Value : true)
                .Where(x => SelectedDocumentType.HasValue ? x.Document.Type == SelectedDocumentType.Value : true)
                .OrderByDescending(x => x.UpdatedOn);
            foreach (var job in jobs)
                Jobs.Add(job);

            HideBusyControl();
        }

        private void _jobsService_JobChangedEvent(Job job)
        {
            App.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (Jobs.Contains(job))
                    Jobs.Remove(job);

                Jobs.Insert(0, job);
            }));
        }

        private void OnPrint(Job obj)
        {
            _jobsService.Print(obj);
        }

        private async void OnDeleteJob(Job obj)
        {
            await _jobsService.DeleteJob(obj);
            Jobs.Remove(obj);
        }

        private async void OnDeleteJobs(DeleteJobAmount obj)
        {
            ShowBusyControl();

            switch (obj)
            {
                case DeleteJobAmount.PreviousWeek:
                    var previousMonday = GetPreviousMonday();
                    await _jobsService.DeleteJobs(previousMonday, previousMonday.AddDays(7));
                    break;
                case DeleteJobAmount.PreviousMonth:
                    var previousMonthStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1);
                    await _jobsService.DeleteJobs(previousMonthStartDate, previousMonthStartDate.AddMonths(1));
                    break;
                case DeleteJobAmount.AllPast:
                    await _jobsService.DeleteJobs(DateTime.MinValue, DateTime.Now.Date);
                    break;
                default: break;
            }

            HideBusyControl();

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