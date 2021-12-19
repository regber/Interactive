using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using InteractiveFallout4.Common.DialogWindows;
using System.Windows.Data;
using System.Globalization;
using System.IO;

namespace InteractiveFallout4.InteractiveBuilder
{
    class InteractiveBuilderWindowViewModel : ViewModel.BaseViewModel
    {

        //private InteractiveRoulette.InteractiveRouletteWindow RouletteWindow = InteractiveRoulette.InteractiveRouletteWindow.GetWindow();
        
        public static List<string> PaymentTypeList { get; set; } = new List<string> { "Donate", "Premium", "Follow" };
        public static List<string> EnableList { get; set; } = new List<string> { "Включено", "Отключено" };
        public static List<string> AlertVoiceList { get; set; } = new List<string> { "off", "oksana", "jane", "omazh", "zahar", "ermil" };
        public static List<string> WriteTypeList { get; set; } = new List<string> { "Простая запись", "Добавление и запись", "Умножение и запись", "Деление и запись" };

        public static PropertyPath ActiveSet { get; set; } = new PropertyPath(typeof(Interactive.InteractiveSets).GetProperty("ActiveSet"));

        //Костыль для обновления списка команд в основном окне при изменении выбора активного набора в окне конструктора
        public static Interactive.InteractiveSets.InteractiveSet CurrentInteractiveSet;

        private Interactive.InteractiveSets.InteractiveSet _SelectedSet;
        public Interactive.InteractiveSets.InteractiveSet SelectedSet
        {
            get { return _SelectedSet; }
            set
            {
                CurrentInteractiveSet = value;
                _SelectedSet = value;
                OnPropertyChanged(nameof(SelectedSet));
            }
        }

        private ObservableCollection<Interactive.InteractiveSets.InteractiveSet> _InteractiveSetsComboBoxData = Interactive.InteractiveSets.InteractiveSetList;
        public ObservableCollection<Interactive.InteractiveSets.InteractiveSet> InteractiveSetsComboBoxData
        {
            get
            {
                //MessageBox.Show("_InteractivSetsComboBoxData");
                return _InteractiveSetsComboBoxData;
            }
            set
            {
                _InteractiveSetsComboBoxData = value;
                OnPropertyChanged(nameof(InteractiveSetsComboBoxData));
            }
        }
        public static ObservableCollection<Interactive.InteractiveCommandTypes.InteractiveCommandType> InteractiveCommandTypesList { get; set; } = Interactive.InteractiveCommandTypes.InteractiveCommandTypesList;
        public static ObservableCollection<Interactive.InteractiveVariables.InteractiveVariable> InteractiveVariablesList { get; set; } = Interactive.InteractiveVariables.InteractiveVariablesList;
        public ViewModel.Command ShowInteractiveRouletteCommand
        {
            get
            {
                return new ViewModel.Command((Object) =>
                {
                    InteractiveRoulette.InteractiveRouletteWindow.GetWindow().Dispatcher.Invoke(()=>
                    {
                        InteractiveRoulette.InteractiveRouletteWindow.GetWindow().Show();
                    });
                });
            }
        }
        public static ViewModel.CommandWithMultParam TestInteractiveCommand
        {
            get
            {
                return new ViewModel.CommandWithMultParam((obj) =>
                {
                    var Command = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand)((object[])obj)[0];
                    int BarrelCount = (int)((object[])obj)[1];

                    //Запускаем тестовое проигрывание рулетки с выбранной коммандой и выбранным кол-вом барабанов
                    InteractiveRoulette.InteractiveRouletteWindow.GetWindow().Dispatcher.Invoke(() =>
                    {
                        ((InteractiveRoulette.InteractiveRouletteWindowViewModel)InteractiveRoulette.InteractiveRouletteWindow.GetWindow().DataContext).StartRoulette(Command, BarrelCount);
                    }); 
                
                });
            }
        }
        public ViewModel.Command CloseBuilderWindow
        {
            get
            {
                return new ViewModel.Command((obj) =>
                {
                    var CurrentSetCommandList = CurrentInteractiveSet.InteractiveSetCommandsList;
                    
                    //Костыль для обновления списка команд в основном окне при изменении выбора активного набора в окне конструктора
                    MainWindow.MainWindowViewModel.CurrentActiveSetCommandList.Clear();
                    foreach (var v in CurrentSetCommandList)
                    {
                        MainWindow.MainWindowViewModel.CurrentActiveSetCommandList.Add(v);
                    }
                });
            }
        }

