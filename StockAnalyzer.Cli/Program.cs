using System;
using StockAnalyzer.Api;

namespace StockAnalyzer.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine(@"Following options to choose from: 
                1. Download Market Cap Files.
                Note: The list of MCap files must already be present
                2. Download Companies List
                Gets the symbols of all the companies, which the rest of the operations depend on
                3. Import all Companies data
                4. Import Market Cap data
                5. Generate Results
                ");

                string result = Console.ReadLine();
                switch(result)
                {
                    case "1":   new Processor().DownloadMarketCapFiles();
                                break;
                    case "3":   new Processor().DownloadCompanies();
                                break;
                    case "5":   Console.WriteLine("Enter Company Code");
                                var companyCode = Console.ReadLine();
                                new Processor().GenerateSpreadSheet(true, companyCode);
                                break;
                    default:    break;
                }
            }
        }
    }
}
