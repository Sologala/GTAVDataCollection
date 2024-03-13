/*
Authors: Artur Filipowicz
Version: 0.9
Copyright: 2016
MIT License
*/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using GTA;
using GTA.Native;
using GTA.Math;
//using Automation.cs
// Controls: 
// NumPad0 - spawns in a new test vehicle 
// I - mounts rendering camera on vehicle 
// O - restores the rendering camera to original control
namespace GTAVControler
{

    public class TestVehicle : Script
    {

        // camera used on the vehicle
        Camera camera = null;
        Vehicle vehicle;
        float x, y, z;
        private Automation extractor;
        private int screen_width, screen_height;

        private bool enableAutoSaveScreenshot = false;
        private static Random rd = new Random();
        private int cnt = 0;
        private int frame_count = 0;
        private Bitmap screenshot;
        private string timestamp;
        private GTA.Math.Vector3 pos, rot;
        public TestVehicle()
        {

            // create a new camera 
            World.DestroyAllCameras();
            camera = World.CreateCamera(new Vector3(), new Vector3(), 50);
            camera.IsActive = true;
            GTA.Native.Function.Call(Hash.RENDER_SCRIPT_CAMS, false, true, camera.Handle, true, true);
            screen_width = Screen.PrimaryScreen.Bounds.Width;
            screen_height = Screen.PrimaryScreen.Bounds.Height;
            extractor = new Automation(screen_width, screen_height);
            extractor.Prepare();
             screenshot = GTAVUtils.Common.GetScreenshot();
            x = 0;
            y = 2;
            z = 30.4f;
            // attach time methods 
            Tick += OnTick;
            KeyUp += onKeyUp;
        }

        // Function used to create a new test vehicle and set its properties. 
        public void spawnVehicle()
        {
            vehicle = World.CreateVehicle(VehicleHash.Adder, Game.Player.Character.Position + Game.Player.Character.ForwardVector * 3.0f, Game.Player.Character.Heading + 90);
            vehicle.CanTiresBurst = false;
            vehicle.CanBeVisiblyDamaged = false;
            vehicle.CanWheelsBreak = false;

            vehicle.PlaceOnGround();
        }

        // Function used to take control of the world rendering camera.
        public void mountCameraOnVehicle()
        {
            if (Game.Player.Character.IsInVehicle())
            {
                GTA.Native.Function.Call(Hash.RENDER_SCRIPT_CAMS, true, true, camera.Handle, true, true);
            }
            else
            {
            }
        }

        // Function used to allows the user original control of the camera.
        public void restoreCamera()
        {
            GTA.Native.Function.Call(Hash.RENDER_SCRIPT_CAMS, false, false, camera.Handle, true, true);
        }

        // Function used to keep camera on vehicle and facing forward on each tick step.
       
        public void keepCameraOnVehicle()
        {
            if (Game.Player.Character.IsInVehicle())
            {
                // keep the camera in the same position relative to the car
                camera.AttachTo(Game.Player.Character.CurrentVehicle, new Vector3(x, y, z));

                // rotate the camera to face the same direction as the car 
                camera.Rotation = Game.Player.Character.CurrentVehicle.Rotation;
                extractor.SaveGTAVData(screenshot, "dsafasd", "0", pos, rot);
            }
        }

        // Test vehicle controls 
        private void onKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.NumPad0)
            {
                spawnVehicle();
            }

            if (e.KeyCode == Keys.I)
            {
                mountCameraOnVehicle();
            }

            if (e.KeyCode == Keys.O)
            {
                restoreCamera();
            }

            if (e.KeyCode == Keys.Y)
            {
                z = z + 0.1f;
                keepCameraOnVehicle();
            }

            if (e.KeyCode == Keys.H)
            {
                z = z - 0.1f;
                keepCameraOnVehicle();
            }

            if (e.KeyCode == Keys.J)
            {
                x = z + 0.1f;
                keepCameraOnVehicle();
            }

            if (e.KeyCode == Keys.G)
            {
                x = z - 0.1f;
                keepCameraOnVehicle();
            }


        }

        void OnTick(object sender, EventArgs e)
        {
            keepCameraOnVehicle();
        }
    }
}