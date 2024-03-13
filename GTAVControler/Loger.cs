using GTA;
using GTA.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GTAVControler
{
    class Loger
    {
        public Loger()
        {

        }
        public void AddLine(string s)
        {
            output_string_queue.Enqueue(s);
            while (output_string_queue.Count >= q_size)
            {
                var t = output_string_queue.Dequeue();
            }
        }
        public void Flush()
        {
            String os = "";
            foreach (var s in output_string_queue)
            {
                os += ("\n" + s);
            }
            GTA.UI.Screen.ShowSubtitle(os);
        }
        private Queue output_string_queue = new Queue();
        private int q_size = 1;
    };
}
