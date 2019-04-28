using System;
using System.Linq;
using System.Threading.Tasks;

namespace GetDataFromFer
{
    class Program
    {
        private static readonly string CMSFileName = "CMScookie";
        static async Task Main(string[] args)
        {
            try
            {
                if (!System.IO.File.Exists(CMSFileName))
                {
                    Console.WriteLine("Enter your CMS cookie");
                    var cms = Console.ReadLine();
                    await System.IO.File.WriteAllTextAsync(CMSFileName, cms);
                }
                var cookie = System.IO.File.ReadAllText(CMSFileName);
                if (cookie == null)
                {
                    Console.WriteLine("ERROR!!!");
                    Console.WriteLine("Add FERCMS variable to your envirement variables");
                    return;
                }

                Log.InitConsole();
                Log.WriteToFile("FerLog.log");
                Log.Start();
                var sync = new Sync(cookie);
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage of this program is:\n>program [list of wanted classes you want to sync] [local path you want to sync to]\neg:\n>program os oop mat3r fiz2r C:\\ferShit\n");
                    Console.WriteLine("Or you can only specify the local path and every class you are taking will be selected\nEg.\n>program C:\\ferShit\n");
                    return;
                }
                else if (args.Length == 1)
                {
                    var path = args[0];
                    await sync.SyncFolderForAllClasses(path);
                }
                else
                {
                        var path = args.Last();
                        var classes = args.Take(args.Length - 1).ToArray();
                        await sync.SyncFolderForClass(path, classes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL ERROR!!!\nEnter new CMScookie and run again\n{ex}");
                var cms = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(cms))
                    await System.IO.File.WriteAllTextAsync(CMSFileName, cms);
            }

            Log.LogData("Done", $"Press any key to exit", Console.WindowHeight - 1);
            await Task.Delay(1000);
            Log.Stop();
            Console.ReadKey();
        }
    }
}