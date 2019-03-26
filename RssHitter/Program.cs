using System;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;

namespace RssHitter
{
    class Program
    {
        private static readonly string _settingsFolder = "settings";
        private static readonly string _settingsFile = "settings.txt";
        private static readonly string _dlDirectory = "/target/directory/";

        private static string _rssUrl { get; set; }

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;

            Console.WriteLine("Starting RSS Hitter");
            Console.WriteLine("");
            Console.WriteLine("Reading Settings");
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
                    Console.WriteLine($"TARGET URL set to:{_rssUrl}");
                    Console.WriteLine("");
                }
            }
            else
            {
                // Make the settings file
                using (var fileStream = System.IO.File.Create(localPath)) 
                using (var fileWriter = new System.IO.StreamWriter(fileStream)) 
                {
                    Console.WriteLine("Settings file made, go imput your RSS feed on /settings/settings.txt");
                    fileWriter.WriteLine("https://rsslink.com");
                    _rssUrl = "https://rsslink.com";
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
                                            // Download if new 
                                            //var fileToSave = AppContext.BaseDirectory + (string)element.Element("title").Value + ".nzb";
                                            //Console.WriteLine(fileToSave);

                                            var fileToSave = _dlDirectory + (string)element.Element("title").Value + ".nzb";
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
