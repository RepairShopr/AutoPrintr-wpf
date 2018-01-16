using AutoPrintr.Core.IServices;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoPrintr.Core.Helpers;

namespace AutoPrintr.Core.Services
{
    internal class FileService : IFileService
    {
        private readonly string _folderPath;

        public FileService()
        {
            _folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AutoPrintr");
        }

        #region Methods
        public bool DeleteFile(string fileName)
        {
            var filePath = GetFilePath(fileName);
            const int max = 10;
            int attempt = 1;
            while (true)
            {
                try
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                    File.Delete(filePath);
                    return true;
                }
                catch (IOException e)
                {
                    if (attempt >= max)
                    {
                        var lockProcesses = FileLockerUtil.WhoIsLockingSafe(filePath, out _);
                        if (lockProcesses != null)
                        {
                            throw new Exception($"Can't delete the '{filePath}' file. It is locked by the process '{string.Join(", ", lockProcesses.Select(p => $"[{p.Id}]{p.ProcessName}"))}'", e);
                        }

                        throw;
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    if (attempt >= max)
                    {
                        var lockProcesses = FileLockerUtil.WhoIsLockingSafe(filePath, out _);
                        if (lockProcesses != null)
                        {
                            throw new Exception($"Can't delete the '{filePath}' file. It is locked by the process '{string.Join(", ", lockProcesses.Select(p => $"[{p.Id}]{p.ProcessName}"))}'", e);
                        }

                        throw;
                    }
                }
                catch (Exception)
                {
                    if (attempt >= max)
                    {
                        throw;
                    }
                }

                attempt++;
                Thread.Sleep(attempt * 100);
            }
        }

        public async Task<string> ReadStringAsync(string fileName)
        {
            var filePath = GetFilePath(fileName);
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
                return null;

            const int max = 10;
            int attempt = 1;
            while (true)
            {
                try
                {
                    using (var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var fileReader = new StreamReader(fileStream))
                    {
                        return await fileReader.ReadToEndAsync();
                    }
                }
                catch (IOException e)
                {
                    if (attempt >= max)
                    {
                        var lockProcesses = FileLockerUtil.WhoIsLockingSafe(filePath, out _);
                        if (lockProcesses != null)
                        {
                            throw new Exception($"Can't read the '{filePath}' file. It is locked by the process '{string.Join(", ", lockProcesses.Select(p => $"[{p.Id}]{p.ProcessName}"))}'", e);
                        }

                        throw;
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    if (attempt >= max)
                    {
                        var lockProcesses = FileLockerUtil.WhoIsLockingSafe(filePath, out _);
                        if (lockProcesses != null)
                        {
                            throw new Exception($"Can't read the '{filePath}' file. It is locked by the process '{string.Join(", ", lockProcesses.Select(p => $"[{p.Id}]{p.ProcessName}"))}'", e);
                        }

                        throw;
                    }
                }
                catch (Exception)
                {
                    if (attempt >= max)
                    {
                        throw;
                    }
                }

                attempt++;
                await Task.Delay(attempt * 100);
            }
        }

        public async Task<byte[]> ReadBytesAsync(string fileName)
        {
            var filePath = GetFilePath(fileName);
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
                return null;

            const int max = 10;
            int attempt = 1;
            while (true)
            {
                try
                {
                    using (var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var data = new byte[fileStream.Length];
                        await fileStream.ReadAsync(data, 0, data.Length);
                        return data;
                    }
                }
                catch (IOException e)
                {
                    if (attempt >= max)
                    {
                        var lockProcesses = FileLockerUtil.WhoIsLockingSafe(filePath, out _);
                        if (lockProcesses != null)
                        {
                            throw new Exception($"Can't read the '{filePath}' file. It is locked by the process '{string.Join(", ", lockProcesses.Select(p => $"[{p.Id}]{p.ProcessName}"))}'", e);
                        }

                        throw;
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    if (attempt >= max)
                    {
                        var lockProcesses = FileLockerUtil.WhoIsLockingSafe(filePath, out _);
                        if (lockProcesses != null)
                        {
                            throw new Exception($"Can't read the '{filePath}' file. It is locked by the process '{string.Join(", ", lockProcesses.Select(p => $"[{p.Id}]{p.ProcessName}"))}'", e);
                        }

                        throw;
                    }
                }
                catch (Exception)
                {
                    if (attempt >= max)
                    {
                        throw;
                    }
                }

                attempt++;
                await Task.Delay(attempt * 100);
            }
        }

        public async Task<T> ReadObjectAsync<T>(string fileName)
        {
            var str = await ReadStringAsync(fileName);
            if (String.IsNullOrEmpty(str))
                return default(T);

            return JsonConvert.DeserializeObject<T>(str);
        }

        public async Task SaveStringAsync(string fileName, string content)
        {
            var filePath = GetFilePath(fileName);
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
                SetupAccessControl(fileInfo);

            using (var fileStream = fileInfo.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            using (var fileWriter = new StreamWriter(fileStream))
            {
                await fileWriter.WriteAsync(content);
            }

            SetupAccessControl(fileInfo);
        }

        public async Task SaveBytesAsync(string fileName, byte[] content)
        {
            var filePath = GetFilePath(fileName);
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
                SetupAccessControl(fileInfo);

            using (var fileStream = fileInfo.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                await fileStream.WriteAsync(content, 0, content.Length);
            }

            SetupAccessControl(fileInfo);
        }

        public Task SaveObjectAsync<T>(string fileName, T content)
        {
            var str = JsonConvert.SerializeObject(content);
            return SaveStringAsync(fileName, str);
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

        public void SetupAccessControl(string fileName)
        {
            SetupAccessControl(new FileInfo(GetFilePath(fileName)));
        }

        private void SetupAccessControl(FileInfo fileInfo)
        {
            try
            {
                var permissions = fileInfo.GetAccessControl();
                permissions.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null), 
                    FileSystemRights.FullControl, 
                    InheritanceFlags.None, 
                    PropagationFlags.NoPropagateInherit, 
                    AccessControlType.Allow));
                fileInfo.SetAccessControl(permissions);

                if (fileInfo.IsReadOnly || (fileInfo.Attributes & FileAttributes.Hidden) != 0)
                    fileInfo.Attributes = FileAttributes.Normal;
            }
            catch
            {
                // TODO: add logger
            }
        }
        #endregion
    }
}