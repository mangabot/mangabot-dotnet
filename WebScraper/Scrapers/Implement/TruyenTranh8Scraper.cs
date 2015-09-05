﻿using Common;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using WebScraper.Data;
using WebScraper.Enums;
using WebScraper.Utils;

namespace WebScraper.Scrapers.Implement
{
    class TruyenTranh8Scraper : IScraper
    {
        private const String BASE_LIST_URL = "http://truyentranh8.net/danh_sach_truyen/";

        public int GetTotalPages()
        {
            string src = HttpUtils.MakeHttpGet(BASE_LIST_URL);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(src);

            HtmlNode tblChap = doc.GetElementbyId("tblChap");
            HtmlNode lastA = tblChap.Descendants().LastOrDefault(
                x => x.Name.Equals("a") && x.GetAttributeValue("data-page", null) != null);
            if (lastA != null)
                return lastA.GetAttributeValue("data-page", 1);

            return 1;
        }

        public List<Manga> GetMangaList(int pageIndex)
        {
            string listUrl = pageIndex > 1 ? String.Format("{0}page={1}", BASE_LIST_URL, pageIndex) : BASE_LIST_URL;
            string src = HttpUtils.MakeHttpGet(listUrl);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(src);

            HtmlNode tblChap = doc.GetElementbyId("tblChap");
            List<HtmlNode> trTags = tblChap.Descendants().Where(x => x.Name.Equals("tr")).ToList();

            List<Manga> mangaList = new List<Manga>();
            foreach (HtmlNode tr in trTags)
            {
                HtmlNode td = tr.Descendants().FirstOrDefault(x => x.Name.Equals("td")
                    && x.GetAttributeValue("class", "").Contains("tit"));

                if (td != null)
                {
                    HtmlNode a = td.Element("a");

                    Manga manga = new Manga();
                    manga.ID = Guid.NewGuid().ToString();
                    manga.Name = WebUtility.HtmlDecode(a.InnerText.Trim());
                    manga.Url = WebUtility.HtmlDecode(a.GetAttributeValue("href", "").Trim());
                    manga.Site = MangaSite.TRUYENTRANH8;

                    if (String.IsNullOrEmpty(manga.Name) || String.IsNullOrEmpty(manga.Url))
                        continue;

                    mangaList.Add(manga);
                }
            }

            return mangaList;
        }

        public List<Chapter> GetChapterList(string mangaUrl)
        {
            List<Chapter> chapterList = new List<Chapter>();
            string src = HttpUtils.MakeHttpGet(mangaUrl);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(src);

            List<HtmlNode> aTags = doc.DocumentNode.Descendants().Where(
                x => x.GetAttributeValue("itemprop", "").Contains("itemListElement")).ToList();

            foreach (HtmlNode a in aTags)
            {
                HtmlNode title = a.Descendants().First(x => x.Name.Equals("h2"));
                HtmlNode date = title.Descendants().First(x => x.GetAttributeValue("itemprop", "").Contains("datePublished"));
                title.RemoveChild(date);

                Chapter chapter = new Chapter();
                chapter.ID = Guid.NewGuid().ToString();
                chapter.Name = WebUtility.HtmlDecode(title.InnerText.Trim());
                chapter.Url = WebUtility.HtmlDecode(a.GetAttributeValue("href", "").Trim());
                chapter.PublishedDate = date.InnerText.Trim();
                chapter.Site = MangaSite.TRUYENTRANH8;

                if (String.IsNullOrEmpty(chapter.Name) || String.IsNullOrEmpty(chapter.Url))
                    continue;

                chapterList.Add(chapter);
            }
            return chapterList;
        }

        public List<Page> GetPageList(string chapterUrl)
        {
            const string pattern = "lstImages.push\\([\"|'](?<URL>.+?)[\"|']\\)";
            List<Page> pageList = new List<Page>();
            int index = 1;

            string src = HttpUtils.MakeHttpGet(chapterUrl);
            MatchCollection list = Regex.Matches(src, pattern, RegexOptions.IgnoreCase);
            foreach (Match img in list)
            {
                string url = img.Groups["URL"].Value.Trim();
                Page page = new Page();
                page.ID = Guid.NewGuid().ToString();
                page.Name = "Trang " + StringUtils.GenerateOrdinal(list.Count, index);
                page.Url = WebUtility.HtmlDecode(url);
                page.Site = MangaSite.TRUYENTRANH8;

                if (String.IsNullOrEmpty(page.Url))
                    continue;

                pageList.Add(page);
                index++;
            }
            return pageList;
        }
    }
}
