using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pixiv_downloader
{
    public class Rankingillust
    {
        public string illustID { get; set; }
        public string titleName { get; set; }

        public string authorName { get; set; }
        public string authorID { get; set; }

        public int thisRank { get; set; }
        public int lastRank { get; set; }

    }
}
