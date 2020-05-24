using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Reflection;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;

namespace FileProviderConcept
{
    public interface IFileManager
    {
        void ShowStructure(Action<int, string> render);
        Task<String> ReadAllTextAsync(String path);
    }


    class Program
    {
        static async Task Main(string[] args)
        {
            static void Print(int layer, string name) => Console.WriteLine($@"{new string(' ', layer * 4)}{name}");
            var container = new ServiceCollection()
                .AddSingleton<IFileProvider>(new PhysicalFileProvider(@"f:\checktest"))
                //.AddSingleton<IFileProvider>(new EmbeddedFileProvider(Assembly.GetEntryAssembly()))
                .AddSingleton<IFileManager, FileManager>()
                .BuildServiceProvider();
            using (var sc = container.CreateScope())
            {
                var fm = sc.ServiceProvider.GetRequiredService<IFileManager>();
                fm.ShowStructure(Print);
                var content = await fm.ReadAllTextAsync("test02.py");
                //var content = await fm.ReadAllTextAsync("test_emb.txt");
                Console.WriteLine(content);

            }
            Console.ReadLine();
        }

        static async Task FsWatcherFromProvider()
        {
            using (var fileProvider = new PhysicalFileProvider(@"f:\checktest")) {
                string origin = null;
                ChangToken.OnChange(() => fileProvider.Watch("test.txt"), CallBack);
                while (true)
                {
                    File.WriteAllText(@"f:\checktest\test.txt", DateTime.Now.ToString());
                    await Task.Delay(5000);

                }
                async void CallBack()
                {
                    using (var stream = fileProvider.GetFileInfo("test.txt").CreateReadStream())
                    {
                        var buffer = new byte[stream.Length];
                        await stream.ReadAsync(buffer, 0, buffer.Length);
                        string current = Encoding.Default.GetString(buffer);
                        if(current != origin)
                        {
                            Console.WriteLine("oringal != current");
                            origin = current;
                        }
                    }
                }

            }
        }
    }
}
