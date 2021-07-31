using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
namespace PubMedDownloader
{
    class Program
    {
        static DirectoryInfo folder;
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(@"PubMedDownloader

You mus specify the files to process.

PubMedDownloader.Exe 2021073113553320210731135533.pmid

The pmid file should consists only of Id, for example
32915650
32895219
32816711
32769591
32728799
32665317
32517822

");
                return;
            }
            folder = new DirectoryInfo(ConfigurationManager.AppSettings["SummaryFolder"]?? "SummaryFolder");
            if (!folder.Exists) folder.Create();
            foreach(var arg in args)
            {
                if(!File.Exists(arg))
                {
                    Console.WriteLine($"The file {arg} was not found");
                }
                else
                {
                    var pmids = File.ReadAllLines(arg);
                    if (true) //do serialize - we do all in one
                    {
                        var shortlist = new List<string>();
                        
                        foreach (var pmid in pmids)
                        {
                            shortlist.Add(pmid);
                            if (shortlist.Count > 100) 
                            {
                                RetrieveAndSaveNcbiPubmedArticleBulk(string.Join(",", shortlist));
                                shortlist.Clear();
                            }                            
                        }
                        RetrieveAndSaveNcbiPubmedArticleBulk(string.Join(",", shortlist));
                    }
                    else
                    {//TOO MANY CALLS ERROR MAY OCCUR
                        Parallel.ForEach(pmids, pmid =>
                         {
                             RetrieveAndSaveNcbiPubmedArticle(pmid);
                         }
                            );
                    }
                }
            }

        }
        public static void RetrieveAndSaveNcbiPubmedArticleBulk(string pmid)
        {
            if (string.IsNullOrWhiteSpace(pmid)) return;
            
            var search = string.Format("http://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi?db=pubmed&id={0}&retmode=xml", pmid);
            new WebClient().DownloadFile(new Uri(search), "temp.Xml");
            var dom = new XmlDocument();
            dom.Load("temp.Xml");
            var nodes = dom.SelectNodes("//PubmedArticle");
            foreach(XmlNode node in nodes)
            {
                var idNode = node.SelectSingleNode("MedlineCitation/PMID");
                var id = idNode.InnerText;
                var fileLocation = Path.Combine(folder.FullName, $"{id}.xml");
                if (!File.Exists(fileLocation))
                {
                    File.WriteAllText(fileLocation, node.OuterXml);
                }
            }
        }
        public static void RetrieveAndSaveNcbiPubmedArticle(string pmid)
        {
            if (string.IsNullOrWhiteSpace(pmid)) return;
            var fileLocation = Path.Combine(folder.FullName, $"{pmid}.xml");            
            if (File.Exists(fileLocation)) return;
            
            var search = string.Format("http://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi?db=pubmed&id={0}&retmode=xml", pmid);
            new WebClient().DownloadFile(new Uri(search), fileLocation);


        }
    }
}
