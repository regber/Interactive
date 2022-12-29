using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InteractiveFallout4.Common.Conductors
{
    class Fallou4Conductor
    {
        // Выделенная именованная память
        public static MemoryMappedFile SimpleConsoleCommand;

        public static MemoryMappedFile RequestGlobalValue;
        public static MemoryMappedFile ResponseGlobalValue;

        static Fallou4Conductor()
        {
            // Создаст, или подключится к уже созданной памяти с таким именем
            SimpleConsoleCommand = MemoryMappedFile.CreateOrOpen("Fallout4SimpleConsoleCommand", 512, MemoryMappedFileAccess.ReadWrite);

            RequestGlobalValue = MemoryMappedFile.CreateOrOpen("Fallout4RequestGlobalValue", 512, MemoryMappedFileAccess.ReadWrite);
            ResponseGlobalValue = MemoryMappedFile.CreateOrOpen("Fallout4ResponseGlobalValue", 512, MemoryMappedFileAccess.ReadWrite);
        }

        public static void SendSimpleConsoleCommand(string consoleCommand)
        {
            SimpleConsoleCommand.CreateViewStream().Write(Encoding.Default.GetBytes(consoleCommand + '\0'), 0, consoleCommand.Length + 1);
        }

        public static string GetGlobalValue(string variableName)
        {
            RequestGlobalValue.CreateViewStream().Write(Encoding.Default.GetBytes("getglobalvalue "+variableName + '\0'), 0, ("getglobalvalue " + variableName).Length + 1);

            string responseGlobalValue = "" + '\0';

            //Выполняем до тех пор пока не получим ответ
            while (responseGlobalValue[0] == '\0')
            {
                var stream = ResponseGlobalValue.CreateViewStream();

                byte[] responseGlobalValueByteArray = new byte[stream.Length];

                stream.Read(responseGlobalValueByteArray, 0, responseGlobalValueByteArray.Length);

                responseGlobalValue = Encoding.Default.GetString(responseGlobalValueByteArray).Replace('\n', '\0');
            }

            //Очищаем поток с ответом
            ResponseGlobalValue.CreateViewStream().Write(Encoding.Default.GetBytes("" + '\0'), 0, 1);


            //если сообщение содержит ошибку
            if (responseGlobalValue.StartsWith("error"))
            {
                return "error";
            }
            else
            {
                return GetFloatNumber(responseGlobalValue);
            }
        }

        private static string GetFloatNumber(string message)
        {
            return Regex.Match(message, @"\d[\d|.]*[\d|.]").Value;
        }
    }
}
