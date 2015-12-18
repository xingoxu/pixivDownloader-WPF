using FirstFloor.ModernUI.Windows.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

namespace pixiv_downloader.Pages
{
    /// <summary>
    /// Interaction logic for SearchPage.xaml
    /// </summary>
    public partial class SearchPage : UserControl
    {
        public SearchPage()
        {
            InitializeComponent();
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            if (search_textbox.Text.Equals("")) return;
            searchDialog dlg = new searchDialog();
            dlg.MaxHeight = int.MaxValue;
            dlg.MaxWidth = int.MaxValue;
            dlg.MinWidth = 700;
            dlg.MinHeight = 500;
            dlg.query = search_textbox.Text;
            dlg.ShowDialog();

        }

        private void author_searchButton_Click(object sender, RoutedEventArgs e)
        {
            if (author_search_textbox.Text.Equals("")) return;
            var pixivAPI = ((MainWindow)App.Current.MainWindow).pixivAPI;
            waitingForProcessing waitdialog = new waitingForProcessing();
            CancellationTokenSource cts = new CancellationTokenSource();
            pixivAuthor author = null;
            string query = author_search_textbox.Text;
            waitdialog.progressbar.Visibility = Visibility.Hidden;
            Task.Run(() =>
            {
                JObject result = null;
                try
                {
                    result = pixivAPI.user_profileAsync(query, cts).Result;
                }
                catch
                {//task cancelled
                    waitdialog.Dispatcher.Invoke(() =>
                    {
                        try { waitdialog.Close(); } catch { }
                    });
                    return;
                }
                if (result != null)
                {
                    foreach (JObject response in result.Value<JArray>("response"))
                    {
                        author = new pixivAuthor() { authorID = (string)response["id"], authorName = (string)response["name"], profileImageURL = (string)response["profile_image_urls"]["px_50x50"] };
                    }
                }
                waitdialog.Dispatcher.Invoke(() =>
                {
                    try { waitdialog.Close(); } catch { }
                });
            });
            waitdialog.ShowDialog();
            if (waitdialog.MessageBoxResult == MessageBoxResult.Cancel)
            {
                cts.Cancel();
                return;
            }
            if (author==null)
            {
                ModernDialog.ShowMessage("没有解析到信息，可能是输入错误，建议重试", "发生错误", MessageBoxButton.OK);
                return;
            }

            authorDialog dlg = new authorDialog();
            dlg.MaxWidth = int.MaxValue;
            dlg.MaxHeight = int.MaxValue;
            dlg.MinWidth = 800;
            dlg.MinHeight = 600;
            dlg.Title = author.authorName + " 的页面";
            dlg.authorDetail.authorIDLabel.Content = author.authorID;
            dlg.authorDetail.authorNameLabel.Content = author.authorName;
            ((MainWindow)App.Current.MainWindow).pixivAuthor = author;
            Task.Run(() =>
            {
                initAuthorImageForAuthorDialog(author.profileImageURL, dlg, pixivAPI);
            });
            dlg.ShowDialog();

        }
        private void initAuthorImageForAuthorDialog(string authorimgURL, authorDialog dlg,pixiv_API.pixivAPI pixivAPI)
        {
            var task_imagedownload = pixivAPI.DownloadFileAsync("temp", authorimgURL);//start download image 
            string imagepath = null;
            try
            {
                imagepath = task_imagedownload.Result;
            }
            catch
            {//image Task Cancelled
                return;
            }
            try
            {
                if (imagepath != null)
                { // Read byte[] from png file
                    BinaryReader binReader = new BinaryReader(File.Open(imagepath, FileMode.Open));
                    FileInfo fileInfo = new FileInfo(imagepath);
                    byte[] bytes = binReader.ReadBytes((int)fileInfo.Length);
                    binReader.Close();

                    // Init bitmap

                    dlg.authorDetail.authorImage.Dispatcher.Invoke(() =>
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = new MemoryStream(bytes);
                        bitmap.EndInit();
                        dlg.authorDetail.authorImage.Source = bitmap;
                    });

                    File.Delete(imagepath);
                }
            }
            catch
            {

            }
        }

    }
}
