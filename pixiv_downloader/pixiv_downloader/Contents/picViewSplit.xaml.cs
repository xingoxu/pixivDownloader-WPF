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

namespace pixiv_downloader.Contents
{
    /// <summary>
    /// Interaction logic for picViewSplit.xaml
    /// </summary>
    public partial class picViewSplit : UserControl
    {
        public Image image;
        public ListView piclistview;
        public Label titleLabel;
        public Label descriptionLabel;
        public ModernButton follow_button;
        public Label illustType;
        public Label createdDate;
        public Button favouriteButton;
        public Button downloadButton;
        public Button intoDownloadSelect;
        public Button lastPageButton;
        public Button nextPageButton;
        public Label pageLabel;
        public Button stopRefreshButton;
        public ProgressBar progressbar;
        public ModernButton showauthorDialogButton;
        public ModernButton ugoirainfo;
        private pixiv_API.pixivAPI pixivAPI;
        
        public picViewSplit()
        {
            InitializeComponent();
            canceltoken = new CancellationTokenSource();
            piclistview = (ListView)piclistViewLeft.picListView;

            piclistview.SelectionChanged += lv_SelectionChanged;

            titleLabel = (Label)picAndButtonViewRight.titleLabel;
            descriptionLabel = (Label)picAndButtonViewRight.descriptionLabel;

            follow_button = (ModernButton)picAndButtonViewRight.follow_button;
            follow_button.Click += Follow_button_Click;

            illustType = (Label)picAndButtonViewRight.illustType;
            createdDate = (Label)picAndButtonViewRight.createdDate;
            image = (Image)picAndButtonViewRight.image;

            favouriteButton = (Button)picAndButtonViewRight.favouriteButton;
            favouriteButton.Click += FavouriteButton_Click;

            downloadButton = (Button)picAndButtonViewRight.downloadButton;
            downloadButton.Click += DownloadButton_Click;

            intoDownloadSelect = (Button)picAndButtonViewRight.intoDownloadSelect;
            intoDownloadSelect.Click += IntoDownloadSelect_Click;

            lastPageButton = (Button)piclistViewLeft.lastPageButton;
            nextPageButton = (Button)piclistViewLeft.nextPageButton;
            pageLabel = (Label)piclistViewLeft.pageLabel;

            stopRefreshButton = (Button)picAndButtonViewRight.stopRefreshButton;
            stopRefreshButton.Click += StopRefreshButton_Click;

            progressbar = (ProgressBar)picAndButtonViewRight.progressbar;

            showauthorDialogButton = picAndButtonViewRight.showauthorDialogButton;
            showauthorDialogButton.Click += ShowauthorDialogButton_Click;

            ugoirainfo = picAndButtonViewRight.ugoirainfo;

        }

        private void IntoDownloadSelect_Click(object sender, RoutedEventArgs e)
        {
            multiDownloadPageForMangaDialog mangadownloaddlg = new multiDownloadPageForMangaDialog();
            mangadownloaddlg.MaxWidth = int.MaxValue;
            mangadownloaddlg.MaxHeight = int.MaxValue;
            mangadownloaddlg.MinWidth = 700;
            mangadownloaddlg.MinHeight = 300;
            mangadownloaddlg.Height = 300;
            mangadownloaddlg.init(illust_selected);
            mangadownloaddlg.ShowDialog();
        }

