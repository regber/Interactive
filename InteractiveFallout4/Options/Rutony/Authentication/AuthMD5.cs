using System;
using System.Text;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace InteractiveFallout4.Options.Rutony
{
    class AuthMD5
    {

        public static bool CheckMD5Code(string md5Code)
        {
            int Days=-1;
            DateTime ActiveForDate = new DateTime();

            return CheckMD5Code(md5Code, ref Days, ref ActiveForDate);
        }

        public static bool CheckMD5Code(string md5Code, ref int days,ref DateTime activeForDate)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            string pass;
            byte[] checkSum;
            string result;
            bool check = false;
            //Текущая дата
            DateTime currentDateTime;
            currentDateTime = GetCurrentDateTime();

            //Проверяем на месяц
            if (check == false)
            {
                for (int i = -30; i < 0; i++)
                {
                    pass = "30" + currentDateTime.AddDays(i).ToShortDateString() + "мартышлюха";
                    checkSum = md5.ComputeHash(Encoding.UTF8.GetBytes(pass));
                    result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                    if (md5Code.ToUpper() == result.ToUpper())
                    {
                        check = true;
                        days = 30;
                        activeForDate = currentDateTime.AddDays(i + 30);
                        //verificatedDateTo = "30 дневный ключ, активен до: " + currentDateTime.AddDays(i + 30).ToShortDateString();
                        break;
                    }
                    else
                    {
                        //verificatedDateTo = "Ключ еще или уже не активен";
                    }
                }
            }
            //Проверяем на два месяца
            if (check == false)
            {
                for (int i = -60; i < 0; i++)
                {
                    pass = "60" + currentDateTime.AddDays(i).ToShortDateString() + "мартышлюха";
                    checkSum = md5.ComputeHash(Encoding.UTF8.GetBytes(pass));
                    result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                    if (md5Code.ToUpper() == result.ToUpper())
                    {
                        check = true;
                        days = 60;
                        activeForDate = currentDateTime.AddDays(i + 60);
                        //verificatedDateTo = "60 дневный ключ, активен до: " + currentDateTime.AddDays(i + 60).ToShortDateString();
                        break;
                    }
                    else
                    {
                        //verificatedDateTo = "Ключ еще или уже не активен";
                    }
                }
            }
            //Проверяем на три месяца
            if (check == false)
            {
                for (int i = -90; i < 0; i++)
                {
                    pass = "90" + currentDateTime.AddDays(i).ToShortDateString() + "мартышлюха";
                    checkSum = md5.ComputeHash(Encoding.UTF8.GetBytes(pass));
                    result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                    if (md5Code.ToUpper() == result.ToUpper())
                    {
                        check = true;
                        days = 90;
                        activeForDate = currentDateTime.AddDays(i + 90);
                        //verificatedDateTo = "90 дневный ключ, активен до: " + currentDateTime.AddDays(i + 90).ToShortDateString();
                        break;
                    }
                    else
                    {
                        //verificatedDateTo = "Ключ еще или уже не активен";
                    }
                }
            }
            //Проверяем на шесть месяца
            if (check == false)
            {
                for (int i = -180; i < 0; i++)
                {
                    pass = "180" + currentDateTime.AddDays(i).ToShortDateString() + "мартышлюха";
                    checkSum = md5.ComputeHash(Encoding.UTF8.GetBytes(pass));
                    result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                    if (md5Code.ToUpper() == result.ToUpper())
                    {
                        check = true;
                        days = 180;
                        activeForDate = currentDateTime.AddDays(i + 180);
                        //verificatedDateTo = "180 дневный ключ, активен до: " + currentDateTime.AddDays(i + 180).ToShortDateString();
                        break;
                    }
                    else
                    {
                        //verificatedDateTo = "Ключ еще или уже не активен";
                    }
                }
            }
            return check;
        }

        private static DateTime GetCurrentDateTime()
        {
            DateTime dateTime = DateTime.MinValue;
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create("http://www.microsoft.com");
            request.Method = "GET";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string todaysDates = response.Headers["date"];

                dateTime = DateTime.ParseExact(todaysDates, "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                    System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.AssumeUniversal);
            }
            request.Abort();
            response.Close();

            return dateTime;
        }
    }
}
