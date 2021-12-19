using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Security.Cryptography;

namespace InteractiveFallout4.Common.Crypting
{
    class Crypting
    {
        public static string WriteEncryptString(string SourceString)
        {
            //Слздаем объект DESCryptoServiceProvider для дальнейшего кодирования строки
            DESCryptoServiceProvider Crypter = new DESCryptoServiceProvider();

            //Строка которая будет кодироватся записывается в массив байтов
            byte[] data = ASCIIEncoding.Default.GetBytes(SourceString);

            //Задаем ключ кодирования и алгоритм шифрования
            Crypter.Key = ASCIIEncoding.UTF8.GetBytes("ABCDEFGH");
            Crypter.IV = ASCIIEncoding.UTF8.GetBytes("ABCDEFGH");

            //Создаем объект MemoryStream для записи в него заштфровонного потока
            MemoryStream Stream = new MemoryStream();
            //Создаем шифровальный поток через который будет выполнятся запись не зашифрованного потока
            CryptoStream CryptedStream = new CryptoStream(Stream, Crypter.CreateEncryptor(), CryptoStreamMode.Write);

            //записываем исходный массив байтов в зашиврованный поток, через него массив байтов будет записан в исходный поток
            CryptedStream.Write(data, 0, data.Length);
            //Обновляет базовый источник данных или хранилище текущим содержимым буфера, а затем очищает буфер.
            CryptedStream.FlushFinalBlock();

            //Создаем объект StringBuilder для записи в него зашифрованного потока
            StringBuilder CryptedString = new StringBuilder();

            //Записываем получившийся в результате шифрования массив байтов в строку с форматированием байтов в формате "{0:X2}" 4D,8A и т.д.
            foreach (byte b in Stream.ToArray())
            {
                CryptedString.AppendFormat("{0:X2}", b);
            }

            //Закрываем потоки исходный и шифровальный?
            CryptedStream.Close();
            Stream.Close();

            //Возвращаем зашифрованную строку
            return CryptedString.ToString();
        }
        //Декодирование строки
        public static string WriteDecryptString(string SourceString)
        {
            try
            {
                //Слздаем объект DESCryptoServiceProvider для дальнейшего кодирования строки
                DESCryptoServiceProvider Decryptor = new DESCryptoServiceProvider();

                //Создаем байтовый массив с размером в половину короче исходной строки(т.к. в каждый элемент байтового массива будет преобрпазовано по два элемента исходной строки)
                byte[] inputByteArray = new byte[SourceString.Length / 2];

                //Преобразуем исходную запись строки в байтовый массив, принимая из строки по 2 символа
                for (int x = 0; x < SourceString.Length / 2; x++)
                {
                    int i = (Convert.ToInt32(SourceString.Substring(x * 2, 2), 16));//Конвертируем каждые 2 элемента строки в целочисленное число с использованием основания 16
                    inputByteArray[x] = (byte)i;//конвертируем целочисленное в элемет байтового массива
                }

                //Задаем ключ кодирования и алгоритм шифрования
                Decryptor.Key = ASCIIEncoding.ASCII.GetBytes("ABCDEFGH");
                Decryptor.IV = ASCIIEncoding.ASCII.GetBytes("ABCDEFGH");

                //Создаем объект MemoryStream для записи в него расшифровонного потока
                MemoryStream ms = new MemoryStream();
                //Создаем дешифровальный поток через который будет выполнятся запись не зашифрованного потока
                CryptoStream cs = new CryptoStream(ms, Decryptor.CreateDecryptor(), CryptoStreamMode.Write);

                //записываем исходный массив байтов в дешефровальный поток, через него массив байтов будет записан в исходный поток
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                //Обновляет базовый источник данных или хранилище текущим содержимым буфера, а затем очищает буфер.
                cs.FlushFinalBlock();

                return System.Text.Encoding.Default.GetString(ms.ToArray());
            }
            catch//Если не получается прочитать зашифрованную строку то возвращаем результат расшифровки пустой зашифрованной строки ""
            {
                return WriteDecryptString(WriteEncryptString(""));
            }
        }
    }
}
