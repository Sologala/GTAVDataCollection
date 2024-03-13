using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GTA;
using Vector3 = GTA.Math.Vector3;
using System;

namespace GTAVControler
{
    public class Automation
    {
        public Automation(int _w, int _h)
        {
            screen_height = _h;
            screen_width = _w;
        }
        public int screen_width { get; set; }
        public int screen_height { get; set; }

        private  GTAVUtils.ROI[] GetRoIs(int width, int height)
        {
            Vector3 camPos = World.RenderingCamera.Position;
            Vector3 camRot = World.RenderingCamera.Rotation;
            Vehicle[] vehicles = World.GetAllVehicles();
            List<GTAVUtils.ROI> rois = new List<GTAVUtils.ROI>();
            foreach (Vehicle vehicle in vehicles)
            {
                if (vehicle.IsVisible == false)
                {
                    continue;
                }
                GTAVUtils.ROI roi = new GTAVUtils.ROI(vehicle, (GTAVUtils.ROI.DetectionType)vehicle.ClassType, vehicle.Model.IsBigVehicle,rois.Count, width, height, camPos, camRot);
                if (roi.BBox.IsValid)
                {
                    rois.Add(roi);
                }
            }
            Ped[] pedestrians = World.GetAllPeds();
            foreach (Ped ped in pedestrians)
            {
                if (ped.IsHuman == false) continue;
                if (ped.IsInBoat || ped.IsInFlyingVehicle || ped.IsInVehicle() || ped.IsSittingInVehicle())
                {
                    continue;
                }
                GTAVUtils.ROI roi = new GTAVUtils.ROI(ped, (GTAVUtils.ROI.DetectionType)(23), false,rois.Count, width, height, camPos, camRot);
                bool flag = false;
                foreach(GTAVUtils.ROI roi_car in rois)
                {
                   if (roi_car.Type == (GTAVUtils.ROI.DetectionType)(23))
                   {
                        break;
                   }
                   //if (roi_car.BBox.(roi.BBox)){
                   //     flag = true;
                   //     break;
                   //}
                }
                if (flag == false && roi.BBox.IsValid)
                {
                    rois.Add(roi);
                }
            }
            return rois.ToArray();
        }

        public  void Prepare()
        {
            DataManager.GTAVManager.Prepare();
        }

        public  GTA.Math.Vector3 lastPostion;
        public  bool Check()
        {
            GTAVUtils.ROI[] rois = GetRoIs(screen_width, screen_height);
            return rois.Length != 0;
        }
        public  void SaveGTAVData(Bitmap screenshot, string timestamp, string cam_flag, GTA.Math.Vector3 pos , GTA.Math.Vector3 rot)
        {
            // roilabel
            GTAVUtils.ROI[] rois = GetRoIs(screenshot.Width, screenshot.Height);

            //foreach (GTAVUtils.ROI r in rois)
            //{
            //    r.DrawOnScreen();
            //}
            // preprocess and save data
            //GTAVUtils.Common.DataPreprocess(screenshot, rois).Save(timestamp, timestamp, cam_flag, pos, rot, true);
            GTAVUtils.Common.DataPreprocess(screenshot, rois).Save(timestamp, timestamp, true);
        }

        public  void Pause()
        {
            Game.Pause(true);
        }

        public  void Resume()
        {
            Game.Pause(false);
        }
    }
}
