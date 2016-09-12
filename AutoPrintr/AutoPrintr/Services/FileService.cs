using AutoPrintr.IServices;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPrintr.Services
{
    internal class FileService : IFileService
    {
        #region Properties
        private readonly string _folderPath;
        private static object _locker = new Object();
        #endregion

        #region Constructors
        public FileService(string folder = null)
        {
            _folderPath = folder ?? AppDomain.CurrentDomain.BaseDirectory;
        }
        #endregion

        #region Methods
        public async Task DeleteFileAsync(string fileName)
        {
            await Task.Factory.StartNew(() =>
            {
                lock (_locker)
                {
                    var filePath = GetFilePath(fileName);

                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
            });
        }

        public async Task<string> ReadStringAsync(string fileName)
        {
            return await Task.Factory.StartNew(() =>
            {
                var filePath = GetFilePath(fileName);

                if (File.Exists(filePath))
                    return File.ReadAllText(filePath);
                else
                    return null;
            });
        }

        public async Task<byte[]> ReadBytesAsync(string fileName)
        {
            return await Task.Factory.StartNew(() =>
            {
                var filePath = GetFilePath(fileName);

                if (File.Exists(filePath))
                    return File.ReadAllBytes(filePath);
                else
                    return null;
            });
        }

        public async Task<T> ReadObjectAsync<T>(string fileName)
        {
            var str = await ReadStringAsync(fileName);
            if (str == null)
                return default(T);

            return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<T>(str));
        }

        public async Task SaveStringAsync(string fileName, string content)
        {
            await Task.Factory.StartNew(() =>
            {
                lock (_locker)
                {
                    var filePath = GetFilePath(fileName);

                    using (var stream = File.Open(filePath, FileMode.Create))
                    {
                        using (var writer = new StreamWriter(stream))
                            writer.Write(content);
                    }
                }
            });
        }

        public async Task SaveBytesAsync(string fileName, byte[] content)
        {
            await Task.Factory.StartNew(() =>
            {
                lock (_locker)
                {
                    var filePath = GetFilePath(fileName);

                    using (var stream = File.Open(filePath, FileMode.Create))
                        stream.Write(content, 0, content.Length);
                }
            });
        }

        public async Task SaveObjectAsync<T>(string fileName, T content)
        {
            var str = JsonConvert.SerializeObject(content);
            await SaveStringAsync(fileName, str);
        }

        public async Task DownloadFileAsync(Uri fileUri, string localFilePath, Action<int> progressChanged = null, Action<bool, Exception> completed = null)
        {
            var filePath = GetFilePath(localFilePath);

            using (var client = new System.Net.WebClient())
            {
                client.DownloadProgressChanged += (o, e) =>
                {
                    progressChanged?.Invoke(e.ProgressPercentage);
                };
                client.DownloadFileCompleted += (o, e) =>
                {
                    completed?.Invoke(!e.Cancelled && e.Error == null, e.Error);
                };
                await client.DownloadFileTaskAsync(fileUri, filePath);
            }
        }

        public string GetFilePath(string fileName)
        {
            var filePath = new StringBuilder(_folderPath.Replace('\\', '/'));
            if (_folderPath.Last() != '/')
                filePath.Append('/');

            filePath.Append(fileName);

            var folderPath = Path.GetDirectoryName(filePath.ToString());
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return filePath.ToString();
        }
        #endregion
    }
}