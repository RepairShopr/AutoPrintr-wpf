using AutoPrintr.IServices;
using Newtonsoft.Json;
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
        #endregion

        #region Constructors
        public FileService(string folder = null)
        {
            _folderPath = folder ?? $"{Path.GetTempPath()}/FilesToPrint";
        }
        #endregion

        #region Methods
        public async Task DeleteFileAsync(string fileName)
        {
            await Task.Factory.StartNew(() =>
            {
                var filePath = GetFilePath(fileName);

                if (File.Exists(filePath))
                    File.Delete(filePath);
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

        public async Task SaveFileAsync<T>(string fileName, T content)
        {
            await Task.Factory.StartNew(() =>
            {
                byte[] bytes = null;
                if (typeof(T) == typeof(string))
                    bytes = Encoding.Unicode.GetBytes(content.ToString());
                else if (typeof(T) == typeof(byte[]))
                    bytes = content as byte[];
                else
                {
                    var str = JsonConvert.SerializeObject(content);
                    bytes = Encoding.Unicode.GetBytes(str);
                }

                var filePath = GetFilePath(fileName);
                using (var stream = File.Open(filePath, FileMode.CreateNew))
                    stream.Write(bytes, 0, bytes.Length);
            });
        }

        private string GetFilePath(string fileName)
        {
            var filePath = new StringBuilder(_folderPath);
            if (_folderPath.Last() != '/')
                filePath.Append('/');

            filePath.Append(fileName);
            return filePath.ToString();
        }
        #endregion
    }
}