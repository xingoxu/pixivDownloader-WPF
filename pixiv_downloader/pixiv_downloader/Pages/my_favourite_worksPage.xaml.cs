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

namespace pixiv_downloader.Pages
{
    /// <summary>
    /// Interaction logic for my_favourite_worksPage.xaml
    /// </summary>
    public partial class my_favourite_worksPage : UserControl,IContent
    {

        private pixiv_API.pixivAPI pixivAPI;
        private pixivIllust[] illust = new pixivIllust[0];
        public my_favourite_worksPage()
        {
            InitializeComponent();
            first_init = true;
            view.lastPageButton.Visibility = Visibility.Hidden;
            view.nextPageButton.Content = "加载更多";
            view.nextPageButton.Width = 85;
            view.pageLabel.Visibility = Visibility.Hidden;
            view.nextPageButton.Click += NextPageButton_Click;
            ((GridView)((ListView)view.piclistViewLeft.FindName("picListView")).View).Columns.RemoveAt(4);
            ((GridView)((ListView)view.piclistViewLeft.FindName("picListView")).View).Columns.RemoveAt(3);
        }
        private async void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            processing = true;
            cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/-sign-ban.png")));
            view.IsEnabled = false;
            progressbar.Visibility = Visibility.Visible;
            //((ListView)view.piclistViewLeft.FindName("picListView")).ItemsSource = null;

            CancelTokenSource = new CancellationTokenSource();
            try
            {
                await listview_load();
            }
            catch
            {
                except_return("User cancelled actions.");
                view.IsEnabled = true;
                view.nextPageButton.IsEnabled = false;

                cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
                processing = false;
                return;
            }

            ((ListView)view.piclistViewLeft.FindName("picListView")).ItemsSource = illust;
            except_return("Done");
            cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
            processing = false;
        }
        private async void refresh()
        {
            if (pixivAPI == null)
            {
                if (((MainWindow)App.Current.MainWindow).pixivAPI == null)
                {
                    view.IsEnabled = false;
                    return;
                }
                pixivAPI = ((MainWindow)App.Current.MainWindow).pixivAPI;
            }
            processing = true;
            this.next_url = null;
            cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/-sign-ban.png")));
            view.IsEnabled = false;
            progressbar.Visibility = Visibility.Visible;
            ((ListView)view.piclistViewLeft.FindName("picListView")).ItemsSource = null;
            illust = new pixivIllust[0];

            CancelTokenSource = new CancellationTokenSource();
            try
            {
                await listview_load();
            }
            catch
            {
                except_return("User cancelled actions.");
                view.IsEnabled = true;
                view.nextPageButton.IsEnabled = false;

                cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
                processing = false;
                return;
            }


            ((ListView)view.piclistViewLeft.FindName("picListView")).ItemsSource = illust;
            except_return("Done");
            cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
            processing = false;
        }
        private bool first_init;

        private string next_url;

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {

        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {

        }
        private CancellationTokenSource CancelTokenSource;
        private bool processing;
        private async Task listview_load()
        {
            CancelTokenSource = new CancellationTokenSource();
            JObject json = null;
            try
            {
                json = await pixivAPI.my_favourite_worksAsync(this.next_url, (bool)isPublicCheckBox.IsChecked, CancelTokenSource);
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
            view.nextPageButton.IsEnabled = (json["next_url"].Type != JTokenType.Null);
            this.next_url = json["next_url"].Type == JTokenType.Null ? null : (string)json["next_url"];

            List<pixivIllust> illustbeforeList;
            illustbeforeList = new List<pixivIllust>();

            foreach (JObject work in json.Value<JArray>("illusts"))
            {

                if (work["id"].Type == JTokenType.Null) continue;
                pixivIllust illust_before = new pixivIllust();
                illust_before.illustID = (string)work["id"];
                illust_before.titleName = (string)work["title"];
                illust_before.authorID = (string)work["user"]["id"];
                illust_before.authorName = (string)work["user"]["name"];

                illust_before.ageLimit = false;
                //if (!work["age_limit"].ToString().Equals("all-age")) illust_before.ageLimit = true;

                illust_before.MediumURL = new List<string>();
                illust_before.MediumURL.Add((string)work["image_urls"]["medium"]);
                illust_before.isSetComplete = true;


                illustbeforeList.Add(illust_before);

            }
            Debug.WriteLine(illustbeforeList);
            illust = illust.Concat(illustbeforeList).ToArray();
        }
        public async void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            setSettings();
            if (((MainWindow)App.Current.MainWindow).pixivAPI == null)
            {
                view.IsEnabled = false;
                return;
            }
            pixivAPI = ((MainWindow)App.Current.MainWindow).pixivAPI;

            if (!first_init) return;
            first_init = false;
            processing = true;
            view.IsEnabled = false;
            cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/-sign-ban.png")));
            progressbar.Visibility = Visibility.Visible;
            ((ListView)view.piclistViewLeft.FindName("picListView")).ItemsSource = null;
            illust = new pixivIllust[0];

            CancelTokenSource = new CancellationTokenSource();
            try
            {
                await listview_load();
            }
            catch (Exception ex)
            {
                except_return("User cancelled actions.");
                view.IsEnabled = true;
                view.nextPageButton.IsEnabled = false;

                cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
                processing = false;
                Debug.WriteLine(ex);
                return;
            }


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

        private void isPublicCheckBox_Click(object sender, RoutedEventArgs e)
        {
            refresh();
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
