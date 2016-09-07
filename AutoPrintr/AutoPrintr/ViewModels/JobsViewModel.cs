using AutoPrintr.Helpers;
using AutoPrintr.IServices;
using AutoPrintr.Models;
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
        #endregion
    }
}