﻿using Common.Enums;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WebScraper.Data;

namespace MangaDownloader.Utils
{
    class MangaUtils
    {
        public static void Export(MangaSite site, List<Manga> mangaList)
        {
            try
            {
                string folder = Application.StartupPath + "\\data";
                Directory.CreateDirectory(folder);

                String mangaFilePath = folder + "\\" + site.ToString().ToLower();
                using (StreamWriter sw = new StreamWriter(mangaFilePath, false, Encoding.UTF8))
                {
                    var csv = new CsvWriter(sw);
                    csv.WriteField("Name");
                    csv.WriteField("Url");
                    csv.NextRecord();

                    foreach (Manga manga in mangaList)
                    {
                        csv.WriteField(manga.Name);
                        csv.WriteField(manga.Url);
                        csv.NextRecord();
                    }

                    sw.Close();
                }
            }
            catch { }
        }

        public static void SaveListToFile(string filePath, MangaSite site, List<Manga> mangaList)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    var csv = new CsvWriter(sw);
                    csv.WriteField("Name");
                    csv.WriteField("Url");
                    csv.NextRecord();

                    foreach (Manga manga in mangaList)
                    {
                        csv.WriteField(manga.Name);
                        csv.WriteField(manga.Url);
                        csv.NextRecord();
                    }

                    sw.Close();
                }
            }
            catch { }
        }

        public static List<Manga> Import(MangaSite site)
        {
            try
            {
                List<Manga> mangaList = new List<Manga>();

                string mangaFilePath = Application.StartupPath + "\\data\\" + site.ToString().ToLower();
                if (File.Exists(mangaFilePath))
                {
                    using (StreamReader sr = new StreamReader(mangaFilePath, Encoding.UTF8))
                    {
                        var csv = new CsvReader(sr);
                        while (csv.Read())
                        {
                            Manga manga = new Manga();
                            manga.ID = Guid.NewGuid().ToString();
                            manga.Name = csv.GetField<string>(0);
                            manga.Url = csv.GetField<string>(1);
                            manga.Site = site;
                            mangaList.Add(manga);
                        }

                        sr.Close();
                    }
                }

                return mangaList;
            }
            catch
            {
                return new List<Manga>();
            }
        }

        public static Bitmap GetLogo(MangaSite site)
        {
            switch (site)
            {
                case MangaSite.BLOGTRUYEN:
                    return Properties.Resources.blogtruyen_logo;

                case MangaSite.TRUYENTRANHNET:
                    return Properties.Resources.vechai;

                case MangaSite.TRUYENTRANHTUAN:
                    return Properties.Resources.truyentranhtuan_logo;

                case MangaSite.TRUYENTRANH8:
                    return Properties.Resources.truyentranh8;

                case MangaSite.IZTRUYENTRANH:
                    return Properties.Resources.izmanga_logo;

                case MangaSite.HOCVIENTRUYENTRANH:
                    return Properties.Resources.hocvientruyentranh;

                case MangaSite.LHMANGA:
                    return Properties.Resources.lhmanga;
                    
                case MangaSite.TRUYENTRANHMOI:
                    return Properties.Resources.truyentranhmoi_favicon;

                case MangaSite.MANGAK:
                    return Properties.Resources.mangak_favicon;

                case MangaSite.UPTRUYEN:
                    return Properties.Resources.uptruyen_favicon;



                case MangaSite.MANGAFOX:
                    return Properties.Resources.mangafox_logo;

                case MangaSite.MANGAPARK:
                    return Properties.Resources.mangapark;

                case MangaSite.KISSMANGA:
                    return Properties.Resources.kissmanga_logo;

                default:
                    // TODO Add logo for more sites
                    throw new NotImplementedException();
            }
        }
    }
}
