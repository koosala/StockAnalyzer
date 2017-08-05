using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzer.Api.Data
{
    public class DataRow
    {
        private readonly DataTable owner;
        private Dictionary<string, object> rowObject;

        protected internal DataRow(DataTable owner)
        {
            this.owner = owner;
        }

        public object this[int columnIndex]
        {
            get
            {
                object result = null;
                var columnName = RowObject.Keys.Skip(columnIndex).FirstOrDefault();
                if (this.RowObject.TryGetValue(columnName, out result)) return result;
                return string.Empty;
            }
            set
            {
                var columnName = RowObject.Keys.Skip(columnIndex).FirstOrDefault();
                if (RowObject.ContainsKey(columnName)) RowObject[columnName] = value;
                else RowObject.Add(columnName, value);
            }
        }

        public object this[string columnName]
        {
            get
            {
                object result = null;
                if (this.RowObject.TryGetValue(columnName, out result)) return result;
                return string.Empty;
            }
            set
            {
                if (RowObject.ContainsKey(columnName)) RowObject[columnName] = value;
                else RowObject.Add(columnName, value);
            }
        }

        internal Dictionary<string, object> RowObject
        {
            get
            {
                this.EnsureRowObject();
                return this.rowObject; 
            }
        }

        private void EnsureRowObject()
        {
            if (this.rowObject == null)
            {
                this.rowObject = new Dictionary<string, object>();
            }
        }
    }
}
