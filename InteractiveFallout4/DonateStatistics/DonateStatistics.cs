using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Serialization;
using System.IO;
using System.Collections.ObjectModel;

namespace InteractiveFallout4.DonateStatistics
{

    class DonateStatistics
    {
        public static double CurrentDonateAmount { get; set; }
        public static ObservableCollection<Donate> DonateList { get; set; }

        public class Donate
        {
            public DateTime Date { get; set; }
            public string CommandName { get; set; }
            public double DonateAmount { get; set; }
            public string DonaterName { get; set; }
            public string Platform { get; set; }
        }

        public static void GetDonateMessage(Newtonsoft.Json.Linq.JObject message)
        {
            CurrentDonateAmount += (double)message["amount"];

            var currentInteractiveSet = InteractiveBuilder.Interactive.InteractiveSets.InteractiveSetList.Where(set => set.Title == InteractiveBuilder.Interactive.InteractiveSets.ActiveSet).First();
            
            //Проверяем есть ли в текущем активном наборе команда за сумму текущего доната, если есть то записываем команду в статистику
            if (currentInteractiveSet.InteractiveSetCommandsList.Any(command => command.CommPayment.Type == "Donate" && command.CommPayment.Price == (int)message["amount"]))
            {
                var currentCommand = currentInteractiveSet.InteractiveSetCommandsList.Where(command => command.CommPayment.Price == (int)message["amount"]).First();

                DonateList.Add(new Donate() { Date = DateTime.Parse(DateTime.Now.ToShortDateString()), CommandName = currentCommand.Title, DonateAmount= (double)message["amount"], DonaterName=message["username"].ToString(), Platform= message["MessageSource"].ToString() });
            }
           
            StatisticsSave();
        }

        static DonateStatistics()
        {
            StatisticsLoad();
        }

        public static void StatisticsLoad()
        {
            DonateStatisticsSerialize DnStSerialize = new DonateStatisticsSerialize();

            XmlSerializer formatter = new XmlSerializer(typeof(DonateStatisticsSerialize));
            using (FileStream fs = new FileStream("DonateStatistics.xml", FileMode.Open))
            {
                DnStSerialize = (DonateStatisticsSerialize)formatter.Deserialize(fs);
            }

            DonateStatistics.CurrentDonateAmount = DnStSerialize.CurrentDonateAmount;

            DonateStatistics.DonateList = new ObservableCollection<Donate>();
            foreach(var Donate in DnStSerialize.DonateList)
            {
                DonateStatistics.DonateList.Add(new DonateStatistics.Donate() { Date=Donate.Date, CommandName= Donate.CommandName, DonateAmount= Donate.DonateAmount, DonaterName= Donate.DonaterName, Platform= Donate.Platform });
            }
        }
        public static void StatisticsSave()
        {
            DonateStatisticsSerialize DnStDeserialize = new DonateStatisticsSerialize();

            DnStDeserialize.CurrentDonateAmount = DonateStatistics.CurrentDonateAmount;

            DnStDeserialize.DonateList = new ObservableCollection<DonateStatisticsSerialize.Donate>();
            foreach (var Donate in DonateStatistics.DonateList)
            {
                DnStDeserialize.DonateList.Add(new DonateStatisticsSerialize.Donate(){ Date = Donate.Date, CommandName = Donate.CommandName, DonateAmount = Donate.DonateAmount, DonaterName = Donate.DonaterName, Platform = Donate.Platform });
            }

            XmlSerializer formatter = new XmlSerializer(typeof(DonateStatisticsSerialize));

            using (FileStream fs = new FileStream("DonateStatistics.xml", FileMode.Create))
            {
                formatter.Serialize(fs, DnStDeserialize);
            }
        }
    }

    [Serializable]
    [XmlRoot("DonateStatistics")]
    public class DonateStatisticsSerialize
    {
        [XmlElement("CurrentDonateAmount")]
        public double CurrentDonateAmount { get; set; }

        [XmlArrayItem("Donate")]
        public ObservableCollection<Donate> DonateList { get; set; }
        public class Donate
        {
            [XmlAttribute]
            public DateTime Date { get; set; }
            [XmlAttribute]
            public string CommandName { get; set; }
            [XmlAttribute]
            public double DonateAmount { get; set; }
            [XmlAttribute]
            public string DonaterName { get; set; }
            [XmlAttribute]
            public string Platform { get; set; }
        }
    }
}
