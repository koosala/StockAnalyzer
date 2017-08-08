using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using StockAnalyzer.Api.Data;

namespace StockAnalyzer.Api
{
    public class MoneyControlHtmlManager
    {
        private const string ProfitParentXPath = "//div[@class='boxBg1']/table[tr/td/@class='detb']";
        private readonly HtmlDocument profitDocument;
        private readonly HtmlDocument balanceDocument;
        private readonly DataTable companiesTable;
        private readonly DataTable marketCapData;

        public MoneyControlHtmlManager(string profitHtmlContent, string balanceHtmlContent, DataTable marketCapData, DataTable allData)
        {
            profitDocument = new HtmlDocument();
            balanceDocument = new HtmlDocument();
            profitDocument.LoadHtml(profitHtmlContent);
            balanceDocument.LoadHtml(balanceHtmlContent);
            companiesTable = new DataTable(); // "ProfitAndBalance"
            this.marketCapData = marketCapData;
            this.companiesTable = allData;
        }

        public void PopulateCompanyDetails(string companyId, bool addAllYears)
        {
            if (profitDocument.DocumentNode.SelectNodes(ProfitParentXPath) == null) return;
            List<DataRow> currentCompanyRows = new List<DataRow>();
            AddYearRows(companyId, addAllYears, currentCompanyRows);
            AddHeaderColumns(profitDocument, false, addAllYears, currentCompanyRows, companyId);
            AddHeaderColumns(balanceDocument, true, addAllYears, currentCompanyRows);
            AddMarketCap(companyId, currentCompanyRows);
        }

        private string GetCompanyName()
        {
            string companyName = profitDocument.DocumentNode.SelectNodes("//table")[2].SelectNodes("//tr/td/b")[0].InnerText;
            return companyName;
        }

        public static string GetCsv(DataTable dataTable)
        {
            var lines = new List<string>();
            string columnNames = string.Empty;
            foreach (DataColumn column in dataTable.Columns)
            {
                columnNames += column.ColumnName + ",";
            }
            lines.Add(columnNames);
            foreach (var row in dataTable.Rows)
            {
                string line = string.Empty;
                foreach (var column in row.RowObject.Keys)
                {
                    line += row.RowObject[column].ToString() + ",";
                }
                lines.Add(line);
            }
            return string.Join("\n", lines) + "\n"; //Environment.NewLine
        }

        private void AddMarketCap(string companyId, IEnumerable<DataRow> dataRows)
        {
            var marketCapRow = marketCapData.Rows.AsEnumerable().FirstOrDefault(row => row.RowObject.ContainsKey(MoneyControlMarketCapHtmlManager.McIdColumn + "='" + companyId + "'"));
            if (marketCapRow != null)
            {
                string marketCap = marketCapRow[MoneyControlMarketCapHtmlManager.MarketCapColumn].ToString();
                foreach (var row in dataRows)
                {
                    row["Market Cap"] = marketCap;
                }
            }
        }

        private void AddYearRows(string companyId, bool addAllYears, List<DataRow> currentCompanyRows)
        {
            List<string> years = new List<string>();
            var yearCollection = profitDocument.DocumentNode.SelectNodes(ProfitParentXPath)[0].ChildNodes[1].SelectNodes("td");
            if (!companiesTable.ContainsColumn("Company"))
            {
                var col = new DataColumn(){ ColumnName = "Company" };
                companiesTable.Columns.Add(col);
            }
            if (!companiesTable.ContainsColumn("Year")) companiesTable.Columns.Add(new DataColumn(){ ColumnName = "Year" });
            if (!companiesTable.ContainsColumn("Company")) companiesTable.Columns.Add(new DataColumn(){ ColumnName = "CompanyId" });
            if (!companiesTable.ContainsColumn("Market Cap")) companiesTable.Columns.Add(new DataColumn(){ ColumnName = "Market Cap" });

            for (int i = 1; i < (addAllYears ? yearCollection.Count : 2); i++)
            {
                years.Add(yearCollection[i].InnerText);
            }
            years.ForEach(year => 
            {
                var row = new DataRow(companiesTable);
                row.RowObject.Add(GetCompanyName(), year);
                companiesTable.Rows.Add(row);
                currentCompanyRows.Add(row);
            });

            foreach (DataRow row in currentCompanyRows)
            {
                row["CompanyId"] = companyId;
            }
        }

