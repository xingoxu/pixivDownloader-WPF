using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using FirstFloor.ModernUI.Windows.Navigation;

namespace pixiv_downloader
{
    /// <summary>
    /// Interaction logic for downloadManage.xaml
    /// </summary>
    public partial class downloadManage : UserControl,IContent
    {
        public downloadManage()
        {
            InitializeComponent();
            tasklistview.ItemsSource = ((MainWindow)App.Current.MainWindow).downloadTasks;
        }
        AsyncObservableCollection<illustTask> downloadTasks;
        private void saveTask_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".xml";
            dlg.Filter = "未下载完成列表（特定格式xml） (.xml)|*.xml";
            bool dlgresult = (bool)dlg.ShowDialog();

            if (dlgresult != true) return;

            string filename = dlg.FileName;
            var AutoSavedTasks = new List<ExportTask>();
            var configsettings = new ConfigSettings();
            configsettings.AutoSavedTasks = AutoSavedTasks;
            foreach (illustTask task in downloadTasks)
            {
                if (task.State != illustTask.state.已完成)
                {
                    if (task.illust.Type == pixivIllust.illustType.manga)
                    {
                        if (task.mangaPages == null)
                        {
                            AutoSavedTasks.Add(new ExportTask() { illustID = task.illust.illustID, TaskName = task.showName, type = ExportTask.Tasktype.manga, workPath = task.workPath });
                        }
                        else {
                            List<int> array = new List<int>();
                            foreach (KeyValuePair<int, illustTask.state> key in task.mangaPages)
                            {
                                if (key.Value != illustTask.state.已完成)
                                    array.Add(key.Key);
                            }
                            AutoSavedTasks.Add(new ExportTask() { illustID = task.illust.illustID, TaskName = task.showName, mangaSelected = array.ToArray(), type = ExportTask.Tasktype.manga, workPath = task.workPath });
                        }
                    }
                    else if (task.illust.Type == pixivIllust.illustType.ugoira)
                    {
                        AutoSavedTasks.Add(new ExportTask() { illustID = task.illust.illustID, TaskName = task.showName, type = ExportTask.Tasktype.ugoira, workPath = task.workPath });
                    }
                    else
                        AutoSavedTasks.Add(new ExportTask() { illustID = task.illust.illustID, TaskName = task.showName, type = ExportTask.Tasktype.illust, workPath = task.workPath });
                }
            }

            XmlSerializer.SaveToXml(filename, configsettings, typeof(ConfigSettings), "ConfigSettings");
        }

        private void cancelTask_Click(object sender, RoutedEventArgs e)
        {
            if (tasklistview.SelectedItem == null) return;
            illustTask illusttask = null;
            try
            {
                illusttask = (illustTask)tasklistview.SelectedItem;
            }
            catch
            {
                ModernDialog.ShowMessage("发生意外错误，可能是本体发生错误，请联系作者", "意料之外的错误", MessageBoxButton.OK);
                return;
            }
            illusttask.taskCancelsource.Cancel();

        }

        private void startTask_Click(object sender, RoutedEventArgs e)
        {
            if (tasklistview.SelectedItem == null) return;
            illustTask illusttask = null;
            try
            {
                illusttask = (illustTask)tasklistview.SelectedItem;
            }
            catch
            {
                ModernDialog.ShowMessage("发生意外错误，可能是本体发生错误，请联系作者", "意料之外的错误", MessageBoxButton.OK);
                return;
            }
            var pixivAPI = ((MainWindow)App.Current.MainWindow).pixivAPI;
            illusttask.restartTask(pixivAPI);
        }

