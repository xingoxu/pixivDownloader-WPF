using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pixiv_downloader
{
    public class pixivIllust
    {
        public pixivIllust()
        {
            isSetComplete = false;
        }
        public string illustID { get; set; }
        public string titleName { get; set; }
        #region author
        //if have time I will replace here with pixivAuthor class
        public string authorName { get; set; }        
        public string authorID { get; set; }
        public bool authorIsFollowing { get; set; }
        public string authorIconURL { get; set; }
        #endregion
        public string favouriteID { get; set; }
        public int FavNum { get; set; }
        public int Scores { get; set; }
        public List<string> MediumURL { get; set; }
        public string ugoiraZipURL { get; set; }
        public List<string> OriginalURL { get; set; }
        /// <summary>
        /// false all-age
        /// </summary>
        public bool ageLimit { get; set; }
        public illustType Type { get; set; }
        public string created_time { get; set; }
        public enum illustType
        {
            illustration,
            manga,
            ugoira
        }
        /// <summary>
        /// 废弃的
        /// </summary>
        public bool isSetComplete { get; set; }
    }
}
