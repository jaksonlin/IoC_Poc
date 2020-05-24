using Microsoft.Extensions.FileProviders;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FileProviderConcept
{
    public class FileManager : IFileManager
    {
        private readonly IFileProvider fileProvider;
        public FileManager(IFileProvider fileProvider) => this.fileProvider = fileProvider;

        public void ShowStructure(Action<int, string> render)
        {
            int indent = -1;
            Render("");
            void Render(string subPath)
            {
                indent += 1;
                foreach(var fileInfo in this.fileProvider.GetDirectoryContents(subPath))
                {
                    render(indent, fileInfo.Name);
                    if (fileInfo.IsDirectory)
                    {
                        Render($@"{subPath}\{fileInfo.Name}".TrimStart('\\'));
                    }
                }
                indent -= 1;
            }
        }

        public async Task<String> ReadAllTextAsync(String path)
        {
            byte[] buffer;
            using(var stream = this.fileProvider.GetFileInfo(path).CreateReadStream())
            {
                buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length);
            }
            return Encoding.Default.GetString(buffer);
        }
    }
}
