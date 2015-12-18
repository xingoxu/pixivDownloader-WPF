using FirstFloor.ModernUI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using FirstFloor.ModernUI.Windows.Navigation;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace pixiv_downloader.Contents
{
    /// <summary>
    /// Interaction logic for FavpicViewSplit.xaml
    /// </summary>
    public partial class FavpicViewSplit : UserControl,IContent
    {
        private pixiv_API.pixivAPI pixivAPI;
        private pixivIllust[] illust;
        public pixivAuthor author;
        public FavpicViewSplit()
        {
            InitializeComponent();
            page = 1;

            view.nextPageButton.Click += NextPageButton_Click;
            view.lastPageButton.Click += LastPageButton_Click;
            ((GridView)((ListView)view.piclistViewLeft.FindName("picListView")).View).Columns.RemoveAt(4);
            ((GridView)((ListView)view.piclistViewLeft.FindName("picListView")).View).Columns.RemoveAt(3);
        }
        //per page:20(have to check it on phone)

        private async void LastPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (page == 1) return;
            processing = true;
            cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/-sign-ban.png")));
            view.IsEnabled = false;
            progressbar.Visibility = Visibility.Visible;
            ((ListView)view.piclistViewLeft.FindName("picListView")).ItemsSource = null;

            CancelTokenSource = new CancellationTokenSource();
            try
            {
                await listview_load(page - 1, 20);
            }
            catch
            {
                except_return("User cancelled actions.");
                view.IsEnabled = true;
                view.nextPageButton.IsEnabled = false;
                view.lastPageButton.IsEnabled = false;

                cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
                processing = false;
                return;
            }
            page--;
            view.pageLabel.Content = page.ToString() + "/" + total.ToString();

            ((ListView)view.piclistViewLeft.FindName("picListView")).ItemsSource = illust;
            except_return("Done");
            cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
            processing = false;
        }

        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            processing = true;
            cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/-sign-ban.png")));
            view.IsEnabled = false;
            progressbar.Visibility = Visibility.Visible;
            ((ListView)view.piclistViewLeft.FindName("picListView")).ItemsSource = null;

            CancelTokenSource = new CancellationTokenSource();
            try
            {
                await listview_load(page + 1, 20);
            }
            catch
            {
                except_return("User cancelled actions.");
                view.IsEnabled = true;
                view.nextPageButton.IsEnabled = false;
                view.lastPageButton.IsEnabled = false;

                cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
                processing = false;
                return;
            }
            page++;
            view.pageLabel.Content = page.ToString() + "/" + total.ToString();
            ((ListView)view.piclistViewLeft.FindName("picListView")).ItemsSource = illust;
            except_return("Done");
            cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
            processing = false;
        }
        private async void refresh()
        {
            if (pixivAPI == null || author == null)
            {
                if (((MainWindow)App.Current.MainWindow).pixivAPI == null)
                {
                    view.IsEnabled = false;
                    return;
                }
                if (((MainWindow)App.Current.MainWindow).pixivAuthor == null)
                {
                    view.IsEnabled = false;
                    return;
                }
                pixivAPI = ((MainWindow)App.Current.MainWindow).pixivAPI;
                author = ((MainWindow)App.Current.MainWindow).pixivAuthor;
            }
            processing = true;
            cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/-sign-ban.png")));
            view.IsEnabled = false;
            progressbar.Visibility = Visibility.Visible;
            ((ListView)view.piclistViewLeft.FindName("picListView")).ItemsSource = null;

            CancelTokenSource = new CancellationTokenSource();
            try
            {
                await listview_load(page, 20);
            }
            catch
            {
                except_return("User cancelled actions.");
                view.IsEnabled = true;
                view.nextPageButton.IsEnabled = false;
                view.lastPageButton.IsEnabled = false;

                cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
                processing = false;
                return;
            }

            view.pageLabel.Content = page.ToString() + "/" + total.ToString();
            ((ListView)view.piclistViewLeft.FindName("picListView")).ItemsSource = illust;
            except_return("Done");
            cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
            processing = false;
        }

        private int page;
        private int total;
        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {

        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {

        }
        private CancellationTokenSource CancelTokenSource;
        private bool processing;
        private async Task listview_load(int page, int per_page)
        {
            CancelTokenSource = new CancellationTokenSource();
            JObject json = null;
            try
            {
                json = await pixivAPI.user_favourite_worksAsync(author.authorID, page, per_page, CancelTokenSource);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            if (json == null)
            {
                except_return("Done");
                Debug.WriteLine(2);
                return;
            }

            //set next and last button
            if (json["pagination"]["next"].Type == JTokenType.Null) view.nextPageButton.IsEnabled = false;
            else view.nextPageButton.IsEnabled = true;
            if (json["pagination"]["previous"].Type == JTokenType.Null) view.lastPageButton.IsEnabled = false;
            else view.lastPageButton.IsEnabled = true;

            total = (int)json["pagination"]["pages"];


            List<pixivIllust> illustbeforeList = new List<pixivIllust>();
            foreach (JObject response in json.Value<JArray>("response"))
            {
                JObject work = response.Value<JObject>("work");

                if (work["id"].Type == JTokenType.Null) continue;
                if (!showR18)
                {
                    if (!work["age_limit"].ToString().Equals("all-age")) continue;
                }
                pixivIllust illust_before = new pixivIllust();

                illust_before.illustID = (string)work["id"];
                illust_before.titleName = (string)work["title"];
                illust_before.authorID = (string)work["user"]["id"];
                illust_before.authorName = (string)work["user"]["name"];

                illust_before.ageLimit = false;
                if (!work["age_limit"].ToString().Equals("all-age")) illust_before.ageLimit = true;


                illust_before.MediumURL = new List<string>();
                illust_before.MediumURL.Add((string)work["image_urls"]["px_480mw"]);
                illust_before.isSetComplete = true;



                illustbeforeList.Add(illust_before);

            }
            illust = illustbeforeList.ToArray();
        }
        

        public async void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            setSettings();
            if (pixivAPI == null)
            {
                if (((MainWindow)App.Current.MainWindow).pixivAPI == null)
                {
                    view.IsEnabled = false;
                    return;
                }
                pixivAPI = ((MainWindow)App.Current.MainWindow).pixivAPI;
            }
            author = ((MainWindow)App.Current.MainWindow).pixivAuthor;
            if (author == null)
            {
                view.IsEnabled = false;
                return;
            }
            if (CancelTokenSource != null)
            {
                CancelTokenSource.Cancel();
                CancelTokenSource.Dispose();
                processing = false;
            }
            processing = true;
            view.IsEnabled = false;
            cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/-sign-ban.png")));
            progressbar.Visibility = Visibility.Visible;
            ((ListView)view.piclistViewLeft.FindName("picListView")).ItemsSource = null;

            CancelTokenSource = new CancellationTokenSource();
            try
            {
                await listview_load(1, 20);
            }
            catch
            {
                except_return("User cancelled actions.");
                view.IsEnabled = true;
                view.nextPageButton.IsEnabled = false;
                view.lastPageButton.IsEnabled = false;

                cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
                processing = false;
                return;
            }
            view.pageLabel.Content = page.ToString() + "/" + total.ToString();

            ((ListView)view.piclistViewLeft.FindName("picListView")).ItemsSource = illust;
            except_return("Done");
            cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
            processing = false;
        }
        private void except_return(string except)
        {
            if (except != null) statusLabel.Dispatcher.Invoke(() =>
            {
                statusLabel.Visibility = Visibility.Visible;
                statusLabel.Content = "Status: " + except;
            });
            progressbar.Dispatcher.Invoke(() =>
            {
                progressbar.Visibility = Visibility.Hidden;
            });
            view.Dispatcher.Invoke(() =>
            {
                view.IsEnabled = true;
            });
            Thread label_hide = new Thread(() =>
            {
                Thread.Sleep(5000);
                try
                {
                    statusLabel.Dispatcher.Invoke(() =>
                    {
                        statusLabel.Visibility = Visibility.Hidden;
                    });
                }
                catch
                {

                }
            });
            label_hide.Start();
        }
        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {

        }
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (processing)
            {
                CancelTokenSource.Cancel();
                CancelTokenSource.Dispose();
                processing = false;
            }
            else
            {
                refresh();
            }
        }
        private ConfigSettings configsetting;
        private bool showR18;
        private void setSettings()
        {
            configsetting = ((MainWindow)App.Current.MainWindow).configsettings;
            if (configsetting == null)
            {
                configsetting = new ConfigSettings();
                ((MainWindow)App.Current.MainWindow).configsettings = configsetting;
            }
            showR18 = configsetting.showR18;
        }

    }
}
