using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pixiv_API
{
    public class pixivUser
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        /// <summary>
        /// 0 small 1 middle 2 big
        /// </summary>
        public string[] avatar { get; set; }
        public int expires_time { get; set; }
    }
}