        private bool ColumnNameDuplicate(string columnName)
        {
            foreach (DataColumn column in companiesTable.Columns)
            {
                if (column.ColumnName == columnName)
                { return true; }
            }
            return false;
        }

        private void AddHeaderColumns(HtmlDocument document, bool balance, bool addAllYears, List<DataRow> currentCompanyRows, string companyId = "")
        {
            string columnName;
            var parentNode = GetDataParentNode(document);
            if (parentNode == null) return;
            var dataRows = parentNode.SelectNodes("tr");
            for (int i = 1; i < dataRows.Count - 2; i++)
            {
                columnName = dataRows[i].SelectNodes("td")[0].InnerText;
                if (! ColumnNameDuplicate(columnName)) // columnName += "_1";
                    companiesTable.Columns.Add(new DataColumn(){ ColumnName = columnName });
                AddYearInfo(currentCompanyRows, dataRows[i], columnName, addAllYears);
            }
            // parentNode = GetSecondLevelDataParentNode(dataRows[dataRows.Count - 1]);
            // dataRows = parentNode.SelectNodes("tr");
            // for (int i = 0; i < (balance ? dataRows.Count - 1 : dataRows.Count - 5); i++)
            // {
            //     columnName = dataRows[i].SelectNodes("td")[0].InnerText;
            //     if (string.IsNullOrEmpty(columnName)) continue;
            //     if (! ColumnNameDuplicate(columnName)) // columnName += "_1"; TODO: Are there duplicate column names in 1 company details
            //         companiesTable.Columns.Add(new DataColumn(){ ColumnName = columnName });
            //     AddYearInfo(currentCompanyRows, dataRows[i], columnName, addAllYears);
            // }
        }

        private void AddYearInfo(List<DataRow> currentCompanyRows, HtmlNode htmlRow, string columnName, bool addAllYears)
        {
            currentCompanyRows[0][columnName] = htmlRow.SelectNodes("td").Count > 1 ? htmlRow.SelectNodes("td")[1].InnerText : string.Empty;
            if (addAllYears)
            {
                if (htmlRow.SelectNodes("td").Count > 2)
                    currentCompanyRows[1][columnName] = htmlRow.SelectNodes("td")[2].InnerText;
                if (htmlRow.SelectNodes("td").Count > 3) 
                    currentCompanyRows[2][columnName] = htmlRow.SelectNodes("td")[3].InnerText;
                if (htmlRow.SelectNodes("td").Count > 4)
                    currentCompanyRows[3][columnName] = htmlRow.SelectNodes("td")[4].InnerText;
                if (htmlRow.SelectNodes("td").Count > 5) 
                    currentCompanyRows[4][columnName] = htmlRow.SelectNodes("td")[5].InnerText;
            }
        }

        private HtmlNode GetDataParentNode(HtmlDocument document)
        {
            var nodes = document.DocumentNode.SelectNodes(ProfitParentXPath);
            if (nodes != null)
            {
                var children = nodes[0].ChildNodes;
                if (children != null && children.Count > 3)
                {
                    var nextChildren = children[3].SelectNodes("tr");
                    if (nextChildren != null && nextChildren.Count > 1)
                    {
                        return nextChildren[1];
                    }
                }
            }
            return null;
        }

        private HtmlNode GetSecondLevelDataParentNode(HtmlNode firstLevelParent)
        {
            return firstLevelParent.SelectNodes("tr")[1];
        }
    }
}
