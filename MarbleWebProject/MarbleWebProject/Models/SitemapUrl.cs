namespace MarbleWebProject.Models
{
    public class SitemapUrl
    {
        public string Loc { get; set; }
        public DateTime LastMod { get; set; }
        public string ChangeFreq { get; set; }
        public string Priority { get; set; }

        public SitemapUrl(string loc, DateTime lastMod, string changeFreq, string priority)
        {
            Loc = loc;
            LastMod = lastMod;
            ChangeFreq = changeFreq;
            Priority = priority;
        }
    }
}
