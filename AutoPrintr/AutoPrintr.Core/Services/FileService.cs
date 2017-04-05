using AutoPrintr.Core.IServices;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace AutoPrintr.Core.Services
{
    internal class FileService : IFileService
    {
        #region Properties
        private readonly string _folderPath;
        private static object _locker = new object();
        #endregion

        #region Constructors
        public FileService()
        {
            _folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AutoPrintr");
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
            string result = null;

            try
            {
                result = await Task.Factory.StartNew(() =>
                {
                    var filePath = GetFilePath(fileName);

                    if (File.Exists(filePath))
                        return File.ReadAllText(filePath);
                    else
                        return null;
                });
            }
            catch (IOException)
            {
                result = await ReadStringAsync(fileName);
            }

            return result;
        }

        public async Task<byte[]> ReadBytesAsync(string fileName)
        {
            byte[] result = null;

            try
            {
                result = await Task.Factory.StartNew(() =>
                {
                    var filePath = GetFilePath(fileName);

                    if (File.Exists(filePath))
                        return File.ReadAllBytes(filePath);
                    else
                        return null;
                });
            }
            catch (IOException)
            {
                result = await ReadBytesAsync(fileName);
            }

            return result;
        }

        public async Task<T> ReadObjectAsync<T>(string fileName)
        {
            T result = default(T);

            var str = await ReadStringAsync(fileName);
            if (str == null)
                return result;

            try
            {
                result = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<T>(str));
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Error in {nameof(FileService)}: {ex.ToString()}");
            }

            return result;
        }

        public async Task SaveStringAsync(string fileName, string content)
        {
            await Task.Factory.StartNew(() =>
            {
                lock (_locker)
                {
                    var filePath = GetFilePath(fileName);

                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.Exists)
                    {
                        var permissions = fileInfo.GetAccessControl();
                        var rules = permissions.GetAccessRules(true, true, typeof(NTAccount));

                        foreach (FileSystemAccessRule fileSystemAccessRule in rules)
                        {
                            if ((FileSystemRights.Write & fileSystemAccessRule.FileSystemRights) == FileSystemRights.Write &&
                                                          fileSystemAccessRule.AccessControlType != AccessControlType.Allow)
                            {
                                permissions.AddAccessRule(new FileSystemAccessRule(fileSystemAccessRule.IdentityReference.ToString(),
                                                                                                              FileSystemRights.Write,
                                                                                                              AccessControlType.Allow));
                            }

                            if ((FileSystemRights.Write & fileSystemAccessRule.FileSystemRights) == FileSystemRights.Write &&
                                                          fileSystemAccessRule.AccessControlType == AccessControlType.Deny)
                            {
                                permissions.RemoveAccessRule(new FileSystemAccessRule(fileSystemAccessRule.IdentityReference.ToString(),
                                                                                                                 FileSystemRights.Write,
                                                                                                                 AccessControlType.Deny));
                            }
                        }

                        fileInfo.SetAccessControl(permissions);

                        if (fileInfo.IsReadOnly || (fileInfo.Attributes & FileAttributes.Hidden) != 0)
                            fileInfo.Attributes = FileAttributes.Normal;
                    }

                    using (var stream = fileInfo.Open(FileMode.Create))
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

                try
                {
                    await client.DownloadFileTaskAsync(fileUri, filePath);
                }
                catch (Exception ex)
                {
                    completed?.Invoke(false, ex);
                }
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