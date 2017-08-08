using System;
using System.Collections.Generic;
using StockAnalyzer.Api.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;

namespace StockAnalyzer.Api
{
    public class MoneyControlMarketCapHtmlManager
    {
        private readonly DataTable marketCapTable;

        public const string MarketCapColumn = "MCap";
        public const string McIdColumn = "MCID";
        public const string CompanyNameColumn = "Company Name";

        public MoneyControlMarketCapHtmlManager()
        {
            marketCapTable = new DataTable();
            marketCapTable.Columns.Add(new DataColumn() { ColumnName = CompanyNameColumn });
            marketCapTable.Columns.Add(new DataColumn() { ColumnName = McIdColumn });
            marketCapTable.Columns.Add(new DataColumn() {ColumnName = MarketCapColumn });
        }

        private void AddFileData(HtmlDocument document)
        {
            var tableNode = document.DocumentNode.SelectSingleNode("//table[contains(@class, 'tbldata14')]");
            if (tableNode == null) return;
            var rows = tableNode.SelectNodes("tr");

            foreach (var row in rows)
            {
                var companyCell = row.SelectNodes("td/a/b");
                if (companyCell != null && companyCell.Count > 0)
                {
                    string href = companyCell.First().ParentNode.Attributes["href"].Value;
                    string companyId = href.Substring(href.LastIndexOf("/", System.StringComparison.Ordinal) + 1);
                    string marketCap = row.SelectSingleNode("td[last()]").InnerText;
                    marketCapTable.Rows.Add(new DataRow(marketCapTable));
                    marketCapTable.Rows[marketCapTable.Rows.Count - 1]["CompanyName"] = companyCell.First().InnerText;
                    marketCapTable.Rows[marketCapTable.Rows.Count - 1][McIdColumn] = companyId;
                    marketCapTable.Rows[marketCapTable.Rows.Count - 1][MarketCapColumn] = marketCap;
                }
            }
        }

        public DataTable GetAllFilesData(string mCapFile, string mCapCacheDir, string companyCode)
        {
            string[] fileNames = File.ReadAllLines(mCapFile);
            foreach (var fileName in fileNames)
            {
                string fileContent = File.ReadAllText($@"{mCapCacheDir}{fileName}").Replace(",", string.Empty);
                var document = new HtmlDocument();
                document.LoadHtml(fileContent);
                AddFileData(document);
            }
            return marketCapTable;
        }
    }
}