        public static ViewModel.Command SaveChangeInInteractiveSet
        {
            get
            {
                return new ViewModel.Command((Object) =>
                {
                    Interactive.InteractiveSave();
                });
            }
        }
        public static ViewModel.Command ComboBoxPriceTypeComandChangeValue
        {
            get
            {
                return new ViewModel.Command(obj =>
                {
                    var currentCommandPayment = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.CommandPayment)obj;

                    if (currentCommandPayment.Type != "Donate")
                    {
                        currentCommandPayment.Price = 0;
                    }
                });
            }
        }
        public static ViewModel.CommandWithMultParam ComboBoxTypeComandChangeValue
        {
            get
            {
                return new ViewModel.CommandWithMultParam(obj =>
                {
                    var currentCommandType = ((Interactive.InteractiveCommandTypes.InteractiveCommandType)((object[])obj)[0]);
                    var currentCommand = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.CommandType)((object[])obj)[1];

                    currentCommand.Type = currentCommandType.Type;
                    currentCommand.Variable = currentCommandType.Variable;
                    currentCommand.Text = currentCommandType.Text;
                    currentCommand.Value = currentCommandType.Value;
                });
            }
        }
        public static ViewModel.Command AddNewInteractiveSet
        {
            get
            {
                return new ViewModel.Command(Object =>
                {
                    string NewSetTitle = "New Set №";
                    int number = 0;

                    while (Interactive.InteractiveSets.InteractiveSetList.Any(obj => obj.Title == NewSetTitle + number))
                    {
                        number++;
                    }

                    Interactive.InteractiveSets.InteractiveSetList.Add(new Interactive.InteractiveSets.InteractiveSet() { Title = NewSetTitle + number });
                });
            }
        }
        public static ViewModel.Command ShowVariableOptionsWindow
        {
            get
            {
                return new ViewModel.Command((Object) =>
                {
                    VariableOptions.VariableOptionsWindow VariableOptionsWindow = new VariableOptions.VariableOptionsWindow();
                    VariableOptionsWindow.ShowDialog();
                });
            }
        }
        public static ViewModel.Command ShowCommandTypeOptionsWindow
        {
            get
            {
                return new ViewModel.Command((Object) =>
                {
                    InteractiveBuilder.CommandTypeOptions.CommandTypeOptionsWindow CommandTypeOptionsWindow = new InteractiveBuilder.CommandTypeOptions.CommandTypeOptionsWindow();
                    CommandTypeOptionsWindow.ShowDialog();
                });
            }
        }
        public ViewModel.Command AddNewInteractiveCommand
        {
            get
            {
                return new ViewModel.Command(obje =>
                {
                    string NewCommandTitle = "New Command №";
                    int number = 0;

                    while (SelectedSet.InteractiveSetCommandsList.Any(obj => obj.Title == NewCommandTitle + number))
                    {
                        number++;
                    }

                    SelectedSet.InteractiveSetCommandsList.Add(new Interactive.InteractiveSets.InteractiveSet.InteractiveCommand()
                    {
                        Title = NewCommandTitle + number,
                        CommPayment = new Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.CommandPayment(),
                        CommType = new Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.CommandType()
                    });
                });
            }
        }
        public static ViewModel.Command AddInteractiveAlert
        {
            get
            {
                return new ViewModel.Command(obj =>
                {

                    var currentCommand = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand)obj;
                    currentCommand.CommAlert = new Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.CommandAlert();
                });
            }
        }
        public ViewModel.Command DeleteInteractiveSet
        {
            get
            {
                return new ViewModel.Command(obje =>
                {
                    var currentWindow = (InteractiveBuilderWindow)obje;
                    var currentViewModel = (InteractiveBuilderWindowViewModel)currentWindow.DataContext;

                    //укдаляем из листа набор имя которого совпадает с выбранным набором
                    Interactive.InteractiveSets.InteractiveSetList.Remove(Interactive.InteractiveSets.InteractiveSetList.Where(set=>set.Title== (string)currentWindow.SetsComboBox.SelectedValue).First());

                    //Если в списке не осталось наборов то добавляем новый
                    if (Interactive.InteractiveSets.InteractiveSetList.Count==0)
                    {
                        string NewSetTitle = "New Set №";
                        int number = 0;

                        while (Interactive.InteractiveSets.InteractiveSetList.Any(obj => obj.Title == NewSetTitle + number))
                        {
                            number++;
                        }

                        Interactive.InteractiveSets.InteractiveSetList.Add(new Interactive.InteractiveSets.InteractiveSet() { Title = NewSetTitle + number });
                    }

                    //выбираем в выпадающем списке наборов первый из них
                    currentWindow.SetsComboBox.SelectedValue = Interactive.InteractiveSets.InteractiveSetList.First().Title;
                });
            }
        }
        public static ViewModel.Command DeleteInteractiveAlert
        {
            get
            {
                return new ViewModel.Command(obj =>
                {

                    var currentCommand = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand)obj;

                    currentCommand.CommAlert = null;
                });
            }
        }

        public static ViewModel.Command AddInteractiveMemoryRead
        {
            get
            {
                return new ViewModel.Command(obj =>
                {
                    var currentCommand = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand)obj;
                    currentCommand.CommMemoryReads.Add(new Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.CommandMemoryRead());
                });
            }
        }
        public static ViewModel.CommandWithMultParam DeleteInteractiveMemoryRead
        {
            get
            {
                return new ViewModel.CommandWithMultParam(obj =>
                {
                    var currentMemmoryRead = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.CommandMemoryRead)((object[])obj)[0];
                    var currentCommand = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand)((object[])obj)[1];

                    currentCommand.CommMemoryReads.Remove(currentMemmoryRead);
                });
            }
        }

        public static ViewModel.Command AddInteractiveMemoryWrite
        {
            get
            {
                return new ViewModel.Command(obj =>
                {
                    var currentCommand = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand)obj;
                    currentCommand.CommMemoryWrites.Add(new Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.CommandMemoryWrite());
                });
            }
        }
        public static ViewModel.CommandWithMultParam DeleteInteractiveMemoryWrite
        {
            get
            {
                return new ViewModel.CommandWithMultParam(obj =>
                {
                    var currentMemmoryWrite = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.CommandMemoryWrite)((object[])obj)[0];
                    var currentCommand = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand)((object[])obj)[1];

                    currentCommand.CommMemoryWrites.Remove(currentMemmoryWrite);
                });
            }
        }

        public static ViewModel.Command AddRouletteToCommand
        {
            get
            {
                return new ViewModel.Command(obj =>
                {

                    var currentCommand = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand)obj;
                    currentCommand.CommBarrels.Add(new Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.Barrel());

                });
            }
        }
        public static ViewModel.Command DeleteRouletteFromCommand
        {
            get
            {
                return new ViewModel.Command((obj) =>
                {
                    var currentCommand = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand)obj;
                    currentCommand.CommBarrels.Clear();
                    //SelectedSet.InteractiveSetCommandsList.Remove(currentCommand);
                });
            }
        }

        public static ViewModel.Command AddBarrelToRoulette
        {
            get
            {
                return new ViewModel.Command(obj =>
                {
                    var currentCommand = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand)obj;
                    currentCommand.CommBarrels.Add(new Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.Barrel());
                });
            }
        }
        public static ViewModel.CommandWithMultParam DeleteBarrel
        {
            get
            {
                return new ViewModel.CommandWithMultParam((obj) =>
                {
                    var currentBarrel = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.Barrel)((object[])obj)[0];
                    var currentCommand = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand)((object[])obj)[1];

                    currentCommand.CommBarrels.Remove(currentBarrel);
                });
            }
        }
        public static ViewModel.Command AddChoiceToBarrel
        {
            get
            {
                return new ViewModel.Command((obj) =>
                {
                    var currentBarrel = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.Barrel)obj;
                    currentBarrel.BarrelChoices.Add(new Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.Barrel.Choice());
                });
            }
        }
        public static ViewModel.CommandWithMultParam DeleteChoiceFromBarrel
        {
            get
            {
                return new ViewModel.CommandWithMultParam((obj) =>
                {
                    var currentChoice = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.Barrel.Choice)((object[])obj)[0];
                    var currentBarrel = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.Barrel)((object[])obj)[1];

                    currentBarrel.BarrelChoices.Remove(currentChoice);
                });
            }
        }
        public static ViewModel.Command AddMemoryWriteToChoice
        {
            get
            {
                return new ViewModel.Command(obj =>
                {
                    var currentChoice = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.Barrel.Choice)obj;
                    currentChoice.ChoiceMemoryWrites.Add(new Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.CommandMemoryWrite());
                });
            }
        }
        public static ViewModel.CommandWithMultParam DeleteMemoryWritefromChoice
        {
            get
            {
                return new ViewModel.CommandWithMultParam((obj) =>
                {
                    var currentMemmoryWrite = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.CommandMemoryWrite)((object[])obj)[0];
                    var currentChoice = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.Barrel.Choice)((object[])obj)[1];

                    currentChoice.ChoiceMemoryWrites.Remove(currentMemmoryWrite);
                });
            }
        }
        public static ViewModel.CommandWithMultParam SliderChanceChangeValue
        {
            get
            {
                return new ViewModel.CommandWithMultParam(obj =>
                {
                    double currentSliderValue = (double)((object[])obj)[0];
                    var currentChoice = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.Barrel.Choice)((object[])obj)[1];
                    var barrel = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.Barrel)((object[])obj)[2];

                    float sumOtherSliderValues = 0;

                    //Определяем сумму остапльных шансов
                    foreach (var slid in barrel.BarrelChoices)
                    {
                        if (!slid.Equals(currentChoice))
                        {
                            sumOtherSliderValues += (float)Math.Round(slid.Chance, 2);
                        }
                    }


                    //Исключаем снижение суммы ниже 100
                    if ((Math.Round(sumOtherSliderValues, 0) == 0 || float.IsNaN(sumOtherSliderValues)) && Math.Round(currentChoice.Chance, 0) != 100)
                    {
                        foreach (var slid in barrel.BarrelChoices)
                        {
                            if (!slid.Equals(currentChoice))
                            {
                                slid.Chance = 0.1F;
                            }
                        }
                    }

                    //Исключаем нулевой коэффициент
                    float coefficient;

                    if (sumOtherSliderValues > 0)
                    {
                        coefficient = ((100 - currentChoice.Chance) / sumOtherSliderValues);
                    }
                    else
                    {
                        coefficient = 1;
                    }

                    //Пересчитываем шансы с учетом коэффициента
                    foreach (var slid in barrel.BarrelChoices)
                    {
                        if (!slid.Equals(currentChoice))
                        {
                            slid.Chance *= (float)Math.Round((float)coefficient, 2);
                        }
                    }
                });
            }
        }
        public static ViewModel.CommandWithMultParam LostMouseCapture
        {
            get
            {
                return new ViewModel.CommandWithMultParam(obj =>
                {
                    double currentSliderValue = (double)((object[])obj)[0];
                    var currentChoice = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.Barrel.Choice)((object[])obj)[1];
                    var barrel = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.Barrel)((object[])obj)[2];

                    float sumOtherSliderValues = 0;

                    //Определяем сумму остапльных шансов
                    foreach (var slid in barrel.BarrelChoices)
                    {
                        if (!slid.Equals(currentChoice))
                        {
                            sumOtherSliderValues += (float)Math.Round(slid.Chance, 2);
                        }
                    }

                    //Выравниваем значения если сумма шансов превысила 100 или оказалась меньше 100
                    if (Math.Round(sumOtherSliderValues + currentChoice.Chance, 2) > 100)
                    {
                        var ChoiceWithMaxChance = barrel.BarrelChoices.Where(obje => obje.Chance == barrel.BarrelChoices.Max(objec => objec.Chance)).First();
                        ChoiceWithMaxChance.Chance -= (float)Math.Round(sumOtherSliderValues + currentChoice.Chance, 2) - 100;
                    }
                    else if (Math.Round(sumOtherSliderValues + currentChoice.Chance, 2) < 100)
                    {
                        var ChoiceWithMaxChance = barrel.BarrelChoices.Where(obje => obje.Chance == barrel.BarrelChoices.Min(objec => objec.Chance)).First();
                        ChoiceWithMaxChance.Chance += 100 - (float)Math.Round(sumOtherSliderValues + currentChoice.Chance, 2);
                    }

                });
            }
        }


        public ViewModel.Command DeleteCommand
        {

            get
            {
                return new ViewModel.Command(obj =>
                {
                    var currentCommand = (Interactive.InteractiveSets.InteractiveSet.InteractiveCommand)obj;
                    SelectedSet.InteractiveSetCommandsList.Remove(currentCommand);

                });
            }

        }
        public ViewModel.Command RenameCommand
        {
            get
            {
                return new ViewModel.Command(obj =>
                {
                    Interactive.InteractiveSets.InteractiveSet.InteractiveCommand currentCommand = ((Interactive.InteractiveSets.InteractiveSet.InteractiveCommand)(((ContextMenu)((MenuItem)obj).Parent).DataContext));

                    RenameCommandDialogWindow RenameCommandDialogWindow = new RenameCommandDialogWindow();
                    RenameCommandDialogWindowViewModel currentDataContext = (RenameCommandDialogWindowViewModel)RenameCommandDialogWindow.DataContext;
                    currentDataContext.currentCommand = currentCommand;

                    currentDataContext.NewObjectName = currentCommand.Title;
                    RenameCommandDialogWindowViewModel.InitalName = currentCommand.Title;

                    RenameCommandDialogWindow.ShowDialog();

                });
            }
        }
        public static ViewModel.Command ImageUriBrowser
        {

            get
            {
                return new ViewModel.Command(obj =>
                {
                    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                    dlg.FileName = "image"; // Default file name
                                            //dlg.DefaultExt = ".gif"; // Default file extension
                    dlg.Filter = "Image files (*.png;*.gif;*.jpeg;*.jpg)|*.png;*.gif;*.jpeg;*.jpg|All files (*.*)|*.*"; // Filter files by extension

                    // Show open file dialog box
                    Nullable<bool> result = dlg.ShowDialog();

                    string RelativeImagePath;

                    //Если  Choice
                    if (obj is Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.Barrel.Choice Choice)
                    {
                        RelativeImagePath = @"\BarrelsImage\";

                        if (result == true)
                        {
                            //если файл с таким именем уже существует
                            if (File.Exists(Directory.GetCurrentDirectory() + RelativeImagePath + Path.GetFileName(dlg.FileName)))
                            {
                                Choice.ChoiceImageUri = RelativeImagePath + Path.GetFileName(dlg.FileName);
                            }
                            else
                            {
                                File.Copy(dlg.FileName, Directory.GetCurrentDirectory() + RelativeImagePath + Path.GetFileName(dlg.FileName));
                                Choice.ChoiceImageUri = RelativeImagePath + Path.GetFileName(dlg.FileName);
                            }
                        }
                        else
                        {
                            Choice.ChoiceImageUri = "";
                        }
                    }
                    //Если Alert
                    if (obj is Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.CommandAlert Alert)
                    {
                        RelativeImagePath = @"\AlertImages\";

                        if (result == true)
                        {
                            //если файл с таким именем уже существует
                            if (File.Exists(Directory.GetCurrentDirectory() + RelativeImagePath + Path.GetFileName(dlg.FileName)))
                            {
                                Alert.ImageUri = RelativeImagePath + Path.GetFileName(dlg.FileName);
                            }
                            else
                            {
                                File.Copy(dlg.FileName, Directory.GetCurrentDirectory() + RelativeImagePath + Path.GetFileName(dlg.FileName));
                                Alert.ImageUri = RelativeImagePath + Path.GetFileName(dlg.FileName);
                            }
                        }
                        else
                        {
                            Alert.ImageUri = "";
                        }
                    }
                });
            }

        }
        public static ViewModel.Command MusicUriBrowser
        {

            get
            {
                return new ViewModel.Command(obj =>
                {
                    Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                    dlg.FileName = "Music";

                    dlg.Filter = "Music files (*.WAVE;*.WAV;*.MP3)|*.WAVE;*.WAV;*.MP3|All files (*.*)|*.*";

                    // Show open file dialog box
                    Nullable<bool> result = dlg.ShowDialog();

                    //Если Alert
                    if (obj is Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.CommandAlert Alert)
                    {
                        string RelativeSoundPath = @"\AlertSounds\";

                        if (result == true)
                        {
                            //если файл с таким именем уже существует
                            if (File.Exists(Directory.GetCurrentDirectory() + RelativeSoundPath + Path.GetFileName(dlg.FileName)))
                            {
                                Alert.MusicUri = RelativeSoundPath + Path.GetFileName(dlg.FileName);
                            }
                            else
                            {
                                File.Copy(dlg.FileName, Directory.GetCurrentDirectory() + RelativeSoundPath + Path.GetFileName(dlg.FileName));
                                Alert.MusicUri = RelativeSoundPath + Path.GetFileName(dlg.FileName);
                            }
                        }
                        else
                        {
                            Alert.MusicUri = "";
                        }
                    }
                });
            }

        }
        public static ViewModel.Command ShowHelp
        {
            get
            {
                return new ViewModel.Command(obj =>
                {
                    MessageBox.Show("В сообщение донатной комманды можно добавлять следующие теги: \n" +
                                    "[donateSource]-Источник доната(сайт) \n" +
                                    "[donaterName]-Ник донатера \n" +
                                    "[donateAmount]-Сумма доната \n" +
                                    "[donateText]-текст донатного сообщения");
                });
            }
        }
    }
}
