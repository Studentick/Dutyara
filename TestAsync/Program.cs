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
        static int time_to_dut_read = 3000;
        int


        static void Main(string[] args)
        {
            dut_list.Add(new Dutyara(1, 44));
            dut_list.Add(new Dutyara(2, 44));
            DutControl();
            while(true)
            {
                Console.WriteLine(Console.ReadLine());
            }
            Console.ReadKey();
        }

        async static void DutControl()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    Console.WriteLine(sw.ElapsedMilliseconds);
                    if (Dutyara.opened)
                    {
                        sw.Restart();
                        Dutyara.opened = false;
                        GetAnsver();
                    }
                    else

                    //if (!Dutyara.opened)
                    {
                        if (dut_data != "")
                        {
                            CheckData();
                        }
                        if (sw.ElapsedMilliseconds > time_to_dut_read)
                        {
                            Dutyara.need_a_stop = true;
                            //Thread.Sleep(50);
                            Dutyara.opened = true;
                            if (dut_data == "")
                            {
                                string ans = (dut_selected + 1) + "N0=65536=65536=65536=094";
                                Console.WriteLine("Error from " + (dut_selected + 1) + ": " + ans);
                            }
                            else
                                Console.WriteLine("Dut[" + (dut_selected + 1) + "] return answer: " + dut_data);
                            dut_data = "";
                            dut_selected = (dut_selected + 1) % dut_list.Count();
                        }
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
        private static int CheckData()
        {
            throw new NotImplementedException();
        }

        // Перейти к опросу следующего ДУТа 
        static void GoToNextDut()
        {
            Dutyara.need_a_stop = true;
            Dutyara.opened = true;
            dut_data = "";
            dut_selected = (dut_selected + 1) % dut_list.Count();

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
