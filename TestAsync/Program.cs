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
        const string MALINA_ID = "00001";
        static ulong i = 0;
        static int dut_selected = 0;
        static List<Dutyara> dut_list = new List<Dutyara>();
        //static Dutyara dut = new Dutyara(1);
        static Stopwatch sw_timeout = new Stopwatch(); // Для проверки потраченного времени на опрос ДУТа
        static Stopwatch sw_request = new Stopwatch(); // Для проверки необходимости повторного опроса ДУТов
        static string dut_data = "";
        // Дремя, которое даётся дуту на то чтобы дать ответ
        static int time_to_dut_read = 4500;
        // Частота опроса ДУТов.
        static int request_time = 10000;
        static int? message_status = null;
        const int MSG_SUCCESS = 1, MSG_FAIL = 0, MSG_DROP = -1;
        const string FAIL_VALUE = "65536" /*Не верный формат данных*/, DROP_VALUE = "65533" /*Часть данных была потеряна*/,
            PORT_VALUE = "65530" /*Ошибка COM-порта*/;
        //public static Dutyara.MessageContent dut_list[dut_selected].msg_cont = new Dutyara.MessageContent();
        static bool need_request = true;


        static void Main(string[] args)
        {
            // TopWindowSet.setTop();
            Dutyara.GetPorts();
            dut_list.Add(new Dutyara(33722, 9600));
            dut_list.Add(new Dutyara(22733, 9600));
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
                sw_request.Start();
                while (true)
                {

                    //bool tt = true;
                    while (need_request)
                    {
                        // Console.WriteLine(sw.ElapsedMilliseconds);
                        if (Dutyara.opened)
                        {
                            sw_timeout.Restart();
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
                            if (sw_timeout.ElapsedMilliseconds > time_to_dut_read)
                            {
                                message_status = MSG_FAIL;
                            }

                            switch (message_status)
                            {
                                case MSG_FAIL:
                                    dut_list[dut_selected].msg_cont.id = dut_list[dut_selected].Id.ToString();
                                    dut_list[dut_selected].msg_cont.water = FAIL_VALUE;
                                    dut_list[dut_selected].msg_cont.fuel = FAIL_VALUE;
                                    dut_list[dut_selected].msg_cont.temp = FAIL_VALUE;
                                    //dut_data = "44N0=65536=65536=65536=094";
                                    break;
                                case MSG_DROP:
                                    dut_list[dut_selected].msg_cont.id = dut_list[dut_selected].Id.ToString();
                                    dut_list[dut_selected].msg_cont.water = DROP_VALUE;
                                    dut_list[dut_selected].msg_cont.fuel = DROP_VALUE;
                                    dut_list[dut_selected].msg_cont.temp = DROP_VALUE;
                                    break;
                                default:
                                    break;
                            }

                            if (message_status != null)
                            {
                                //if (message_status != MSG_SUCCESS)
                                    dut_data = dut_list[dut_selected].msg_cont.id + "N0=" + dut_list[dut_selected].msg_cont.temp + "=" +
                                                    dut_list[dut_selected].msg_cont.fuel + "=" + dut_list[dut_selected].msg_cont.water + "=094"; //"44N0=65536=65536=65536=094";
                                Console.WriteLine(dut_data);

                                GoToNextDut();
                            }
                            else dut_data = dut_list[dut_selected].GetData();
                            Thread.Sleep(500);
                        }

                        


                        // Если таймер больше Х то меняется номер дута и открываем opened 
                        //          Если у нового дута другая скорость меняем текущую скорость


                    }
                    if (sw_request.ElapsedMilliseconds > request_time)
                    {
                        need_request = true;
                        sw_request.Restart();
                    }
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
                // dut_id = dut_id.Substring(1, dut_id.Length-2);
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
                // Если айдишник отличается от запрашиваемого - данные считаются битыми, т.к. пришли от другого ДУТа
                var idish = dut_list[dut_selected].Id.ToString();
                if (dut_id != idish)
                {
                    message_status = MSG_DROP; Console.WriteLine("Err!"); return;
                    // Альтернативный способ решения: 
                    //message_status = null; return;


                    //int irr = 0;
                    //while (irr <= 25)
                    //{
                    //Console.WriteLine("Err!");
                    //    irr++;
                    //}
                }
                // ХЗ что хз зачем, но вдроуг пригодится
                //bb = float.TryParse(dut_data_arr[4].Replace(".", ","), out i);
                //if (!bb)
                //{
                //    message_status = MSG_DROP; return;
                //}

                message_status = MSG_SUCCESS;
                dut_list[dut_selected].msg_cont.id = dut_id;
                dut_list[dut_selected].msg_cont.fuel = ViaDataFormater.CorrectoinNull(dut_data_arr[2], dut_list[dut_selected].Corrector);
                dut_list[dut_selected].msg_cont.water = ViaDataFormater.CorrectoinNull(dut_data_arr[3], dut_list[dut_selected].Corrector);
                dut_list[dut_selected].msg_cont.temp = dut_data_arr[1];

            }
            return;
        }

        // Перейти к опросу следующего ДУТа 
        static void GoToNextDut()
        {
            Dutyara.need_a_stop = true;
            Dutyara.opened = true;
            dut_data = "";
            dut_selected = (dut_selected + 1);
            if (dut_selected >= dut_list.Count())
            {
                // need_request = false;
                dut_selected %= dut_list.Count();
                SendToVialon();
                need_request = false;
            }
            message_status = null;
        }

        private static void SendToVialon()
        {
            string params_string = String.Empty;
            int dl_len = dut_list.Count();
            Dutyara counter;
            for (int iterator = 0; iterator < dl_len; iterator++)
            {
                counter = dut_list[iterator];
                params_string += ViaDataFormater.GenerateString(counter, iterator);
            }
            params_string = params_string.Remove(params_string.Length-1);

            Console.WriteLine(params_string);
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
                    sw_timeout.Stop();
                    Console.WriteLine(sw_timeout.ElapsedMilliseconds);
                }
            }
        }
    }
}
