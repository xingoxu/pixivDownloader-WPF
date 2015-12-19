using System.IO;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading;
using System.Net;

namespace pixiv_API
{
    /// <summary>
    /// Caution: 0. JObject Items will return null
    /// </summary>
    public class pixivAPI
    {
        private OAuth oauth;
        public pixivAPI(OAuth oauth)
        {
            this.oauth = oauth;
        }

        public async Task<bool> reAuthAsync(CancellationTokenSource tokensource = null)
        {
            return await oauth.reAuthAsync(tokensource);
        }
        public List<string> bad_words()
        {
            string url = "https://public-api.secure.pixiv.net/v1.1/bad_words.json";

            //
            var task = oauth.HttpGetAsync(url, null);

            if (!task.Result.IsSuccessStatusCode)
            {
                Debug.WriteLine(task.Result);
                return null;
            }

            var json = JObject.Parse(task.Result.Content.ReadAsStringAsync().Result);

            List<string> result = new List<string>();

            do
            {
                foreach (JObject word in json.Value<JArray>("response"))
                {
                    if (word["word"] == null) continue;
                    result.Add(word["word"].ToString());
                }
                //next cursor shold be add at here(now must be null)
            }
            while (json["next"] != null);

            return result;
        }

        public JObject illust_work(string illust_id)
        {
            return illust_workAsync(illust_id).Result;
        }
        public async Task<JObject> illust_workAsync(string illust_id, CancellationTokenSource cancellationtokensource = null)
        {
            string url = ("https://public-api.secure.pixiv.net/v1/works/" + illust_id + ".json");
            var parameters = new Dictionary<string, object>(){
                   {"image_sizes", "px_128x128,small,medium,large,px_480mw" },
                   { "include_stats","true" }
            };

            var task = oauth.HttpGetAsync(url, parameters, cancellationtokensource);
            System.Net.Http.HttpResponseMessage message = null;
            try
            {
                message = await task;
            }
            catch (Exception ex)
            {//Task cancelled(out of time) or httpexception
                throw ex;
                //return null;
            }

            if (!message.IsSuccessStatusCode)
            {
                Debug.WriteLine(message);
                return null;
            }
            return JObject.Parse(message.Content.ReadAsStringAsync().Result);
        }
        public List<string> illust_work_originalPicURL(string illust_id)
        {
            var json = illust_work(illust_id);

            if (json == null) return null;

            List<string> result = new List<string>();

            foreach (JObject response in json.Value<JArray>("response"))//though now it will be only one response
            {

                if (!response["metadata"].HasValues)//illust
                    result.Add(response["image_urls"]["large"].ToString());
                else //优先遍历metadata中的原图
                {
                    if (!(bool)response["is_manga"])//ugoira
                    {
                        result.Add(response["image_urls"]["large"].ToString());
                        result.Add(response["metadata"]["zip_urls"]["ugoira600x600"].ToString());
                    }
                    else// manga
                    {
                        foreach (JObject image in response["metadata"]["pages"].Value<JArray>())
                        {
                            result.Add(image["image_urls"]["large"].ToString());
                        }
                    }
                }
            }
            return result;
        }
        public JObject user_profile(string user_id)
        {
            return user_profileAsync(user_id).Result;
        }
        public async Task<JObject> user_profileAsync(string user_id, CancellationTokenSource cancellationtokensource = null)
        {
            string url = "https://public-api.secure.pixiv.net/v1/users/" + user_id + ".json";
            var parameters = new Dictionary<string, object>(){
                {"profile_image_sizes","px_170x170,px_50x50"},
                {"image_sizes","px_128x128,small,medium,large,px_480mw"},
                {"include_stats", 1},
                {"include_profile", 1},
                {"include_workspace", 1},
                {"include_contacts", 1}
            };
            var task = oauth.HttpGetAsync(url, parameters, cancellationtokensource);
            System.Net.Http.HttpResponseMessage http = null;
            try
            {
                http = await task;
            }
            catch (Exception ex)
            {//Task cancelled(out of time) or httpexception
                throw ex;
            }
            if (!http.IsSuccessStatusCode)
            {
                Debug.WriteLine(http);
                return null;
            }

            return JObject.Parse(http.Content.ReadAsStringAsync().Result);
        }
        /// <summary>
        /// Feeds 动态 フィード 
        /// </summary>
        /// <param name="show_r18"></param>
        /// <param name="max_id">start from illust_id</param>
        /// <returns>return IsSuccess</returns>
        public JObject my_feeds(bool show_r18, string max_id)
        {
            string url = "https://public-api.secure.pixiv.net/v1/me/feeds.json";

            int r18 = 0;
            if (show_r18) r18 = 1;

            var parameters = new Dictionary<string, object>{
                {"relation","all"},
                {"type","touch_nottext"},
                {"show_r18",r18}
            };
            if (max_id != null) parameters.Add("max_id", max_id);

            var task = oauth.HttpGetAsync(url, parameters);

            if (!task.Result.IsSuccessStatusCode)
            {
                Debug.WriteLine(task.Result);
                return null;
            }

            return JObject.Parse(task.Result.Content.ReadAsStringAsync().Result);
        }
        public JObject my_following_works(int page = 1, int per_page = 30, bool include_stats = true, bool include_sanity_level = true)//关注的人的新作品
        {
            return my_following_worksAsync(page, per_page, include_stats, include_sanity_level).Result;
        }
        public async Task<JObject> my_following_worksAsync(int page = 1, int per_page = 30, bool include_stats = true, bool include_sanity_level = true, CancellationTokenSource cancellationtokensource = null)//关注的人的新作品
        {
            string url = "https://public-api.secure.pixiv.net/v1/me/following/works.json";
            var parameters = new Dictionary<string, object>{
               {"page",page},
               {"per_page", per_page},
               {"image_sizes","px_128x128,px_480mw,large"},
               {"include_stats",include_stats},//score and score count
               {"include_sanity_level",include_sanity_level}//unknown
            };

            var task = oauth.HttpGetAsync(url, parameters, cancellationtokensource);

            System.Net.Http.HttpResponseMessage http = null;
            try
            {
                http = await task;
            }
            catch (Exception ex)
            {//Task cancelled(out of time) or httpexception
                throw ex;
            }
            if (!http.IsSuccessStatusCode)
            {
                Debug.WriteLine(http);
                return null;
            }

            return JObject.Parse(http.Content.ReadAsStringAsync().Result);
        }
        public JObject my_favourite_works(int page = 1, int per_page = 50, bool IsPublic = true)//收藏夹作品
        {
            return my_favourite_worksAsync(page, per_page, IsPublic).Result;
        }
        public async Task<JObject> my_favourite_worksAsync(int page = 1, int per_page = 50, bool IsPublic = true, CancellationTokenSource cancellationTokenSource = null)//收藏夹作品
        {
            string url = "https://public-api.secure.pixiv.net/v1/me/favorite_works.json";
            string publicity = "private";
            if (IsPublic) publicity = "public";
            var parameters = new Dictionary<string, object>{
               {"page",page},
               {"per_page", per_page},
               {"image_sizes","px_128x128,px_480mw,large"},
               {"publicity",publicity }
            };
            var task = oauth.HttpGetAsync(url, parameters, cancellationTokenSource);

            System.Net.Http.HttpResponseMessage http = null;
            try
            {
                http = await task;
            }
            catch (Exception ex)
            {//Task cancelled(out of time) or httpexception
                throw ex;
            }
            if (!http.IsSuccessStatusCode)
            {
                Debug.WriteLine(http);
                return null;
            }

            return JObject.Parse(http.Content.ReadAsStringAsync().Result);
        }
        public bool my_favourite_work_add(string work_id, bool ispublic)
        {
            return my_favourite_work_addAsync(work_id, ispublic).Result;
        }
        public async Task<bool> my_favourite_work_addAsync(string work_id, bool ispublic, CancellationTokenSource cancellationtokensource = null)
        {
            string url = "https://public-api.secure.pixiv.net/v1/me/favorite_works.json";
            string publicity = "private";
            if (ispublic) publicity = "public";
            var parameters = new Dictionary<string, object>{
               {"work_id",work_id},
               {"publicity", publicity}
            };

            var task = oauth.HttpPostAsync(url, parameters, cancellationtokensource);
            System.Net.Http.HttpResponseMessage message = null;
            try
            {
                message = await task;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            if (!message.IsSuccessStatusCode)
            {
                Debug.WriteLine(message);
                return false;
            }

            return true;
        }
        public bool my_favourite_works_delete(string favourite_ids)//原API上注明需要输入publicity参数，经测试无需输入，都可以删除
        {
            return my_favourite_works_deleteAsync(favourite_ids).Result;
        }
        public async Task<bool> my_favourite_works_deleteAsync(string favourite_ids, CancellationTokenSource cancellationtokensource = null)//原API上注明需要输入publicity参数，经测试无需输入，都可以删除
        {
            string url = "https://public-api.secure.pixiv.net/v1/me/favorite_works.json";
            var parameters = new Dictionary<string, object>{
               {"ids",favourite_ids}
            };

            var task = oauth.HttpDeleteAsync(url, null, parameters, cancellationtokensource);
            System.Net.Http.HttpResponseMessage message = null;
            try
            {
                message = await task;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            if (!message.IsSuccessStatusCode)
            {
                Debug.WriteLine(message);
                return false;
            }

            return true;
        }
        public JObject my_following_user(int page=1, int per_page=30, bool IsPublic=true)
        {
            return my_following_userAsync(page, per_page, IsPublic).Result;
        }
        public async Task<JObject> my_following_userAsync(int page=1, int per_page=30, bool IsPublic=true, CancellationTokenSource cancellationtokensource = null)
        {
            string url = "https://public-api.secure.pixiv.net/v1/me/following.json";
            string publicity = "private";
            if (IsPublic) publicity = "public";
            var parameters = new Dictionary<string, object>{
               {"page",page},
               {"per_page", per_page},
               {"publicity",publicity}
            };

            var task = oauth.HttpGetAsync(url, parameters, cancellationtokensource);
            System.Net.Http.HttpResponseMessage http = null;
            try
            {
                http = await task;
            }
            catch (Exception ex)
            {//Task cancelled(out of time) or httpexception
                throw ex;
            }
            if (!http.IsSuccessStatusCode)
            {
                Debug.WriteLine(http);
                return null;
            }

            return JObject.Parse(http.Content.ReadAsStringAsync().Result);
        }
        //my_favourite_users api has been removed because of its unexpcted returns
        //public JObject my_favourite_users(int page)//same as my_following_user
        //{
        //    string url = "https://public-api.secure.pixiv.net/v1/me/favorite-users.json";
        //    var parameters = new Dictionary<string, object>()
        //    {
        //        {"page",page }
        //    };

        //    var task = oauth.HttpGetAsync(url, parameters);

        //    if (!task.Result.IsSuccessStatusCode)
        //    {
        //        Debug.WriteLine(task.Result);
        //        return null;
        //    }

        //    return JObject.Parse(task.Result.Content.ReadAsStringAsync().Result);
        //}
        public bool my_favourite_user_follow(string user_id, bool IsPublic)
        {
            string url = "https://public-api.secure.pixiv.net/v1/me/favorite-users.json";
            string publicity = "private";
            if (IsPublic) publicity = "public";
            var parameters = new Dictionary<string, object>()
            {
                {"target_user_id",user_id },
                {"publicity" ,publicity}
            };

            var task = oauth.HttpPostAsync(url, parameters);

            if (!task.Result.IsSuccessStatusCode)
            {
                Debug.WriteLine(task.Result);
                return false;
            }

            return true;
        }
        public async Task<bool> my_favourtie_user_followAsync(string user_id, bool IsPublic, CancellationTokenSource cancellationtokensource = null)
        {
            string url = "https://public-api.secure.pixiv.net/v1/me/favorite-users.json";
            string publicity = "private";
            if (IsPublic) publicity = "public";
            var parameters = new Dictionary<string, object>()
            {
                {"target_user_id",user_id },
                {"publicity" ,publicity}
            };

            var task = oauth.HttpPostAsync(url, parameters, cancellationtokensource);
            System.Net.Http.HttpResponseMessage result = null;
            try
            {
                result = await task;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (!result.IsSuccessStatusCode)
            {
                Debug.WriteLine(result);
                return false;
            }

            return true;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="author_ids">can put more than 2 id at here, use ',' to split them</param>
        /// <returns></returns>
        public bool my_favourite_users_unfollow(string user_ids)
        {
            return my_favourite_users_unfollowAsync(user_ids).Result;
        }
        public async Task<bool> my_favourite_users_unfollowAsync(string user_ids, CancellationTokenSource cancellationtokensource = null)
        {
            string url = "https://public-api.secure.pixiv.net/v1/me/favorite-users.json";
            var parameters = new Dictionary<string, object>
            {
                {"delete_ids",user_ids }
            };

            var task = oauth.HttpDeleteAsync(url, null, parameters, cancellationtokensource);

            System.Net.Http.HttpResponseMessage message = null;
            try
            {
                message = await task;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            if (!message.IsSuccessStatusCode)
            {
                Debug.WriteLine(message);
                return false;
            }

            return true;
        }
        public JObject user_works(string user_id, int page = 1, int per_page = 30, bool include_stats = true, bool include_sanity_level = true)
        {
            return user_worksAsync(user_id, page, per_page, include_stats, include_sanity_level).Result;
        }
        public async Task<JObject> user_worksAsync(string user_id, int page = 1, int per_page = 30, bool include_stats = true, bool include_sanity_level = true, CancellationTokenSource cancellationtokensource = null)
        {
            string url = string.Format("https://public-api.secure.pixiv.net/v1/users/{0}/works.json", user_id);

            var parameters = new Dictionary<string, object>{
               {"page",page},
               {"per_page", per_page},
               {"image_sizes","px_128x128,px_480mw,large"},
               {"include_stats",include_stats},//score and score count
               {"include_sanity_level",include_sanity_level}//unknown
            };

            var task = oauth.HttpGetAsync(url, parameters, cancellationtokensource);
            System.Net.Http.HttpResponseMessage http = null;
            try
            {
                http = await task;
            }
            catch (Exception ex)
            {//Task cancelled(out of time) or httpexception
                throw ex;
            }
            if (!http.IsSuccessStatusCode)
            {
                Debug.WriteLine(http);
                return null;
            }

            return JObject.Parse(http.Content.ReadAsStringAsync().Result);
        }
        public JObject user_favourite_works(string user_id, int page, int per_page)
        {
            return user_favourite_worksAsync(user_id, page, per_page).Result;
        }
        public async Task<JObject> user_favourite_worksAsync(string user_id, int page = 1, int per_page = 30, CancellationTokenSource cancellationtokensource = null)
        {
            string url = string.Format("https://public-api.secure.pixiv.net/v1/users/{0}/favorite_works.json", user_id);

            var parameters = new Dictionary<string, object>{
               {"page",page},
               {"per_page", per_page},
               {"image_sizes","px_128x128,px_480mw,large"},
               {"include_sanity_level",true}//unknown
            };

            var task = oauth.HttpGetAsync(url, parameters, cancellationtokensource);
            System.Net.Http.HttpResponseMessage http = null;
            try
            {
                http = await task;
            }
            catch (Exception ex)
            {//Task cancelled(out of time) or httpexception
                throw ex;
            }
            if (!http.IsSuccessStatusCode)
            {
                Debug.WriteLine(http);
                return null;
            }

            return JObject.Parse(http.Content.ReadAsStringAsync().Result);
        }
        /// <summary>
        /// 用户活动
        /// </summary>
        /// <param name="author_id"></param>
        /// <param name="show_r18"></param>
        /// <param name="max_id">start from illust_id (can be null)</param>
        /// <returns></returns>
        public JObject user_feeds(string user_id, bool show_r18, string max_id)
        {
            string url = string.Format("https://public-api.secure.pixiv.net/v1/users/{0}/feeds.json", user_id);

            int r18 = 0;
            if (show_r18) r18 = 1;

            var parameters = new Dictionary<string, object>{
                {"relation","all"},
                {"type","touch_nottext"},
                {"show_r18",r18}
            };
            if (max_id != null) parameters.Add("max_id", max_id);

            var task = oauth.HttpGetAsync(url, parameters);

            if (!task.Result.IsSuccessStatusCode)
            {
                Debug.WriteLine(task.Result);
                return null;
            }

            return JObject.Parse(task.Result.Content.ReadAsStringAsync().Result);
        }
        public JObject user_following_users(string user_id, int page=1, int per_page=30)
        {
            string url = string.Format("https://public-api.secure.pixiv.net/v1/users/{0}/following.json", user_id);
            var parameters = new Dictionary<string, object>()
            {
                {"page",page },
                {"per_page",per_page }
            };

            var task = oauth.HttpGetAsync(url, parameters);

            if (!task.Result.IsSuccessStatusCode)
            {
                Debug.WriteLine(task.Result);
                return null;
            }

            return JObject.Parse(task.Result.Content.ReadAsStringAsync().Result);
        }

        public JObject ranking(string ranking_type, string mode, int page = 1, int per_page = 50, string date = null)
        {
            return rankingAsync(ranking_type, mode, page, per_page, date).Result;
        }
        /// <summary>
        /// ranking_type and mode, are must values and not allow null.
        /// </summary>
        /// <param name="ranking_type">all,illust,manga,ugoira(gif/动图)</param>
        /// <param name="mode">about mode please see documents</param>
        /// <param name="page"></param>
        /// <param name="per_page"></param>
        /// <param name="date">format:yyyy-MM-dd (today should be null!)</param>
        /// <param name="cancellationtokensource"></param>
        /// <returns></returns>
        public async Task<JObject> rankingAsync(string ranking_type, string mode, int page, int per_page, string date, CancellationTokenSource cancellationtokensource = null)
        {
            string url = string.Format("https://public-api.secure.pixiv.net/v1/ranking/{0}.json", ranking_type);
            var parameters = new Dictionary<string, object>()
            {
                {"mode",mode },
                {"page",page },
                {"per_page",per_page },
                {"include_stats",true },
                {"include_sanity_level",true },
                {"image_sizes","px_128x128,px_480mw,large"},
                {"profile_image_sizes","px_170x170,px_50x50" }
            };
            if (date != null) parameters.Add("date", date);

            var task = oauth.HttpGetAsync(url, parameters, cancellationtokensource);
            System.Net.Http.HttpResponseMessage http = null;
            try
            {
                http = await task;
            }
            catch (Exception ex)
            {//Task cancelled(out of time) or httpexception
                throw ex;
            }
            if (!http.IsSuccessStatusCode)
            {
                Debug.WriteLine(http);
                return null;
            }

            return JObject.Parse(http.Content.ReadAsStringAsync().Result);
        }
        public JObject search_works(string query, int page = 1, int per_page = 30, string mode = "text", string period = "all", string order = "desc", string sort = "date", bool include_stats = true, bool include_sanity_level = true, bool show_r18 = true)
        {
            return search_worksAsync(query, page, per_page, mode, period, order, sort, include_stats, include_sanity_level, show_r18).Result;
        }
        /// <summary>
        /// search api only sort with date
        /// </summary>
        /// <param name="query">query string</param>
        /// <param name="page">1-n</param>
        /// <param name="per_page"></param>
        /// <param name="mode">[text,tag,caption,exact_tag] text means title/caption</param>
        /// <param name="period">[all,day,week,month]</param>
        /// <param name="order">[desc,asc] desc(new to old),asc(old to new)</param>
        /// <param name="sort">just "date"</param>
        /// <param name="show_r18">true or false</param>
        /// <returns></returns>
        public async Task<JObject> search_worksAsync(string query, int page=1, int per_page=30, string mode = "text", string period = "all", string order = "desc", string sort = "date", bool include_stats = true, bool include_sanity_level = true, bool show_r18 = true, CancellationTokenSource cancellationtokensource = null)
        {
            string url = "https://public-api.secure.pixiv.net/v1/search/works.json";
            int r18 = 0;
            if (show_r18) r18 = 1;
            var parameters = new Dictionary<string, object>()
            {
                {"q",query },
                {"page",page },
                {"per_page",per_page },
                {"period",period },
                {"order",order },
                {"sort",sort },
                {"mode",mode },
                {"types","illustration,manga,ugoira" },
                {"include_stats",include_stats },
                {"include_sanity_level",include_sanity_level },
                {"image_sizes","px_128x128,px_480mw,large"},
                {"show_r18",r18}
            };

            var task = oauth.HttpGetAsync(url, parameters, cancellationtokensource);
            System.Net.Http.HttpResponseMessage http = null;
            try
            {
                http = await task;
            }
            catch (Exception ex)
            {//Task cancelled(out of time) or httpexception
                throw ex;
            }
            if (!http.IsSuccessStatusCode)
            {
                Debug.WriteLine(http);
                return null;
            }

            return JObject.Parse(http.Content.ReadAsStringAsync().Result);
        }

        public JObject latest_works(int page = 1, int per_page = 30, bool include_stats = true, bool include_sanity_level = true)//won't return stats and sanity_level
        {
            return latest_worksAsync(page, per_page, include_stats, include_sanity_level).Result;
        }
        public async Task<JObject> latest_worksAsync(int page = 1, int per_page = 30, bool include_stats = true, bool include_sanity_level = true, CancellationTokenSource cancellationTokenSource = null)//won't return stats and sanity_level
        {
            string url = "https://public-api.secure.pixiv.net/v1/works.json";
            var parameters = new Dictionary<string, object>()
            {
                {"page",page },
                {"per_page",per_page },
                {"include_stats",include_stats },
                {"include_sanity_level",include_sanity_level },
                {"image_sizes","px_128x128,px_480mw,large"},
                {"profile_image_sizes","px_170x170,px_50x50" }
            };

            var task = oauth.HttpGetAsync(url, parameters, cancellationTokenSource);
            System.Net.Http.HttpResponseMessage http = null;
            try
            {
                http = await task;
            }
            catch (Exception ex)
            {//Task cancelled(out of time) or httpexception
                throw ex;
            }
            if (!http.IsSuccessStatusCode)
            {
                Debug.WriteLine(http);
                return null;
            }

            return JObject.Parse(http.Content.ReadAsStringAsync().Result);
        }


        /// <summary>
        /// a download file method with async (but pixiv server doesn't support resume download)
        /// </summary>
        /// <param name="strPathName"></param>
        /// <param name="strUrl"></param>
        /// <param name="header"></param>
        /// <param name="tokensource"></param>
        /// <returns>fileroute (include fileName)</returns>
        public async Task<string> DownloadFileAsync(string strPathName, string strUrl, Dictionary<string, object> header = null, CancellationTokenSource tokensource = null)
        {
            FileStream FStream = null;

            var task = oauth.HttpGetStreamAsync(header, strUrl, tokensource);
            //打开上次下载的文件或新建文件
            int CompletedLength = 0;//记录已完成的大小 
                                    //long ContentLength=0; Can't get in streamheader and getasync method is not good enough to download
            int progress = 0;//进度
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
            try
            {
                Stream myStream = await task;


                if (tokensource != null)
                {
                    await Task.Run(() =>
                    {
                        while ((CompletedLength = myStream.Read(btContent, 0, 1024)) > 0)
                        {
                            FStream.Write(btContent, 0, CompletedLength);
                            progress += CompletedLength;
                        }
                    }, tokensource.Token);
                }
                else {
                    await Task.Run(() =>
                    {
                        while ((CompletedLength = myStream.Read(btContent, 0, 1024)) > 0)
                        {
                            FStream.Write(btContent, 0, CompletedLength);
                            progress += CompletedLength;
                        }
                    });
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
                try
                {
                    File.Delete(fileRoute);
                }
                catch { }
                throw e;
            }
        }
        #region tempBaseApi
        public async Task<Stream> HttpGetStreamAsync(Dictionary<string, object> header, string strUrl, CancellationTokenSource tokensource = null)
        {
            return await oauth.HttpGetStreamAsync(header, strUrl, tokensource);
        }
        #endregion
    }
}
