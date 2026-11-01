using System;
using System.Collections.Generic;

namespace QueryEngine
{
    public class FilterCategory
    {
        public string Name { get; set; }
        public string SearchMacro { get; set; }
        public string FilterQuery { get; set; }

        public FilterCategory(string name, string macro, string filterQuery)
        {
            this.Name = name;
            this.SearchMacro = macro;
            this.FilterQuery = filterQuery;
        }

        public static readonly FilterCategory Everything = new FilterCategory(
            "Everything", string.Empty, string.Empty);

        public static readonly FilterCategory Audio = new FilterCategory(
            "Audio", "audio:", "ext:aac;ac3;aif;aifc;aiff;au;cda;dts;fla;flac;it;m1a;m2a;m3u;m4a;mid;midi;mka;mod;mp2;mp3;mpa;ogg;ra;rmi;spx;umx;voc;wav;wma;xm");

        public static readonly FilterCategory Compressed = new FilterCategory(
            "Compressed", "zip:", "ext:7z;ace;arj;bz2;cab;gz;gzip;jar;r00;r01;rar;tar;tgz;z;zip;iso");

        public static readonly FilterCategory Document = new FilterCategory(
            "Document", "doc:", "ext:c;chm;cpp;cs;csv;cxx;doc;docm;docx;dot;dotm;dotx;h;hpp;htm;html;hxx;ini;java;js;json;log;lua;md;mht;mhtml;pdf;pot;potm;potx;ppam;pps;ppsm;ppsx;ppt;pptm;pptx;py;rtf;sql;txt;vbs;vsd;vsdx;wks;wps;wri;xls;xlsb;xlsm;xlsx;xlt;xltm;xltx;xml");

        public static readonly FilterCategory Executable = new FilterCategory(
            "Executable", "exe:", "ext:bat;cmd;exe;msi;msp;scr;ps1;vbs;com;wsf");

        public static readonly FilterCategory Folder = new FilterCategory(
            "Folder", "folder:", "folder:");

        public static readonly FilterCategory Picture = new FilterCategory(
            "Picture", "pic:", "ext:ani;bmp;cur;gif;ico;img;jpe;jpeg;jpg;pbm;pgm;png;ppm;psd;svg;tga;tif;tiff;webp;wmf;emf");

        public static readonly FilterCategory Video = new FilterCategory(
            "Video", "video:", "ext:3g2;3gp;3gp2;3gpp;amv;asf;asx;avi;bik;dat;divx;dvr-ms;f4v;flv;hdmov;m1v;m2p;m2t;m2ts;m2v;m4v;mkv;mov;mp2v;mp4;mp4v;mpe;mpeg;mpg;mpv2;mpv4;ogm;ogv;qt;rm;rmvb;swf;ts;vob;webm;wm;wmv");

        public static List<FilterCategory> GetDefaultFilters()
        {
            return new List<FilterCategory>
            {
                Everything,
                Audio,
                Compressed,
                Document,
                Executable,
                Folder,
                Picture,
                Video
            };
        }
    }
}
