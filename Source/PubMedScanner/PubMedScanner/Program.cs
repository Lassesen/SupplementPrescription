using System;

namespace PubMedScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Scan");
            var instance = new PubmedSearch();
            Console.WriteLine($"{instance.FullScan()} new studies found");
            Console.WriteLine("Scan Completed");
        }
    }
}
