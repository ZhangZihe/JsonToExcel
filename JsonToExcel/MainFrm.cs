using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JsonToExcel
{
    public partial class MainFrm : Form
    {
        public MainFrm()
        {
            InitializeComponent();
            txtContent.MaxLength = int.MaxValue;
            //lstResult.Groups.Add(new ListViewGroup("预览仅展示前50条"));
        }

        private List<string> Result;

        private void btnPreview_Click(object sender, EventArgs e)
        {
            var alllines = txtContent.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Where(x => !x.StartsWith("#"));
            var content = string.Join("\r\n", alllines);

            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("请选择文件或粘贴Json文本");
                return;
            }

            var lines = new List<string>();
            try
            {
                var paths = txtJsonPath.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Where(x => !x.StartsWith("#"));
                var arr = string.IsNullOrWhiteSpace(txtRoot.Text) ? JArray.Parse(content) : JObject.Parse(content).SelectToken(txtRoot.Text);

                lines.Add(string.Join(",", paths.Select(x => x.Substring(x.LastIndexOf('.') + 1))));
                foreach (var jtoken in arr)
                {
                    var items = paths.Count() == 0 ? jtoken.Select(x => x?.Value<object>()) : paths.Select(x => jtoken.SelectToken(x)?.Value<object>());
                    var line = string.Join(",", items);
                    lines.Add(line);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("请检查数据格式:" + ex.Message);
                return;
            }
            
            lstResult.GridLines = true;
            lstResult.Columns.Clear();
            lstResult.Items.Clear();
            lines[0].Split(',').ToList().ForEach(x => lstResult.Columns.Add(new ColumnHeader() { Text = x, Width = 100 }));
            Result = lines;

            foreach (var item in lines.Skip(1).Take(100))
            {
                var split = item.Split(',').ToList();
                var lstItem = new ListViewItem(split[0]) { Name = split[0] }; //, Group = lstResult.Groups[0]
                split.Skip(1).ToList().ForEach(x => lstItem.SubItems.Add(x));
                lstResult.Items.Add(lstItem);
            }
        }

        private void btnFile_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = "C://";
            dialog.Filter = "文本文件|*.txt";
            dialog.RestoreDirectory = false;

            if (dialog.ShowDialog() == DialogResult.OK)
                txtContent.Text = File.ReadAllText(dialog.FileName);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (Result == null || Result.Count == 0)
            {
                MessageBox.Show("请先点击预览");
                return;
            }

            var dialog = new SaveFileDialog();
            dialog.InitialDirectory = "C://";
            dialog.Filter = "表格文件|*.csv";
            dialog.RestoreDirectory = false;
            dialog.FileName = "json.csv";

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            File.WriteAllLines(dialog.FileName, Result.ToArray());
            var ans = MessageBox.Show("是否打开以保存的文件?", "保存成功", MessageBoxButtons.YesNo);
            if (ans == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start("explorer.exe", dialog.FileName);
            }
        }
    }
}
