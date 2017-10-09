using GetDataFromFer.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace GetDataFromFer
{
    public class Sync
    {
        private FerCrawl _crawl;
        public string RootUriAdderss = "https://www.fer.unizg.hr";
        public Sync(string CMSCookie)
        {
            _crawl = new FerCrawl(CMSCookie);
        }
        
        public async Task SyncFolderForAllClasses(string pathOnDisc)
        {
            var tasks = new List<Task>();
            var classess = await _crawl.GetClasses();
            var ii = 0;
            var jj = 1;
            Log.LogData("Classes", $"Found {classess?.Count} classess: {classess.Aggregate((i, j) => $"{i}, {j}")}", Console.WindowHeight - 4);
            foreach (var cl in classess)
            {
                var actions = await _crawl.GetActions(cl);
                Log.LogData(cl, $"Found {actions?.Count ?? 0} actions: {actions?.Aggregate((i, j) => $"{i}, {j}") ?? "null"}", Console.WindowHeight - 4 - jj);
                jj++;
                if (!(actions?.Any() ?? false))
                    continue;
                foreach(var action in actions)
                {
                    tasks.Add(SyncFolderForClass(pathOnDisc, cl, action, ii));
                    ii++;
                }
                
            }
            await Task.WhenAll(tasks);
        }
        public async Task SyncFolderForClass(string pathOnDisc, string className, string actionName, int id)
        {
            try
            {
                Log.LogData($"{className}/{actionName}", $"Started crlwling", id);
                var root = await _crawl.GetClassFolderRoot(className, actionName);
                if (root == null)
                {
                    Log.LogData($"{className}/{actionName}", $"Bad action, nothing here", id);
                    return;
                }
                Log.LogData($"{className}/{actionName}", $"Started syncing", id);
                int n = await SyncFolders(Path.Combine(pathOnDisc, className, actionName), $"{className}/{actionName}", root.items, id);
                Log.LogData($"{className}/{actionName}", $"Sync successful. Downloaded {n} new files", id);
            }
            catch (Exception ex)
            {
                Log.LogData("Error", "Error", Console.WindowHeight);
            }
        }
        public async Task SyncFolderForClass(string pathOnDisc, string className, int id)
        {
            try
            {
                Log.LogData(className, $"Started crlwling", id);
                var root = await _crawl.GetClassFolderRoot(className);
                Log.LogData(className, $"Started syncing", id);
                int n = await SyncFolders(Path.Combine(pathOnDisc, className), className, root.items, id);
                Log.LogData(className, $"Sync successful. Downloaded {n} new files", id);
            }catch(Exception ex)
            {
                Log.LogData("Error", "Error", Console.WindowHeight);
            }
        }
        public async Task SyncFolderForClass(string pathOnDisc, params string[] classNames)
        {
            var tasks = new List<Task>();
            int i = 0;
            foreach(var className in classNames)
            {
                tasks.Add(SyncFolderForClass(pathOnDisc, className, i));
                i++;
            }
            await Task.WhenAll(tasks);
        }
        private async Task<int> SyncFolders(string pathOnDisc, string className, Item[] items, int id)
        {
            var tasks = new List<Task<int>>();
            var downloadTasks = new List<Task>();
            int sum = 0;
            if (!Directory.Exists(pathOnDisc))
            {
                Log.LogData(className, $"New directory: {pathOnDisc}", id);
                Directory.CreateDirectory(pathOnDisc);
            }
            foreach (var item in items)
            {
                
                if (item.type == "file" && item.subtype == "file")
                {
                    var newItem = Path.Combine(pathOnDisc, item.path.Split('/')
                                                                    .Last()
                                                                    .Replace("/", string.Empty)
                                                                    .Replace("\\", string.Empty)
                                                                    .Replace(",", string.Empty)
                                                                    .Replace("\"", string.Empty)
                                                                    .Replace("<", string.Empty)
                                                                    .Replace(">", string.Empty)
                                                                    .Replace(":", string.Empty)
                                                                    .Replace("|", string.Empty)
                                                                    .Replace("*", string.Empty)
                                                                    .Replace("?", string.Empty));
                    if (!File.Exists(newItem))
                    {
                        sum++;
                        Log.LogData(className, $"New file found on ferWeb: {newItem}", id);
                        downloadTasks.Add(_crawl.DownloadFile(RootUriAdderss + item.path, newItem));
                    }
                }
                else if (item.type == "folder" && item.items != null && item.items.Length > 0)
                {
                    var newItem = Path.Combine(pathOnDisc, item.name.Replace("/", string.Empty)
                                                                .Replace("\\", string.Empty)
                                                                .Replace(",", string.Empty)
                                                                .Replace("\"", string.Empty)
                                                                .Replace("<", string.Empty)
                                                                .Replace(">", string.Empty)
                                                                .Replace(":", string.Empty)
                                                                .Replace("|", string.Empty)
                                                                .Replace("*", string.Empty)
                                                                .Replace("?", string.Empty));
                    tasks.Add(SyncFolders(newItem, className, item.items, id));
                        
                }    
            }
            await Task.WhenAll(downloadTasks);
            
            return sum + (await Task.WhenAll(tasks)).Sum();
        }
    }
}