        private void ShowauthorDialogButton_Click(object sender, RoutedEventArgs e)
        {

            authorDialog dlg = new authorDialog();
            dlg.MaxWidth = int.MaxValue;
            dlg.MaxHeight = int.MaxValue;
            dlg.MinWidth = 800;
            dlg.MinHeight = 600;
            dlg.Title = illust_selected.authorName+" 的页面";
            dlg.authorDetail.authorIDLabel.Content = illust_selected.authorID;
            dlg.authorDetail.authorNameLabel.Content = illust_selected.authorName;
            ((MainWindow)App.Current.MainWindow).pixivAuthor = new pixivAuthor() { authorID = illust_selected.authorID, authorName = illust_selected.authorName, profileImageURL = illust_selected.authorIconURL };
            Debug.WriteLine(illust_selected.authorIconURL);
            Task.Run(() =>
            {
                initAuthorImageForAuthorDialog(illust_selected.authorIconURL, dlg);
            });
            dlg.ShowDialog();
        }
        private void initAuthorImageForAuthorDialog(string authorimgURL,authorDialog dlg)
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
            try {
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


        private void FavouriteButton_Click(object sender, RoutedEventArgs e)
        {
            //show publicOrPrivateDialog
            bool IsPublic = false;
            if (piclistview.SelectedItems.Count > 1 || (piclistview.SelectedItems.Count == 1 && illust_selected.favouriteID.Equals("0")))
            {
                var publicOrPrivateDialog = new ModernDialog
                {
                    Title = "隐私",
                    Content = "选择可见度："
                };
                publicOrPrivateDialog.OkButton.Content = "公开";
                publicOrPrivateDialog.NoButton.Content = "私人";
                publicOrPrivateDialog.Buttons = new Button[] { publicOrPrivateDialog.OkButton, publicOrPrivateDialog.NoButton, publicOrPrivateDialog.CancelButton };
                publicOrPrivateDialog.ShowDialog();

                var messageBoxResult = publicOrPrivateDialog.MessageBoxResult;
                if (messageBoxResult == MessageBoxResult.Cancel)
                {
                    return;
                }
                if (messageBoxResult == MessageBoxResult.OK) IsPublic = true;
            }
            //start process
            waitingForProcessing waitdialog = new waitingForProcessing();
            CancellationTokenSource cts = new CancellationTokenSource();
            if (piclistview.SelectedItems.Count == 1)
            {
                waitdialog.progressbar.Visibility = Visibility.Hidden;
                Task.Run(() =>
                {
                    bool result = false;
                    try
                    {
                        if (illust_selected.favouriteID.Equals("0"))
                            result = pixivAPI.my_favourite_work_addAsync(illust_selected.illustID, IsPublic, cts).Result;
                        else result = pixivAPI.my_favourite_works_deleteAsync(illust_selected.favouriteID, cts).Result;
                    }
                    catch
                    {//task cancelled
                        favouriteButton.Dispatcher.Invoke(() =>
                        {
                            if (illust_selected.favouriteID.Equals("0")) favouriteButton.Content = "收藏";
                            else favouriteButton.Content = "取消收藏";
                        });
                        return;
                    }
                    finally
                    {
                        waitdialog.Dispatcher.Invoke(() =>
                        {
                            try { waitdialog.Close(); } catch { }
                        });
                    }
                    if (!result)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            ModernDialog.ShowMessage("已经收藏？获取数据失败以外的未知问题，请尝试刷新数据？", "收藏失败", MessageBoxButton.OK);
                        });
                        favouriteButton.Dispatcher.Invoke(() =>
                        {
                            if (illust_selected.favouriteID.Equals("0")) favouriteButton.Content = "收藏";
                            else favouriteButton.Content = "取消收藏";
                        });
                        return;
                    }
                });
                waitdialog.ShowDialog();
                if (waitdialog.MessageBoxResult == MessageBoxResult.Cancel)
                {
                    cts.Cancel();
                    if (illust_selected.favouriteID.Equals("0")) favouriteButton.Content = "收藏";
                    else favouriteButton.Content = "取消收藏";
                    return;
                }
                favouriteButton.IsEnabled = false;

                if (processing)
                {
                    StopRefreshButton_Click(null, null);
                    StopRefreshButton_Click(null, null);
                }
                else StopRefreshButton_Click(null, null);

