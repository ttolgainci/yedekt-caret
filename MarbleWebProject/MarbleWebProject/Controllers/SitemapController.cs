using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Xml.Linq;

namespace MarbleWebProject.Controllers
{
    [Route("sitemap.xml")]
    public class SitemapController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var sitemapXml = GenerateSitemap();
            return Content(sitemapXml, "application/xml", Encoding.UTF8);
        }
        private string GenerateSitemap()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var urlList = FilterParametersHelper.SiteMapUrlList.Where(c=>c.LanguageCode == AppConfig.CMSService.LanguageCode);
            var urls = new List<SitemapUrl>();
            foreach (var item in urlList)
            {
                switch (item.Type)
                {
                    case "CATEGORY":
                        urls.Add(new SitemapUrl(
                                $"{baseUrl}/{item.Url}",
                                DateTime.Now,
                                "daily",
                                "1.0"
                                ));
                        break;
                    case "PRODUCT":
                        urls.Add(new SitemapUrl(
                                 $"{baseUrl}/{item.Url}",
                                 DateTime.Now,
                                 "daily",
                                 "1.0"
                                 ));
                        break;
                    case "INFORMATION":
                        urls.Add(new SitemapUrl(
                                $"{baseUrl}/pages/{item.Url}",
                                DateTime.Now,
                                "daily",
                                "1.0"
                                ));
                        break;
                    case "FAQ":
                        urls.Add(new SitemapUrl(
                                $"{baseUrl}/{item.Url}",
                                DateTime.Now,
                                "daily",
                                "1.0"
                                ));
                        break;
                }
            }

            //var urls = new List<SitemapUrl>
            //{
            //    new SitemapUrl($"{baseUrl}/", DateTime.Now, "daily", "1.0"),
            //    new SitemapUrl($"{baseUrl}/about", DateTime.Now.AddDays(-1), "monthly", "0.8"),
            //    new SitemapUrl($"{baseUrl}/contact", DateTime.Now.AddDays(-2), "monthly", "0.6")
            //};

            // Define the sitemap namespace
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

            // Create the XML document
            var sitemap = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(ns + "urlset", // Use namespace with the root element
                    urls.Select(url =>
                        new XElement(ns + "url", // Use namespace for child elements too
                            new XElement(ns + "loc", url.Loc),
                            new XElement(ns + "lastmod", url.LastMod.ToString("yyyy-MM-dd")),
                            new XElement(ns + "changefreq", url.ChangeFreq),
                            new XElement(ns + "priority", url.Priority)
                        )
                    )
                )
            );

            return sitemap.ToString();
        }
    }
}
