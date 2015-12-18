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
using System.Windows.Threading;

namespace pixiv_downloader.Contents
{
    /// <summary>
    /// Interaction logic for authorList.xaml
    /// </summary>
    public partial class authorList : UserControl
    {
        public authorList()
        {
            InitializeComponent();
        }
        CancellationTokenSource canceltokensource;
        pixiv_API.pixivAPI pixivAPI;
        private void authorlistbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            authorDetailShow.IsEnabled = false;
            if (((MainWindow)App.Current.MainWindow).pixivAPI == null)
            {
                return;
            }
            pixivAPI = ((MainWindow)App.Current.MainWindow).pixivAPI;

            if (authorlistbox.SelectedItem == null) return;

            if (canceltokensource != null)
            {
                canceltokensource.Cancel();
            }
            authorDetailShow.TabChooser.SelectedSource = null;//init right zone;
            canceltokensource = new CancellationTokenSource();
            pixivAuthor selectedAuthor = null;
            try
            {
                selectedAuthor = (pixivAuthor)authorlistbox.SelectedItem;
            }
            catch
            {
                authorDetailShow.IsEnabled = false;
                return;
            }
            authorDetailShow.authorNameLabel.Content = selectedAuthor.authorName;
            authorDetailShow.authorIDLabel.Content = selectedAuthor.authorID;
            authorDetailShow.authorSelected = selectedAuthor;
            ((MainWindow)App.Current.MainWindow).pixivAuthor = selectedAuthor;
            //init Image
            Task.Run(() =>
            {
                initImage(selectedAuthor.profileImageURL, canceltokensource);
            });

            authorDetailShow.IsEnabled = true;



        }

        private void initImage(string authorimgURL,CancellationTokenSource cts)
        {
            var task_imagedownload = pixivAPI.DownloadFileAsync("temp", authorimgURL, null, cts);//start download image 
            string imagepath = null;
            try
            {
                imagepath = task_imagedownload.Result;
            }
            catch
            {//image Task Cancelled
                return;
            }

            if (imagepath != null)
            { // Read byte[] from png file
                BinaryReader binReader = new BinaryReader(File.Open(imagepath, FileMode.Open));
                FileInfo fileInfo = new FileInfo(imagepath);
                byte[] bytes = binReader.ReadBytes((int)fileInfo.Length);
                binReader.Close();

                // Init bitmap

                authorDetailShow.authorImage.Dispatcher.Invoke(() =>
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(bytes);
                    bitmap.EndInit();
                    authorDetailShow.authorImage.Source = bitmap;
                });

                File.Delete(imagepath);


            }
        }

    }
}
