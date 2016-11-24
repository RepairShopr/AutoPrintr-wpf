using AutoPrintr.Core.Helpers;
using AutoPrintr.Core.IServices;
using AutoPrintr.Core.Models;
using AutoPrintr.Service.IServices;
using Newtonsoft.Json;
using PusherClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AutoPrintr.Service.Services
{
    internal class JobsService : IJobsService
    {
        #region Properties
        private readonly ISettingsService _settingsService;
        private readonly IFileService _fileService;
        private readonly IPrinterService _printerService;
        private readonly ILoggerService _loggingService;

        private readonly string _applicationKey;
        private readonly string _newJobsFileName = $"Data/New{nameof(Job)}s.json";
        private readonly string _downloadedJobsFileName = $"Data/Downloaded{nameof(Job)}s.json";
        private readonly string _doneJobsFileName = $"Data/Done{nameof(Job)}s.json";

        private string _channel;
        private Pusher _pusher;
        private int _downloadingJobCount;
        private Dictionary<Printer, Job> _printingJobs;

        private ObservableCollection<Job> _newJobs;
        private ObservableCollection<Job> _downloadedJobs;
        private ObservableCollection<Job> _doneJobs;

        public bool IsRunning { get; private set; }

        public event JobChangedEventHandler JobChangedEvent;
        #endregion

        #region Constructors
        public JobsService(ISettingsService settingsService,
            IFileService fileService,
            IPrinterService printerService,
            ILoggerService loggingService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _printerService = printerService;
            _loggingService = loggingService;

            _applicationKey = "4a12d53c136a2d3dade7";
            _printingJobs = new Dictionary<Printer, Job>();

            _settingsService.ChannelChangedEvent += _settingsService_ChannelChangedEvent;
        }
        #endregion

        #region Methods

        #region Jobs Methods
        public IEnumerable<Job> GetJobs()
        {
            return _newJobs
                .Union(_downloadedJobs)
                .Union(_doneJobs);
        }

        public void Print(Job job)
        {
            var localJob = GetJobs().FirstOrDefault(x => x.Id == job.Id);
            if (localJob == null)
                return;

            PrintDocument(localJob, true);
        }

        public async void DeleteJobs(IEnumerable<Job> jobs)
        {
            if (jobs == null)
                return;

            foreach (var job in jobs)
            {
                var localJob = GetJobs().FirstOrDefault(x => x.Id == job.Id);

                _loggingService.WriteInformation($"Starting remove job {localJob.Document.TypeTitle}");

                if (!string.IsNullOrEmpty(localJob.Document.LocalFilePath))
                    await _fileService.DeleteFileAsync(localJob.Document.LocalFilePath);

                if (_doneJobs.Contains(localJob))
                    _doneJobs.Remove(localJob);
                else if (_downloadedJobs.Contains(localJob))
                    _downloadedJobs.Remove(localJob);
                else if (_newJobs.Contains(localJob))
                    _newJobs.Remove(localJob);

                _loggingService.WriteInformation($"Job {localJob.Document.TypeTitle} is removed");
            }
        }

        public async Task RunAsync()
        {
            if (IsRunning)
                return;

            _loggingService.WriteInformation($"Startring {nameof(JobsService)}");

            await ReadJobsFromFiles();
            await RunPusherAsync();

            _loggingService.WriteInformation($"{nameof(JobsService)} is started");

            IsRunning = true;
        }

        public async Task StopAsync()
        {
            if (!IsRunning)
                return;

            _loggingService.WriteInformation($"Stopping {nameof(JobsService)}");

            await StopPusher();

            _loggingService.WriteInformation($"{nameof(JobsService)} is stopped");

            IsRunning = false;
        }

        private async void _settingsService_ChannelChangedEvent(Core.Models.Channel newChannel)
        {
            if (!IsRunning)
                return;

            await RunPusherAsync();
        }
        #endregion

        #region Printer Methods
        private async void PrintDocument(Job job, bool manual = false)
        {
            var jobPrinters = await GetPrintersForPrinting(job);
            var printerToPrint = jobPrinters.FirstOrDefault(x => !_printingJobs.Keys.Any(p => string.Compare(x.Name, p.Name) == 0));
            if (printerToPrint == null)
                return;

            _loggingService.WriteInformation($"Starting print document {job.Document.TypeTitle} on {printerToPrint.Name}");

            _printingJobs.Add(printerToPrint, job);

            job.Printer = printerToPrint.Name;
            job.Quantity = printerToPrint.DocumentTypes.Where(x => x.DocumentType == job.Document.Type).Select(x => x.Quantity).Single();
            job.State = JobState.Printing;
            job.UpdatedOn = DateTime.Now;
            JobChangedEvent?.Invoke(job);

            await _printerService.PrintDocumentAsync(printerToPrint, job.Document, job.Quantity, (r, e) =>
            {
                if (r)
                    _loggingService.WriteInformation($"Document {job.Document.TypeTitle} is printed on {printerToPrint.Name}");
                else
                {
                    Debug.WriteLine($"Error in {nameof(JobsService.PrintDocument)}: {e.ToString()}");
                    _loggingService.WriteInformation($"Printing document {job.Document.TypeTitle} on {printerToPrint.Name} is failed");
                    _loggingService.WriteError(e.ToString());
                }

                job.Error = e;
                job.State = r ? JobState.Printed : JobState.Error;
                job.UpdatedOn = DateTime.Now;
                JobChangedEvent?.Invoke(job);

                _printingJobs.Remove(printerToPrint);
                if (!_printingJobs.Any())
                    MovePrintedJobs();
            });
        }

        private async Task<IEnumerable<Printer>> GetPrintersForPrinting(Job job)
        {
            var installedPrinters = await _printerService.GetPrintersAsync();
            return installedPrinters
                .Where(x => job.Document.Register.HasValue ? job.Document.Register == x.Register : true)
                .Where(x => x.DocumentTypes.Any(d => d.DocumentType == job.Document.Type && d.Enabled && (job.Document.AutoPrint ? d.AutoPrint : true)))
                .ToList();
        }

        private IEnumerable<Printer> GetPrintersForDownloading(Job job)
        {
            return _settingsService.Settings.Printers
                .Where(x => job.Document.Register.HasValue ? job.Document.Register == x.Register : true)
                .Where(x => x.DocumentTypes.Any(d => d.DocumentType == job.Document.Type && d.Enabled && (job.Document.AutoPrint ? d.AutoPrint : true)))
                .ToList();
        }

        private void MovePrintedJobs()
        {
            var jobs = _downloadedJobs.Where(x => x.State == JobState.Printed || x.State == JobState.Error).ToList();
            foreach (var job in jobs)
            {
                _downloadedJobs.Remove(job);
                _doneJobs.Add(job);
            }

            var newPrintingJobs = _downloadedJobs.Where(x => x.State == JobState.Downloaded).ToList();
            foreach (var newJob in newPrintingJobs)
                PrintDocument(newJob);
        }
        #endregion

        #region Files Methods
        private async Task ReadJobsFromFiles()
        {
            _loggingService.WriteInformation($"Starting read jobs");

            _newJobs = await _fileService.ReadObjectAsync<ObservableCollection<Job>>(_newJobsFileName);
            if (_newJobs == null)
                _newJobs = new ObservableCollection<Job>();
            _newJobs.CollectionChanged += _newJobs_CollectionChanged;

            _downloadedJobs = await _fileService.ReadObjectAsync<ObservableCollection<Job>>(_downloadedJobsFileName);
            if (_downloadedJobs == null)
                _downloadedJobs = new ObservableCollection<Job>();
            _downloadedJobs.CollectionChanged += _downloadedJobs_CollectionChanged;

            _doneJobs = await _fileService.ReadObjectAsync<ObservableCollection<Job>>(_doneJobsFileName);
            if (_doneJobs == null)
                _doneJobs = new ObservableCollection<Job>();
            _doneJobs.CollectionChanged += _doneJobs_CollectionChanged;

            _loggingService.WriteInformation($"Jobs are read");

            if (!IsRunning)
                return;

            foreach (var newJob in _newJobs)
                DownloadDocument(newJob);

            foreach (var newJob in _downloadedJobs)
                PrintDocument(newJob);
        }

        private async void _doneJobs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            await _fileService.SaveObjectAsync(_doneJobsFileName, _doneJobs);
        }

        private async void _downloadedJobs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            await _fileService.SaveObjectAsync(_downloadedJobsFileName, _downloadedJobs);

            if (e.NewItems != null)
            {
                foreach (Job newJob in e.NewItems)
                    PrintDocument(newJob);
            }
        }

        private async void _newJobs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            await _fileService.SaveObjectAsync(_newJobsFileName, _newJobs);

            if (e.NewItems != null)
            {
                foreach (Job newJob in e.NewItems)
                    DownloadDocument(newJob);
            }
        }

        private async void DownloadDocument(Job job)
        {
            var rotation = GetPrintersForDownloading(job).Any(x => x.Rotation);
            if (rotation)
                job.Document.FileUri = new Uri($"{job.Document.FileUri}&orientation=portrait");

            _loggingService.WriteInformation($"Starting download document {job.Document.TypeTitle} from {job.Document.FileUri}");

            _downloadingJobCount++;
            job.State = JobState.Processing;
            job.UpdatedOn = DateTime.Now;
            JobChangedEvent?.Invoke(job);

            var localFilePath = $"Documents/{Guid.NewGuid()}.pdf";
            await _fileService.DownloadFileAsync(job.Document.FileUri,
                localFilePath,
                p =>
                {
                    job.DownloadProgress = p;
                    job.State = JobState.Downloading;
                    job.UpdatedOn = DateTime.Now;
                    JobChangedEvent?.Invoke(job);
                },
                (r, e) =>
                {
                    if (r)
                        _loggingService.WriteInformation($"Document {job.Document.TypeTitle} is downloaded to {localFilePath}");
                    else
                    {
                        Debug.WriteLine($"Error in {nameof(JobsService.DownloadDocument)}: {e.ToString()}");
                        _loggingService.WriteInformation($"Downloading document {job.Document.TypeTitle} is failed");
                        _loggingService.WriteError(e.ToString());
                    }

                    _downloadingJobCount--;
                    job.Error = e;
                    job.State = r ? JobState.Downloaded : JobState.Error;
                    job.Document.LocalFilePath = r ? localFilePath : null;
                    job.UpdatedOn = DateTime.Now;
                    JobChangedEvent?.Invoke(job);

                    if (_downloadingJobCount == 0)
                        MoveDownloadedJobs();
                });
        }

        private void MoveDownloadedJobs()
        {
            var jobs = _newJobs.Where(x => x.State == JobState.Downloaded || x.State == JobState.Error).ToList();
            foreach (var job in jobs)
            {
                _newJobs.Remove(job);

                if (job.State == JobState.Downloaded)
                    _downloadedJobs.Add(job);
                else if (job.State == JobState.Error)
                    _doneJobs.Add(job);
            }
        }
        #endregion

        #region Pusher Methods
        private async Task RunPusherAsync()
        {
            if (_settingsService.Settings.Channel == null || string.IsNullOrEmpty(_settingsService.Settings.Channel.Value))
            {
                _channel = null;
                await StopPusher();
                return;
            }

            if (string.Compare(_channel, _settingsService.Settings.Channel.Value, true) != 0)
                _channel = _settingsService.Settings.Channel.Value;

            await StopPusher();
            await StartPusher();
        }

        private async Task StartPusher()
        {
            if (_pusher != null)
                return;

            await Task.Factory.StartNew(() =>
            {
                _loggingService.WriteInformation($"Starting Pusher");

                _pusher = new Pusher(_applicationKey);
                _pusher.Error += _pusher_Error;
                _pusher.ConnectionStateChanged += _pusher_ConnectionStateChanged;
                _pusher.Subscribe(_channel)
                       .Bind("print-job", _pusher_ReadResponse);

                _pusher.Connect();

                _loggingService.WriteInformation($"Pusher is started");
            });
        }

        private async Task StopPusher()
        {
            if (_pusher == null)
                return;

            await Task.Factory.StartNew(() =>
            {
                _loggingService.WriteInformation($"Stopping Pusher");

                _pusher.Disconnect();
                _pusher = null;

                _loggingService.WriteInformation($"Pusher is stopped");
            });
        }

        private void _pusher_ReadResponse(dynamic message)
        {
            _loggingService.WriteInformation($"Starting read Pusher response: {message.ToString()}");

            var stringMessage = message.ToString();
            Document document = null;

            try
            {
                document = JsonConvert.DeserializeObject<Document>(stringMessage, new DocumentSizeJsonConverter());
            }
            catch (Exception ex)
            {
                _loggingService.WriteInformation($"New job is not added");

                Debug.WriteLine($"Error in Pusher: {ex.ToString()}");
                _loggingService.WriteError(ex);

                return;
            }

            if (!document.Location.HasValue || _settingsService.Settings.Locations.Any(l => l.Id == document.Location))
            {
                var newJob = new Job { Document = document };

                if (!GetPrintersForDownloading(newJob).Any())
                    return;

                _newJobs.Add(newJob);
                JobChangedEvent?.Invoke(newJob);

                _loggingService.WriteInformation($"New job {newJob.Document.TypeTitle} is added");
            }
        }

        private void _pusher_ConnectionStateChanged(object sender, ConnectionState state)
        {
            _loggingService.WriteInformation($"Pusher is {state}");
        }

        private void _pusher_Error(object sender, PusherException error)
        {
            Debug.WriteLine($"Error in Pusher: {error.ToString()}");
            _loggingService.WriteError(error);
        }
        #endregion

        #endregion
    }
}