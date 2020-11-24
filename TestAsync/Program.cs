using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestAsync
{
    class Program
    {
        static ulong i = 0;
        static int dut_selected = 0;
        static List<Dutyara> dut_list = new List<Dutyara>();
        //static Dutyara dut = new Dutyara(1);
        static Stopwatch sw = new Stopwatch();
        static string dut_data = "";
        // Дремя, которое даётся уту на то чтобы дать ответ
        static int time_to_dut_read = 5000;
        static int? message_status = null;
        const int MSG_SUCCESS = 1, MSG_FAIL = 0, MSG_DROP = -1;
        const string FAIL_VALUE = "65536", DROP_VALUE = "65533";
        public static Dutyara.MessageContent message_content = new Dutyara.MessageContent();


        static void Main(string[] args)
        {
            Dutyara.GetPorts();
            dut_list.Add(new Dutyara(44, 9600));
            dut_list.Add(new Dutyara(56, 9600));
            DutControl();
            //while (true)
            {
                Console.WriteLine(Console.ReadLine());
            }
            Console.ReadKey();
        }

        async static void DutControl()
        {
            await Task.Run(() =>
            {
                bool tt = true;
                while (tt)
                {
                    // Console.WriteLine(sw.ElapsedMilliseconds);
                    if (Dutyara.opened)
                    {
                        sw.Restart();
                        Dutyara.opened = false;
                        dut_list[dut_selected].GetData();
                        dut_list[dut_selected].SendMsg();
                        //GetAnsver();
                    }
                    else

                    //if (!Dutyara.opened)
                    {
                        //dut_data = "44N0=+210=01345.27=00632.55=094";
                        if (dut_data != "")
                        {
                            CheckData(dut_data);
                        }
                        // Так должно быть лучше, но нужно проверит, а на это нет времени:
                        else
                        if (sw.ElapsedMilliseconds > time_to_dut_read)
                        {
                            message_status = MSG_FAIL;  
                        }

                        switch (message_status)
                        {
                            case MSG_FAIL:
                                message_content.id = dut_list[dut_selected].Id.ToString();
                                message_content.water = FAIL_VALUE;
                                message_content.fuel = FAIL_VALUE;
                                message_content.temp = FAIL_VALUE;
                                //dut_data = "44N0=65536=65536=65536=094";
                                break;
                            case MSG_DROP:
                                message_content.id = dut_list[dut_selected].Id.ToString();
                                message_content.water = DROP_VALUE;
                                message_content.fuel = DROP_VALUE;
                                message_content.temp = DROP_VALUE;
                                break;
                            default:
                                break;
                        }

                        if (message_status != null)
                        {
                            if (message_status != MSG_SUCCESS)
                            dut_data = message_content.id + "N0=" + message_content.temp + "=" +
                                            message_content.fuel + "=" + message_content.water + "=094"; //"44N0=65536=65536=65536=094";
                            Console.WriteLine(dut_data);

                            GoToNextDut();
                        }
                        else dut_data = dut_list[dut_selected].GetData();
                    }

                    Thread.Sleep(1000);


                    // Если таймер больше Х то меняется номер дута и открываем opened 
                    //          Если у нового дута другая скорость меняем текущую скорость


                }
            }
            );
        }

        // Проверка полученных данных от ДУТа
        // в случае, если данные дошли в целосности - отправляет их получателю
        // в случае если данные пришли в повреждённом виде - отправляем получателю соответствующий код ошибки
        private static void CheckData(string input_text)
        {
            string[] dut_data_arr = input_text.Split('=');
            int arr_len = dut_data_arr.Length;
            if (arr_len != 5)
            {
                message_status = MSG_DROP;
            }
            else
            {
                // Проверяем является ли айдишник числом
                string dut_id = dut_data_arr[0].Substring(0, dut_data_arr[0].Length - 2);
                float i = 0;
                var bb = float.TryParse(dut_id, out i);
                if (!bb)
                {
                    message_status = MSG_DROP; return;
                }
                if (dut_data_arr[1][0] != '+' && dut_data_arr[1][0] != '-')
                {
                    message_status = MSG_DROP; return;
                }
                bb = float.TryParse(dut_data_arr[2].Replace(".", ","), out i);
                if (!bb)
                {
                    message_status = MSG_DROP; return;
                }
                bb = float.TryParse(dut_data_arr[3].Replace(".", ","), out i);
                if (!bb)
                {
                    message_status = MSG_DROP; return;
                }
                // ХЗ что хз зачем, но вдроуг пригодится
                //bb = float.TryParse(dut_data_arr[4].Replace(".", ","), out i);
                //if (!bb)
                //{
                //    message_status = MSG_DROP; return;
                //}

                message_status = MSG_SUCCESS;
                message_content.id = dut_id;
                message_content.fuel = dut_data_arr[2];
                message_content.water = dut_data_arr[3];
                message_content.temp = dut_data_arr[1];

            }
            return;
        }

        // Перейти к опросу следующего ДУТа 
        static void GoToNextDut()
        {
            Dutyara.need_a_stop = true;
            Dutyara.opened = true;
            dut_data = "";
            dut_selected = (dut_selected + 1) % dut_list.Count();
            message_status = null;
        }

        // Полечить ответ от ДУТа
        async static void GetAnsver()
        {
            await Task.Run(() =>
            {
                dut_data = dut_list[dut_selected].GetData();
                return;
                //Test();
            }
            );
        }

        static void Test()
        {
            int i = 0;
            while(true)
            {
                i++;
                Console.WriteLine(i);
                //Task.Delay(5000);
                Thread.Sleep(1000);
                if (i == 10)
                {
                    sw.Stop();
                    Console.WriteLine(sw.ElapsedMilliseconds);
                }
            }
        }
    }
}
