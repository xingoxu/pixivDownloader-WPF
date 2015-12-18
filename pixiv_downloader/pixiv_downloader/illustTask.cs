using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pixiv_downloader
{
    public class illustTask: INotifyPropertyChanged
    {
        public illustTask(pixivIllust illust,string path)
        {
            this.downloadedSize = "没有开始";
            this.workPath = path;
            this.illust = illust;
            this.taskCancelsource = new CancellationTokenSource();
            this.showName = this.illust.titleName;
            this.State = illustTask.state.未启动;
        }
        private int downloadProgress;
        private object _lock = new object();
        public event PropertyChangedEventHandler PropertyChanged;
        public illustTask(pixivIllust illust, pixiv_API.pixivAPI pixivAPI,string path)
        {
            this.downloadedSize = "获取中...";
            this.illust = illust;
            this.workPath = path;
            this.taskCancelsource = new CancellationTokenSource();
            this.showName = this.illust.titleName;
            this.State = illustTask.state.未启动;
            this.task = Task.Factory.StartNew(() =>
            {
                Task[] tasks = new Task[this.illust.OriginalURL.Count];
                if (this.illust.Type == pixivIllust.illustType.illustration)
                {
                    //this.downloadedSize = "0K";
                    tasks[0] = DownloadFileAsync(this.workPath, this.illust.OriginalURL[0], pixivAPI, this, null, this.taskCancelsource);
                }
                else if (this.illust.Type == pixivIllust.illustType.manga)
                {
                    int i = 0; this.downloadProgress = 0; this.downloadedSize = "0/" + this.illust.OriginalURL.Count;
                    mangaPages = new Dictionary<int, state>();
                    foreach (string url in this.illust.OriginalURL)
                    {
                        int x = i;
                        tasks[i] = runDownloadForMultiPageThings(x, pixivAPI, url, this.illust.OriginalURL.Count,path);
                        i++;
                    }
                }
                else if (this.illust.Type == pixivIllust.illustType.ugoira)
                {
                    tasks = new Task[2]; this.downloadedSize = "0K";
                    tasks[0] = pixivAPI.DownloadFileAsync(null, this.illust.OriginalURL[0], null, this.taskCancelsource);
                    tasks[1] = DownloadFileAsync(this.workPath, this.illust.ugoiraZipURL, pixivAPI, this, null, this.taskCancelsource);
                }
                else
                {
                    Debug.WriteLine("illustTask went wrong");
                    return;
                }
                this.State = illustTask.state.下载中;
                try
                {
                    Task.WaitAll(tasks);
                }
                catch
                {//cancelled or httpexception
                    this.State = state.被取消或网络错误;
                    return;
                }
                this.State = state.已完成;
            });
        }
        /// <summary>
        /// for single manga picture
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pixivAPI"></param>
        /// <param name="showName"></param>
        /// <param name="illust"></param>
        public illustTask(mangaListModel url,pixiv_API.pixivAPI pixivAPI,string showName,pixivIllust illust,string path)
        {
            downloadedSize = "获取中...";
            this.State = illustTask.state.未启动;
            this.illust = illust;
            this.workPath = path;
            this.showName = showName;
            this.taskCancelsource = new CancellationTokenSource();
            this.task = Task.Factory.StartNew(() =>
            {
                //this.downloadedSize = "0K";
                mangaPages = new Dictionary<int, state>();
                var task = DownloadFileAsync(this.workPath, url.URL, pixivAPI, this, null, this.taskCancelsource);
                this.State = illustTask.state.下载中;
                mangaPages.Add(url.Number, state.下载中);
                try
                {
                    string x = task.Result;
                }
                catch
                {//cancelled or httpexception
                    this.State = state.被取消或网络错误;
                    return;
                }
                this.State = state.已完成;
                mangaPages[url.Number] = state.已完成;
            });
        }
        /// <summary>
        /// for multi manga pictures
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="pixivAPI"></param>
        /// <param name="showName"></param>
        /// <param name="illust"></param>
        /// <param name="ids"></param>
        public illustTask(List<mangaListModel> mangaPages, pixiv_API.pixivAPI pixivAPI, string showName,pixivIllust illust,string path)
        {
            downloadedSize = "获取中...";
            this.State = illustTask.state.未启动;
            this.illust = illust;
            this.workPath = path;
            this.showName = showName;
            this.taskCancelsource = new CancellationTokenSource();
            this.task = Task.Factory.StartNew(() =>
            {
                Task[] tasks = new Task[mangaPages.Count];


                int i = 0; this.downloadProgress = 0; this.downloadedSize = "0/" + mangaPages.Count;
                this.mangaPages = new Dictionary<int, state>();
                foreach (mangaListModel mangapage in mangaPages)
                {
                    int x = mangapage.Number;
                    tasks[i] = runDownloadForMultiPageThings(x - 1, pixivAPI, mangapage.URL, mangaPages.Count,path);
                    i++;
                }
                this.State = illustTask.state.下载中;
                try
                {
                    Task.WaitAll(tasks);
                }
                catch
                {//cancelled
                    this.State = state.被取消或网络错误;
                    return;
                }
                this.State = state.已完成;
            });
        }

        //not good method for here but the async method doesn't support ref
        private async Task<string> DownloadFileAsync(string strPathName, string strUrl, pixiv_API.pixivAPI oauth, illustTask tsk, Dictionary<string, object> header = null, CancellationTokenSource tokensource = null)
        {
            FileStream FStream = null;
            try
            {
                var task = oauth.HttpGetStreamAsync(header, strUrl, tokensource);
                //打开上次下载的文件或新建文件
                int CompletedLength = 0;//记录已完成的大小 
                //long ContentLength=0; Can't get in streamheader and getasync method is not good enough to download
                int progress = 0;//进度
                tsk.downloadedSize = (String)"0K";
                #region get filename
                string filename = null;

                if (filename != null) filename.Trim(' ');

                if (filename == null || filename.Equals(""))
                {
                    string[] split = strUrl.Split('/');
                    filename = split[split.Length - 1];
                }
                #endregion

                string fileRoute = "";
                if (strPathName == null) fileRoute = filename;
                else {
                    fileRoute = strPathName + '/' + filename;
                    if (!Directory.Exists(strPathName)) Directory.CreateDirectory(strPathName);
                }
                if (File.Exists(fileRoute)) File.Delete(fileRoute);
                FStream = new FileStream(fileRoute, FileMode.Create);

                byte[] btContent = new byte[1024];

                Stream myStream = await task;


                if (tokensource != null)
                {
                    try {
                        await Task.Run(() =>
                        {
                            while ((CompletedLength = myStream.Read(btContent, 0, 1024)) > 0)
                            {
                                FStream.Write(btContent, 0, CompletedLength);
                                progress += CompletedLength;
                                tsk.downloadedSize = (String)((((double)progress) / 1024).ToString("f2") + "K");
                            }
                        }, tokensource.Token);
                    }
                    catch(Exception e)
                    {
                        throw e;
                    }
                }
                else {
                    try {
                        await Task.Run(() =>
                        {
                            while ((CompletedLength = myStream.Read(btContent, 0, 1024)) > 0)
                            {
                                FStream.Write(btContent, 0, CompletedLength);
                                progress += CompletedLength;
                                tsk.downloadedSize = (((double)progress) / 1024).ToString("f2") + "K";
                            }
                        });
                    }
                    catch(Exception e)
                    {
                        throw e;
                    }
                }
                FStream.Close();
                myStream.Close();
                return fileRoute;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (FStream != null)
                {
                    FStream.Close();
                    FStream.Dispose();
                }
                throw e;
            }
        }
        private async Task runDownloadForMultiPageThings(int x, pixiv_API.pixivAPI pixivAPI, string url, int maxcount,string path)//取名困难症orz
        {
            mangaPages.Add(x + 1, state.下载中);
            try
            {
                var _string = await pixivAPI.DownloadFileAsync(path, url, null, this.taskCancelsource);
            }
            catch (Exception ex)
            {//task cancelled or network problem
                throw ex;
            }
            mangaPages[x + 1] = state.已完成;
            // should have a lock here
            lock (_lock)
            {
                this.downloadProgress++;
                this.downloadedSize = this.downloadProgress.ToString() + "/" + maxcount;
            }
        }


        public void restartTask(pixiv_API.pixivAPI pixivAPI)
        {
            taskCancelsource = new CancellationTokenSource();
            this.downloadedSize = "获取中...";
            this.taskCancelsource = new CancellationTokenSource();
            this.State = illustTask.state.未启动;
            if (this.illust.Type== pixivIllust.illustType.illustration)
            {
                this.task = Task.Factory.StartNew(() =>
                {
                    Task[] tasks = new Task[this.illust.OriginalURL.Count];

                    //this.downloadedSize = "0K";
                    tasks[0] = DownloadFileAsync(this.workPath, this.illust.OriginalURL[0], pixivAPI, this, null, this.taskCancelsource);
                    
                    this.State = illustTask.state.下载中;
                    try
                    {
                        Task.WaitAll(tasks);
                    }
                    catch
                    {//cancelled or httpexception
                        this.State = state.被取消或网络错误;
                        return;
                    }
                    this.State = state.已完成;
                });
            }
            else if(this.illust.Type == pixivIllust.illustType.ugoira)
            {
                this.task = Task.Factory.StartNew(() =>
                {
                    Task[] tasks = new Task[this.illust.OriginalURL.Count];
                    tasks = new Task[2]; this.downloadedSize = "0K";
                    tasks[0] = pixivAPI.DownloadFileAsync(null, this.illust.OriginalURL[0], null, this.taskCancelsource);
                    tasks[1] = DownloadFileAsync(this.workPath, this.illust.ugoiraZipURL, pixivAPI, this, null, this.taskCancelsource);
                    this.State = illustTask.state.下载中;
                    try
                    {
                        Task.WaitAll(tasks);
                    }
                    catch
                    {//cancelled or httpexception
                        this.State = state.被取消或网络错误;
                        return;
                    }
                    this.State = state.已完成;
                });
            }
            else
            {
                this.task = Task.Factory.StartNew(() =>
                {
                    if (mangaPages == null || mangaPages.Count == 0)
                    {
                        Task[] tasks = new Task[this.illust.OriginalURL.Count];
                        int i = 0; this.downloadProgress = 0; this.downloadedSize = "0/" + this.illust.OriginalURL.Count;
                        mangaPages = new Dictionary<int, state>();
                        foreach (string url in this.illust.OriginalURL)
                        {
                            int x = i;
                            tasks[i] = runDownloadForMultiPageThings(x, pixivAPI, url, this.illust.OriginalURL.Count,this.workPath);
                            i++;
                        }
                        this.State = illustTask.state.下载中;
                        try
                        {
                            Task.WaitAll(tasks);
                        }
                        catch
                        {//cancelled or httpexception
                            this.State = state.被取消或网络错误;
                            return;
                        }
                        this.State = state.已完成;
                    }
                    else
                    {
                        List<int> uncompletePage = new List<int>();
                        foreach (KeyValuePair<int, state> x in mangaPages)
                        {
                            if(x.Value!= state.已完成)
                            {
                                uncompletePage.Add(x.Key);
                            }
                        }
                        mangaPages.Clear();
                        if (uncompletePage.Count == 0) return;
                        Task[] tasks = new Task[uncompletePage.Count];
                        int i = 0; this.downloadProgress = 0; this.downloadedSize = "0/" + uncompletePage.Count;
                        mangaPages = new Dictionary<int, state>();
                        foreach (int x in uncompletePage)
                        {
                            tasks[i] = runDownloadForMultiPageThings(x - 1, pixivAPI, illust.OriginalURL[x-1], uncompletePage.Count,this.workPath);
                            i++;
                        }
                        this.State = illustTask.state.下载中;
                        try
                        {
                            Task.WaitAll(tasks);
                        }
                        catch
                        {//cancelled or httpexception
                            this.State = state.被取消或网络错误;
                            return;
                        }
                        this.State = state.已完成;

                    }

                });
            }

        }

        public CancellationTokenSource taskCancelsource { get; set; }

        public Dictionary<int, state> mangaPages;

        private string _showName;
        public string showName { get { return _showName; } set { _showName = value; if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("showName"));
                }
            } }
        public pixivIllust illust { get; set; }
        private string _downloadedSize;
        public string downloadedSize {
            get { return _downloadedSize; }
            set
            {
                _downloadedSize = value; if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("downloadedSize"));
                }
            }
        }
        public Task task { get; set; }
        private string _workPath;
        public string workPath
        {
            get { return _workPath; }
            set
            {
                _workPath = value; if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("workPath"));
                }
            }
        }
        private state _State = state.未启动;
        public state State {
            get { return _State; }
            set
            {
                _State = value; if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("State"));
                }
            }
        } 
        public enum state
        {
            未启动,
            下载中,
            已完成,
            被取消或网络错误
        }
    }
}
