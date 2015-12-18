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

namespace pixiv_downloader
{
    /// <summary>
    /// Interaction logic for authorPage.xaml
    /// </summary>
    public partial class authorPage : UserControl, IContent
    {
        private pixiv_API.pixivAPI pixivAPI;
        private pixivAuthor[] authors;
        public authorPage()
        {
            InitializeComponent();
            page = 1;
            first_init = true;
            view.nextPageButton.Click += NextPageButton_Click;
            view.lastPageButton.Click += LastPageButton_Click;
        }
        //per page:20(have to check it on phone)

        private async void LastPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (page == 1) return;
            processing = true;
            cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/-sign-ban.png")));
            view.IsEnabled = false;
            progressbar.Visibility = Visibility.Visible;
            view.authorlistbox.ItemsSource = null;

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

            view.authorlistbox.ItemsSource = authors;
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
            view.authorlistbox.ItemsSource = null;

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
            view.authorlistbox.ItemsSource = authors;
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
            cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/-sign-ban.png")));
            view.IsEnabled = false;
            progressbar.Visibility = Visibility.Visible;
            view.authorlistbox.ItemsSource = null;

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
            view.authorlistbox.ItemsSource = authors;
            except_return("Done");
            cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
            processing = false;
        }
        private bool first_init;
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
                json = await pixivAPI.my_following_userAsync(page, per_page, (bool)isPublicCheckBox.IsChecked, CancelTokenSource);
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

            pixivAuthor[] illust_before;
            illust_before = new pixivAuthor[json.Value<JArray>("response").Count];

            int count = 0;
            foreach (JObject work in json.Value<JArray>("response"))
            {

                if (work["id"].Type == JTokenType.Null) continue;
                illust_before[count] = new pixivAuthor();
                illust_before[count].authorID = (string)work["id"];
                illust_before[count].authorName = (string)work["name"];
                illust_before[count].profileImageURL = (string)work["profile_image_urls"]["px_50x50"];

                count++;

            }
            authors = illust_before;
        }
        public async void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
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
            view.authorlistbox.ItemsSource = null;

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

            view.authorlistbox.ItemsSource = authors;
            except_return("Done");
            cancelButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
            processing = false;
        }
        private void except_return(string except)
        {

            progressbar.Dispatcher.Invoke(() =>
            {
                progressbar.Visibility = Visibility.Hidden;
            });
            view.Dispatcher.Invoke(() =>
            {
                view.IsEnabled = true;
            });

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
    }


}
