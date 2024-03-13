using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace GTAVUtils
{
    public class Common
    {


        public static GTAVData DataPreprocess(Bitmap screenshot, ROI[] RoIs)
        {
            float cutBorderWidth = 0.1f;

            // cutImage
            int cutWidth = (int)(screenshot.Width * cutBorderWidth);
            int cutHeight = (int)(screenshot.Height * cutBorderWidth);
            Rectangle rect = new Rectangle(cutWidth, cutHeight, screenshot.Width - 2 * cutWidth, screenshot.Height - 2 * cutHeight);
            Bitmap cutedScreenshot = screenshot.Clone(rect, System.Drawing.Imaging.PixelFormat.DontCare);

            // filterRoIs
            List<ROI> filteredRoIs = new List<ROI>();
            foreach (ROI roi in RoIs)
            {
                ROI filteredRoI = new ROI(roi)
                {
                    ImageWidth = cutedScreenshot.Width,
                    ImageHeight = cutedScreenshot.Height
                };
                float ratio = roi.ImageWidth / (float)filteredRoI.ImageWidth;
                if (roi.BBox.Quality != GTABoundingBox2.DataQuality.Low)
                {
                    if (roi.BBox.Min.X > cutBorderWidth && roi.BBox.Min.Y > cutBorderWidth)
                    {
                        if (roi.BBox.Max.X < (1 - cutBorderWidth) && roi.BBox.Max.Y < (1 - cutBorderWidth))
                        {
                            filteredRoI.BBox.Min = new GTA.Math.Vector2((roi.BBox.Min.X - cutBorderWidth) * ratio, (roi.BBox.Min.Y - cutBorderWidth) * ratio);
                            filteredRoI.BBox.Max = new GTA.Math.Vector2((roi.BBox.Max.X - cutBorderWidth) * ratio, (roi.BBox.Max.Y - cutBorderWidth) * ratio);
                            filteredRoIs.Add(filteredRoI);
                        }
                    }
                }
            }
            GTA.UI.Screen.ShowHelpTextThisFrame(string.Format("{0}, {1}", filteredRoIs.Count, RoIs.Length));
            return new GTAVData(cutedScreenshot, filteredRoIs.ToArray());
        }
    }
}
