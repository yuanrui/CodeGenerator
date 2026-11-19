using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Banana.AutoCode.DbSchema;
using WeifenLuo.WinFormsUI.Docking;

namespace Banana.AutoCode.Forms
{
    public partial class TablePanel : DockContent
    {
        protected Table Table;
        protected ConnectionStringSettings ConnSetting;

        public TablePanel(Table table, ConnectionStringSettings connSetting)
        {
            InitializeComponent();

            Table = table;
            ConnSetting = connSetting;

            Init();
        }

        public void Init()
        {
            this.dgvColumns.AutoGenerateColumns = false;

            this.txtTable.Text = this.Table.Name;
            this.txtClazz.Text = this.Table.DisplayName;
            this.txtComment.Text = this.Table.Comment;

            if (Table.Columns != null && Table.Columns.Any())
            {
                this.dgvColumns.DataSource = Table.Columns;
            }
            else
            {
                this.dgvColumns.DataSource = new DbSchemaManager(ConnSetting).GetColumns(Table);
            }
        }

        /// <summary>
        /// 刷新字段列表：用最新的 Columns 集合重新绑定 DataGridView。
        /// 调用方负责重新查询 DbSchemaManager.GetColumns 以确保数据是最新的。
        /// </summary>
        public void RefreshFields(IEnumerable<Column> columns)
        {
            // 先解绑再重新绑定，强制 DataGridView 刷新显示。
            this.dgvColumns.DataSource = null;
            this.dgvColumns.DataSource = columns.ToList();
        }
    }
}
