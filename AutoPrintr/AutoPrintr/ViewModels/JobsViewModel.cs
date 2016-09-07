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

        public IEnumerable<JobState?> JobStates { get; private set; }

        private JobState? _selectedJobState;
        public JobState? SelectedJobState
        {
            get { return _selectedJobState; }
            set { Set(ref _selectedJobState, value); LoadJobs(); }
        }

        public IEnumerable<DocumentType?> DocumentTypes { get; private set; }

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

            Jobs = new ObservableCollection<Job>();
            JobStates = Enum.GetValues(typeof(JobState))
                .OfType<JobState?>()
                .Union(new[] { (JobState?)null })
                .ToList();
            DocumentTypes = Enum.GetValues(typeof(DocumentType))
                .OfType<DocumentType?>()
                .OrderBy(x => Document.GetTypeTitle(x.Value))
                .Union(new[] { (DocumentType?)null })
                .ToList();

            PrintCommand = new RelayCommand<Job>(OnPrint);
        }
        #endregion

        #region Methods
        public override void NavigatedTo(object parameter = null)
        {
            base.NavigatedTo(parameter);

            Jobs.Clear();
            _selectedJobState = null;
            _selectedDocumentType = null;
        }

        public void NavigatedFrom()
        {
            Jobs.Clear();
        }

        private void LoadJobs()
        {
            var jobs = _jobsService.Jobs
                .Where(x => SelectedJobState.HasValue ? x.State == SelectedJobState.Value : true)
                .Where(x => SelectedDocumentType.HasValue ? x.Document.Type == SelectedDocumentType.Value : true)
                .OrderByDescending(x => x.UpdatedOn);
            foreach (var job in jobs)
                Jobs.Add(job);
        }

        private void OnPrint(Job obj)
        {

        }
        #endregion
    }
}