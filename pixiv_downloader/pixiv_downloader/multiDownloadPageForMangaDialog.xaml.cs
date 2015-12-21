using FirstFloor.ModernUI.Windows.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace pixiv_downloader
{
    /// <summary>
    /// Interaction logic for multiDownloadPageForMangaDialog.xaml
    /// </summary>
    public partial class multiDownloadPageForMangaDialog : ModernDialog
    {
        private pixiv_API.pixivAPI pixivAPI;

        public multiDownloadPageForMangaDialog()
        {
            InitializeComponent();
            canceltoken = new CancellationTokenSource();

            downloadButton.Click += DownloadButton_Click;
            picListView.SelectionChanged += lv_SelectionChanged;

            stopRefreshButton.Click += StopRefreshButton_Click;
        }

        public pixivIllust illust;
        public void init(pixivIllust illust)
        {
            this.illust = illust;
            int i = 1;
            foreach(string x in this.illust.OriginalURL)
            {
                list.Add(new mangaListModel() { Number = i, URL = x });
                i++;
            }
            picListView.ItemsSource = list;
        }
        private List<mangaListModel> list = new List<mangaListModel>();
        private ObservableCollection<illustTask> downloadTasks;
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            downloadTasks = ((MainWindow)App.Current.MainWindow).downloadTasks;
            ConfigSettings setting = ((MainWindow)App.Current.MainWindow).configsettings;
            string workPath = setting.workPath;
            if (setting.showDownloadDialog)
            {
                chooseRoute chooseRoutedlg = new chooseRoute();
                chooseRoutedlg.FolderTextBox.Text = setting.workPath;
                chooseRoutedlg.ShowDialog();
                if (chooseRoutedlg.MessageBoxResult == MessageBoxResult.Cancel) return;

                if ((bool)chooseRoutedlg.routeByselfCheckBox.IsChecked)//I found this name problem but I don't want to change it...
                {
                    workPath = chooseRoutedlg.FolderTextBox.Text;
                    setting.workPath = workPath;
                }
                else
                {
                    workPath = setting.workPath;
                }
                bool showchooseRoutedlg = (bool)chooseRoutedlg.remind.IsChecked;
                setting.showDownloadDialog = !showchooseRoutedlg;
            }

            if (picListView.SelectedItems.Count == 1)
            {
                illustTask illusttask = new illustTask(mangaPage_selected, pixivAPI, illust.titleName + mangaPage_selected.Number.ToString(), illust, setting.workPath);
                downloadTasks.Add(illusttask);
            }
            else if (picListView.SelectedItems.Count > 1)
            {
                illustTask illusttask = new illustTask(mangaPages_selected, pixivAPI, illust.titleName + mangaPages_selected[0].Number.ToString() + " " + mangaPages_selected[1].Number.ToString() + " 等", illust, setting.workPath);
                downloadTasks.Add(illusttask);
            }
        }

        private bool processing;
        private void StopRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (processing)
            {
                canceltoken.Cancel();
                processing = false;
            }
            else
            {
                mangaPage_selected = null;
                lv_SelectionChanged(null, null);
            }
        }

        private CancellationTokenSource canceltoken;
        private mangaListModel mangaPage_selected;
        private List<mangaListModel> mangaPages_selected;
        private async void lv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            processing = true;
            StopRefreshButton_Click(null, null);
            processing = true;
            canceltoken = new CancellationTokenSource();
            if (picListView.SelectedItem == null)
            {//TODO:if select null then disable button/thread stop
                progressbar.Visibility = Visibility.Hidden;
                stopRefreshButton.Visibility = Visibility.Hidden;
                image.Source = null;
                illustType.Content = "尚未选择项";
                downloadButton.IsEnabled = false;
                processing = false;
                return;
            }
            mangaListModel mangaPage = null;
            try
            {
                mangaPage = (mangaListModel)picListView.SelectedItem;
            }
            catch
            {
                Debug.WriteLine(3);
                processing = false;
                return;
            }

            //do a nice optimize to check if the first select item haven't changed
            if (picListView.SelectedItems.Count > 1)
            {
                illustType.Content = "共选择" + picListView.SelectedItems.Count + "项";
                downloadButton.Content = "全部下载";
                mangaPages_selected = picListView.SelectedItems.Cast<mangaListModel>().ToList();
            }
            else
            {

                downloadButton.Content = "下载";
            }
            if (mangaPage != null && mangaPage_selected != null)
                if (mangaPage.Number.Equals(mangaPage_selected.Number))
                {
                    processing = false; return;
                }
            //end optimize

            //start to init
            progressbar.Visibility = Visibility.Visible;
            stopRefreshButton.Visibility = Visibility.Visible;
            stopRefreshButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/-sign-ban.png")));

            mangaPage_selected = mangaPage;

            image.Source = null;

            pixivAPI = ((MainWindow)App.Current.MainWindow).pixivAPI;


            downloadButton.IsEnabled = true;

            illustType.Content = "正在加载...";


            var task_imagedownload = pixivAPI.DownloadFileAsync("temp", illust.MediumURL[mangaPage.Number - 1], null, canceltoken);//start download image 

            //wait loading picture 
            string imagepath = null;
            try
            {
                imagepath = await task_imagedownload;
            }
            catch
            {//image Task Cancelled
                progressbar.Visibility = Visibility.Hidden;
                stopRefreshButton.Visibility = Visibility.Visible;
                stopRefreshButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
            }

            if (imagepath != null)
            { // Read byte[] from png file
                BinaryReader binReader = new BinaryReader(File.Open(imagepath, FileMode.Open));
                FileInfo fileInfo = new FileInfo(imagepath);
                byte[] bytes = binReader.ReadBytes((int)fileInfo.Length);
                binReader.Close();

                // Init bitmap
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = new MemoryStream(bytes);
                bitmap.EndInit();

                File.Delete(imagepath);

                image.Source = bitmap;
            }

            progressbar.Visibility = Visibility.Hidden;
            stopRefreshButton.Visibility = Visibility.Visible;
            illustType.Content = "加载完成";
            stopRefreshButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
            processing = false;
        }

    }
}
