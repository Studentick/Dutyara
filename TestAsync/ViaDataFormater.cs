using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAsync
{
    class ViaDataFormater
    {
        public static string GenerateString(Dutyara dut, int iter)
        {
            string rtn = "id-" + (iter + 1) + ":3:" + dut.Id + ",fuel:2:" + FormatFluid(dut.msg_cont.fuel)
                + ",water:2:" + FormatFluid(dut.msg_cont.water) + ",temp:2:" + FormatTemp(dut.msg_cont.temp) + ",";
            return rtn;
        }

        // Преобразовываем данные (переносим запятую влево)
        private static string FormatTemp(string input)
        {
            if (input == "65536" || input == "65533")
                return input;
            float float_box = float.Parse(input.Replace('.', ',')) / 10;
            string output = Convert.ToString(float_box).Replace(',', '.');
            return output;
        }

        // Преобразование данных для жидкостей (переносим запятую вправо)
        private static string FormatFluid(string input, float corrector = 0)
        {
            float float_box = float.Parse(input.Replace('.', ','));
            float_box += corrector;
            string output = Convert.ToString(float_box).Replace(',', '.');
            return output;
        }


        static public string CorrectoinNull(string input, float corrector)
        {
            string output = FormatFluid(input, corrector);
            return output;
        }

        
    }
}
