﻿using Common;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebScraper.Scrapers.Scripts
{
    public class BlogTruyenScript
    {
        const string ROOT_MANGALIST_URL = "http://blogtruyen.com/ajax/Search/AjaxLoadListManga?key=tatca&orderBy=1&p=";

        public int GetTotalPages()
        {
            int totalPages = 1;
            try
            {
                string pagingFilter = "<(?<TAG>\\w+)[^>]*?class\\s*=\\s*['|\"]\\s*page\\s*['|\"][^>]*?>.*?</\\k<TAG>>";
                string pagingIndexFilter = "<a[^>]*?href[^>]*?LoadListMangaPage\\s*\\((?<INDEX>\\d+)\\)[^>]*?>(?<TEXT>.*?)</a>";
                string pagingBlock = "";
                string firstPageOfMangaListHtmlSrc = HttpUtils.MakeHttpGet(ROOT_MANGALIST_URL + 1);
                MatchCollection mc = Regex.Matches(firstPageOfMangaListHtmlSrc, pagingFilter, RegexOptions.IgnoreCase);
                foreach (Match m in mc)
                {
                    pagingBlock += m.Value + Environment.NewLine;
                }

                mc = Regex.Matches(pagingBlock, pagingIndexFilter, RegexOptions.IgnoreCase);
                totalPages = mc.Count > 0 ? int.Parse(mc[mc.Count - 1].Groups["INDEX"].Value) : 1;
            }
            catch { }
            return totalPages;
        }

        public List<Dictionary<string, string>> GetMangaList(int pageIndex)
        {
            List<Dictionary<string, string>> results = new List<Dictionary<string, string>>();

            try
            {
                string blockFilter = "<(?<TAG>\\w+)[^>]*?class\\s*=\\s*[\"|']\\s*tiptip[^>]*?[\"|'][^>]*?>(?<TEXT>.*?)</\\k<TAG>>";
                string nameAndUrlFilter = "<a[^>]*?href\\s*=\\s*[\"|'](?<MANGA_URL>.*?)[\"|'][^>]*?>(?<MANGA_NAME>.*?)</a>";

                string mangaListHtmlSrc = HttpUtils.MakeHttpGet(ROOT_MANGALIST_URL + pageIndex);

                MatchCollection blockes = Regex.Matches(mangaListHtmlSrc, blockFilter, RegexOptions.IgnoreCase);
                foreach (Match blk in blockes)
                {
                    Match nameAndUrlGroup = Regex.Match(blk.Groups["TEXT"].Value, nameAndUrlFilter, RegexOptions.IgnoreCase);
                    string name = Regex.Replace(nameAndUrlGroup.Groups["MANGA_NAME"].Value, "<img.*?>", "", RegexOptions.IgnoreCase).Trim();
                    string url = nameAndUrlGroup.Groups["MANGA_URL"].Value;

                    if (string.IsNullOrWhiteSpace(name) == false && string.IsNullOrWhiteSpace(url) == false)
                    {
                        results.Add(new Dictionary<string, string>()
                            {
                                { "id", Guid.NewGuid().ToString() },
                                { "name", name },
                                { "url", url }
                            });
                    }
                }
            }
            catch { }

            return results;
        }

        public List<Dictionary<string, string>> GetChapterList(string mangaUrl)
        {
            string mangaUrlPattern = @"(http://|https://)?blogtruyen.com/(?<MANGA_ID>[^/]*?)/.*";
            string mangaId = Regex.Match(mangaUrl, mangaUrlPattern, RegexOptions.IgnoreCase).Groups["MANGA_ID"].Value;
            string ajaxUrl = "http://blogtruyen.com/ajax/Chapter/PartialLoadListChapter?mangaId=" + mangaId;

            List<Dictionary<string, string>> results = new List<Dictionary<string, string>>();

            string mangaHtmlSrc = HttpUtils.MakeHttpGet(ajaxUrl);
            HtmlDocument chapterDoc = new HtmlDocument();
            chapterDoc.LoadHtml(mangaHtmlSrc);

            HtmlNode listChapters = chapterDoc.GetElementbyId("list-chapters");
            if (listChapters != null)
            {
                List<HtmlNode> chapterBlocks = listChapters.Descendants().Where(x => x.Name.Equals("p")).ToList();
                foreach (HtmlNode chapterBlk in chapterBlocks)
                {
                    try
                    {
                        HtmlNode title = chapterBlk.Descendants().FirstOrDefault(x => x.GetAttributeValue("class", "").Contains("title")).Descendants().FirstOrDefault(x => x.Name.Equals("a"));
                        string name = title.InnerText.Trim();
                        string url = title.GetAttributeValue("href", "").Replace("../", "").Trim();

                        if (string.IsNullOrWhiteSpace(name) == false && string.IsNullOrWhiteSpace(url) == false)
                        {
                            results.Add(new Dictionary<string, string>()
                                {
                                    { "id", Guid.NewGuid().ToString() },
                                    { "name", name },
                                    { "url", url }
                                });
                        }
                    }
                    catch { }
                }
            }
            else
            {
                Uri uri = new Uri(mangaUrl);
                string mobileMangaSrc = HttpUtils.MakeHttpGet(uri.Scheme + "://m." + uri.Host + uri.PathAndQuery);
                HtmlDocument mobileDoc = new HtmlDocument();
                mobileDoc.LoadHtml(mobileMangaSrc);

                List<HtmlNode> rows = mobileDoc.DocumentNode.Descendants().FirstOrDefault(x => x.GetAttributeValue("class", "").Contains("danhsach")).Descendants().Where(x => x.GetAttributeValue("class", "").Contains("row")).ToList();
                foreach (HtmlNode r in rows)
                {
                    try
                    {
                        HtmlNode info = r.Descendants().First(x => x.Name.Equals("a"));
                        string name = info.InnerText.Trim();
                        string url = info.GetAttributeValue("href", "").Replace("../", "").Trim();

                        if (string.IsNullOrWhiteSpace(name) == false && string.IsNullOrWhiteSpace(url) == false)
                        {
                            results.Add(new Dictionary<string, string>()
                                {
                                    { "id", Guid.NewGuid().ToString() },
                                    { "name", name },
                                    { "url", url }
                                });
                        }
                    }
                    catch { }
                }
            }

            return results;
        }

        public List<Dictionary<string, string>> GetPageList(string chapterUrl)
        {
            List<Dictionary<string, string>> results = new List<Dictionary<string, string>>();

            try
            {
                string listExp = "<(?<TAG>\\w+)[^>]*?id\\s*=\\s*[\"|']\\s*content\\s*[\"|'][^>]*?>(?<LIST>.*?)</\\k<TAG>>";
                string pageExp = "<img.*?src\\s*=\\s*[\"|'](?<URL>[^'|\"]*?)[\"|'].*?>";
                int index = 1;

                string chapterHtmlSrc = HttpUtils.MakeHttpGet(chapterUrl);
                Match listBlock = Regex.Match(chapterHtmlSrc, listExp, RegexOptions.IgnoreCase);
                MatchCollection pageBlockes = Regex.Matches(listBlock.Groups["LIST"].Value, pageExp, RegexOptions.IgnoreCase);
                foreach (Match blk in pageBlockes)
                {
                    string url = blk.Groups["URL"].Value.Trim();
                    Match m = Regex.Match(url, ".+(&|&amp;)url=(?<ACTUAL_URL>[^&]+)", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        url = m.Groups["ACTUAL_URL"].Value.Trim();
                    }
                    url = RewriteGoogleImage(url);

                    if (string.IsNullOrWhiteSpace(url) == false)
                    {
                        results.Add(new Dictionary<string, string>()
                            {
                                { "id", Guid.NewGuid().ToString() },
                                { "name", "Trang " + StringUtils.GenerateOrdinal(pageBlockes.Count, index) },
                                { "url", Uri.UnescapeDataString(url) }
                            });

                        index++;
                    }
                }
            }
            catch { }

            return results;
        }

        private string RewriteGoogleImage(string url)
        {
            string reUrl = url;
            if (string.IsNullOrWhiteSpace(url) == false)
            {
                reUrl = url.Trim();

                Match match = Regex.Match(reUrl, @"^.+(&|\?)url=");
                if (match.Success)
                {
                    reUrl = reUrl.Replace(match.Value, "");
                }

                match = Regex.Match(reUrl, @"(https?:\/\/)lh(\d)(\.bp\.blogspot\.com)");
                if (match.Success)
                {
                    reUrl = reUrl.Replace(match.Value, match.Groups[1].ToString() + match.Groups[2].ToString() + match.Groups[3].ToString());
                }

                match = Regex.Match(reUrl, @"(https?:\/\/)lh\d\.(googleusercontent|ggpht)\.com");
                if (match.Success)
                {
                    reUrl = reUrl.Replace(match.Value, match.Groups[1].ToString() + "4.bp.blogspot.com");
                }

                match = Regex.Match(reUrl, @"\?.+$");
                if (match.Success)
                {
                    reUrl = reUrl.Replace(match.Value, "");
                }

                if (reUrl.Contains("blogspot.com") && Regex.IsMatch(reUrl, @"\/(s|w|h)0\/") == false)
                {
                    match = Regex.Match(reUrl, @"\/(s|h|w)\d+\/");
                    if (match.Success)
                    {
                        reUrl = reUrl.Replace(match.Value, "/");
                    }
                    reUrl += "?imgmax=0";
                }

                if (reUrl.Contains("i.imgur.com"))
                {
                    //reUrl = "http://images-focus-opensocial.googleusercontent.com/gadgets/proxy?container=focus&url=" + reUrl;
                }
            }
            return reUrl;
        }
    }
}