                return;
            }
            else if (piclistview.SelectedItems.Count > 1)
            {
                int total = piclistview.SelectedItems.Count;
                waitdialog.progressbar.Maximum = total;
                waitdialog.progressbar.Value = 0;
                Task.Run(() =>
                {
                    int i = 1; bool flag = false; int failed_count = 0;
                    foreach (pixivIllust illust in illusts_selected)
                    {
                        waitdialog.textBlock.Dispatcher.Invoke(() =>
                        {
                            waitdialog.textBlock.Text = "我们需要一点时间来处理刚才的操作...第" + i.ToString() + "个，共" + total.ToString() + "个";
                        });

                        bool result = false;
                        try
                        {
                            result = pixivAPI.my_favourite_work_addAsync(illust.illustID, IsPublic, cts).Result;
                        }
                        catch
                        {//task cancelled or network problem
                            failed_count++;
                            flag = true;
                            continue;
                        }
                        finally
                        {
                            waitdialog.progressbar.Dispatcher.Invoke(() =>
                            {
                                waitdialog.progressbar.Value = i;
                            });
                            i++;
                        }
                        if (!result)
                        {//result false
                            failed_count++;
                            flag = true;
                        }
                    }
                    Debug.WriteLine("circle complete");
                    waitdialog.Dispatcher.Invoke(() =>
                    {
                        try { waitdialog.Close(); } catch { }
                    });
                    if (flag)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            ModernDialog.ShowMessage(failed_count.ToString() + "个任务收藏失败，需要重新收藏", "部分任务没有成功", MessageBoxButton.OK);
                        });
                    }
                });
                waitdialog.ShowDialog();
                if (waitdialog.MessageBoxResult != MessageBoxResult.Cancel) return;
                Debug.WriteLine("has cancelled");
                cts.Cancel();

            }
        }

        private void Follow_button_Click(object sender, RoutedEventArgs e)
        {
            Geometry loading_icon = Geometry.Parse("M5,10A2,2 0 0,0 3,12C3,13.11 3.9,14 5,14C6.11,14 7,13.11 7,12A2,2 0 0,0 5,10M5,16A4,4 0 0,1 1,12A4,4 0 0,1 5,8A4,4 0 0,1 9,12A4,4 0 0,1 5,16M10.5,11H14V8L18,12L14,16V13H10.5V11M5,6C4.55,6 4.11,6.05 3.69,6.14C5.63,3.05 9.08,1 13,1C19.08,1 24,5.92 24,12C24,18.08 19.08,23 13,23C9.08,23 5.63,20.95 3.69,17.86C4.11,17.95 4.55,18 5,18C5.8,18 6.56,17.84 7.25,17.56C8.71,19.07 10.74,20 13,20A8,8 0 0,0 21,12A8,8 0 0,0 13,4C10.74,4 8.71,4.93 7.25,6.44C6.56,6.16 5.8,6 5,6Z");
            Geometry follow_icon = Geometry.Parse("M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M13,7H11V11H7V13H11V17H13V13H17V11H13V7Z");
            Geometry followed_icon = Geometry.Parse("M20,12A8,8 0 0,1 12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4C12.76,4 13.5,4.11 14.2,4.31L15.77,2.74C14.61,2.26 13.34,2 12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12M7.91,10.08L6.5,11.5L11,16L21,6L19.59,4.58L11,13.17L7.91,10.08Z");


            follow_button.IsEnabled = false;
            follow_button.IconData = loading_icon;

            bool IsPublic = false;
            if (piclistview.SelectedItems.Count > 1 || (piclistview.SelectedItems.Count == 1 && !illust_selected.authorIsFollowing))
            {
                var publicOrPrivateDialog = new ModernDialog
                {
                    Title = "隐私",
                    Content = "选择可见度："
                };
                publicOrPrivateDialog.OkButton.Content = "公开";
                publicOrPrivateDialog.NoButton.Content = "私人";
                publicOrPrivateDialog.Buttons = new Button[] { publicOrPrivateDialog.OkButton, publicOrPrivateDialog.NoButton, publicOrPrivateDialog.CancelButton };
                publicOrPrivateDialog.ShowDialog();

                var messageBoxResult = publicOrPrivateDialog.MessageBoxResult;
                if (messageBoxResult == MessageBoxResult.Cancel)
                {
                    follow_button.IsEnabled = true;
                    if (!illust_selected.authorIsFollowing)
                    {
                        follow_button.IconData = follow_icon;
                        follow_button.Content = "关注";
                    }
                    else { follow_button.IconData = followed_icon; follow_button.Content = "取消关注"; }
                    return;
                }

                if (messageBoxResult == MessageBoxResult.OK) IsPublic = true;
            }
            waitingForProcessing waitdialog = new waitingForProcessing();
            waitdialog.progressbar.Visibility = Visibility.Hidden;

            CancellationTokenSource cts = new CancellationTokenSource();
            Task.Run(() =>
            {
                bool result = false;
                try
                {
                    if (illust_selected.authorIsFollowing) result = pixivAPI.my_favourite_users_unfollowAsync(illust_selected.authorID, cts).Result;
                    else result = pixivAPI.my_favourtie_user_followAsync(illust_selected.authorID, IsPublic, cts).Result;
                }
                catch
                {//task cancelled or network error
                    follow_button.Dispatcher.Invoke(() =>
                    {
                        follow_button.IsEnabled = true;
                        if (!illust_selected.authorIsFollowing)
                        {
                            follow_button.IconData = follow_icon;
                            follow_button.Content = "关注";
                        }
                        else {
                            follow_button.IconData = followed_icon;
                            follow_button.Content = "取消关注";
                        }
                    });
                    return;
                }
                finally
                {
                    waitdialog.Dispatcher.Invoke(() =>
                    {
                        try { waitdialog.Close(); } catch { }
                    });
                }

                if (!result)
                {
                    follow_button.Dispatcher.Invoke(() =>
                    {
                        follow_button.IsEnabled = true;
                    });
                    if (!illust_selected.authorIsFollowing)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            ModernDialog.ShowMessage("已经关注？获取数据失败以外的未知问题，请尝试刷新数据？", "关注失败", MessageBoxButton.OK);
                        });
                        follow_button.Dispatcher.Invoke(() =>
                        {
                            follow_button.IconData = follow_icon;
                            follow_button.Content = "关注";
                        });
                    }
                    else
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            ModernDialog.ShowMessage("没有关注？获取数据失败以外的未知问题，请尝试刷新数据？", "取消关注失败", MessageBoxButton.OK);
                        });
                        follow_button.Dispatcher.Invoke(() =>
                        {
                            follow_button.IconData = followed_icon;
                            follow_button.Content = "取消关注";
                        });
                    }
                }
            });
            waitdialog.ShowDialog();
            if (waitdialog.MessageBoxResult == MessageBoxResult.Cancel)
            {
                cts.Cancel();
                follow_button.IsEnabled = true;
                if (!illust_selected.authorIsFollowing) { follow_button.IconData = follow_icon; follow_button.Content = "关注"; }
                else { follow_button.IconData = followed_icon; follow_button.Content = "取消关注"; }
                return;
            }

            follow_button.IsEnabled = true;
            if (illust_selected.authorIsFollowing) { follow_button.IconData = follow_icon; illust_selected.authorIsFollowing = false; follow_button.Content = "关注"; }
            else { follow_button.IconData = followed_icon; illust_selected.authorIsFollowing = true; follow_button.Content = "取消关注"; }
            illust_selected.authorIsFollowing = true;
        }

        private ObservableCollection<illustTask> downloadTasks;
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigSettings setting = ((MainWindow)App.Current.MainWindow).configsettings;
            downloadTasks = ((MainWindow)App.Current.MainWindow).downloadTasks;
            chooseRoute chooseRoutedlg = new chooseRoute();
            chooseRoutedlg.FolderTextBox.Text = setting.workPath;
            chooseRoutedlg.ShowDialog();
            if (chooseRoutedlg.MessageBoxResult == MessageBoxResult.Cancel) return;
            string workPath = null;
            if ((bool)chooseRoutedlg.routeByselfCheckBox.IsChecked)//I found this name problem but I don't want to change it...
            {
                workPath = chooseRoutedlg.FolderTextBox.Text;
            }
            else
            {
                workPath = setting.workPath;
            }
            bool showchooseRoutedlg = (bool)chooseRoutedlg.remind.IsChecked;
            //next update TODO
            if (piclistview.SelectedItems.Count == 1)
            {
                illustTask illusttask = new illustTask(illust_selected, pixivAPI, workPath);
                downloadTasks.Add(illusttask);
            }
            else if (piclistview.SelectedItems.Count > 1)
            {
                waitingForProcessing waitDialog = new waitingForProcessing();
                int total = piclistview.SelectedItems.Count;
                waitDialog.progressbar.Maximum = total;
                waitDialog.progressbar.Value = 0;
                CancellationTokenSource cts = new CancellationTokenSource();
                Task.Run(() =>
                  {
                      int i = 1; bool flag = false; int failed_count = 0;
                      foreach (pixivIllust illust_before in illusts_selected)
                      {
                          waitDialog.textBlock.Dispatcher.Invoke(() =>
                          {
                              waitDialog.textBlock.Text = "我们需要一点时间来处理选中的下载信息...第" + i.ToString() + "个，共" + total.ToString() + "个";
                          });

                          var task_illust = pixivAPI.illust_workAsync(illust_before.illustID, cts);
                          JObject returns = null;
                          try
                          {
                              returns = task_illust.Result;
                          }
                          catch
                          {//task cancelled or network problem
                              failed_count++;
                              flag = true;
                              continue;
                          }
                          finally
                          {
                              waitDialog.progressbar.Dispatcher.Invoke(() =>
                              {
                                  waitDialog.progressbar.Value = i;
                              });
                              i++;
                          }
                          var illust = fromJsonSetIllust_detail(returns);
                          if (illust == null)
                          {//if return null 
                              failed_count++;
                              flag = true;
                              continue;
                          }
                          illustTask illusttask = new illustTask(illust, pixivAPI,workPath);
                          downloadTasks.Add(illusttask);

                      }
                      Debug.WriteLine("circle complete");
                      waitDialog.Dispatcher.Invoke(() =>
                      {
                          try { waitDialog.Close(); } catch { }
                      });
                      if (flag)
                      {
                          App.Current.Dispatcher.Invoke(() =>
                          {
                              ModernDialog.ShowMessage(failed_count.ToString() + "个任务没有解析到信息，需要重新添加下载", "部分任务可能没有添加", MessageBoxButton.OK);
                          });
                      }
                  });
                waitDialog.ShowDialog();
                if (waitDialog.MessageBoxResult != MessageBoxResult.Cancel) return;
                Debug.WriteLine("has cancelled");
                cts.Cancel();
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
                illust_selected = null;
                lv_SelectionChanged(null, null);
            }
        }

        private CancellationTokenSource canceltoken;
        private pixivIllust illust_selected;
        private pixivIllust[] illusts_selected;
        private async void lv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Geometry loading_icon = Geometry.Parse("M5,10A2,2 0 0,0 3,12C3,13.11 3.9,14 5,14C6.11,14 7,13.11 7,12A2,2 0 0,0 5,10M5,16A4,4 0 0,1 1,12A4,4 0 0,1 5,8A4,4 0 0,1 9,12A4,4 0 0,1 5,16M10.5,11H14V8L18,12L14,16V13H10.5V11M5,6C4.55,6 4.11,6.05 3.69,6.14C5.63,3.05 9.08,1 13,1C19.08,1 24,5.92 24,12C24,18.08 19.08,23 13,23C9.08,23 5.63,20.95 3.69,17.86C4.11,17.95 4.55,18 5,18C5.8,18 6.56,17.84 7.25,17.56C8.71,19.07 10.74,20 13,20A8,8 0 0,0 21,12A8,8 0 0,0 13,4C10.74,4 8.71,4.93 7.25,6.44C6.56,6.16 5.8,6 5,6Z");
            Geometry follow_icon = Geometry.Parse("M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M13,7H11V11H7V13H11V17H13V13H17V11H13V7Z");
            Geometry followed_icon = Geometry.Parse("M20,12A8,8 0 0,1 12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4C12.76,4 13.5,4.11 14.2,4.31L15.77,2.74C14.61,2.26 13.34,2 12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12M7.91,10.08L6.5,11.5L11,16L21,6L19.59,4.58L11,13.17L7.91,10.08Z");

            processing = true;
            StopRefreshButton_Click(null, null);
            processing = true;
            canceltoken = new CancellationTokenSource();
            if (piclistview.SelectedItem == null)
            {//TODO:if select null then disable button/thread stop
                progressbar.Visibility = Visibility.Hidden;
                stopRefreshButton.Visibility = Visibility.Hidden;
                image.Source = null;
                illust_selected = null;
                titleLabel.Content = "作品标题";
                descriptionLabel.Content = "作者";
                showauthorDialogButton.IsEnabled = false;
                illustType.Content = "尚未选择项";
                createdDate.Content = "";
                follow_button.IsEnabled = false;
                favouriteButton.IsEnabled = false;
                downloadButton.IsEnabled = false;
                intoDownloadSelect.IsEnabled = false;
                Debug.WriteLine(2);
                processing = false;
                return;
            }
            pixivIllust illustSelected = null;
            try
            {
                illustSelected = (pixivIllust)piclistview.SelectedItem;
            }
            catch
            {
                Debug.WriteLine(3);
                processing = false;
                return;
            }
            //do a nice optimize to check if the first select item haven't changed
            if (piclistview.SelectedItems.Count > 1)
            {
                illustType.Content = "共选择" + piclistview.SelectedItems.Count + "项";
                createdDate.Content = "";
                favouriteButton.Content = "全部收藏";
                downloadButton.Content = "全部下载";
                follow_button.IsEnabled = false;
                showauthorDialogButton.IsEnabled = false;
                illusts_selected = piclistview.SelectedItems.Cast<pixivIllust>().ToArray();
            }
            else
            {
                favouriteButton.Content = "收藏";
                downloadButton.Content = "下载";
            }

            if (illust_selected != null)
                if (illust_selected.illustID.Equals(illustSelected.illustID))
                {
                    processing = false; return;
                }

            //start to init
            progressbar.Visibility = Visibility.Visible;
            stopRefreshButton.Visibility = Visibility.Visible;
            ugoirainfo.Visibility = Visibility.Hidden;
            stopRefreshButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/-sign-ban.png")));

            image.Source = null;

            pixivAPI = ((MainWindow)App.Current.MainWindow).pixivAPI;
            var task = pixivAPI.illust_workAsync(illustSelected.illustID, canceltoken);

            follow_button.IsEnabled = false;
            showauthorDialogButton.IsEnabled = false;
            favouriteButton.IsEnabled = false;
            downloadButton.IsEnabled = false;
            intoDownloadSelect.IsEnabled = false;

            titleLabel.Content = "正在加载...";
            descriptionLabel.Content = "正在加载...";
            illustType.Content = "正在加载...";
            createdDate.Content = "正在加载...";
            follow_button.IconData = loading_icon;
            follow_button.Content = "正在加载...";


            JObject returns = null;
            try
            {
                returns = await task;//run first item's detail
            }
            catch
            {//Task Cancelled
                illust_selected = null;
                progressbar.Visibility = Visibility.Hidden;
                titleLabel.Content = "加载取消或超时";
                descriptionLabel.Content = "请尝试重试";
                illustType.Content = "";
                createdDate.Content = "";
                follow_button.IsEnabled = false;
                showauthorDialogButton.IsEnabled = false;
                follow_button.IconData = null;
                favouriteButton.IsEnabled = false;
                downloadButton.IsEnabled = false;
                intoDownloadSelect.IsEnabled = false;
                stopRefreshButton.Visibility = Visibility.Visible;
                stopRefreshButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
                Debug.WriteLine("Task Cancelled");
                Debug.WriteLine(3);
                processing = false;
                return;
            }

            pixivIllust illust = fromJsonSetIllust_detail(returns);
            if (illust == null)
            {
                illust_selected = null;
                progressbar.Visibility = Visibility.Hidden;
                stopRefreshButton.Visibility = Visibility.Visible;
                stopRefreshButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
                titleLabel.Content = "加载失败";
                descriptionLabel.Content = "请尝试重试";
                illustType.Content = "";
                createdDate.Content = "";
                follow_button.IsEnabled = false;
                showauthorDialogButton.IsEnabled = false;
                favouriteButton.IsEnabled = false;
                downloadButton.IsEnabled = false;
                intoDownloadSelect.IsEnabled = false;
                Debug.WriteLine("加载失败");
                processing = false;
                return;
            }
            var task_imagedownload = pixivAPI.DownloadFileAsync("temp", illust.MediumURL[0], null, canceltoken);//start download image 

            //wait detail has added complete
            illust_selected = illust;

            intoDownloadSelect.IsEnabled = false;
            titleLabel.Content = illust.titleName;
            descriptionLabel.Content = illust.authorName;

            createdDate.Content = illust.created_time;

            if (piclistview.SelectedItems.Count > 1)
            {
                follow_button.IsEnabled = false;
                showauthorDialogButton.IsEnabled = false;
            }
            else
            {
                illustType.Content = "插图";
                if (illust.Type == pixivIllust.illustType.manga)
                {
                    intoDownloadSelect.IsEnabled = true;
                    illustType.Content = "漫画";
                }
                else if (illust.Type == pixivIllust.illustType.ugoira)
                {
                    illustType.Content = "动图";
                    ugoirainfo.Visibility = Visibility.Visible;
                }
                if (illust.favouriteID.Equals("0"))
                {

                    favouriteButton.Content = "收藏";
                }
                else
                {
                    favouriteButton.Content = "取消收藏";
                }
                if (illust.authorIsFollowing)
                {
                    follow_button.IconData = followed_icon;
                    follow_button.Content = "取消关注";
                }
                else {
                    follow_button.IconData = follow_icon;
                    follow_button.Content = "关注";
                }

            }
            favouriteButton.IsEnabled = true;
            downloadButton.IsEnabled = true;
            follow_button.IsEnabled = true;
            showauthorDialogButton.IsEnabled = true;

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
            stopRefreshButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/img/loading.gif")));
            processing = false;
        }

        private pixivIllust fromJsonSetIllust_detail(JObject json_illust)
        {
            if (json_illust == null) return null;
            pixivIllust illust_before = null;
            foreach (JObject response_illust in json_illust.Value<JArray>("response"))//though now it will be only one response
            {
                illust_before = new pixivIllust();
                illust_before.illustID = (string)response_illust["id"];
                illust_before.titleName = (string)response_illust["title"];
                illust_before.authorID = (string)response_illust["user"]["id"];
                illust_before.authorName = (string)response_illust["user"]["name"];
                illust_before.authorIconURL = (string)response_illust["user"]["profile_image_urls"]["px_50x50"];                
                illust_before.created_time = (string)response_illust["created_time"];

                illust_before.FavNum = (int)response_illust["stats"]["favorited_count"]["public"] + (int)response_illust["stats"]["favorited_count"]["private"];
                illust_before.favouriteID = (string)response_illust["favorite_id"];

                illust_before.ageLimit = false;
                if (!response_illust["age_limit"].ToString().Equals("all-age")) illust_before.ageLimit = true;

                illust_before.authorIsFollowing = false;
                if (response_illust["user"]["is_following"] != null)
                    if ((bool)response_illust["user"]["is_following"])
                        illust_before.authorIsFollowing = true;

                illust_before.OriginalURL = new List<string>();//start to get original and medium pic URL
                illust_before.MediumURL = new List<string>();
                if (!response_illust["metadata"].HasValues)//illust
                {
                    illust_before.Type = pixivIllust.illustType.illustration;
                    illust_before.MediumURL.Add((string)response_illust["image_urls"]["px_480mw"]);
                    illust_before.OriginalURL.Add(response_illust["image_urls"]["large"].ToString());
                }
                else //优先遍历metadata中的原图
                {
                    if (!(bool)response_illust["is_manga"])//ugoira
                    {
                        illust_before.Type = pixivIllust.illustType.ugoira;
                        illust_before.MediumURL.Add((string)response_illust["image_urls"]["px_480mw"]);
                        illust_before.OriginalURL.Add(response_illust["image_urls"]["large"].ToString());
                        illust_before.ugoiraZipURL = (response_illust["metadata"]["zip_urls"]["ugoira600x600"].ToString());
                    }
                    else// manga
                    {
                        illust_before.Type = pixivIllust.illustType.manga;
                        foreach (JObject image in response_illust["metadata"]["pages"].Value<JArray>())
                        {
                            illust_before.MediumURL.Add((string)image["image_urls"]["px_480mw"]);
                            illust_before.OriginalURL.Add(image["image_urls"]["large"].ToString());
                        }
                    }
                }
            }
            return illust_before;
        }

    }
}
