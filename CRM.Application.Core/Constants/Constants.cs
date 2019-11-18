using System;
using System.Collections.Generic;

namespace CRM.Application.Core.Constants
{
    public static class Constants
    {
        public static readonly List<String> FileSupportedTypes =
            new List<string> { "doc ", "docx", "log", "odt", "pages", "rtf", "tex", "txt", "wpd",
                "wps" ,"csv","dat","ged","key","ketchain","pps","ppt","pptx","sdf","tar","tax2016",
            "vcf","xml","aif","iff","m3u","m4a","mid","mp3","mpa","wav","wma","3g2","3gp",
            "asf","avi","flv","m4v","mov","mp4","mpg","rm","srt","swf","vob","wmv",
            "bmp","dds","gif","jpg","png","psd","pspimage","tga","thm","tif","tiff","yuv","xlr","xls","xlsx","pct","pdf"};

        public static readonly List<int> Hours = new List<int> { 00,01,02,03,04,05,06,07,08,09,10,
        11,12,13,14,15,16,17,18,19,20,21,22,23};

        public static readonly List<int> Minutes = new List<int> { 00, 15, 30, 45 };

    }
}
