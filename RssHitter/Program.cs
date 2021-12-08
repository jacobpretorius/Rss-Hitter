using System;
using System.IO;
using System.Threading;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;

namespace RssHitter
{
    class Program
    {
        private static readonly string _settingsFolder = "settings";
        private static readonly string _settingsFile = "settings.txt";
        private static readonly string _filenameChangesFile = "filenameChanges.txt";
        private static readonly string _dlDirectorySettingFile = "downloadDirectory.txt";

        private static string _rssUrl { get; set; }
        private static string[] _filenameChanges { get; set; }
        private static string _dlDir { get; set; }

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;

            Console.WriteLine("Starting RSS Hitter v1.3 - 2021");
            Console.WriteLine("");
            Console.WriteLine("1. Reading Settings");
            Console.WriteLine("");

            // Create folder
            if (!Directory.Exists(AppContext.BaseDirectory + _settingsFolder))
            {
                Directory.CreateDirectory(AppContext.BaseDirectory + _settingsFolder);
            }

            // Read/create settings file
            var localPath = Path.Combine(AppContext.BaseDirectory + _settingsFolder, _settingsFile);
            if (File.Exists(localPath))
            {
                // Read file
                FileStream fileStream = new FileStream(localPath, FileMode.Open);
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    _rssUrl = reader.ReadLine()?.Trim();
                    Console.WriteLine($"TARGET URL set to: {_rssUrl}");
                    Console.WriteLine("");
                }
            }
            else
            {
                // Make the settings file
                using (var fileStream = System.IO.File.Create(localPath))
                {
                    using (var fileWriter = new System.IO.StreamWriter(fileStream))
                    {
                        Console.WriteLine("Settings file made, go imput your RSS feed on /settings/settings.txt");
                        fileWriter.WriteLine("https://rsslink.com");
                        _rssUrl = "https://rsslink.com";
                    }
                }
            }

            Console.WriteLine("2. Reading Download dir");
            Console.WriteLine("");

            // Read/create dl dir file
            var dlPathFile = Path.Combine(AppContext.BaseDirectory + _settingsFolder, _dlDirectorySettingFile);
            if (File.Exists(dlPathFile))
            {
                // Read file
                FileStream fileStream = new FileStream(dlPathFile, FileMode.Open);
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    _dlDir = reader.ReadLine()?.Trim();

                    if (!_dlDir.StartsWith(Path.DirectorySeparatorChar))
                        _dlDir = Path.DirectorySeparatorChar + _dlDir;
                    if (!_dlDir.EndsWith(Path.DirectorySeparatorChar))
                        _dlDir = _dlDir + Path.DirectorySeparatorChar;

                    Console.WriteLine($"Downloading to: {_dlDir}");
                    Console.WriteLine("");
                }
            }
            else
            {
                // Make the dl file
                using (var fileStream = System.IO.File.Create(dlPathFile))
                {
                    using (var fileWriter = new System.IO.StreamWriter(fileStream))
                    {
                        Console.WriteLine("direcotry file made, go imput your absolute target directory in /settings/downloadDirectory.txt");
                        fileWriter.WriteLine("/var/www/");
                        _rssUrl = "/var/www/";
                    }
                }
            }

            Console.WriteLine("3. Reading Filename Changes");
            Console.WriteLine("");

            var filenameLocalPath = Path.Combine(AppContext.BaseDirectory + _settingsFolder, _filenameChangesFile);
            if (File.Exists(filenameLocalPath))
            {
                // Read file
                FileStream fileStream = new FileStream(filenameLocalPath, FileMode.Open);
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    var fileNameContents = reader.ReadToEnd();
                    _filenameChanges = fileNameContents?.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)?.Select(s => s.Trim())?.ToArray();

                    foreach (var removal in _filenameChanges)
                    {
                        Console.WriteLine($"REMOVING '{removal}' from file names");
                    }
                    
                    Console.WriteLine("");
                }
            }
            else
            {
                // Make the settings file
                using (var fileStream = System.IO.File.Create(filenameLocalPath))
                {
                    using (var fileWriter = new System.IO.StreamWriter(fileStream))
                    {
                        Console.WriteLine("Name Changes file made, go update /settings/filenameChanges.txt and restart the app");
                        fileWriter.WriteLine("one.per.line-removethisall");
                    }
                }
            }

            while (true)
            {
                // Hit the target
                if (_rssUrl != "https://rsslink.com")
                {
                    using (var client = new HttpClient())
                    {
                        // Hit the rss target
                        var result = client.GetStringAsync(_rssUrl).Result;
                        if (!string.IsNullOrWhiteSpace(result))
                        {
                            // Parse the xml
                            XElement feed = XElement.Parse(result);
                            if (feed != null && feed.HasAttributes)
                            {
                                using (var wc = new System.Net.WebClient())
                                {
                                    // Loop the elements
                                    foreach (var element in feed.Descendants("item"))
                                    {
                                        // Get the links
                                        var link = element.Element("link").Value;
                                        if (!string.IsNullOrWhiteSpace(link))
                                        {
                                            // Clean the file names
                                            var filename = (string)element.Element("title").Value;

                                            if (_filenameChanges != null && _filenameChanges.Length > 0)
                                            {
                                                foreach (var change in _filenameChanges)
                                                {
                                                    if (filename.Contains(change))
                                                    {
                                                        filename = filename.Replace(change, string.Empty);
                                                    }
                                                }
                                            }

                                            //var fileToSave = AppContext.BaseDirectory + filename + ".nzb";
                                            //Console.WriteLine(fileToSave);

                                            // Download if new 
                                            var fileToSave = _dlDir + filename + ".nzb";
                                            if (!File.Exists(fileToSave))
                                            {
                                                Console.WriteLine($"DOWNLOADING: {(string)element.Element("title").Value}");
                                                wc.DownloadFile(link, fileToSave);
                                            }

                                            Thread.Sleep(1500);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: Change target in settings.txt");
                }

                // Sleep for 5 mins
                Thread.Sleep(300000);
            }
        }
    }
}