        private void newTask_Click(object sender, RoutedEventArgs e)
        {
            newDownloadTaskDialog dlg = new newDownloadTaskDialog();
            dlg.ShowDialog();
            if (dlg.MessageBoxResult != MessageBoxResult.OK) return;
            string illustID = dlg.textBox.Text;
            bool startDownload = (bool)dlg.checkbox.IsChecked;
            string path = dlg.FolderTextBox.Text;
            if (path.Equals("")) path = null;
            waitingForProcessing waitDialog = new waitingForProcessing();
            waitDialog.progressbar.Visibility = Visibility.Hidden;
            CancellationTokenSource cts = new CancellationTokenSource();
            var pixivAPI = ((MainWindow)App.Current.MainWindow).pixivAPI;
            Task.Run(() =>
            {
                bool flag = false;
                var task_illust = pixivAPI.illust_workAsync(illustID, cts);
                JObject returns = null;
                try
                {
                    returns = task_illust.Result;
                }
                catch
                {//task cancelled or network problem
                    flag = true;
                }
                var illust = fromJsonSetIllust_detail(returns);
                if (illust == null)
                {//if return null 

                    flag = true;

                }
                illustTask illusttask = null;
                if (startDownload)
                    illusttask = new illustTask(illust, pixivAPI, path);
                else
                    illusttask = new illustTask(illust, path);

                downloadTasks.Add(illusttask);


                Debug.WriteLine("circle complete");
                waitDialog.Dispatcher.Invoke(() =>
                {
                    try { waitDialog.Close(); } catch { }
                });
                if (flag)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ModernDialog.ShowMessage("任务没有完全解析到信息，可能需要重新添加下载", "任务可能没有添加", MessageBoxButton.OK);
                    });
                }
            });
            waitDialog.ShowDialog();
            if (waitDialog.MessageBoxResult != MessageBoxResult.Cancel) return;
            Debug.WriteLine("has cancelled");
            cts.Cancel();
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

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {

        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {

        }

        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            this.downloadTasks = ((MainWindow)App.Current.MainWindow).downloadTasks;
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {
        }

        private void tasklistview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(tasklistview.SelectedItem==null)
            {
                startTask.IsEnabled = false;
                cancelTask.IsEnabled = false;
                return;
            }
            illustTask illusttask = null;
            try
            {
                illusttask = (illustTask)tasklistview.SelectedItem;
            }
            catch
            {
                ModernDialog.ShowMessage("发生意外错误，可能是本体发生错误，请联系作者", "意料之外的错误", MessageBoxButton.OK);
                return;
            }
            if(illusttask.State!= illustTask.state.已完成)
            {
                cancelTask.IsEnabled = true;
                startTask.IsEnabled = true;

            }
            else
            {
                cancelTask.IsEnabled = true;
                startTask.IsEnabled = true;
            }

        }

        private void loadTask_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".xml";
            dlg.Filter = "未下载完成列表（特定格式xml） (.xml)|*.xml";
            bool dlgresult = (bool)dlg.ShowDialog();

            if (dlgresult != true) return;

            string filename = dlg.FileName;
            List<ExportTask> AutoSavedTasks;
            ConfigSettings setting;
            try {
                setting = (ConfigSettings)XmlSerializer.LoadFromXml(filename, typeof(ConfigSettings));
            }
            catch
            {
                ModernDialog.ShowMessage("打开文件发生错误，该文件可能不是有效的未完成列表。", "读取错误", MessageBoxButton.OK);
                return;
            }
            AutoSavedTasks = setting.AutoSavedTasks;
            var pixivAPI = ((MainWindow)App.Current.MainWindow).pixivAPI;
            var configsettings = ((MainWindow)App.Current.MainWindow).configsettings;

            waitingForProcessing waitDialog = new waitingForProcessing();
            int total = AutoSavedTasks.Count;
            waitDialog.progressbar.Maximum = total;
            waitDialog.progressbar.Value = 0;
            CancellationTokenSource cts = new CancellationTokenSource();
            Task.Run(() =>
            {
                int i = 1; bool flag = false; int failed_count = 0;
                List<ExportTask> failed_task = new List<ExportTask>();
                foreach (ExportTask x in AutoSavedTasks)
                {
                    waitDialog.textBlock.Dispatcher.Invoke(() =>
                    {
                        waitDialog.textBlock.Text = "我们需要一点时间来处理选中的下载信息...第" + i.ToString() + "个，共" + total.ToString() + "个";
                    });

                    var task_illust = pixivAPI.illust_workAsync(x.illustID, cts);
                    JObject returns = null;
                    try
                    {
                        returns = task_illust.Result;
                    }
                    catch
                    {//task cancelled or network problem
                        failed_count++;
                        failed_task.Add(x);
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
                        failed_task.Add(x);
                        flag = true;
                        continue;
                    }
                    illustTask illusttask = null;
                    if (x.type != ExportTask.Tasktype.manga)
                    {
                        illusttask = new illustTask(illust, configsettings.workPath);
                    }
                    else
                    {
                        if (x.mangaSelected == null)
                        {
                            illusttask = new illustTask(illust, configsettings.workPath);
                        }
                        else {
                            Dictionary<int, illustTask.state> mangaPages_temp = new Dictionary<int, illustTask.state>();
                            foreach (int y in x.mangaSelected)
                            {
                                mangaPages_temp.Add(y, illustTask.state.未启动);
                            }
                            illusttask = new illustTask(illust, configsettings.workPath) { mangaPages = mangaPages_temp, showName = illust.titleName + "等" + mangaPages_temp.Count + "页" };
                        }
                    }

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
                        ModernDialog.ShowMessage(failed_count.ToString() + "个任务没有解析到信息，需要重新添加下载，请手动添加", "部分任务没有添加", MessageBoxButton.OK);
                    });
                }
            });
            waitDialog.ShowDialog();
            if (waitDialog.MessageBoxResult != MessageBoxResult.Cancel) return;
            Debug.WriteLine("has cancelled");
            cts.Cancel();

        }

        private void deleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (tasklistview.SelectedItem == null) return;
            illustTask illusttask = null;
            try
            {
                illusttask = (illustTask)tasklistview.SelectedItem;
            }
            catch
            {
                ModernDialog.ShowMessage("发生意外错误，可能是本体发生错误，请联系作者", "意料之外的错误", MessageBoxButton.OK);
                return;
            }
            downloadTasks.Remove(illusttask);
        }
    }

}

