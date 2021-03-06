﻿using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Collections.ObjectModel;

namespace StockAnalyzer.Api.Data
{
    public class DataTable : IEnumerable, INotifyCollectionChanged
    {
        private IList<DataColumn> columns;

        public DataTable()
        {
            columns = new List<DataColumn>();
        }

        private ObservableCollection<DataRow> rows;
        private IList internalView;
        private Type elementType;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IList<DataColumn> Columns
        {
            get
            {
                return columns;
            }
        }


        public IList<DataRow> Rows
        {
            get
            {
                if (this.rows == null)
                {
                    this.rows = new ObservableCollection<DataRow>();
                    this.rows.CollectionChanged += OnRowsCollectionChanged;
                }

                return rows;
            }
        }

        public DataRow NewRow()
        {
            return new DataRow(this);
        }


        private void OnRowsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // switch (e.Action)
            // {
            //     case NotifyCollectionChangedAction.Add:
            //         this.InternalView.Insert(e.NewStartingIndex, ((DataRow)e.NewItems[0]).RowObject);
            //         break;
            //     case NotifyCollectionChangedAction.Remove:
            //         this.InternalView.RemoveAt(e.OldStartingIndex);
            //         break;
            //     case NotifyCollectionChangedAction.Replace:
            //         this.InternalView.Remove(((DataRow)e.OldItems[0]).RowObject);
            //         this.InternalView.Insert(e.NewStartingIndex, ((DataRow)e.NewItems[0]).RowObject);
            //         break;
            //     case NotifyCollectionChangedAction.Reset:
            //     default:
            //         this.InternalView.Clear();
            //         this.Rows.Select(r => r.RowObject).ToList().ForEach(o => this.InternalView.Add(o));
            //         break;
            // }
        }

        private IList InternalView
        {
            get
            {
                if (this.internalView == null)
                {
                    this.CreateInternalView();
                }

                return this.internalView;
            }
        }

        private void CreateInternalView()
        {
            this.internalView = (IList)Activator.CreateInstance(typeof(ObservableCollection<>).MakeGenericType(this.ElementType));
            ((INotifyCollectionChanged)internalView).CollectionChanged += (s, e) => { this.OnCollectionChanged(e); };
        }

        internal Type ElementType
        {
            get
            {
                if (this.elementType == null)
                {
                    this.InitializeElementType();
                }

                return this.elementType;
            }
        }

        private void InitializeElementType()
        {
            this.Seal();
            // this.elementType = DynamicObjectBuilder.GetDynamicObjectBuilderType(this.Columns);
        }

        private void Seal()
        {
            this.columns = new ReadOnlyCollection<DataColumn>(this.Columns);
        }

        public IEnumerator GetEnumerator()
        {
            return this.InternalView.GetEnumerator();
        }

        public IList ToList()
        {
            return this.InternalView;
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = this.CollectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public bool ContainsColumn(string columnName)
        {
            foreach (var column in this.columns)
            {
                if (column.ColumnName == columnName) return true;
            }

            return false;
        }
    }
}