﻿using System.Drawing;
using System.IO;

namespace DataManager
{
    public class GTAVManager
    {
        public static void Prepare()
        {
            FileMamager.Prepare();
        }

        public static void SaveImage(string fileName, Bitmap image)
        {
            FileMamager.SaveImage(fileName, image);
        }

        public static void SaveTxt(string fileName, string txt)
        {
            FileMamager.SaveTxt(fileName, txt);
        }

        public static void Commit()
        {
            FileMamager.Commit();
        }
    }
}
