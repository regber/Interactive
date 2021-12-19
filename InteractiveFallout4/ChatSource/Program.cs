using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using System.Drawing;
using System.Xml.Linq;
using System.IO;

using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;

using Quobject.SocketIoClientDotNet.Client;

using WebSocket4Net;

using System.Collections.Specialized;
using System.Net.Http;



namespace ChatSource
{
    class Program
    {
        private static void Main(string[] args)
        {

        }
    }
    public class GoodGameChat
    {
        //Событие для отработки сообщений чата
        public delegate void Message(object sender, Newtonsoft.Json.Linq.JObject message);
        public event Message MessageEvent;

        //Переменные для работы с HttpWeb через POST и GET запросы
        //информация из кукесов ответа Goodgame.ru и для кукесов запроса
        private string cookie_Domain = null;
        private string cookie_IdBad = null;
        private string cookie_TokenBad = null;
        //Регулярные выражения для выдергивания необходимой информации из кукесов 
        private Regex Regex_Cookie_Domain = new Regex(@"domain=\D*;");
        private Regex Regex_Cookie_IdBad = new Regex(@"id@bad=\d*;");
        private Regex Regex_Cookie_TokenBad = new Regex(@"token@bad=\S*;");
        private Regex Regex_Followers_Users = new Regex(@"<a href=""/user/\d*"" target=""[\S]*"">[\S]*</a>");
        private Regex Regex_Followers_User = new Regex(@">[\S]*<");
        private List<string> followersUsersList = new List<string>();
        private bool authAccount = false;
        //Таймер пингующий канал для получения списка вновь подписавшихся пользователей
        System.Windows.Forms.Timer followersTimer;


        //Переменные для работы с API чата GoodGame
        private string channelID;
        private string userID;
        private string userToken;
        private string innerChannelName;
        private string innerUserName;
        private string innerPassword;
        System.Net.CookieContainer countCookies = new System.Net.CookieContainer();

        //Создаем сокет с указанным адресом сервера чата
        //WebSocketSharp.WebSocket socket = new WebSocketSharp.WebSocket(@"ws://chat.goodgame.ru:8081/chat/websocket");
        WebSocketSharp.WebSocket socket = new WebSocketSharp.WebSocket(@"wss://chat-1.goodgame.ru/chat2/");
        
        //Конструктор класса служит для передачи ссылки на externalTextBox
        /* public GoodGameChat(TextBox externalJObjectTextBox)
         {
             internalJObjectTextBox = externalJObjectTextBox;
         }*/

        //Подключение к конкретному каналу ГГ чата
        public void Connect(string channelName, string userName, string Password)
        {
            innerChannelName = channelName;
            innerUserName = userName;
            innerPassword = Password;
            authAccount = false;

            //Задаем тип протокола(добавлено дополнительно позже)
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            //GG

            //System.Console.WriteLine("{\"type\": \"join\", \"data\": { \"channel_id\": \"18865\", \"hidden\": false } }");
            //Получение параметров аутентификации в API ГГ
            getUserIDAndToken(userName, Password);
            getChannelID(channelName);
            //Получение первичного списка подписчиков
            //getChannelSubscribers(channelName,userName, Password);
            //Подключение к серверу ГГ согласно вышеуказанному адресу сервера
            socket.Connect();
            //Авторизация на сервере ГГ
            socket.Send("{\"type\": \"auth\", \"data\": { \"user_id\": \"" + userID + "\", \"token\": \"" + userToken + "\" } }");
            //Отправление сообщения серверу для подключения к конкретному каналу
            socket.Send("{\"type\": \"join\", \"data\": { \"channel_id\": \"" + channelID + "\", \"hidden\": false } }");
            //Отправляем сообщение в указанный TextBox о подключении к чату
            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"GoodGame\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + channelName + "\"" + "," + "\"text\":\"Чат GoodGame подключен к каналу \"" + "}"));
            //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":\"GoodGame\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + channelName + "\"" + "," + "\"text\":\"Чат GoodGame подключен к каналу \"" + "}" + Environment.NewLine); }));
            //Привязываем сорбытие о приходе сообщения
            socket.OnMessage += Socket_OnMessage;

