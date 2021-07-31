using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.IO;

namespace PubMedScanner
{
    class PubmedSearch
    {
        List<string> seenFiles = new List<string>();
        List<string> newPmids = new List<string>();
        string[] keywords = { };
        string pmidFile = "lastpmid.txt";
        string[] searchWords = { };
        string scanresultsFolder = "";
        private int _pmid = 0;
        private int _daysAgo = 365; // Last year
        int lastPmid = 0;
        public PubmedSearch()
        {
            var keywordFile = ConfigurationManager.AppSettings["keywords"];
            var keywordFileInfo = new FileInfo(keywordFile);
            if (!keywordFileInfo.Exists)
            {
                if (!keywordFileInfo.Directory.Exists) keywordFileInfo.Directory.Create();
                keywordFileInfo.Create();
                File.WriteAllText(keywordFile,"Long Covid");
                
            }
            keywords=File.ReadAllLines(keywordFile);
            pmidFile = ConfigurationManager.AppSettings["lastpcmid"];
            if(!File.Exists(pmidFile))
            {
                File.WriteAllText(pmidFile, "0");
            }
            var allLines = File.ReadAllLines(pmidFile);
            if(allLines.Length > 0)
            {
                int.TryParse(allLines[0], out _pmid);
            }
            
                int.TryParse(ConfigurationManager.AppSettings["daysago"], out _daysAgo);
            
            scanresultsFolder = ConfigurationManager.AppSettings["scanresultsFolder"]??"Results";
            var folder = new DirectoryInfo(ConfigurationManager.AppSettings["scanresultsFolder"]);
            if (!folder.Exists) folder.Create();
            //Get a list of all articles already identified
            foreach(FileInfo file in folder.GetFiles("*.PMID"))
            {
                var lines = File.ReadAllLines(file.FullName);
                foreach(var line in lines)
                {
                    if (!seenFiles.Contains(line)) seenFiles.Add(line);
                }
            }

            FullScan();

        }
        /// <summary>
        /// Returns the number of new files found
        /// </summary>
        /// <returns></returns>
        public int FullScan()
        {
            var scanfileName = Path.Combine(scanresultsFolder, $"{DateTime.Now.ToString("yyyyMMddHHmmss")}.pmid");
            foreach (var keyword in keywords)
            {
                var scanPmId = GetPubMedIds(keyword);
                foreach(var pmId in scanPmId)
                {
                    if (!seenFiles.Contains(pmId))
                    {
                        if (!newPmids.Contains(pmId))
                            newPmids.Add(pmId);
                    }
                }
            }
            File.WriteAllLines(scanfileName, newPmids.ToArray());
            Console.WriteLine($"Full Scan returned {newPmids.Count} new items listed in {scanfileName}");
            return newPmids.Count;
        }
        
        private List<string> GetPubMedIds(string term)
        {

            term = term.Replace("[", "").Replace("]", "");
            List<string> result = new List<string>();
            string url = $"https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi?db=pubmed&term={term}&retmode=json&retmax=100000&reldate={_daysAgo}";
            try
            {
                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                {
                    string json = client.GetStringAsync(url).Result;
                    Models.SearchResult pubmed = Models.SearchResult.FromJson(json);
                    foreach (string item in pubmed.esearchresult.idlist)
                    {
                        int pmId = int.Parse(item);
                        if(! result.Contains(item))
                            result.Add(item);
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
            Console.WriteLine($"{term} returned {result.Count} items");
            return result;
        }
        public static readonly JsonSerializerSettings Settings = new Newtonsoft.Json.JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter()
                {
                    DateTimeStyles = DateTimeStyles.AssumeUniversal,
                },
            },
        };
    }
}
