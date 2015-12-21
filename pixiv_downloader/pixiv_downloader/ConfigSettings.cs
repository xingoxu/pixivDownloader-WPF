using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace pixiv_downloader
{
    [Serializable]
    [XmlRoot("ConfigSettings")]
    public class ConfigSettings
    {
        public ConfigSettings()
        {

        }
        [XmlElement]
        public string UserName { get; set; }
        [XmlElement]
        public string PassWord { get; set; }
        [XmlElement]
        public bool savePassword { get; set; }
        [XmlElement]
        public string currentTheme { get; set; }
        [XmlElement]
        public Color currentColor { get; set; }
        [XmlElement("DefaultWorkPath")]
        public string workPath { get; set; }
        [XmlElement]
        public bool showR18 { get; set; }
        [XmlElement]
        public bool autoSaveTask { get; set; }
        [XmlArray]
        public List<ExportTask> AutoSavedTasks { get; set; }
        private bool showdownloaddlg = true;
        [XmlElement]
        public bool showDownloadDialog { get { return showdownloaddlg; }  set { showdownloaddlg = value; } }

    }
    [Serializable]
    [XmlRoot("TaskInfo")]
    public class ExportTask
    {
        [XmlAttribute("ShowName")]
        public string TaskName { get; set; }
        [XmlAttribute("Type")]
        public Tasktype type;
        [XmlElement("id")]
        public string illustID { get; set; }
        [XmlArrayItem("No")]
        public int[] mangaSelected {
            get; set; }
        [XmlElement("workPath")]
        public string workPath { get; set; }
        public enum Tasktype
        {
            illust,
            manga,
            ugoira
        }
    }
}