using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using GetDataFromFer.Models;
namespace GetDataFromFer
{
    public class FerCrawl
    {
        public string CMScookie { get; set; } = System.Environment.GetEnvironmentVariable("FERCMS");
        private HttpClient _client = new HttpClient();
        public FerCrawl(string CMScookie)
        {
            InitClient(CMScookie);
        }
        public void InitClient(string cookie)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Clear();
            //text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8
            _client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml");
            _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            _client.DefaultRequestHeaders.Add("Accept-Language", "hr-HR");
            _client.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
            _client.DefaultRequestHeaders.Connection.Add(
                "keep-alive");
            _client.DefaultRequestHeaders.Add("Cookie", $"CMS={cookie}");
            _client.DefaultRequestHeaders.Host = "www.fer.unizg.hr";
            _client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");

            _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
        }
        public async Task<List<string>> GetClasses()
        {
            var html = await DecodeGzipAsync(await _client.GetAsync($"https://www.fer.unizg.hr/intranet"));
            var htmldoc = new HtmlAgilityPack.HtmlDocument();
            htmldoc.LoadHtml(html);
            var classes = htmldoc.DocumentNode.Descendants()
                .Where(i => i.Attributes?["class"]?.Value == "orig")
                .Select(i => i.ChildNodes.
                              Where(j => j.Name == "a")
                              .FirstOrDefault()
                              ?.Attributes["href"].Value
                                                  .Split('/')
                                                  .Last())
                .Where(i => i != null)
                .Distinct();
            return classes.ToList();
        }
        public async Task<List<string>> GetActions(string className)
        {
            try
            {
                var html = await DecodeGzipAsync(await _client.GetAsync($"https://www.fer.unizg.hr/predmet/{className}"));
                var htmldoc = new HtmlAgilityPack.HtmlDocument();
                htmldoc.LoadHtml(html);
                var actions = htmldoc.DocumentNode.Descendants().Where(i => i?.Attributes?["class"]?.Value == "active collapse in")
                                                                .Where(i => i?.Attributes["aria-expanded"]?.Value == "true")
                                                                .Where(i => i?.Name == "ul")
                                                                .FirstOrDefault()
                                                                ?.Descendants()
                                                                ?.Where(i => i?.Name == "a")
                                                                ?.Select(i => i?.Attributes?["href"]
                                                                              ?.Value
                                                                              ?.Split('/')
                                                                              ?.Last())
                                                                ?.Where(i => i != null)
                                                                ?.Where(i => i.ToLower() != "forum")
                                                                ?.Where(i => i.ToLower() != "obavijesti")
                                                                ?.Distinct();
                if (!actions?.Any() ?? false)
                    actions = htmldoc.DocumentNode.Descendants().Where(i => i?.Attributes?["id"]?.Value == "left_sidebar")
                                                                .FirstOrDefault()
                                                                ?.Descendants()
                                                                ?.Where(i => i?.Name == "a")
                                                                ?.Select(i => i?.Attributes?["href"]
                                                                              ?.Value)
                                                                ?.Where(i => i != null)
                                                                ?.Where(i => i.Split('/').Length > 3)
                                                                ?.Where(i => !i.ToLower().Contains("forum"))
                                                                ?.Where(i => !i.ToLower().Contains("obavijesti"))
                                                                ?.Where(i => !i.ToLower().Contains("nastavne_aktivnosti"))
                                                                ?.Distinct()
                                                                ?.Select(i => i.Split('/').Last());

                return actions?.ToList();
            }catch(Exception ex)
            {
                Log.LogData("Error", $"Error while getting actions for {className}", Console.WindowHeight - 1);
            }
            return null;
        }
        public async Task Login(string userName, string pass)
        {
            try
            {
                var content = new MultipartFormDataContent();
                var userNameContent = new StringContent(userName, Encoding.UTF8, "text/html");
                var passContent = new StringContent(pass, Encoding.UTF8, "text/html");
                content.Add(userNameContent, "username");
                content.Add(passContent, "password");
                var res = await _client.PostAsync("https://www.fer.unizg.hr/login/Compound", content);
            }catch(Exception ex)
            {
                Log.LogData("Error", "Error while logging in", Console.WindowHeight - 1);
                throw;
            }
        }
        public async Task<string> GetRespounseAsync(string className, string actionName)
        {
            try
            {
                return await DecodeGzipAsync(await _client.GetAsync($"https://www.fer.unizg.hr/predmet/{className}/{actionName}"));

            }
            catch (Exception ex)
            {
                Log.LogData("Error", "Error while getting the resopounse from ferWeb", Console.WindowHeight - 1);
                throw;
            }
        }
        public async Task<string> GetRespounseAsync(string className)
        {
            try
            {
                return await DecodeGzipAsync(await _client.GetAsync($"https://www.fer.unizg.hr/predmet/{className}/materijali"));
                
            }catch(Exception ex)
            {
                Log.LogData("Error", "Error while getting the resopounse from ferWeb", Console.WindowHeight - 1);
                throw;
            }
        }
        public async Task<ClassFolderRoot> GetClassFolderRoot(string className)
        {
            var res = await GetRespounseAsync(className);
            var root = DecodeResounse(res);
            return root;
        }
        public async Task<ClassFolderRoot> GetClassFolderRoot(string className, string actionName)
        {
            var res = await GetRespounseAsync(className, actionName);
            var root = DecodeResounse(res);
            return root;
        }
        public ClassFolderRoot DecodeResounse(string res)
        {
            try
            {
                var split = res.Split("quiltRepositoryBrowser(");
                var interrestingPart = split[1];
                interrestingPart = interrestingPart.Split(", {\"menuFiles\"")[0];
                var startIndex = interrestingPart.IndexOf("{");
                interrestingPart = interrestingPart.Substring(startIndex);

                var files = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.ClassFolderRoot>(interrestingPart);
                return files;
            }catch(Exception ex)
            {
                //Log.LogData("Error", $"Error decoding - res:\n{res}", Console.WindowHeight - 1);
            }
            return null;
        }
        private async Task<string> DecodeGzipAsync(HttpResponseMessage message)
        {
            var stream = await message.Content.ReadAsStreamAsync();
            var gzip = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress);
            var size = (2 << 20);
            var (_s, whileStr) = (string.Empty, string.Empty);
            do
            {
                var byteBuff = new byte[size];
                var siz = gzip.Read(byteBuff, 0, size);
                _s = Encoding.UTF8.GetString(byteBuff);
                whileStr += _s;
            } while (_s.Any(i => i != '\0'));
            return whileStr.Replace("\0", string.Empty);
        }
        public async Task DownloadFile(string uri, string pathOnDisc)
        {
            try
            {
                var getRes = await _client.GetAsync(uri);
                var fileStream = new System.IO.FileStream(pathOnDisc, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write);
                await getRes.Content.CopyToAsync(fileStream);
            }catch(Exception ex)
            {
                Log.LogData("Error", "Error happened while trying to download the file from ferWeb", Console.WindowHeight - 1);
            }
        }
    }
}
