using FirstFloor.ModernUI.Windows.Controls;
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
using System.IO;
using pixiv_API;
using System.Diagnostics;
using System.Threading;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using System.Xml;
using FirstFloor.ModernUI.Presentation;

namespace pixiv_downloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public pixivAPI pixivAPI;
        public AsyncObservableCollection<illustTask> downloadTasks;
        public pixivAuthor pixivAuthor;
        public pixivUser pixivUser;
        public ConfigSettings configsettings;//should do a control here in order let the following pages can't get it
        public MainWindow()
        {
            InitializeComponent();
            //load settings
            if (File.Exists("Config.xml")) configsettings = (ConfigSettings)XmlSerializer.LoadFromXml("Config.xml", typeof(ConfigSettings));
            else configsettings = new ConfigSettings() { workPath = Environment.CurrentDirectory, autoSaveTask = true };
            if (configsettings.currentTheme != null) AppearanceManager.Current.ThemeSource = new Uri(configsettings.currentTheme, UriKind.Relative);
            if (!configsettings.currentColor.Equals(Color.FromArgb(0, 0, 0, 0))) AppearanceManager.Current.AccentColor = configsettings.currentColor;

            downloadTasks = new AsyncObservableCollection<illustTask>();
            Login loginwindow = new Login();
            //show setting on loginwindow
            if (configsettings.PassWord != null) loginwindow.passwordTextBox.Password = Decrypt(configsettings.PassWord);
            if (configsettings.UserName != null) loginwindow.usernameTextBox.Text = configsettings.UserName;
            if (configsettings.refresh_token != null) loginwindow.refresh_token = Decrypt(configsettings.refresh_token);
            loginwindow.savepasswordCheckBox.IsChecked = configsettings.savePassword;
            loginwindow.showR18CheckBox.IsChecked = configsettings.showR18;


            loginwindow.ShowDialog();
            if (loginwindow.DialogResult == true)
            {//transfer information I need
                this.pixivAPI = loginwindow.pixivAPI;
                this.pixivUser = loginwindow.pixivUser;

                configsettings.savePassword = (bool)loginwindow.savepasswordCheckBox.IsChecked;
                configsettings.showR18 = (bool)loginwindow.showR18CheckBox.IsChecked;

                if (configsettings.savePassword)
                {
                    configsettings.UserName = loginwindow.usernameTextBox.Text;
                    configsettings.PassWord = Encrypt(loginwindow.passwordTextBox.Password);
                    configsettings.refresh_token = Encrypt(loginwindow.refresh_token);
                }
                else
                {
                    configsettings.UserName = null;
                    configsettings.PassWord = null;
                }
                System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                timer.Tick += Timer_Tick;
                timer.Interval = new TimeSpan(0, 0, pixivUser.expires_time);
                timer.Start();
            }
            else
            {
                this.Close();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {

            waitingForProcessing waitDialog = new waitingForProcessing();
            waitDialog.progressbar.Visibility = Visibility.Hidden;
            waitDialog.Title = "刷新用户信息";
            waitDialog.textBlock.Text = "你的登陆钥匙已经过期，正在重新刷新以便能继续浏览...";
            CancellationTokenSource cts = new CancellationTokenSource();
            bool returns = false;            
            Task.Run(() =>
            {
                try
                {
                    returns = pixivAPI.reAuthAsync(cts).Result;
                }
                catch
                {//task cancelled or network problem
                    waitDialog.Dispatcher.Invoke(() =>
                    {
                        waitDialog.textBlock.Text = "刷新时发生网络或超时错误，正在尝试重新刷新";
                    });
                    try
                    {
                        returns= pixivAPI.reAuthAsync(cts).Result;
                    }
                    catch
                    {

                    }
                }

                    waitDialog.Dispatcher.Invoke(() =>
                    {
                        try { waitDialog.Close(); } catch { }
                    });
            });
            waitDialog.ShowDialog();
            if (waitDialog.MessageBoxResult != MessageBoxResult.Cancel) return;
            Debug.WriteLine("has cancelled");
            cts.Cancel();
            if (!returns)
            {//if return null 
                ModernDialog.ShowMessage("用户数据没有刷新成功，可能无法进行浏览或下载，建议您关闭后重新打开", "重新认证失败", MessageBoxButton.OK);
            }
        }

        private static string Encrypt(string toEncrypt)
        {
            // 256-AES key    
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes("8$jk!dcz*-Swor$6.?$@*=-+dEyes@(#");
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        public static string Decrypt(string toDecrypt)
        {
            // 256-AES key    
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes("8$jk!dcz*-Swor$6.?$@*=-+dEyes@(#");
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        private void ModernWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
            configsettings.currentTheme = AppearanceManager.Current.ThemeSource.OriginalString;
            configsettings.currentColor = AppearanceManager.Current.AccentColor;
            if (configsettings.autoSaveTask)
            {
                foreach(illustTask task in downloadTasks)
                {
                    if(task.State!= illustTask.state.已完成)
                    {
                        if (configsettings.AutoSavedTasks == null) configsettings.AutoSavedTasks = new List<ExportTask>();
                        if (task.illust.Type == pixivIllust.illustType.manga)
                        {
                            if (task.mangaPages == null)
                            {
                                configsettings.AutoSavedTasks.Add(new ExportTask() { illustID = task.illust.illustID, TaskName = task.showName, type = ExportTask.Tasktype.manga, workPath=task.workPath});
                            }
                            else {
                                List<int> array = new List<int>();
                                foreach (KeyValuePair<int, illustTask.state> key in task.mangaPages)
                                {
                                    if (key.Value != illustTask.state.已完成)
                                        array.Add(key.Key);
                                }
                                configsettings.AutoSavedTasks.Add(new ExportTask() { illustID = task.illust.illustID, TaskName = task.showName, mangaSelected = array.ToArray(), type = ExportTask.Tasktype.manga ,workPath=task.workPath});
                            }
                        }
                        else if(task.illust.Type== pixivIllust.illustType.ugoira)
                        {
                            configsettings.AutoSavedTasks.Add(new ExportTask() { illustID = task.illust.illustID, TaskName = task.showName, type = ExportTask.Tasktype.ugoira ,workPath=task.workPath});
                        }
                        else
                            configsettings.AutoSavedTasks.Add(new ExportTask() { illustID = task.illust.illustID, TaskName = task.showName, type = ExportTask.Tasktype.illust ,workPath=task.workPath});
                    }
                }
            }

            if (File.Exists("Config.xml")) File.Delete("Config.xml");

            XmlSerializer.SaveToXml("Config.xml", configsettings, typeof(ConfigSettings), "ConfigSettings");

        }

        private void ModernWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if(configsettings.AutoSavedTasks!=null && configsettings.AutoSavedTasks.Count > 0)
            {
                var result = ModernDialog.ShowMessage("发现上次没有下载完的任务，是否加载入下载管理？\n如您选择否，任务仍然保留在配置文件中\n（加载需要重新解析，可能会失败，提醒您注意备份Config.xml）", "自动恢复下载任务提醒", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes) return;
                waitingForProcessing waitDialog = new waitingForProcessing();
                int total = configsettings.AutoSavedTasks.Count;
                waitDialog.progressbar.Maximum = total;
                waitDialog.progressbar.Value = 0;
                CancellationTokenSource cts = new CancellationTokenSource();
                Task.Run(() =>
                {
                    int i = 1; bool flag = false; int failed_count = 0;
                    List<ExportTask> failed_task = new List<ExportTask>();
                    foreach (ExportTask x in configsettings.AutoSavedTasks)
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
                            ModernDialog.ShowMessage(failed_count.ToString() + "个任务没有解析到信息，需要重新添加下载，仍在内存中，重启软体能重新读取", "部分任务可能没有添加", MessageBoxButton.OK);
                        });
                        configsettings.AutoSavedTasks = failed_task;
                    }
                });
                waitDialog.ShowDialog();
                if (waitDialog.MessageBoxResult != MessageBoxResult.Cancel) return;
                Debug.WriteLine("has cancelled");
                cts.Cancel();
            }

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
