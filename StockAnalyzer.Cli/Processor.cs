using System;
using System.Diagnostics;
using System.IO;
using StockAnalyzer.Api;
using StockAnalyzer.Api.Data;
using StockAnalyzer.Cli.Web;

namespace StockAnalyzer.Cli
{
    public class Processor
    {
        private const string MCStocksBack = "http://www.moneycontrol.com/stocks/";

        private const string MCStocks = "http://www.moneycontrol.com/financials/test/";
        private readonly string CacheDir;
        private readonly string CompanyCacheDir;
        private readonly string MCapCacheDir;
        private readonly string CompaniesListFile;
        private readonly string MCapFile;

        private const string Bal = "bal";
        private const string Prof = "prof";
        private const string Cash = "cash";

    
        public Processor()
        {
            CacheDir = new DirectoryInfo($@"{AppContext.BaseDirectory}\..\..\..\..\FinancialCache\").FullName;
            CompanyCacheDir = $@"{CacheDir}CompaniesCache\";
            MCapCacheDir = $@"{CacheDir}MarketCapCache\";
            CompaniesListFile = $"{CacheDir}CompaniesList.txt";
            MCapFile = $"{CacheDir}MarketCapFiles.txt";
        }

        public void DownloadCompanies()
        {
            string financialsBack = "company_info/print_financials.php?sc_did=";
            Console.WriteLine($"Companies cache will be created at: {new DirectoryInfo(CompanyCacheDir).FullName}");
            if (!Directory.Exists(CompanyCacheDir)) Directory.CreateDirectory(CompanyCacheDir);
            String[] companies = File.ReadAllLines(CompaniesListFile);
            foreach (var company in companies)
            {
                for (int counter = 1; counter >= 0; counter--)
                {
                    if (!File.Exists($"{CompanyCacheDir}{company}_{Prof}_{counter}.html"))
                    {
                        DownloadFinancial(Prof, company, counter);
                    }

                    if (!File.Exists($"{CompanyCacheDir}{company}_{Bal}_{counter}.html"))
                    {
                        DownloadFinancial(Bal, company, counter);
                    }
                }
            }
        }

        private void DownloadFinancial(string type, string company, int counter)
        {
            var year1 = (DateTime.Now.Year - 5);
            var year2 = DateTime.Now.Year;
            string startYear = counter == 0 ? year1.ToString() : year2.ToString();
            string endYear = counter == 0 ? year2.ToString() : (year1 + 1).ToString();
            string direction = counter == 0 ? "prev" : "next";

            string urlType = type == Bal ? "balance-sheetVI" : type == Prof ? "profit-lossVI" : "cash?";
            string postType = type == Bal ?  "balance_VI" : type == Prof ? "profit_VI" : "cash?";
            var request = new HttpRequestManager()
            {
                // Uri = $"{MCStocks}{financials}{company}&type=balance"
                Uri = $"{MCStocks}{urlType}/{company}",
                Method = HttpRequestManager.MethodPost,
                PostContent = $"nav={direction}&type={postType}&sc_did={company}&start_year={startYear}03&end_year={endYear}03&max_year={startYear}03"
            };
            File.WriteAllText($"{CompanyCacheDir}{company}_{type}_{counter}.html", request.GetData());
            Console.WriteLine($"Downloaded financial: {request.Uri}");
            Console.WriteLine($"Body: {request.PostContent}");
        }

        public void DownloadCompaniesList()
        {
            try
            {
                String[] urls = new[]
                {
                    $"{MCStocks}bse-group/a-group-companies-list.html",
                    $"{MCStocks}bse-group/b-group-companies-list.html",
                    $"{MCStocks}bse-group/s-group-companies-list.html",
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void DownloadMarketCapFiles()
        {
            string currentFile = string.Empty;
            try
            {
                String[] categories = File.ReadAllLines(MCapFile);
                if (!Directory.Exists(MCapCacheDir)) Directory.CreateDirectory(MCapCacheDir);
                foreach (var category in categories)
                {
                    if (File.Exists(MCapCacheDir + category)) continue;

                    var request = new HttpRequestManager()
                    {
                        Uri = $"{MCStocks}marketinfo/marketcap/bse/" + category
                    };
                    currentFile = request.Uri;
                    File.WriteAllText(MCapCacheDir + category, request.GetData());
                    Console.WriteLine($"Downloaded file: {currentFile}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + $"\n{currentFile}");
            }
        }

        public void GenerateSpreadSheet(bool addAllYears, string companyCode)
        {
            try
            {
                File.WriteAllText($"{CacheDir}Results.csv", String.Empty);
                String[] companies = File.ReadAllText($"{CacheDir}CompaniesList.txt").Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                // DataTable marketCapData = new MoneyControlMarketCapHtmlManager().GetAllFilesData(MCapFile, MCapCacheDir, companyCode);
                DataTable allData = new DataTable();
                DataTable marketCapData = new DataTable();
                // foreach (string companyId in companies)
                // {
                    if (File.Exists($"{CompanyCacheDir}{companyCode}_prof_0.html") && File.Exists($"{CompanyCacheDir}{companyCode}_bal_0.html"))
                    {
                        string profitResponse = File.ReadAllText($"{CompanyCacheDir}{companyCode}_prof_0.html");
                        string balanceResponse = File.ReadAllText($"{CompanyCacheDir}{companyCode}_bal_0.html");
                        new MoneyControlHtmlManager(profitResponse, balanceResponse, marketCapData, allData).PopulateCompanyDetails(companyCode, addAllYears);
                    }
                // }

                var csv = MoneyControlHtmlManager.GetCsv(allData);
                File.AppendAllText($"{CacheDir}Results.csv", csv);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}