            //Задаем параметры таймера
            followersTimer = new System.Windows.Forms.Timer();
            followersTimer.Interval = 5000;
            followersTimer.Start();
            followersTimer.Tick += FollowersTimer_Tick;
        }

        //Отключение сокета от сервера(и канала очевидно)
        public void Disconnect()
        {
            socket.OnMessage -= Socket_OnMessage;
            socket.Close();
            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"GoodGame\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":\"Чат GoodGame отключен от канала \"" + "}"));
            //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":\"GoodGame\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":\"Чат GoodGame отключен от канала \"" + "}" + Environment.NewLine); }));
            followersTimer.Tick -= FollowersTimer_Tick;
            followersTimer.Stop();
        }
        //События при приходе сообщения из Чата ГГ 
        private void Socket_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            string rawString = e.Data;
            Newtonsoft.Json.Linq.JObject rawJObject;
            string usersListString;

            try
            {
                rawString = rawString.Substring(0, 1).Replace("{", "{" + "\"MessageSource\":\"GoodGame\"" + ",") + rawString.Remove(0, 1);
                rawJObject = Newtonsoft.Json.Linq.JObject.Parse(rawString);

                if ((string)rawJObject["type"] == "message")
                {
                    if (rawJObject.ToString().Replace("\v", "").Replace("\t", "").Replace("\n", "").Replace(" ", "").Contains(@"""private"":1,") == true)
                    {
                        MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":" + "\"" + rawJObject["MessageSource"] + "\"" + "," + "\"type\":" + "\"private_message\"" + "," + "\"username\":" + "\"" + rawJObject["data"]["user"]["nickname"] + "\"" + "," + "\"tousername\":" + "\"" + rawJObject["data"]["to"]["nickname"] + "\"" + "," + "\"text\":" + "\"" + rawJObject["data"]["text"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}"));
                        //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":" + "\"" + rawJObject["MessageSource"] + "\"" + "," + "\"type\":" + "\"private_message\"" + "," + "\"username\":" + "\"" + rawJObject["data"]["user"]["nickname"] + "\"" + "," + "\"tousername\":" + "\"" + rawJObject["data"]["to"]["nickname"] + "\"" + "," + "\"text\":" + "\"" + rawJObject["data"]["text"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}" + Environment.NewLine); }));
                    }
                    else
                    {
                        MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":" + "\"" + rawJObject["MessageSource"] + "\"" + "," + "\"type\":" + "\"message\"" + "," + "\"username\":" + "\"" + rawJObject["data"]["user_name"] + "\"" + "," + "\"text\":" + "\"" + rawJObject["data"]["text"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}"));
                        //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":" + "\"" + rawJObject["MessageSource"] + "\"" + "," + "\"type\":" + "\"message\"" + "," + "\"username\":" + "\"" + rawJObject["data"]["user_name"] + "\"" + "," + "\"text\":" + "\"" + rawJObject["data"]["text"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}" + Environment.NewLine); }));
                    }
                }
                if ((string)rawJObject["type"] == "users_list")
                {
                    usersListString = "{" + "\"MessageSource\":" + "\"" + rawJObject["MessageSource"] + "\"" + "," + "\"type\":\"users_list\"" + "," + "\"users\":[";

                    foreach (var viewers in rawJObject["data"]["users"])
                    {
                        usersListString += "{\"username\":" + "\"" + viewers["name"] + "\"" + "},";
                    }
                    if (usersListString[usersListString.Length - 1] != '[')
                    {
                        usersListString = usersListString.Remove(usersListString.Length - 1);
                    }
                    usersListString += "]}";

                    MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse(usersListString));
                    //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText(usersListString + Environment.NewLine); }));
                }
                if ((string)rawJObject["type"] == "payment")
                {
                    //MessageBox.Show(rawJObject.ToString());////Сообщение для отлова ошибки
                    MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":" + "\"" + rawJObject["MessageSource"] + "\"" + "," + "\"type\":\"payment\"" + "," + "\"username\":" + "\"" + rawJObject["data"]["userName"] + "\"" + "," + "\"amount\":" + "\"" + float.Parse(rawJObject["data"]["amount"].ToString().Replace('.', ',')) + "\"" + "," + "\"text\":" + "\"" + rawJObject["data"]["message"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}"));
                    //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":" + "\"" + rawJObject["MessageSource"] + "\"" + "," + "\"type\":\"payment\"" + "," + "\"username\":" + "\"" + rawJObject["data"]["userName"] + "\"" + "," + "\"amount\":" + "\"" + float.Parse(rawJObject["data"]["amount"].ToString().Replace('.', ',')) + "\"" + "," + "\"text\":" + "\"" + rawJObject["data"]["message"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}" + Environment.NewLine); }));
                }
                if ((string)rawJObject["type"] == "premium")
                {
                    //MessageBox.Show(rawJObject.ToString());////Сообщение для отлова ошибки
                    MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":" + "\"" + rawJObject["MessageSource"] + "\"" + "," + "\"type\":\"new_subscriber\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":\"На канале новый премиум подписчик:\"," + "\"username\":\"" + rawJObject["data"]["userName"] + "\"" + "}"));
                    //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":" + "\"" + rawJObject["MessageSource"] + "\"" + "," + "\"type\":\"new_subscriber\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":\"На канале новый премиум подписчик:\"," + "\"username\":\"" + rawJObject["data"]["userName"] + "\"" + "}" + Environment.NewLine); }));

                }
            }
            catch
            {
                rawString = "{" + "\"MessageSource\":\"GoodGame\"" + "," + "\"type\":\"channel_error_message\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":" + "\"" + rawString.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}";
                MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse(rawString));
                //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText(rawString.ToString() + Environment.NewLine); }));
            }

        }
        //Получить список зрителей из канала
        public void getChannelUsersList()
        {
            socket.Send("{\"type\": \"get_users_list2\", \"data\": { \"channel_id\": \"" + channelID + "\" } }");
        }
        //Отправить сообщение в чат
        public void sendMessage(string text)
        {
            socket.Send("{\"type\": \"send_message\", \"data\": { \"channel_id\": \"" + channelID + "\"" + "," + "\"text\": \"" + text.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "," + "\"hideIcon\": \"false\"," + "\"mobile\": \"false\"" + "}}");
        }
        //Отправить ЛС
        public void sendPrivateMessage(string userName, string text)
        {
            //Отчегото не работает так как надо//socket.Send("{\"type\": \"send_private_message\", \"data\": { \"channel_id\": \"" + channelID + "\"" + "," + "\"user_id\": \"" + userID + "\"" + "," + "\"username\": \"" + userName + "\"" + "," + "\"text\": \"" + text + "\"" + "}}");
            /*
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create("https://goodgame.ru/ajax/chatlogin/?login="+ innerUserName + "&password="+ innerPassword);
            request.CookieContainer = new System.Net.CookieContainer();

            System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
            System.Net.CookieContainer cont = new System.Net.CookieContainer();
            System.Uri uriSend = new Uri("https://goodgame.ru/ajax/dialogs/send");
            cont.Add(uriSend, response.Cookies);
            */

            try
            {
                sendPrivateMessageTextEncoding(userName, text);
            }
            catch
            {
                //Получение кукисов
                System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create("https://goodgame.ru/ajax/chatlogin/");
                req.Method = "POST";
                req.Timeout = 100000;
                req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                byte[] sentData = Encoding.GetEncoding(1251).GetBytes("login=" + innerUserName + "&" + "password=" + innerPassword);
                req.ContentLength = sentData.Length;
                System.IO.Stream sendStream = req.GetRequestStream();
                sendStream.Write(sentData, 0, sentData.Length);
                sendStream.Close();
                req.CookieContainer = new System.Net.CookieContainer();
                System.Net.HttpWebResponse respons = (System.Net.HttpWebResponse)req.GetResponse();
                System.IO.Stream ReceiveStream = respons.GetResponseStream();
                System.IO.StreamReader sr = new System.IO.StreamReader(ReceiveStream, Encoding.UTF8);
                String Out = sr.ReadToEnd();
                sr.Close();

                System.Uri uriSend = new Uri("https://goodgame.ru/ajax/dialogs/send");
                countCookies = new System.Net.CookieContainer();
                countCookies.Add(uriSend, respons.Cookies);
                sendPrivateMessage(userName, text);
            }


        }
        //Декодирование текста приватного сообщения (для решения проблем с кирилицей)
        private void sendPrivateMessageTextEncoding(string userName, string text)
        {
            //перекодирование текста и его отправка
            var sourceEncoding = Encoding.GetEncoding(1251);
            var resultEncoding = Encoding.GetEncoding("windows-1251");
            byte[] sourceBytes = Encoding.UTF8.GetBytes(text);
            byte[] resultBytes = Encoding.Convert(sourceEncoding, resultEncoding, sourceBytes);
            var result = resultEncoding.GetString(resultBytes);

            System.Uri uriSend = new Uri("https://goodgame.ru/ajax/dialogs/send");
            string Data1 = "nickname=" + userName + "&" + "text=" + result;
            System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(uriSend);
            req.CookieContainer = countCookies;
            req.Method = "POST";
            req.Timeout = 100000;
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            byte[] sentData = Encoding.GetEncoding(1251).GetBytes(Data1);
            req.ContentLength = sentData.Length;
            System.IO.Stream sendStream = req.GetRequestStream();
            sendStream.Write(sentData, 0, sentData.Length);
            sendStream.Close();
            System.Net.HttpWebResponse respons = (System.Net.HttpWebResponse)req.GetResponse();

            System.IO.Stream ReceiveStream = respons.GetResponseStream();
            System.IO.StreamReader sr = new System.IO.StreamReader(ReceiveStream, Encoding.UTF8);
            String Out = sr.ReadToEnd();
            sr.Close();
        }

        //Методы для авторизации и определения ID канала по нику стримера
        private void getUserIDAndToken(string userName, string Password)
        {
            string URI = @"https://goodgame.ru/ajax/chatlogin";
            string Parameters = "login=" + userName + "&" + "password=" + Password;
            System.Net.WebRequest req = System.Net.WebRequest.Create(URI);
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(Parameters);
            req.ContentLength = bytes.Length;
            System.IO.Stream os = req.GetRequestStream(); // создаем поток 
            os.Write(bytes, 0, bytes.Length); // отправляем в сокет 
            os.Close();
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            var rawMessage = Newtonsoft.Json.Linq.JObject.Parse(sr.ReadToEnd().Trim());
            userID = (string)rawMessage["user_id"];
            userToken = (string)rawMessage["token"];

            //Проверка правильности логиа и пароля что бы при проверке подписчиков не вываливалась рекапча, т.к. при попытке отправмить не правильный логин или пароль методом POST запрашивает рекапчу котору можно проверить только при перезаходе под своим логином на сайте GG, таким обрпазом легче избежать данной ситуации.
            if ((bool)rawMessage["result"])
            {
                authAccount = true;
            }
            //MessageBox.Show((string)rawMessage["user_id"]+" / "+ (string)rawMessage["token"]);
        }
        private void getChannelID(string StrimerName)
        {
            string URI = @"http://goodgame.ru/api/getchannelstatus?id=" + StrimerName + "&fmt=json";

            System.Net.WebRequest req = System.Net.WebRequest.Create(URI);
            req.Method = "POST";
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            var rawMessage = Newtonsoft.Json.Linq.JObject.Parse(sr.ReadToEnd().Trim());

            string rawString = new Regex(@"(\d)+").Match(new Regex(@"""+(\d)+""+:").Match(rawMessage.ToString()).ToString()).ToString();

            //userID = (string)rawMessage["user_id"];
            //userToken = (string)rawMessage["token"];

            channelID = rawString;
        }

        private void getHttpCookiesAuthParametrs(string userName, string Password)
        {
            //Создаем и настраиваем запрос для получения кукесов для дальнейшей аутентификации на https://goodgame.ru/channel/"+ channalName + "/subscriptions/
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://goodgame.ru/api/4/login/password");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Method = "POST";

            //заполнение JSON строки запроса
            using (var streamWriter = new System.IO.StreamWriter(httpWebRequest.GetRequestStream()))
            {
                //Формируем JSON строку с параметрами запроса(httpWebRequest)
                string json = "{\"username\":" + "\"" + userName + "\"," + "\"password\":" + "\"" + Password + "\"}";

                //Записываем JSON строку с параметрами в запрос(httpWebRequest)
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            System.IO.StreamReader responseStream = new System.IO.StreamReader(httpResponse.GetResponseStream());
            //Получение информации из хэдера в котором в том числе находится информация о id@bad=3089485; и token@bad=%24sQ.TNJnxZs577BRRTWMUIOruHRCAgViA.qSUBGBysjNSMIQsIcrtG;
            for (int i = 0; i < httpResponse.Headers.GetValues("set-cookie").Count(); i++)
            {
                //Выбираем домен из кукесов
                if (Regex_Cookie_Domain.IsMatch(httpResponse.Headers.GetValues("set-cookie")[i].ToString()))
                {
                    cookie_Domain = Regex_Cookie_Domain.Match(httpResponse.Headers.GetValues("set-cookie")[i].ToString()).Value;
                    cookie_Domain = cookie_Domain.Remove(0, 7).TrimEnd(';');
                }
                //Выбираем id@bad из кукесов
                if (Regex_Cookie_IdBad.IsMatch(httpResponse.Headers.GetValues("set-cookie")[i].ToString()))
                {
                    cookie_IdBad = Regex_Cookie_IdBad.Match(httpResponse.Headers.GetValues("set-cookie")[i].ToString()).Value;
                    cookie_IdBad = cookie_IdBad.Remove(0, 7).TrimEnd(';');
                }
                //Выбираем token@bad из кукесов
                if (Regex_Cookie_TokenBad.IsMatch(httpResponse.Headers.GetValues("set-cookie")[i].ToString()))
                {
                    cookie_TokenBad = Regex_Cookie_TokenBad.Match(httpResponse.Headers.GetValues("set-cookie")[i].ToString()).Value;
                    cookie_TokenBad = cookie_TokenBad.Remove(0, 10).TrimEnd(';');
                }
            }
            //var rawMessage = Newtonsoft.Json.Linq.JObject.Parse(responseStream.ReadToEnd().Trim());
        }

        private void getChannelFollowers(string channelName, string userName, string Password)
        {
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpResponse = null;
            System.IO.StreamReader responseStream = null;

            List<string> newFollowersUsersList = new List<string>();
            List<string> tempFollowersUsersList = new List<string>();
            List<string> newUsers;
            MatchCollection matches;

            try
            {
                //Если стартовый список пользователей пуст то заполняем его текущими подписанными пользователями
                if (followersUsersList.LongCount() == 0)
                {
                    /////Формирование и отправка запроса о фоловерах/////
                    httpWebRequest = (HttpWebRequest)WebRequest.Create("https://goodgame.ru/channel/" + channelName + "/subscriptions/");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";


                    //Заполняем кукесы запроса
                    httpWebRequest.Headers.Add("Cookie", "id@bad="+ cookie_IdBad+";"+"token@bad="+ cookie_TokenBad);
                    //httpWebRequest.CookieContainer = new CookieContainer();
                    //httpWebRequest.CookieContainer.Add(new Cookie("id@bad", cookie_IdBad, "/", cookie_Domain));
                    //httpWebRequest.CookieContainer.Add(new Cookie("token@bad", cookie_TokenBad, "/", cookie_Domain));

                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    responseStream = new System.IO.StreamReader(httpResponse.GetResponseStream());
                    //Собираем колекцию подписанных пользователей
                    matches = Regex_Followers_Users.Matches(responseStream.ReadToEnd().ToString());

                    //Заполняем список исходных подписанных пользователей
                    foreach (Match user in matches)
                    {
                        followersUsersList.Add(Regex_Followers_User.Match(user.Value).Value.TrimEnd('<').TrimStart('>'));
                    }
                }
                else//Если список уже заполнен то выводим сообщение о новых подписчиках и обновляем стартовый список новыми пользователями
                {
                    /***Получение информации необходимой для формирования запроса о фоловерах***/

                    /////Формирование и отправка запроса о фоловерах/////
                    httpWebRequest = (HttpWebRequest)WebRequest.Create("https://goodgame.ru/channel/" + channelName + "/subscriptions/");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";

                    //Заполняем кукесы запроса
                    httpWebRequest.Headers.Add("Cookie", "id@bad=" + cookie_IdBad + ";" + "token@bad=" + cookie_TokenBad);
                    //httpWebRequest.CookieContainer = new CookieContainer();
                    //httpWebRequest.CookieContainer.Add(new Cookie("id@bad", cookie_IdBad, "/", cookie_Domain));
                    //httpWebRequest.CookieContainer.Add(new Cookie("token@bad", cookie_TokenBad, "/", cookie_Domain));

                    //Заправшиваем ответ на отправленный запрос и записываем его в "Stream"
                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    responseStream = new System.IO.StreamReader(httpResponse.GetResponseStream());

                    //Собираем колекцию подписанных пользователей
                    matches = Regex_Followers_Users.Matches(responseStream.ReadToEnd().ToString());

                    //Заполняем временный список пользователей старым списком пользователей
                    tempFollowersUsersList.AddRange(followersUsersList);

                    //создаем новый лист пользователей
                    foreach (Match user in matches)
                    {
                        newFollowersUsersList.Add(Regex_Followers_User.Match(user.Value).Value.TrimEnd('<').TrimStart('>'));
                    }

                    //Сравниваем старый и новый список пользователей получая список вновь подписавшихся пользователей
                    newUsers = newFollowersUsersList.Except(followersUsersList).ToList();

                    //заполнение промежуточного массива новыми подписавшимеся пользователями
                    foreach (string user in newUsers)
                    {
                        tempFollowersUsersList.Add(user);
                        MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"GoodGame\"" + "," + "\"type\":\"new_follower\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":\"На канале новый фолловер:\"," + "\"username\":\"" + user + "\"" + "}"));
                        //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":\"GoodGame\"" + "," + "\"type\":\"new_follower\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":\"На канале новый фолловер:\"," + "\"username\":\"" + user + "\"" + "}" + Environment.NewLine); }));
                    }

                    //обновляем список пользователей новыми подписавшимеся пользователями
                    followersUsersList = tempFollowersUsersList;
                }
            }
            catch
            {
                try
                {
                    //Создаем и настраиваем запрос для получения кукесов для дальнейшей аутентификации на https://goodgame.ru/channel/"+ channalName + "/subscriptions/
                    httpWebRequest = (HttpWebRequest)WebRequest.Create("https://goodgame.ru/api/4/login/password");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Accept = "application/json";
                    httpWebRequest.Method = "POST";

                    //заполнение JSON строки запроса//Error тут
                    using (var streamWriter = new System.IO.StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        //Формируем JSON строку с параметрами запроса(httpWebRequest)
                        string json = "{\"username\":" + "\"" + userName + "\"," + "\"password\":" + "\"" + Password + "\"}";

                        //Записываем JSON строку с параметрами в запрос(httpWebRequest)
                        streamWriter.Write(json);
                    }

                    //Заправшиваем ответ на отправленный запрос и записываем его в "Stream"
                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    responseStream = new System.IO.StreamReader(httpResponse.GetResponseStream());

                    //Получение информации из хэдера в котором в том числе находится информация о id@bad=3089485; и token@bad=%24sQ.TNJnxZs577BRRTWMUIOruHRCAgViA.qSUBGBysjNSMIQsIcrtG;
                    for (int i = 0; i < httpResponse.Headers.GetValues("set-cookie").Count(); i++)
                    {
                        //Выбираем домен из кукесов
                        if (Regex_Cookie_Domain.IsMatch(httpResponse.Headers.GetValues("set-cookie")[i].ToString()))
                        {
                            cookie_Domain = Regex_Cookie_Domain.Match(httpResponse.Headers.GetValues("set-cookie")[i].ToString()).Value;
                            cookie_Domain = cookie_Domain.Remove(0, 7).TrimEnd(';');
                        }
                        //Выбираем id@bad из кукесов
                        if (Regex_Cookie_IdBad.IsMatch(httpResponse.Headers.GetValues("set-cookie")[i].ToString()))
                        {
                            cookie_IdBad = Regex_Cookie_IdBad.Match(httpResponse.Headers.GetValues("set-cookie")[i].ToString()).Value;
                            cookie_IdBad = cookie_IdBad.Remove(0, 7).TrimEnd(';');
                        }
                        //Выбираем token@bad из кукесов
                        if (Regex_Cookie_TokenBad.IsMatch(httpResponse.Headers.GetValues("set-cookie")[i].ToString()))
                        {
                            cookie_TokenBad = Regex_Cookie_TokenBad.Match(httpResponse.Headers.GetValues("set-cookie")[i].ToString()).Value;
                            cookie_TokenBad = cookie_TokenBad.Remove(0, 10).TrimEnd(';');
                        }
                    }
                }
                catch
                {
                    getChannelFollowers(innerChannelName, innerUserName, innerPassword);
                }

            }
            finally//Выполняется в конце не зависомо от try или catch
            {
                //Очищаем запрос
                httpWebRequest?.Abort(); //Если request не равен null, то вызываем метод Abort.
                httpResponse?.Close();
                responseStream?.Close();
            }

        }

        private void FollowersTimer_Tick(object sender, EventArgs e)
        {
            //Если аккаунт авторизировался нормально то запускать приверку подписчиков.
            if (authAccount)
            {
                //Получение первичного списка подписчиков
                getChannelFollowers(innerChannelName, innerUserName, innerPassword);
            }

        }

    }

    public class Peka2tvChat
    {
        //Событие для отработки сообщений чата
        public delegate void Message(object sender, Newtonsoft.Json.Linq.JObject message);
        public event Message MessageEvent;

        private string channelID;
        private string userID;
        private string userToken;

        string innerChannelName;

        //Таймер пингующий сервер Peka2tv
        System.Windows.Forms.Timer PingTimer = new System.Windows.Forms.Timer();

        //Создаем внутренний TextBox
        //TextBox internalJObjectTextBox;

        //Создаем сокет с указанным адресом сервера чата
        WebSocketSharp.WebSocket socket = new WebSocketSharp.WebSocket(@"wss://chat.sc2tv.ru/?EIO=3&transport=websocket");


        //Конструктор класса служит для передачи ссылки на externalTextBox
        /*public Peka2tvChat(TextBox externalJObjectTextBox)
        {
            internalJObjectTextBox = externalJObjectTextBox;
        }*/

        //Подключение к конкретному каналу Пека2тв чата
        public void Connect(string channelName)
        {
            innerChannelName = channelName;
            //Peka2TV
            getChannelID(innerChannelName);
            //System.Console.WriteLine("{\"type\": \"join\", \"data\": { \"channel_id\": \"18865\", \"hidden\": false } }");

            //Подключение к серверу ГГ согласно вышеуказанному адресу сервера
            socket.Connect();
            //Авторизация на сервере Пека2тв(похоже токен можно получить только после одобрения кода Администрацией funstream)
            //socket.Send("42" + (1) + $"[\"{"/chat/login"}\",{Newtonsoft.Json.JsonConvert.SerializeObject(new { token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9"})}]");
            //Отправление сообщения серверу для подключения к конкретному каналу
            socket.Send("42" + (1) + $"[\"{"/chat/join"}\",{Newtonsoft.Json.JsonConvert.SerializeObject(new { channel = "stream/" + channelID })}]");
            //socket.Send("42" + (1) + $"[\"{"/chat/login"}\",{Newtonsoft.Json.JsonConvert.SerializeObject(new { token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzUxMiJ9.eyJpZCI6Mjg1NDUwLCJpcCI6IjQ2LjE4MC4xNTguMjE4IiwidXNlckFnZW50IjoiTW96aWxsYVwvNS4wIChXaW5kb3dzIE5UIDYuMTsgV2luNjQ7IHg2NDsgcnY6NzcuMCkgR2Vja29cLzIwMTAwMTAxIEZpcmVmb3hcLzc3LjAiLCJvYXV0aCI6eyJpZCI6MCwiYXBwcm92ZWQiOnRydWV9LCJleHAiOjE2NTY5MTgzMzV9.yxuXSTS7aSQpedYrNXcZK9EISvL4rDzzNh3UKACiuliARHJmPAzBeHCpGTltCXCyOLu9_gUe4LsAE_nkCuDbOg" })}]");
            //MessageBox.Show("42" + (1) + $"[\"{"/chat/login"}\",{Newtonsoft.Json.JsonConvert.SerializeObject(new { token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzUxMiJ9.eyJpZCI6MTQ4MzIzLCJpcCI6IjEyNy4wLjAuMSIsInVzZXJBZ2VudCI6Ik1vemlsbGFcLzUuMCAoV2luZG93cyBOVCA2LjE7IFdpbjY0OyB4NjQ7IHJ2Ojc3LjApIEdlY2tvXC8yMDEwMDEwMSBGaXJlZm94XC83Ny4wIiwib2F1dGgiOnsiaWQiOjAsImFwcHJvdmVkIjp0cnVlfSwiZXhwIjoxNjU2NzQ0MTYyfQ.gf9tfCMwHXzvZ-Q-JS5-gVxmOGLGfG5i2nxjjSIe1wzqKxhN00_McH3PRgYjJA5-Mjuq7uaa0QEfD6EDRMrARQ" })}]");
            //Отправляем сообщение в указанный TextBox о подключении к чату
            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"Peka2tv\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + channelName + "\"" + "," + "\"text\":\"Чат Peka2tv подключен к каналу: \"" + "}"));
            //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":\"Peka2tv\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + channelName + "\"" + "," + "\"text\":\"Чат Peka2tv подключен к каналу: \"" + "}" + Environment.NewLine); }));
            //Привязываем сорбытие о приходе сообщения
            socket.OnMessage += Socket_OnMessage;

            //System.Threading.Thread tick = new System.Threading.Thread()

            //(new System.Threading.Thread(delegate (){ pingServer(); })).Start();

            //Задаем параметры таймера
            PingTimer.Interval = 30000;
            PingTimer.Start();
            PingTimer.Tick += Ping;
        }

        //Пингование сервера чата
        private void Ping(object sender, EventArgs e)
        {
            //internalTextBox.Invoke(new Action(() => { internalTextBox.AppendText("tick" + Environment.NewLine); }));
            socket.Send("42" + (1) + "[{}]");
        }
        //Отключение сокета от сервера(и канала очевидно)
        public void Disconnect()
        {
            socket.OnMessage -= Socket_OnMessage;
            socket.Close();
            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"Peka2tv\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":\"Чат Peka2tv отключен от канала: \"" + "}"));
            //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":\"Peka2tv\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":\"Чат Peka2tv отключен от канала: \"" + "}" + Environment.NewLine); }));
            PingTimer.Tick -= Ping;
        }

        //События при приходе сообщения из Чата Peka2tv
        private void Socket_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            //Отфильтровываем мусор от исходного сообщения для правильной конвертации в JObject
            string rawString = new Regex(@"{+(\w|\W)+}").Match(e.Data).ToString();
            Newtonsoft.Json.Linq.JObject rawJObject;
            string usersListString;

            try
            {
                if (rawString.Contains(@"""users"":[") == false && rawString.Contains(@"""type""") == true)//Если объект сообщение в общий чат
                {
                    rawString = rawString.Substring(0, 1).Replace("{", "{" + "\"MessageSource\":\"Peka2tv\"" + "," + "\"type\":\"message\"" + ",") + rawString.Remove(0, 1);
                    rawJObject = Newtonsoft.Json.Linq.JObject.Parse(rawString);

                    //Если сообщение в обращено к чату
                    if (rawString.ToString().Replace("\v", "").Replace("\t", "").Replace("\n", "").Replace(" ", "").Contains(@"""to"":null") == true)
                    {
                        MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":" + "\"" + rawJObject["MessageSource"] + "\"" + "," + "\"type\":\"message\"" + "," + "\"username\":" + "\"" + (string)rawJObject["from"]["name"] + "\"" + "," + "\"text\":" + "\"" + rawJObject["text"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}"));
                        //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":" + "\"" + rawJObject["MessageSource"] + "\"" + "," + "\"type\":\"message\"" + "," + "\"username\":" + "\"" + (string)rawJObject["from"]["name"] + "\"" + "," + "\"text\":" + "\"" + rawJObject["text"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}" + Environment.NewLine); }));
                    }
                    else//Если сообщение обращенно к кому-то
                    {
                        MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":" + "\"" + rawJObject["MessageSource"] + "\"" + "," + "\"type\":\"message\"" + "," + "\"username\":" + "\"" + (string)rawJObject["from"]["name"] + "\"" + "," + "\"text\":" + "\"" + (string)rawJObject["to"]["name"] + ", " + rawJObject["text"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}"));
                        //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":" + "\"" + rawJObject["MessageSource"] + "\"" + "," + "\"type\":\"message\"" + "," + "\"username\":" + "\"" + (string)rawJObject["from"]["name"] + "\"" + "," + "\"text\":" + "\"" + (string)rawJObject["to"]["name"] + ", " + rawJObject["text"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}" + Environment.NewLine); }));
                    }
                }
                if (rawString.Contains(@"""users"":[") == true && rawString.Contains(@"""type""") == false)//Если объект список пользователей
                {
                    rawString = rawString.Substring(0, 1).Replace("{", "{" + "\"MessageSource\":\"Peka2tv\"" + "," + "\"type\":\"users_list\"" + ",") + rawString.Remove(0, 1);
                    rawJObject = Newtonsoft.Json.Linq.JObject.Parse(rawString);

                    usersListString = "{" + "\"MessageSource\":" + "\"" + rawJObject["MessageSource"] + "\"" + "," + "\"type\":\"users_list\"" + "," + "\"users\":[";

                    foreach (var viewers in rawJObject["result"]["users"])
                    {
                        usersListString += "{\"username\":" + "\"" + viewers["name"] + "\"" + "},";
                    }
                    if (usersListString[usersListString.Length - 1] != '[')
                    {
                        usersListString = usersListString.Remove(usersListString.Length - 1);
                    }
                    usersListString += "]}";

                    MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse(usersListString));
                    //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText(usersListString + Environment.NewLine); }));
                }
            }
            catch
            {
                MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"Peka2tv\"" + "," + "\"type\":\"channel_error_message\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "\"text\":" + "\"" + rawString.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}"));
                //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":\"Peka2tv\"" + "," + "\"type\":\"channel_error_message\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "\"text\":" + "\"" + rawString.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}" + Environment.NewLine); }));
            }

        }

        //Отправить сообщение в чат
        public void sendMessage(string text)
        {
            //не доделан нужна аутентификация
            //socket.Send("{\"type\": \"send_message\", \"data\": { \"channel_id\": \"" + channelID + "\"" + "," + "\"text\": \"" + text + "\"" + "," + "\"hideIcon\": \"false\"," + "\"mobile\": \"false\"" + "}}");
        }
        public void sendPrivateMessage(string userName, string text)
        {
            //пока ничего не сделано так как необходимо сделать аутентификацию
        }
        //Получить список зрителей из канала
        public void getChannelUsersList()
        {
            socket.Send("42" + (1) + $"[\"{"/chat/channel/list"}\",{Newtonsoft.Json.JsonConvert.SerializeObject(new { channel = "stream/" + channelID })}]");
        }

        //Методы для авторизации и определения ID канала по нику стримера
        private void getChannelID(string channelName)
        {

            try
            {
                string URI = @"https://sc2tv.ru/api/stream";
                string Parameters = "slug=" + channelName;
                System.Net.WebRequest req = System.Net.WebRequest.Create(URI);
                req.ContentType = "application/x-www-form-urlencoded";
                req.Method = "POST";
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(Parameters);
                req.ContentLength = bytes.Length;
                System.IO.Stream os = req.GetRequestStream(); // создаем поток 
                os.Write(bytes, 0, bytes.Length); // отправляем в сокет 
                os.Close();
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                var rawMessage = Newtonsoft.Json.Linq.JObject.Parse(sr.ReadToEnd().Trim());
                channelID = (string)rawMessage["owner"]["id"];
            }
            catch
            {
                getChannelID(innerChannelName);
            }
        }
        //Получить список зрителей из канала
        public void getAuthentificationWithTwitchOAuth()
        {
            //socket.Send("42" + (1) + $"[\"{"/api/oauth/thirdparty/register"}\",{Newtonsoft.Json.JsonConvert.SerializeObject(new { name = "fartper" , token = "oauth:rliti2amwlpkqi1jldrcrv432w1o8f" })}]");
        }

    }

    public class TwitchChat
    {
        //Событие для отработки сообщений чата
        public delegate void Message(object sender, Newtonsoft.Json.Linq.JObject message);
        public event Message MessageEvent;

        private string innerChannelName;
        private string innerUserName;
        private string innerOauth;


        //Таймер пингующий канал для получения списка вновь подписавшихся пользователей
        System.Windows.Forms.Timer followersTimer = new System.Windows.Forms.Timer();
        //Получение списка новых подписчиков

        private List<string> followersUsersList = new List<string>();
        int channel_id;
        private bool authAccount = false;

        //Создаем внутренний TextBox
        //TextBox internalJObjectTextBox;

        //Создаем TwitchClient чата
        TwitchClient client;

        //Конструктор класса служит для передачи ссылки на externalTextBox
        /*public TwitchChat(TextBox externalJObjectTextBox)
        {
            internalJObjectTextBox = externalJObjectTextBox;
        }*/

        //Подключение к конкретному каналу Твич чата
        public void Connect(string channelName, string userName, string oauth)
        {
            innerChannelName = channelName;
            innerUserName = userName;
            innerOauth = oauth;
            authAccount = true;
            //Задаем параметры пользователя под которыми заходим в чат
            ConnectionCredentials credentials = new ConnectionCredentials(userName, oauth);
            //Задаем протокол по которому будут идти GET запросы
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            //Twitch
            client = new TwitchClient();
            client.Initialize(credentials, channelName);

            client.Connect();

            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnWhisperReceived += Client_OnWhisperReceived;
            client.OnNewSubscriber += Client_OnNewSubscriber;
            //Отправляем сообщение в указанный TextBox о подключении к чату                                                                                         
            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"Twitch\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":\"Чат Twitch подключен к каналу: \"" + "}"));
            //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":\"Twitch\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":\"Чат Twitch подключен к каналу: \"" + "}" + Environment.NewLine); }));
            //Привязываем сорбытие о приходе сообщения

            //System.Threading.Thread tick = new System.Threading.Thread()

            //Задаем параметры таймера
            followersTimer.Interval = 5000;
            followersTimer.Start();
            followersTimer.Tick += followersTimer_Tick;
        }

        private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"Twitch\"" + "," + "\"type\":\"new_subscriber\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":\"На канале новый премиум подписчик:\"," + "\"username\":\"" + e.Subscriber.Login + "\"" + "}"));
            //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":\"Twitch\"" + "," + "\"type\":\"new_subscriber\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":\"На канале новый премиум подписчик:\"," + "\"username\":\"" + e.Subscriber.Login + "\"" + "}" + Environment.NewLine); }));
            //MessageBox.Show("{" + "\"MessageSource\":\"Twitch\"" + "," + "\"type\":\"new_subscriber\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":\"На канале новый премиум подписчик:\"," + "\"username\":\"" + e.Subscriber.Login + "\"" + "}");
        }

        private void followersTimer_Tick(object sender, EventArgs e)
        {
            if (authAccount)
            {
                getChannelFollowers(innerChannelName, innerUserName, innerOauth);
            }
        }

        private void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"Twitch\"" + "," + "\"type\":\"private_message\"" + "," + "\"username\":\"" + e.WhisperMessage.Username + "\"," + "\"text\":\"" + e.WhisperMessage.Message.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"}"));
            //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":\"Twitch\"" + "," + "\"type\":\"private_message\"" + "," + "\"username\":\"" + e.WhisperMessage.Username + "\"," + "\"text\":\"" + e.WhisperMessage.Message.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"}" + Environment.NewLine); }));
            //internalTextBox.Invoke(new Action(() => { internalTextBox.AppendText(e.WhisperMessage.Username + ": " + e.WhisperMessage.Message + Environment.NewLine); }));
        }

        //События при приходе сообщения из Чата Twitch
        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            try
            {
                if (e.ChatMessage.Bits > 0)
                {
                    //MessageBox.Show(e.ChatMessage.Bits.ToString());
                    MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":" + "\"" + "Twitch" + "\"" + "," + "\"type\":\"payment\"" + "," + "\"username\":" + "\"" + e.ChatMessage.Username + "\"" + "," + "\"amount\":" + "\"" + float.Parse(e.ChatMessage.Bits.ToString().Replace('.', ',')) + "\"" + "," + "\"text\":" + "\"" + e.ChatMessage.Message.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}"));
                    //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":" + "\"" + "Twitch" + "\"" + "," + "\"type\":\"payment\"" + "," + "\"username\":" + "\"" + e.ChatMessage.Username + "\"" + "," + "\"amount\":" + "\"" + float.Parse(e.ChatMessage.Bits.ToString().Replace('.', ',')) + "\"" + "," + "\"text\":" + "\"" + e.ChatMessage.Message.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}" + Environment.NewLine); }));
                }
                else
                {
                    MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"Twitch\"" + "," + "\"type\":\"message\"" + "," + "\"username\":\"" + e.ChatMessage.Username + "\"," + "\"text\":\"" + e.ChatMessage.Message.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"}"));
                    //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":\"Twitch\"" + "," + "\"type\":\"message\"" + "," + "\"username\":\"" + e.ChatMessage.Username + "\"," + "\"text\":\"" + e.ChatMessage.Message.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"}" + Environment.NewLine); }));
                }
            }
            catch
            {
                MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"Twitch\"" + "," + "\"type\":\"channel_error_message\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":" + "\"" + e.ChatMessage.RawIrcMessage.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}"));
                //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":\"Twitch\"" + "," + "\"type\":\"channel_error_message\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":" + "\"" + e.ChatMessage.RawIrcMessage.Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}" + Environment.NewLine); }));
            }
        }
        //Получить список пользователей на канале
        public void getChannelUsersList()
        {
            string usersListString;
            Newtonsoft.Json.Linq.JObject rawJObject;

            try
            {
                string URI = @"https://tmi.twitch.tv/group/user/" + innerChannelName + "/chatters";
                System.Net.WebRequest req = System.Net.WebRequest.Create(URI);
                req.Method = "POST";
                System.IO.Stream os = req.GetRequestStream(); // создаем поток 
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                var rawMessage = Newtonsoft.Json.Linq.JObject.Parse(sr.ReadToEnd().Trim());
                string rawString = rawMessage.ToString();
                rawString = (rawString.Substring(0, 1).Replace("{", "{" + "\"MessageSource\":\"Twitch\"" + "," + "\"type\":\"users_list\"" + ",") + rawString.Remove(0, 1)).Replace("\r", "").Replace("\v", "").Replace("\t", "").Replace("\n", "").Replace(" ", "");
                rawJObject = Newtonsoft.Json.Linq.JObject.Parse(rawString);
                usersListString = "{" + "\"MessageSource\":" + "\"" + rawJObject["MessageSource"] + "\"" + "," + "\"type\":\"users_list\"" + "," + "\"users\":[";

                foreach (var chatters in rawJObject["chatters"])
                {
                    foreach (var categoryViewers in chatters)
                    {
                        foreach (var viewers in categoryViewers)
                        {
                            usersListString += "{\"username\":" + "\"" + viewers.ToString() + "\"" + "},";
                        }
                    }
                }
                if (usersListString[usersListString.Length - 1] != '[')
                {
                    usersListString = usersListString.Remove(usersListString.Length - 1);
                }
                usersListString += "]}";

                MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse(usersListString));
                //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText(usersListString + Environment.NewLine); }));
            }
            catch
            {
                getChannelUsersList();
            }
        }
        //Отправить сообщение в чат
        public void sendMessage(string text)
        {
            //JoinedChannel channel = client.JoinedChannels[0];
            client.SendMessage(innerChannelName, text, false);
        }
        //Отправить ЛС
        public void sendPrivateMessage(string userName, string text)
        {
            client.SendWhisper(userName, text, false);
        }

        //Отключение сокета от сервера(и канала очевидно)
        public void Disconnect()
        {
            client.Disconnect();
            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"Twitch\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":\"Чат Twitch отключен от канала: \"" + "}"));
            //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":\"Twitch\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":\"Чат Twitch отключен от канала: \"" + "}" + Environment.NewLine); }));
            followersTimer.Tick -= followersTimer_Tick;
        }

        private void getChannelFollowers(string channelName, string userName, string OAuth)
        {
            //string client_id = "qlbdplequwpf6ng2m4bklovua8fyb2";
            int followers_limit = 20;//количество подписчиков на один запрос(для твича не больше 100 за раз)

            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpResponse = null;
            System.IO.StreamReader responseStream = null;

            List<string> newFollowersUsersList = new List<string>();
            List<string> tempFollowersUsersList = new List<string>();
            IEnumerable<string> newUsers;
            Newtonsoft.Json.Linq.JObject rawMessage;

            try
            {
                if (followersUsersList.LongCount() == 0)
                {

                    httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.twitch.tv/kraken/users?login=" + channelName);
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Accept = "application/vnd.twitchtv.v5+json";
                    httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "OAuth " + OAuth);
                    //httpWebRequest.Headers.Add("Client-ID: " + client_id);
                    httpWebRequest.Method = "GET";

                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    responseStream = new System.IO.StreamReader(httpResponse.GetResponseStream());
                    rawMessage = Newtonsoft.Json.Linq.JObject.Parse(responseStream.ReadToEnd().Trim());

                    //MessageBox.Show(rawMessage.ToString());
                    channel_id = (int)rawMessage["users"][0]["_id"];

                    httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.twitch.tv/kraken/channels/" + channel_id + "/follows?limit=" + followers_limit + "&offset=0");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Accept = "application/vnd.twitchtv.v5+json";
                    httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "OAuth " + OAuth);
                    //httpWebRequest.Headers.Add("Client-ID: " + client_id);
                    httpWebRequest.Method = "GET";

                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    responseStream = new System.IO.StreamReader(httpResponse.GetResponseStream());
                    rawMessage = Newtonsoft.Json.Linq.JObject.Parse(responseStream.ReadToEnd().Trim());

                    foreach (var user in rawMessage["follows"])
                    {
                        followersUsersList.Add(user["user"]["name"].ToString());
                        //Console.WriteLine(v["user"]["name"].ToString());
                    }
                }
                else
                {
                    /***Получение информации необходимой для формирования запроса о фоловерах***/

                    /////Формирование и отправка запроса о фоловерах/////
                    httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.twitch.tv/kraken/channels/" + channel_id + "/follows?limit=" + followers_limit + "&offset=0");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Accept = "application/vnd.twitchtv.v5+json";
                    httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "OAuth " + OAuth);
                    //httpWebRequest.Headers.Add("Client-ID: " + client_id);
                    httpWebRequest.Method = "GET";

                    httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    responseStream = new System.IO.StreamReader(httpResponse.GetResponseStream());
                    rawMessage = Newtonsoft.Json.Linq.JObject.Parse(responseStream.ReadToEnd().Trim());

                    foreach (var user in rawMessage["follows"])
                    {
                        newFollowersUsersList.Add(user["user"]["name"].ToString());
                        //Console.WriteLine(v["user"]["name"].ToString());
                    }

                    //Заполняем временный список пользователей старым списком пользователей
                    tempFollowersUsersList.AddRange(followersUsersList);


                    //Сравниваем старый и новый список пользователей получая список вновь подписавшихся пользователей
                    newUsers = newFollowersUsersList.Except(followersUsersList);

                    //заполнение промежуточного массива новыми подписавшимеся пользователями
                    foreach (string user in newUsers)
                    {
                        tempFollowersUsersList.Add(user);
                        MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"Twitch\"" + "," + "\"type\":\"new_follower\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":\"На канале новый фолловер:\"," + "\"username\":\"" + user + "\"" + "}"));
                        //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":\"Twitch\"" + "," + "\"type\":\"new_follower\"" + "," + "\"channel_name\":" + "\"" + innerChannelName + "\"" + "," + "\"text\":\"На канале новый фолловер:\"," + "\"username\":\"" + user + "\"" + "}" + Environment.NewLine); }));
                    }

                    //обновляем список пользователей новыми подписавшимеся пользователями
                    followersUsersList = tempFollowersUsersList;
                }
            }
            catch
            {
                authAccount = false;

            }

        }
    }

    public class DonatePay
    {
        //Событие для отработки сообщений DonatePay
        public delegate void Message(object sender, Newtonsoft.Json.Linq.JObject message);
        public event Message MessageEvent;

        string internalDonatePayToken;
        string internalLastID;
        //TextBox internalJObjectTextBox;
        System.Windows.Forms.Timer PingTimer = new System.Windows.Forms.Timer();
        TimerCallback tm;
        System.Threading.Timer timer;

        DateTime programmStartDateTime = Convert.ToDateTime("2100-09-16 05:51:37.590573");

        public DonatePay(/*string donatePayToken, TextBox externalJObjectTextBox*/)
        {
            //internalDonatePayToken = donatePayToken;
            //internalJObjectTextBox = externalJObjectTextBox;
            //getProgrammStartDateTime();
        }
        public void Connect(string donatePayToken)
        {
            internalDonatePayToken = donatePayToken;

            getProgrammStartDateTime();
            int num = 0;
            // устанавливаем метод обратного вызова
            tm = new TimerCallback(getLastDonation);
            // создаем таймер
            timer = new System.Threading.Timer(tm, num, 0, 21000);


            //Задаем параметры таймера
            /*
            PingTimer.Interval = 1000;///21000
                PingTimer.Tick += Ping;
                PingTimer.Start();
            */
            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"DonatePay\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + "DonatePay" + "\"" + "," + "\"text\":\"Подключен\"" + "}"));
            //MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"Twitch\"" + "," + "\"type\":\"message\"" + "," + "\"text\":\"" + "DonatePay подключен" + "\"}"));
        }
        public void Disconnect()
        {
            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"DonatePay\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + "DonatePay" + "\"" + "," + "\"text\":\"Отключен\"" + "}"));
            timer.Dispose();
            //PingTimer.Stop();
        }
        /*
        private void Ping(object sender, EventArgs e)
        {
            //getLastDonation();
        }*/

        private void getLastDonation(object obj)
        {
            //xDocument = XDocument.Load(internalXMLDonatPropertyPath);
            //internalLastID = xDocument.Element("root").Element("donatepay").Element("lastID").Value;
            HttpWebRequest httpWebRequest;
            HttpWebResponse httpResponse;

            try
            {
                //GET запрос на donatepay
                string access_token = internalDonatePayToken;
                string limit = "100";
                string before = "";
                string after = internalLastID;
                string skip = "0";
                string order = "DESC";
                string type = "donation";
                string status = "success";//success - для донатов,user - для проверки API
                string URI = @"https://donatepay.ru/api/v1/transactions?access_token=" + access_token + "&limit=" + limit + "&before=" + before + "&after=" + after + "&skip=" + skip + "&order=" + order + "&type=" + type + "&status=" + status;

                httpWebRequest = (HttpWebRequest)WebRequest.Create(URI);
                httpWebRequest.Method = "GET";
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";

                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader responseStream = new System.IO.StreamReader(httpResponse.GetResponseStream());
                var rawMessage = Newtonsoft.Json.Linq.JObject.Parse(responseStream.ReadToEnd());

                /*
                System.Net.WebRequest req = System.Net.WebRequest.Create(URI);
                req.Method = "GET";
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                var rawMessage = Newtonsoft.Json.Linq.JObject.Parse(sr.ReadToEnd().Trim());
                */
                if ((string)rawMessage["status"] == "success")
                {
                    //MessageBox.Show(rawMessage.ToString());
                    foreach (var viewers in rawMessage["data"])
                    {
                        string created_at = ((string)viewers["created_at"]).Replace('/', '-').TrimEnd('Z');
                        string date = created_at.Substring(6, 4) + "-" + created_at.Substring(0, 2) + "-" + created_at.Substring(3, 2) + " " + created_at.Substring(11, 8);
                        DateTime created_atDateTime = DateTime.Parse(date);

                        if (created_atDateTime > /*new DateTime(2020,1,10)*/ programmStartDateTime)
                        {

                            internalLastID = (string)viewers["id"];
                            //xDocument.Element("root").Element("donatepay").Element("lastID").Value = internalLastID;
                            //xDocument.Save(internalXMLDonatPropertyPath);
                            break;
                        }
                    }
                    foreach (var viewers in rawMessage["data"])
                    {
                        string created_at = ((string)viewers["created_at"]).Replace('/', '-').TrimEnd('Z');
                        string date = created_at.Substring(6, 4) + "-" + created_at.Substring(0, 2) + "-" + created_at.Substring(3, 2) + " " + created_at.Substring(11, 8);
                        DateTime created_atDateTime = DateTime.Parse(date);

                        if (created_atDateTime >/* new DateTime(2020, 1, 10)*/programmStartDateTime)
                        {
                            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":" + "\"" + "DonatePay" + "\"" + "," + "\"type\":\"payment\"" + "," + "\"username\":" + "\"" + viewers["what"] + "\"" + "," + "\"amount\":" + "\"" + float.Parse(viewers["sum"].ToString().Replace('.', ',')) + "\"" + "," + "\"text\":" + "\"" + viewers["vars"]["comment"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}"));
                            //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":" + "\"" + "DonatePay" + "\"" + "," + "\"type\":\"payment\"" + "," + "\"username\":" + "\"" + viewers["what"] + "\"" + "," + "\"amount\":" + "\"" + float.Parse(viewers["sum"].ToString().Replace('.', ',')) + "\"" + "," + "\"text\":" + "\"" + viewers["vars"]["comment"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}" + Environment.NewLine); }));
                        }
                    }
                }
            }
            catch
            {
                getLastDonation(null);
            }


        }

        private void getProgrammStartDateTime()
        {
            HttpWebRequest httpWebRequest;
            HttpWebResponse httpResponse;

            try
            {
                /////GET запрос на donatepay/////
                string access_token = internalDonatePayToken;
                string URI = @"https://donatepay.ru/api/v1/user?access_token=" + access_token;

                httpWebRequest = (HttpWebRequest)WebRequest.Create(URI);
                httpWebRequest.Method = "GET";
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";

                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader responseStream = new System.IO.StreamReader(httpResponse.GetResponseStream());
                var rawMessage = Newtonsoft.Json.Linq.JObject.Parse(responseStream.ReadToEnd());

                //MessageBox.Show((string)rawMessage["status"]);
                //GET запрос на donatepay
                /*
                string access_token = internalDonatePayToken;
                string URI = @"https://donatepay.ru/api/v1/user?access_token=" + access_token;
                System.Net.WebRequest req = System.Net.WebRequest.Create(URI);
                req.Method = "GET";
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                var rawMessage = Newtonsoft.Json.Linq.JObject.Parse(sr.ReadToEnd().Trim());
                */
                //MessageBox.Show(rawMessage.ToString());
                if ((string)rawMessage["status"] != "success")
                {
                    getProgrammStartDateTime();
                }
                else
                {

                    string date = ((string)rawMessage["time"]).Replace('/', '-');

                    //Convert.ToDateTime("2100-09-16 05:51:37.590573");
                    programmStartDateTime = DateTime.Parse(date.Substring(6, 4) + "-" + date.Substring(0, 2) + "-" + date.Substring(3, 2) + " " + date.Substring(11, 8));
                    //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":" + "\"" + "GoodGame" + "\"" + "," + "\"type\":" + "\"message\"" + "," + "\"username\":" + "\"" + "test" + "\"" + "," + "\"text\":" + "\"" + programmStartDateTime+"fhdhdfhd" + "\"" + "}" + Environment.NewLine); }));
                }
            }
            catch
            {
                getProgrammStartDateTime();
            }
        }
    }

    public class DonationAlerts
    {
        //Событие для отработки сообщений DonationAlerts
        public delegate void Message(object sender, Newtonsoft.Json.Linq.JObject message);
        public event Message MessageEvent;

        private int lastDonaterID;
        private int newLastDonaterID;

        TimerCallback timerCallback;
        System.Threading.Timer timer;

        Newtonsoft.Json.Linq.JObject rawMessage;
        WebRequest request = WebRequest.Create("https://www.donationalerts.com/api/v1/alerts/donations");

        static string internalAccessToken;
        static string internalRefreshToken;

        public void Connect(string AccessToken,string RefreshToken)
        {
            internalAccessToken = AccessToken;
            internalRefreshToken = RefreshToken;

            rawMessage = GetDonationList(); 

            lastDonaterID = (int)rawMessage["data"].First()["id"];

            int num = 0;
            // устанавливаем метод обратного вызова
            timerCallback = new TimerCallback(getLastDonation);
            // создаем таймер
            timer = new System.Threading.Timer(timerCallback, num, 0, 21000);

            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"DonationAlerts\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + "DonationAlerts" + "\"" + "," + "\"text\":\"Подключен\"" + "}"));
        }
        public void Disconnect()
        {
            timer.Dispose();
            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"DonationAlerts\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + "DonationAlerts" + "\"" + "," + "\"text\":\"Отключен\"" + "}"));
        }

        private Newtonsoft.Json.Linq.JObject GetDonationList()
        {
            Stream responseStream;

            try
            {
                request = WebRequest.Create("https://www.donationalerts.com/api/v1/alerts/donations");
                request.Method = "GET";
                request.Headers.Add("Authorization: Bearer " + internalAccessToken);

                responseStream = request.GetResponse().GetResponseStream();

                rawMessage = Newtonsoft.Json.Linq.JObject.Parse(new StreamReader(responseStream).ReadToEnd().Trim());
            }
            catch//Если текущий access_token не прошел запрашиваем новый через refresh_token и передаем значения обратино через вызов события MessageEvent
            {
                using (WebClient client = new WebClient())
                {
                    var reqparm = new System.Collections.Specialized.NameValueCollection();
                    reqparm.Add("grant_type", "refresh_token");
                    reqparm.Add("refresh_token", internalRefreshToken);
                    reqparm.Add("client_id", "8302");
                    reqparm.Add("client_secret", "M0dpeq9v7S13RfEYngZZJOTSqhAYmHBT8JY3wYfu");
                    reqparm.Add("scope", "oauth-user-show oauth-donation-index");

                    client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                    byte[] responsebytes = client.UploadValues("https://www.donationalerts.com/oauth/token", reqparm);
                    string responsebody = Encoding.UTF8.GetString(responsebytes);
                    var rawMessage = Newtonsoft.Json.Linq.JObject.Parse(responsebody.Trim());

                    internalAccessToken = rawMessage["access_token"].ToString();
                    internalRefreshToken = rawMessage["refresh_token"].ToString();

                    MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"DonationAlerts\"" + "," + "\"type\":\"application\"" + "," + "\"access_token\":" + "\"" + internalAccessToken + "\"" + "," + "\"refresh_token\":\""+ internalRefreshToken + "\"" + "}"));

                    rawMessage = GetDonationList();
                }


            }

            return rawMessage;
        }

        private void getLastDonation(object obj)
        {
            rawMessage = GetDonationList();

            newLastDonaterID = (int)rawMessage["data"].First()["id"];

            foreach(var donate in rawMessage["data"])
            {
                if(lastDonaterID!=(int)donate["id"])
                {
                    MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":" + "\"" + "DonationAlerts" + "\"" + "," + "\"type\":\"payment\"" + "," + "\"username\":" + "\"" + donate["username"] + "\"" + "," + "\"amount\":" + "\"" + float.Parse(donate["amount"].ToString().Replace('.', ',')) + "\"" + "," + "\"text\":" + "\"" + donate["message"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}"));
                }
                else
                {
                    break;
                }
            }

            lastDonaterID = newLastDonaterID;
        }
    }

    public class RutonyChat
    {
        //Событие для отработки сообщений DonationAlerts
        public delegate void Message(object sender, Newtonsoft.Json.Linq.JObject message);
        public event Message MessageEvent;

        Newtonsoft.Json.Linq.JObject rawJObject = new Newtonsoft.Json.Linq.JObject();

        //Создание и присвоение переменной WebSocket значения "ws://localhost:8080"  (ip сервера : Порт сервера)
        //WebSocket websocket = new WebSocket("ws://46.180.104.135:8383/Chat");
        WebSocket websocketChat = new WebSocket("ws://localhost:8383/Chat");
        WebSocket websocketDonate = new WebSocket("ws://localhost:8383/Donate");
        WebSocket websocketFollower = new WebSocket("ws://localhost:8383/Follower");
        WebSocket websocketSubPremium = new WebSocket("ws://localhost:8383/AnySub");
        
        /*
        public RutonyChat(TextBox externalJObjectTextBox)
        {
            internalJObjectTextBox = externalJObjectTextBox;
        }*/



        //Подключение к серверу
        public void Connect()
        {

            //InternalChatTextBox = ExternalTextBox;
            //internalSoundFromChatForm = ExternalSoundFromChat;
            //настройка соединения
            websocketChat.EnableAutoSendPing = true;
            websocketChat.AutoSendPingInterval = 30;//30 секунд, по стандарту 60
            websocketDonate.EnableAutoSendPing = true;
            websocketDonate.AutoSendPingInterval = 30;//30 секунд, по стандарту 60
            websocketFollower.EnableAutoSendPing = true;
            websocketFollower.AutoSendPingInterval = 30;//30 секунд, по стандарту 60
            websocketSubPremium.EnableAutoSendPing = true;
            websocketSubPremium.AutoSendPingInterval = 30;//30 секунд, по стандарту 60
            //Подключение к серверу(открытие соединения)
            websocketChat.Open();
            websocketDonate.Open();
            websocketFollower.Open();
            websocketSubPremium.Open();
            //webSocketisConnected = true;

            //Создание слушателей
            websocketChat.Opened += new EventHandler(websocket_Opened);
            websocketChat.Closed += Websocket_Closed;
            websocketChat.MessageReceived += Websocket_MessageReceived;
            websocketChat.Error += Websocket_Error;

            websocketDonate.MessageReceived += WebsocketDonate_MessageReceived;

            websocketFollower.MessageReceived += WebsocketFollower_MessageReceived;
            websocketSubPremium.MessageReceived += WebsocketSubPremium_MessageReceived;


            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"Rutony\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + "Ваш канал" + "\"" + "," + "\"text\":\"Чат Rutony подключен: \"" + "}"));

        }

        public void Disconnect()
        {
            websocketChat.Close();
            websocketDonate.Close();
            websocketFollower.Close();
            websocketSubPremium.Close();
            //webSocketisConnected = false;

            //Создание слушателей
            websocketChat.Opened -= new EventHandler(websocket_Opened);
            websocketChat.Closed -= Websocket_Closed;
            websocketChat.MessageReceived -= Websocket_MessageReceived;
            websocketChat.Error -= Websocket_Error;

            websocketDonate.MessageReceived -= WebsocketDonate_MessageReceived;

            websocketFollower.MessageReceived -= WebsocketFollower_MessageReceived;
            websocketSubPremium.MessageReceived -= WebsocketSubPremium_MessageReceived;

            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\"Rutony\"" + "," + "\"type\":\"channel_message\"" + "," + "\"channel_name\":" + "\"" + "Ваш канал" + "\"" + "," + "\"text\":\"Чат Rutony отключен: \"" + "}"));
        }

        private void Websocket_Closed(object sender, EventArgs e)
        {
            //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("Соединение с сервером разорвано" + Environment.NewLine); }));
        }

        private void Websocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("Ошибка подключения: " + e.Exception.Message + Environment.NewLine); }));
        }

        //Событие при приходе сообщения от сервера
        public void Websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            string rawString = e.Message.ToString();
            Newtonsoft.Json.Linq.JObject rawJObject;

            //rawString = rawString.Substring(0, 1).Replace("{", "{" + "\"MessageSource\":\"GoodGame\"" + ",") + rawString.Remove(0, 1);
            rawJObject = Newtonsoft.Json.Linq.JObject.Parse(rawString);

            //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText(e.Message + Environment.NewLine); }));
            if (rawJObject["type"].ToString() == "message")
            {
                MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":" + "\"" + rawJObject["site"] + "\"" + "," + "\"type\":" + "\"message\"" + "," + "\"username\":" + "\"" + rawJObject["user"] + "\"" + "," + "\"text\":" + "\"" + rawJObject["text"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}"));
                //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":" + "\"" + rawJObject["site"] + "\"" + "," + "\"type\":" + "\"message\"" + "," + "\"username\":" + "\"" + rawJObject["user"] + "\"" + "," + "\"text\":" + "\"" + rawJObject["text"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}" + Environment.NewLine); }));
            }

        }

        //Событие при донате
        private void WebsocketDonate_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            string rawString = e.Message.ToString();
            Newtonsoft.Json.Linq.JObject rawJObject;

            //rawString = rawString.Substring(0, 1).Replace("{", "{" + "\"MessageSource\":\"GoodGame\"" + ",") + rawString.Remove(0, 1);
            rawJObject = Newtonsoft.Json.Linq.JObject.Parse(rawString);
            //MessageBox.Show(rawString);
            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":" + "\"" + rawJObject["site"] + "\"" + "," + "\"type\":\"payment\"" + "," + "\"username\":" + "\"" + rawJObject["nick"] + "\"" + "," + "\"amount\":" + "\"" + float.Parse(rawJObject["summ"].ToString().Replace('.', ',')) + "\"" + "," + "\"text\":" + "\"" + rawJObject["text"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}"));
            //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":" + "\"" + rawJObject["site"] + "\"" + "," + "\"type\":\"payment\"" + "," + "\"username\":" + "\"" + rawJObject["nick"] + "\"" + "," + "\"amount\":" + "\"" + rawJObject["summ"] + "\"" + "," + "\"text\":" + "\"" + rawJObject["text"].ToString().Replace(@"\", @"\\").Replace(@"""", @"\""") + "\"" + "}" + Environment.NewLine); }));
            websocketDonate.Send("finished");
        }
        //Событие при фолове на канал
        private void WebsocketFollower_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            string rawString = e.Message.ToString();
            Newtonsoft.Json.Linq.JObject rawJObject;
            rawJObject = Newtonsoft.Json.Linq.JObject.Parse(rawString);

            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":\""+ rawJObject["site"] + "\"" + "," + "\"type\":\"new_follower\"" + "," + "\"channel_name\":" + "\"" + "Ваш канал" + "\"" + "," + "\"text\":\"На канале новый фолловер:\"," + "\"username\":\"" + rawJObject["nick"] + "\"" + "}"));
            //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":" + "\"" + rawJObject["site"] + "\"" + "," + "\"type\":\"new_follower\"" + "," + "\"channel_name\":" + "\"" + "Ваш канал" + "\"" + "," + "\"text\":\"На канале новый фолловер:\"," + "\"username\":\"" + rawJObject["nick"] + "\"" + "}" + Environment.NewLine); }));
            websocketDonate.Send("finished");
        }
        //Событие при оформлении платной подписки
        private void WebsocketSubPremium_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            string rawString = e.Message.ToString();
            Newtonsoft.Json.Linq.JObject rawJObject;
            rawJObject = Newtonsoft.Json.Linq.JObject.Parse(rawString);

            MessageEvent?.Invoke(this, Newtonsoft.Json.Linq.JObject.Parse("{" + "\"MessageSource\":" + "\"" + rawJObject["site"] + "\"" + "," + "\"type\":\"new_subscriber\"" + "," + "\"channel_name\":" + "\"" + "Ваш канал" + "\"" + "," + "\"text\":\"На канале новый премиум подписчик:\"," + "\"username\":\"" + rawJObject["nick"] + "\"" + "}"));
            //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("{" + "\"MessageSource\":" + "\"" + rawJObject["site"] + "\"" + "," + "\"type\":\"new_subscriber\"" + "," + "\"channel_name\":" + "\"" + "Ваш канал" + "\"" + "," + "\"text\":\"На канале новый премиум подписчик:\"," + "\"username\":\"" + rawJObject["nick"] + "\"" + "}" + Environment.NewLine); }));
            websocketDonate.Send("finished");
        }




        //Событие при подключении к серверу
        private void websocket_Opened(object sender, EventArgs e)
        {
            //internalJObjectTextBox.Invoke(new Action(() => { internalJObjectTextBox.AppendText("Соединение с сервером установлено" + Environment.NewLine); }));
        }
    }

}

