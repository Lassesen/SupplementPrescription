using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PubMedScanner.Models
{
    public class SearchResult
    {
        public static SearchResult FromJson(string json) => JsonConvert.DeserializeObject<SearchResult>(json, PubmedSearch.Settings);

        public Header header { get; set; }
        public Esearchresult esearchresult { get; set; }
    }

    public class Header
    {
        public static Header FromJson(string json) => JsonConvert.DeserializeObject<Header>(json, PubmedSearch.Settings);

        public string type { get; set; }

        public string version
        {
            get; set;
        }
    }

    public class Translationset
    {
        public string from { get; set; }
        public string to { get; set; }
    }

    public class Esearchresult
    {
        public string count { get; set; }
        public string retmax { get; set; }
        public string retstart { get; set; }
        public List<string> idlist { get; set; }
        public List<Translationset> translationset { get; set; }
        public List<object> translationstack { get; set; }
        public string querytranslation { get; set; }
    }
}
