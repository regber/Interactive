using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InteractiveFallout4.Common.Conductors;

namespace InteractiveFallout4.MainWindow.ConsoleProcessor
{
    class ConsoleProcessor
    {
        public static bool ChechValueConcur(string variableName,float comparableValue)
        {
            var value = GetGlobalValue(variableName);

            return float.Parse(value.Replace('.',',')) == comparableValue;
        }

        public static void WriteValue(string variableName, float value)
        {
            Fallou4Conductor.SendSimpleConsoleCommand("set "+ variableName+" to "+ value);
        }

        public static void WriteAddValue(string variableName, float addedValue)
        {
            var value = GetGlobalValue(variableName);

            var valueSumm = float.Parse(value) + addedValue;

            Fallou4Conductor.SendSimpleConsoleCommand("set " + variableName + " to " + valueSumm);
        }

        public static void WriteMultipleValue(string variableName, float multipliedValue)
        {
            var value = GetGlobalValue(variableName);

            var valueMultipled = float.Parse(value) * multipliedValue;

            Fallou4Conductor.SendSimpleConsoleCommand("set " + variableName + " to " + valueMultipled);
        }

        public static void WriteDevideValue(string variableName, float dividetValue)
        {
            var value = GetGlobalValue(variableName);

            var valueDivided = float.Parse(value) / dividetValue;

            Fallou4Conductor.SendSimpleConsoleCommand("set " + variableName + " to " + valueDivided);
        }


        private static string GetGlobalValue(string variableName)
        {
            string value = "error";

            while (value == "error")
            {
                value = Fallou4Conductor.GetGlobalValue(variableName);
            }

            return value;
        }
    }
}
