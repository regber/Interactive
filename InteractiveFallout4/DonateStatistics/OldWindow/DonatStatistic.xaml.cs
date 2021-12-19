using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.IO;

using System.Collections.ObjectModel;

namespace InteractiveFallout4.DonatStatistic
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class DonatStatisticWindow : UserControl
    {
        private static System.Windows.Controls.SelectedDatesCollection chosenDatesCollection;
        private static int countChosenDays;

        /*
        private static XDocument xDocument;//Объект для хранения XML документа
        private static IEnumerable<XElement> elements;
        private string donatXMLPath = Directory.GetCurrentDirectory() + @"\" + "DonatsStatistics.xml";*/
        /*
        private List<string> Users;
        private List<string> Donats;
        private List<string> Platforms;*/

        public DonatStatisticWindow()
        {
            InitializeComponent();
            LoadStartValues();
            //loadStartValues();
        }
        private void LoadStartValues()
        {
            ObservableCollection<DonateStatistics.DonateStatistics.Donate> DonateList = DonateStatistics.DonateStatistics.DonateList;
            int DonateCount = DonateList.Count;

            List<string> Donaters;
            List<string> Platforms;
            List<string> Commands;

            //Группируем донаты по отдельным их параметрам чтобы исключить повторения
            Donaters = DonateList.GroupBy(x => x.DonaterName).Select(y =>y.Key).ToList();
            Platforms = DonateList.GroupBy(x => x.Platform).Select(y => y.Key ).ToList();
            Commands = DonateList.GroupBy(x => x.CommandName).Select(y => y.Key ).ToList();

            //Заносим имена уникальных донатеров в комбобокс
            foreach(var Donater in Donaters)
            {
                DonatersComboBox.Items.Add(new Label() { Content=Donater });
            }
            DonatersComboBox.SelectedIndex = 0;

            //Заносим названия уникальных площадок в комбобокс
            foreach (var Platform in Platforms)
            {
                PlatformComboBox.Items.Add(new Label() { Content = Platform });
            }
            PlatformComboBox.SelectedIndex = 0;

            //Заносим названия уникальных команд в комбобокс
            foreach (var Command in Commands)
            {
                DonatComboBox.Items.Add(new Label() { Content = Command });
            }
            DonatComboBox.SelectedIndex = 0;

            displayModeComboBox.SelectedIndex = 0;

        }
        /*
        private void loadStartValues()
        {
            xDocument = XDocument.Load(donatXMLPath);

            elements = from x in xDocument.Element("Donations").Elements("Donat") select x;
            int countDonatsInXML = elements.ToArray().Count();
            Users = new List<string>();
            Platforms = new List<string>();
            Donats = new List<string>();

            for (int i=0; i<countDonatsInXML; i++)
            {
                //Занесение в лист и комбобокс уникальных имен донатеров
                if (!Users.Exists(item => item == elements.ToArray()[i].Attribute("donaterName").Value.ToString()))
                {
                    //Если донатер не TestDonation то дабавляем его имя в статистику
                    if (elements.ToArray()[i].Attribute("donaterName").Value.ToString()!= "TestDonation")
                    {
                        Users.Add(elements.ToArray()[i].Attribute("donaterName").Value.ToString());

                        Label user = new Label();
                        user.Content = elements.ToArray()[i].Attribute("donaterName").Value.ToString();
                        DonatersComboBox.Items.Add(user);
                        DonatersComboBox.SelectedIndex = 0;
                    }

                }
                if (!Donats.Exists(item => item == elements.ToArray()[i].Attribute("donatName").Value.ToString()))
                {
                    Donats.Add(elements.ToArray()[i].Attribute("donatName").Value.ToString());

                    Label donat = new Label();
                    donat.Content = elements.ToArray()[i].Attribute("donatName").Value.ToString();
                    DonatComboBox.Items.Add(donat);
                    DonatComboBox.SelectedIndex = 0;
                }
                //Занесение в лист и комбобокс уникальных платформ с которых донатили
                if (!Platforms.Exists(item => item == elements.ToArray()[i].Attribute("platform").Value.ToString()))
                {
                    Platforms.Add(elements.ToArray()[i].Attribute("platform").Value.ToString());

                    Label platform = new Label();
                    platform.Content = elements.ToArray()[i].Attribute("platform").Value.ToString();
                    PlatformComboBox.Items.Add(platform);
                    PlatformComboBox.SelectedIndex = 0;
                }
            }
            displayModeComboBox.SelectedIndex = 0;

        }
        */
        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            chosenDatesCollection = mainCalendar.SelectedDates;
            countChosenDays = chosenDatesCollection.Count;
            loadSelectedDatesFromOptions();
        }

        string Donater;
        string Platform;
        string Command;

        private void loadSelectedDatesFromOptions()
        {


            displayColumnsStackPanel.Children.Clear();
            datesColumnStackPanel.Children.Clear();
            if (loadOn == true)//Определяем что выбрано в комбобоксах после того когда прога прогрузится
            {
                Label selectedItem = (Label)PlatformComboBox.SelectedItem;
                Platform = selectedItem.Content.ToString();
                selectedItem = (Label)DonatComboBox.SelectedItem;
                Command = selectedItem.Content.ToString();
                selectedItem = (Label)DonatersComboBox.SelectedItem;
                Donater = selectedItem.Content.ToString();
            }


            IEnumerable<DonateStatistics.DonateStatistics.Donate> information = DonateStatistics.DonateStatistics.DonateList;

            information= information.Where(Donate => Donate.DonaterName != "TestDonation");

            if (Platform!="Все")
            {
                information = information.Where(Donate=>Donate.Platform== Platform);
            }
            if (Donater != "Все")
            {
                information = information.Where(Donate => Donate.DonaterName == Donater);
            }
            if (Command != "Все")
            {
                information = information.Where(Donate => Donate.CommandName == Command);
            }



            Dictionary<DateTime, string> showDictionary = new Dictionary<DateTime, string>();
            for (int k = 0; k < information.Count(); k++)
            {
                DateTime dt3 = information.ToArray()[k].Date;

                if (chosenDatesCollection.Contains(dt3))
                {
                    if (showDictionary.Keys.Contains(information.ToArray()[k].Date))
                    {
                        double firstValue;
                        Double.TryParse(showDictionary[information.ToArray()[k].Date], out firstValue);//Значение из диктинари
                        double secondaryValue;
                        secondaryValue=information.ToArray()[k].DonateAmount;//Значение из запроса LINQ
                        double summaValue = firstValue + secondaryValue;
                        showDictionary[information.ToArray()[k].Date] = summaValue.ToString();
                    }
                    else
                    {
                        showDictionary.Add(information.ToArray()[k].Date, information.ToArray()[k].DonateAmount.ToString());
                    }
                }
            }

            double greaterAmount = 0;
            for (int i = 0; i < showDictionary.Count(); i++)//Определение масимального доната(по дням) в выбранном диапазоне
            {
                double intermediateValue;
                double.TryParse(showDictionary.Values.ToArray()[i], out intermediateValue);
                if (intermediateValue > greaterAmount)
                {
                    greaterAmount = intermediateValue;
                }
            }
            double sumChosenPeriod = 0;
            if (loadOn)
            {
                for (int j = 0; j < showDictionary.Count(); j++)
                {
                    DockPanel dc = new DockPanel();
                    dc.Height = 175;
                    dc.Width = 20;
                    dc.VerticalAlignment = VerticalAlignment.Bottom;
                    dc.Margin = new Thickness(10, 0, 0, 0);

                    double oneUnitValue = 100 / greaterAmount;
                    Canvas canvasColumn = new Canvas();
                    canvasColumn.Width = 20;
                    double intermediateParseHeight = 0;
                    double.TryParse(showDictionary.Values.ToArray()[j], out intermediateParseHeight);
                    sumChosenPeriod += intermediateParseHeight;
                    canvasColumn.Height = intermediateParseHeight * oneUnitValue;
                    canvasColumn.Background = Brushes.BlanchedAlmond;
                    canvasColumn.VerticalAlignment = VerticalAlignment.Bottom;
                    DockPanel.SetDock(canvasColumn, Dock.Bottom);
                    dc.Children.Add(canvasColumn);

                    Label labelAmountDonat = new Label();
                    labelAmountDonat.Content = showDictionary.Values.ToArray()[j] + " руб.";
                    labelAmountDonat.Margin = new Thickness(-5, 0, 0, 0);
                    RotateTransform rtTransform = new RotateTransform(-90);
                    labelAmountDonat.LayoutTransform = rtTransform;
                    dc.Children.Add(labelAmountDonat);

                    displayColumnsStackPanel.Children.Add(dc);

                    DockPanel dockPanelBottom = new DockPanel();
                    dockPanelBottom.Height = 80;
                    dockPanelBottom.Width = 20;
                    dockPanelBottom.VerticalAlignment = VerticalAlignment.Top;
                    dockPanelBottom.Margin = new Thickness(10, 0, 0, 0);

                    Label labelDate = new Label();
                    labelDate.Content = showDictionary.Keys.ToArray()[j].ToShortDateString();
                    labelDate.VerticalAlignment = VerticalAlignment.Top;
                    labelDate.Margin = new Thickness(-5, 0, 0, 0);
                    labelDate.LayoutTransform = rtTransform;
                    dockPanelBottom.Children.Add(labelDate);

                    datesColumnStackPanel.Children.Add(dockPanelBottom);
                }
            }
            sumTextBlock.Text = sumChosenPeriod.ToString();
            DateTime dt2 = mainCalendar.DisplayDate;
        }

        //string platform;
        //string donatName;
        //string donaterName;
        bool loadOn = false;
        /*
        private void loadSelectedDatesFromOptions()
        {
            displayColumnsStackPanel.Children.Clear();
            datesColumnStackPanel.Children.Clear();
            if (loadOn == true)//Определяем что выбрано в комбобоксах после того когда прога прогрузится
            {
                Label selectedItem = (Label)PlatformComboBox.SelectedItem;
                platform = selectedItem.Content.ToString();
                selectedItem = (Label)DonatComboBox.SelectedItem;
                donatName = selectedItem.Content.ToString();
                selectedItem = (Label)DonatersComboBox.SelectedItem;
                donaterName = selectedItem.Content.ToString();
            }
            
            //MessageBox.Show("Выбраны донаты с платформы:"+platform+" , за посос:"+donatName+" , от человека с ником: "+donaterName);

            IEnumerable<XElement> information=null;
            int countStringChosen = 0;
            //Когда в каждом комбобоксе выбран какой либо вариант
            if (platform!="Все" && donatName!="Все" && donaterName!= "Все")
            {
                information = from x in xDocument.Element("Donations").Elements("Donat") where (string)x.Attribute("platform") == platform where (string)x.Attribute("donatName") == donatName where (string)x.Attribute("donaterName") == donaterName && (string)x.Attribute("donaterName") != "TestDonation" select x;
                countStringChosen = information.ToArray().Count();
            }
            //Когда выбрано "Все" в каждом комбобоксе
            if (platform == "Все" && donatName == "Все" && donaterName == "Все")
            {
                information = from x in xDocument.Element("Donations").Elements("Donat") where (string)x.Attribute("platform") != platform where (string)x.Attribute("donatName") != donatName where (string)x.Attribute("donaterName") != donaterName && (string)x.Attribute("donaterName") != "TestDonation" select x;
                countStringChosen = information.ToArray().Count();
            }

            //платформы
            if (platform == "Все" && donatName != "Все" && donaterName != "Все")
            {
                information = from x in xDocument.Element("Donations").Elements("Donat") where (string)x.Attribute("platform") != platform where (string)x.Attribute("donatName") == donatName where (string)x.Attribute("donaterName") == donaterName && (string)x.Attribute("donaterName") != "TestDonation" select x;
                countStringChosen = information.ToArray().Count();
            }
            if (platform == "Все" && donatName == "Все" && donaterName != "Все")
            {
                information = from x in xDocument.Element("Donations").Elements("Donat") where (string)x.Attribute("platform") != platform where (string)x.Attribute("donatName") != donatName where (string)x.Attribute("donaterName") == donaterName && (string)x.Attribute("donaterName") != "TestDonation" select x;
                countStringChosen = information.ToArray().Count();
            }
            if (platform == "Все" && donatName != "Все" && donaterName == "Все")
            {
                information = from x in xDocument.Element("Donations").Elements("Donat") where (string)x.Attribute("platform") != platform where (string)x.Attribute("donatName") == donatName where (string)x.Attribute("donaterName") != donaterName && (string)x.Attribute("donaterName") != "TestDonation" select x;
                countStringChosen = information.ToArray().Count();
            }
            //Донаты
            if (platform != "Все" && donatName == "Все" && donaterName != "Все")
            {
                information = from x in xDocument.Element("Donations").Elements("Donat") where (string)x.Attribute("platform") == platform where (string)x.Attribute("donatName") != donatName where (string)x.Attribute("donaterName") == donaterName && (string)x.Attribute("donaterName") != "TestDonation" select x;
                countStringChosen = information.ToArray().Count();
            }
            if (platform == "Все" && donatName == "Все" && donaterName != "Все")
            {
                information = from x in xDocument.Element("Donations").Elements("Donat") where (string)x.Attribute("platform") != platform where (string)x.Attribute("donatName") != donatName where (string)x.Attribute("donaterName") == donaterName && (string)x.Attribute("donaterName") != "TestDonation" select x;
                countStringChosen = information.ToArray().Count();
            }
            if (platform != "Все" && donatName == "Все" && donaterName == "Все")
            {
                information = from x in xDocument.Element("Donations").Elements("Donat") where (string)x.Attribute("platform") == platform where (string)x.Attribute("donatName") != donatName where (string)x.Attribute("donaterName") != donaterName && (string)x.Attribute("donaterName") != "TestDonation" select x;
                countStringChosen = information.ToArray().Count();
            }
            //Донатеры
            if (platform != "Все" && donatName != "Все" && donaterName == "Все")
            {
                information = from x in xDocument.Element("Donations").Elements("Donat") where (string)x.Attribute("platform") == platform where (string)x.Attribute("donatName") == donatName where (string)x.Attribute("donaterName") != donaterName && (string)x.Attribute("donaterName") != "TestDonation" select x;
                countStringChosen = information.ToArray().Count();
            }
            if (platform == "Все" && donatName != "Все" && donaterName == "Все")
            {
                information = from x in xDocument.Element("Donations").Elements("Donat") where (string)x.Attribute("platform") != platform where (string)x.Attribute("donatName") == donatName where (string)x.Attribute("donaterName") != donaterName && (string)x.Attribute("donaterName") != "TestDonation" select x;
                countStringChosen = information.ToArray().Count();
            }
            if (platform != "Все" && donatName == "Все" && donaterName == "Все")
            {
                information = from x in xDocument.Element("Donations").Elements("Donat") where (string)x.Attribute("platform") == platform where (string)x.Attribute("donatName") != donatName where (string)x.Attribute("donaterName") != donaterName && (string)x.Attribute("donaterName") != "TestDonation" select x;
                countStringChosen = information.ToArray().Count();
            }

            Dictionary<string, string> showDictionary= new Dictionary<string, string>();
            for(int k=0; k<countStringChosen; k++)
            {
                string[] splitDatesChosen = information.ToArray()[k].Attribute("Date").Value.Split('.');
                int dayChosen;
                int monthChosen;
                int yearChosen;
                int.TryParse(splitDatesChosen[0], out dayChosen);
                int.TryParse(splitDatesChosen[1], out monthChosen);
                int.TryParse(splitDatesChosen[2], out yearChosen);
                DateTime dt3 = new DateTime(yearChosen, monthChosen, dayChosen);
                if (chosenDatesCollection.Contains(dt3))
                {
                    if (showDictionary.Keys.Contains(information.ToArray()[k].Attribute("Date").Value))
                    {
                        double firstValue;
                        Double.TryParse(showDictionary[information.ToArray()[k].Attribute("Date").Value], out firstValue);//Значение из диктинари
                        double secondaryValue;
                        Double.TryParse(information.ToArray()[k].Attribute("donatAmount").Value, out secondaryValue);//Значение из запроса LINQ
                        double summaValue = firstValue + secondaryValue;
                        showDictionary[information.ToArray()[k].Attribute("Date").Value] = summaValue.ToString();
                    }
                    else
                    {
                        showDictionary.Add(information.ToArray()[k].Attribute("Date").Value, information.ToArray()[k].Attribute("donatAmount").Value);
                    }
                }
            }

            double greaterAmount = 0;
            for (int i = 0; i < showDictionary.Count(); i++)//Определение масимального доната(по дням) в выбранном диапазоне
            {
                double intermediateValue;
                double.TryParse(showDictionary.Values.ToArray()[i], out intermediateValue);
                if (intermediateValue > greaterAmount)
                {
                    greaterAmount = intermediateValue;
                }
            }
            double sumChosenPeriod = 0;
            if (loadOn)
            {
                for (int j = 0; j<showDictionary.Count(); j++)
                {
                    DockPanel dc = new DockPanel();
                    dc.Height = 175;
                    dc.Width = 20;
                    dc.VerticalAlignment = VerticalAlignment.Bottom;
                    dc.Margin = new Thickness(10, 0, 0, 0);

                    double oneUnitValue = 100 / greaterAmount;
                    Canvas canvasColumn = new Canvas();
                    canvasColumn.Width = 20;
                    double intermediateParseHeight = 0;
                    double.TryParse(showDictionary.Values.ToArray()[j], out intermediateParseHeight);
                    sumChosenPeriod += intermediateParseHeight;
                    canvasColumn.Height = intermediateParseHeight * oneUnitValue;
                    canvasColumn.Background = Brushes.BlanchedAlmond;
                    canvasColumn.VerticalAlignment = VerticalAlignment.Bottom;
                    DockPanel.SetDock(canvasColumn, Dock.Bottom);
                    dc.Children.Add(canvasColumn);

                    Label labelAmountDonat = new Label();
                    labelAmountDonat.Content = showDictionary.Values.ToArray()[j] + " руб.";
                    labelAmountDonat.Margin = new Thickness(-5, 0, 0, 0);
                    RotateTransform rtTransform = new RotateTransform(-90);
                    labelAmountDonat.LayoutTransform = rtTransform;
                    dc.Children.Add(labelAmountDonat);

                    displayColumnsStackPanel.Children.Add(dc);

                    DockPanel dockPanelBottom = new DockPanel();
                    dockPanelBottom.Height = 80;
                    dockPanelBottom.Width = 20;
                    dockPanelBottom.VerticalAlignment = VerticalAlignment.Top;
                    dockPanelBottom.Margin = new Thickness(10, 0, 0, 0);

                    Label labelDate = new Label();
                    labelDate.Content = showDictionary.Keys.ToArray()[j];
                    labelDate.VerticalAlignment = VerticalAlignment.Top;
                    labelDate.Margin = new Thickness(-5, 0, 0, 0);
                    labelDate.LayoutTransform = rtTransform;
                    dockPanelBottom.Children.Add(labelDate);

                    datesColumnStackPanel.Children.Add(dockPanelBottom);
                }
            }
            sumTextBlock.Text = sumChosenPeriod.ToString();
            DateTime dt2 = mainCalendar.DisplayDate;
        }
        */
        /*//Поправить метод для определения изначально выбранного периода?
        private void mainCalendar_Loaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(mainCalendar.DisplayDate.ToShortDateString().ToString());
        }
        */

        private void ChangeSelectionOptions(object sender, RoutedEventArgs e)
        {
            loadSelectedDatesFromOptions();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadOn = true;
        }
    }
}
