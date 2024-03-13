using GTA;
using GTA.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GTAVControler
{
    class Cotroler : Script
    {
        public Cotroler()
        {
            Tick += OnTick;
            Interval = 1;
            KeyDown += OnKeyDown;

            screen_width = Screen.PrimaryScreen.Bounds.Width;
            screen_height = Screen.PrimaryScreen.Bounds.Height;
            extractor = new Automation(screen_width, screen_height);
            extractor.Prepare();
        }

        private int screen_width, screen_height;
        private Automation extractor;
        private bool enableAutoSaveScreenshot = false;
        private static Random rd = new Random();
        private int cnt = 0;
        private int frame_count = 0;
        private Queue<TASK> queue = new Queue<TASK>();
        private Loger loger = new Loger();
        private Bitmap screenshot;
        private string timestamp;
        private GTA.Math.Vector3 pos, rot;
        private GTA.Math.Vector3 tf = new GTA.Math.Vector3((float)(5), (float)(5), 0);
        static T RandomEnumValue<T>()
        {
            var v = Enum.GetValues(typeof(T));
            return (T)v.GetValue(rd.Next(v.Length));
        }

        public void FisherYatesShuffle<T>(List<T> list)
        {
            List<T> cache = new List<T>();
            int currentIndex;
            while (list.Count > 0)
            {
                currentIndex = rd.Next(0, list.Count);
                cache.Add(list[currentIndex]);
                list.RemoveAt(currentIndex);
            }
            for (int i = 0; i < cache.Count; i++)
            {
                list.Add(cache[i]);
            }
        }

        private void PlaceOnGround(Entity obj)
        {
            var outArg = new OutputArgument();
            var npos = obj.Position;
            npos.Z = 200;
            Function.Call(Hash.GET_GROUND_Z_FOR_3D_COORD, npos.X, npos.Y, npos.Z, outArg, true, true);
            var gz = outArg.GetResult<float>();
            outArg.Dispose();
            npos.Z = Math.Max(0, gz + (float)0.3);
            loger.AddLine(string.Format("{0}", gz));
            obj.Position = npos;
        }
        private void CreateRandomCar(float groundz, int cnt = 1)
        {
            var cars = World.GetAllVehicles();
            int cnt_car = 0;
            foreach (var car in cars)
            {
                if (cnt_car++ >= cnt) break;

                if (car.ClassType == VehicleClass.Boats ||
                    car.ClassType == VehicleClass.Helicopters ||
                    car.ClassType == VehicleClass.Planes ||
                    car.ClassType == VehicleClass.Trains)
                    continue;

                var npos = World.RenderingCamera.Position;
                npos += new GTA.Math.Vector3(rd.Next(-25, 25), rd.Next(-25, 25), 0);
                npos.Z = groundz;
                car.Position = npos;
                car.Speed = 0;
                car.Velocity = new GTA.Math.Vector3(0, 0, 0);
                //car.CreateRandomPedOnSeat(VehicleSeat.Driver);
                if (rd.Next(1, 5) == 2)
                {
                    car.MarkAsNoLongerNeeded();
                    cnt_car--;
                }

                if (rd.Next(0, 5) >= 3)
                    car.PlaceOnGround();
                else
                    car.PlaceOnNextStreet();

                //car.CreatePedOnSeat(VehicleSeat.Driver, (RandomEnumValue<PedHash>()));
            }
            for (int i = cars.Length; i < cnt; ++i)
            {
                var npos = World.RenderingCamera.Position;
                npos += new GTA.Math.Vector3(rd.Next(-10, 10), rd.Next(-10, 10), 0);
                npos.Z = groundz;
                SpawnVehicle(npos);
            }


        }

        private void CreateRandomHuman(float groundz, int cnt = 1)
        {
            var cnt_ped = 0;
            var peds = World.GetAllPeds();
            foreach (var ped in peds)
            {
                if (cnt_ped++ > cnt) break;
                if (ped.IsPlayer) continue;
                if (ped.IsAlive == false || ped.IsDead || ped.IsInVehicle()) continue;
                //if (ped.GetHashCode() == PedHash)
                if (pet_hash.Contains(ped.GetHashCode())) continue;
                var pos = World.GetNextPositionOnStreet(World.RenderingCamera.Position, true);
                ped.Position = pos;
                ped.Task.WanderAround();
                //ped.HasGravity = false;
                //ped.Task.EnterAnyVehicle();
                //PlaceOnGround(ped);
                if (rd.Next(1, 5) == 2)
                {
                    ped.Kill();
                    ped.MarkAsNoLongerNeeded();
                    cnt_ped--;
                }
            }

            var lastpos = World.RenderingCamera.Position;
            for (int i = peds.Length; i < cnt; ++i)
            {
                //var pos = World.GetNextPositionOnSidewalk(World.RenderingCamera.Position);
                var pos = World.GetNextPositionOnSidewalk(World.RenderingCamera.Position);
                var ped = SpawnPedestrian(pos);
                if (ped != null)
                {
                    ped.Task.WanderAround();
                }
            }

        }
        private float CalGroundZ(GTA.Math.Vector3 npos)
        {
            var outArg = new OutputArgument();
            float groundZ = -200f; // -200f is the lowest limit of Z coord in the stock game
            Function.Call(Hash.GET_GROUND_Z_FOR_3D_COORD, npos.X, npos.Y, npos.Z, outArg, true, true);
            // Function.Call(Hash._GET_GROUND_Z_COORD_WITH_OFFSETS, npos.X, npos.Y, 300, outArg);
            groundZ = outArg.GetResult<float>();
            outArg.Dispose();
            return groundZ;
        }
        private float CalGroundZ()
        {
            var npos = World.RenderingCamera.Position;
            return CalGroundZ(npos);
        }
        private void GrabScreen()
        {
            screenshot = GTAVUtils.Common.GetScreenshot();
        }

        private void Extract(int cam_id)
        {
            //GrabScreen();
            //Get_Transform();
            //loger.AddLine("extract");
            extractor.SaveGTAVData(screenshot, timestamp, $"cam_{cam_id}", pos, rot);
            if (cam_id == 0)
            {
                // 变换相机到下一帧的pos
                //World.RenderingCamera.Position += tf;
            }
            else if (cam_id == 1)
            {
                //World.RenderingCamera.Position -= tf;
            }
        }

        private void RandomPose()
        {
            World.RenderingCamera.Position
                = new GTA.Math.Vector3(rd.Next(-2000, 2000), rd.Next(-2000, 2000), rd.Next(100, 200));
            var off_player = new GTA.Math.Vector3(0, 0, 20);
            //GTA.Game.Player.Character.Position = World.RenderingCamera.Position + off_player;

            queue.Enqueue(TASK.WAIT_TIME);
            queue.Enqueue(TASK.ADJUST_CAMPOS);
            queue.Enqueue(TASK.CREATE_RANDOM_OBJ);
            Interval = 1500;
        }
        private void RandomMove()
        {
            var cur_pose = World.RenderingCamera.Position;
            var disturb = new GTA.Math.Vector3(rd.Next(-15, 15), rd.Next(-15, 15), 0);
            cur_pose += disturb;
            World.RenderingCamera.Position = cur_pose;
            var off_player = new GTA.Math.Vector3(0, 0, 20);
            //GTA.Game.Player.Character.Position = World.RenderingCamera.Position + off_player;
        }
        private static HashSet<Ped> created_ped;
        private static HashSet<Vehicle> created_car;
        internal static Vehicle SpawnVehicle(GTA.Math.Vector3 pos)
        {
            var model = new Model(RandomEnumValue<VehicleHash>());

            var vehicle = World.CreateVehicle(model, pos);
            if (vehicle == null || vehicle.ClassType == VehicleClass.Planes
                || vehicle.ClassType == VehicleClass.Helicopters
                || vehicle.ClassType == VehicleClass.Boats
                ) return vehicle;
            model.MarkAsNoLongerNeeded();
            return vehicle;
        }
        private static HashSet<Int64> pet_hash = new HashSet<Int64>() {
0xAD7844BB,
0xA148614D,
0x431FC24C,
0x6C3F072 ,
0x3C831724,
0xD3939DFD,
0x9563221D,
0xC2D06F53,
0x349F33E1,
0xC3B52966,
0xDFB55C81,
0x6D362854,
0x431D501C,
0x1250D7BA,
0x6A20728 ,
0x8D8AC8B9,
0xE71D5E68,
0xB11BAB56,
0x4E8F95A2,
0x471BE4B2,
0x6AF51FAF,
0x2FD800B7,
0x8BBAB455,
0xD86B5A95,
0x18012A9F,
0x644AC75E,
0xFCFA9E1E,
0x56E29962,
0xA8683715,
0x14EC17EA,
0xAAB71F62,
0x573201B8,
0xCE5FF074
            };

        internal static Ped SpawnPedestrian(GTA.Math.Vector3 pos)
        {
            var model_hash = RandomEnumValue<PedHash>();
            if (pet_hash.Contains((Int64)model_hash))
            {
                return null;
            }
            var model = new Model(model_hash);
            if (model.IsValid == false)
            {
                return null;
            }
            var ped = World.CreatePed(model, pos);
            model.MarkAsNoLongerNeeded();
            //created_ped.Add(ped);
            return ped;
        }

        private void CreateRandomObject()
        {
            var groundz = CalGroundZ();
            try
            {
                CreateRandomCar(groundz, 15);
                CreateRandomHuman(groundz, 15);
            }
            catch (Exception e)
            {
                loger.AddLine("Exception !!!!");
            }
            queue.Enqueue(TASK.WAIT_TIME);
            Interval = 600;
        }
        private void AdjustCamHeight()
        {
            var groundZ = CalGroundZ();
            if (groundZ < 5)
            {
                // 1. 水面
                queue.Clear();
                //queue.Enqueue(TASK.RANDOM_POS);
                return;
            }
            var cur_cam_pose = World.RenderingCamera.Position;
            cur_cam_pose.Z = rd.Next(-10, 10) + groundZ + 50;
            World.RenderingCamera.Position = cur_cam_pose;
            GTA.Game.Player.Character.Position = cur_cam_pose + new GTA.Math.Vector3(0, 0, 10);
            Interval = 600;
        }

        enum TASK
        {
            WAIT_TIME = 0,
            FREEZE = 1,
            UNFREEZE = 2,
            EXTRACTION = 3,
            EXTRACTION1 = 4,
            CHECK = 5,
            RANDOM_POS = 6,
            ADJUST_CAMPOS = 7,
            CREATE_RANDOM_OBJ = 8,
            RANDOM_MOVE = 9,
        }
        private void Freeze()
        {
            GTA.Game.TimeScale = 0f;
        }
        private void UnFreeze()
        {
            GTA.Game.TimeScale = 1f;
        }
        private static int success_cnt = 0;
        private void Check()
        {
            bool chk_res = extractor.Check();
            if (chk_res == false)
            {
                loger.AddLine(string.Format("check res : {0}", chk_res));
                queue.Enqueue(TASK.RANDOM_POS);
                Interval = 1;
            }
            else
            {
                queue.Enqueue(TASK.FREEZE);
                queue.Enqueue(TASK.EXTRACTION);
                queue.Enqueue(TASK.EXTRACTION1);
                queue.Enqueue(TASK.UNFREEZE);

                var prob = rd.Next(0, 400);
                if (prob >= 100 && prob <= 125 || success_cnt++ > 20)
                {
                    queue.Enqueue(TASK.RANDOM_POS);
                    success_cnt = 0;
                }
                else
                {
                    queue.Enqueue(TASK.RANDOM_MOVE);
                }
            }
        }
        private void Get_timeStamp()
        {
            timestamp = GTAVUtils.Timer.GetTimeStamp();
        }
        private void Get_Transform()
        {
            pos = World.RenderingCamera.Position;
            rot = World.RenderingCamera.Rotation;
            //loger.AddLine(string.Format("{0}", rot));
        }

        private void OnTick(object sender, EventArgs e)
        {
            //Record_Video();
            //return;

            if (enableAutoSaveScreenshot == false) return;
            //var t = CalGroundZ();
            loger.AddLine(string.Format("queue：{0}", queue.Count));
            extractor.Pause();
            //System.Threading.Thread.Sleep(100);
            Extract(0);
            extractor.Resume();
            return;
            //CreateRandomCar(CalGroundZ(), 30);
            //CreateRandomHuman(CalGroundZ(), 30);
            //loger.AddLine(string.Format("{0}", t));
            if (queue.Count == 0)
            {
                queue.Enqueue(TASK.CHECK);
                Interval = 1;
            }
            if (queue.Count != 0)
            {
                var cur_task = queue.Dequeue();
                if (cur_task == TASK.WAIT_TIME)
                {
                    // recover intervel 
                }
                else if (cur_task == TASK.CHECK)
                {
                    Check();
                }
                else if (cur_task == TASK.FREEZE)
                {
                    //Freeze();
                }
                else if (cur_task == TASK.UNFREEZE)
                {
                    UnFreeze();
                }
                else if (cur_task == TASK.EXTRACTION)
                {
                    Get_timeStamp();
                    Extract(-1);
                    //loger.AddLine(string.Format("left tims:{0} pos: {1} , rot: {2}", timestamp, pos, rot));
                }
                else if (cur_task == TASK.EXTRACTION1)
                {
                    //Extract(1);
                    //loger.AddLine(string.Format("right tims:{0} pos: {1} , rot: {2}", timestamp, pos, rot));
                }
                else if (cur_task == TASK.RANDOM_POS)
                {
                    loger.AddLine(string.Format("{0}", cur_task));
                    RandomPose();
                }
                else if (cur_task == TASK.ADJUST_CAMPOS)
                {
                    loger.AddLine(string.Format("{0}", cur_task));
                    AdjustCamHeight();
                }
                else if (cur_task == TASK.CREATE_RANDOM_OBJ)
                {
                    loger.AddLine(string.Format("{0}", cur_task));

                    CreateRandomObject();
                }
                else if (cur_task == TASK.RANDOM_MOVE)
                {
                    RandomMove();
                }
                else
                {
                    loger.AddLine(string.Format("[ERROR] : {0} not exist", cur_task));
                }
            }
            //loger.AddLine(string.Format("screen "));
            loger.Flush();
            frame_count += 1;
        }

        private void Record_Video()
        {
            if (enableAutoSaveScreenshot == false) { UnFreeze(); return; }
            //var t = CalGroundZ();
            //loger.AddLine(string.Format("queue：{0}", queue.Count));
            //Extract(0);
            //CreateRandomCar(CalGroundZ(), 30);
            //CreateRandomHuman(CalGroundZ(), 30);
            //loger.AddLine(string.Format("{0}", t));
            if (queue.Count == 0)
            {
                queue.Enqueue(TASK.FREEZE);
                queue.Enqueue(TASK.EXTRACTION);
                queue.Enqueue(TASK.EXTRACTION1);
                queue.Enqueue(TASK.UNFREEZE);
            }
            if (queue.Count != 0)
            {
                var cur_task = queue.Dequeue();
                if (cur_task == TASK.WAIT_TIME)
                {
                    // recover intervel 
                }
                else if (cur_task == TASK.FREEZE)
                {
                    Freeze();
                }
                else if (cur_task == TASK.UNFREEZE)
                {
                    UnFreeze();
                }
                else if (cur_task == TASK.EXTRACTION)
                {
                    Get_timeStamp();
                    Extract(0);
                    //loger.AddLine(string.Format("left tims:{0} pos: {1} , rot: {2}", timestamp, pos, rot));
                }
                else if (cur_task == TASK.EXTRACTION1)
                {
                    Extract(1);
                    //loger.AddLine(string.Format("right tims:{0} pos: {1} , rot: {2}", timestamp, pos, rot));
                }
                else
                {
                    loger.AddLine(string.Format("[ERROR] : {0} not exist", cur_task));
                }
            }
            //loger.AddLine(string.Format("screen "));
            //loger.Flush();
            frame_count += 1;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F6)
            {
                enableAutoSaveScreenshot = !enableAutoSaveScreenshot;
                if (enableAutoSaveScreenshot == true)
                {
                    GTA.Game.Player.Character.HasGravity = false;
                    GTA.Game.Player.Character.IsVisible = false;
                    GTA.Game.Player.IsInvincible = true;
                    //GTA.World.RenderingCamera.Rotation = new GTA.Math.Vector3(-78, 0, 48);
                    Function.Call(Hash.SET_CLOCK_TIME, 12, 0, 0);
                    UnFreeze();

                    queue.Clear();
                    //queue.Enqueue(TASK.CHECK);
                    //loger.AddLine("switch~~~");
                }
                else
                {
                    queue.Clear();
                }
            }
            if (e.KeyCode == Keys.F7)
            {
                enableAutoSaveScreenshot = !enableAutoSaveScreenshot;
                if (enableAutoSaveScreenshot == true)
                {
                    GTA.Game.Player.Character.HasGravity = false;
                    GTA.Game.Player.Character.IsVisible = false;
                    GTA.Game.Player.IsInvincible = true;
                    //GTA.World.RenderingCamera.Rotation = new GTA.Math.Vector3(-78, 0, 48);
                    Function.Call(Hash.SET_CLOCK_TIME, 12, 0, 0);
                    Interval = 10;
                    UnFreeze();
                    queue.Clear();
                    //loger.AddLine("switch~~~");
                }
                else
                {
                    queue.Clear();
                }
            }

            if (e.KeyCode == Keys.U)
            {
                // Automation.Pause();
            }

            if (e.KeyCode == Keys.I)
            {
                // Automation.Resume();
            }
        }
    }
}
