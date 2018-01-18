using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq;

namespace LetItPlay.YoutubeDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            string[] searchPatterns = searchPattern.Split('|');
            List<string> files = new List<string>();
            foreach (string sp in searchPatterns)
                files.AddRange(System.IO.Directory.GetFiles(path, sp, searchOption));
            files.Sort();
            return files.ToArray();
        }
        private void btnLanuch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.textUrl.Text))
            {
                MessageBox.Show("url empty");
                return;
            }
            DateTime dt = this.dateTimePicker1.Value;
            string dat = dt.ToString("yyyyMMdd");
            string url = this.textUrl.Text;
            //--download-archive archive.txt
            string cmd = String.Format(
                "  --dateafter {0} -o \"{2}/%(title)s.%(ext)s\" --write-info-json  -f \"worstvideo + bestaudio[ext = mp3] / best[ext = mp3] / best\" -x --audio-format mp3 {1}", 
                dat,url, Application.StartupPath + "\\Downloaded");
           
            string processName = "dl/youtube-dl.exe";
            Directory.CreateDirectory("Downloaded");

            string[] files = GetFiles("Downloaded", "*.mp3|*.json");
            for (var i = files.Length - 1; i >= 0; i--)
            {
                File.Delete(files[i]);
            }

            var process = Process.Start(Application.StartupPath + "\\"+processName,cmd);
            process.WaitForExit();
            ProcessJsons("Downloaded\\");

            Process.Start(@"Downloaded\\");
        }

        public void ProcessJsons (string path)
        {
            string[] files = GetFiles("Downloaded", "*.json");
            for (var i = files.Length - 1; i >= 0; i--)
            {
                var inpJson = File.ReadAllText(files[i]);
                var outJson =ExtractJson(inpJson);
                File.Delete(files[i]);
                File.WriteAllText(files[i],outJson);
            }

        }

        public string ExtractJson(string jsonInput) {
            string res="";
            JObject jInput = JObject.Parse(jsonInput);
            JObject jOUt = new JObject();
            jOUt.Add("url",jInput.GetValue("url"));
            jOUt.Add("title", jInput.GetValue("title"));
            jOUt.Add("thumbnail", jInput.GetValue("thumbnail"));
            jOUt.Add("fulltitle", jInput.GetValue("fulltitle"));
            JArray tags = new JArray(jInput.GetValue("tags"));
            jOUt.Add("tags", tags);

            return jOUt.ToString();
        }
    }
}
