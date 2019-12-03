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
            txtResult.MaxLength = int.MaxValue;
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

                lines.Add("\r\n" + string.Join(",", paths.Select(x => x.Substring(x.LastIndexOf('.') + 1))));
                foreach (var jtoken in arr)
                {
                    var items = paths.Count() == 0 ? jtoken.Select(x => x.ToString(Formatting.None)) : paths.Select(x => jtoken.SelectToken(x).ToString(Formatting.None));
                    var line = string.Join(",", items);
                    lines.Add(line);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("请检查数据格式:" + ex.Message);
                return;
            }

            var prelines = lines.Take(40).Select(x => x.Replace(",", "\r\t")).ToList();
            prelines.Insert(1, "-----------------------------------------------------------------------------------------");
            txtResult.Text = "预览仅展示前40条:\r\n";
            txtResult.Text += string.Join("\r\n", prelines);
            Result = lines;
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
            var dialog = new SaveFileDialog();
            dialog.InitialDirectory = "C://";
            dialog.Filter = "表格文件|*.csv";
            dialog.RestoreDirectory = false;
            dialog.FileName = "json.csv";

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            File.WriteAllLines(dialog.FileName, Result.ToArray());
        }
    }
}
