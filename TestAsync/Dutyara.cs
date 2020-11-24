using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAsync
{
    class Dutyara
    {
        int id;
        int speed;
        static Random rnd = new Random();
        static public bool opened = true;
        public static int current_speed;
        static public bool need_a_stop = false;

        public int Id
        {
            get
            {
                return id;
            }
        }

        public struct MessageContent
        {
            public string id;
            public string fuel;
            public string water;
            public string temp;
        }

        public Dutyara(int id, int speed = 9600)
        {
            this.id = id;
            this.speed = speed;
        }

        public string GetData()
        {
            var ans = "N0=+210=01345.27=00632.55=094";
            //string answer = "Ответ от " + this.id + ": " + ans;
            string answer = this.id + ans;
            int timeout = rnd.Next(1000, 5000);
            if (timeout <= 5000)
            {
                return answer;
            }
            else
            {
                while (true)
                {
                    if (need_a_stop)
                    {
                        need_a_stop = false;
                        return "";
                    }
                }
                return "";
            }
            
        }
    }
}
