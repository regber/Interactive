using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.IO;
using System.Windows;
using System.ComponentModel;
using System.Windows.Threading;

namespace InteractiveFallout4.InteractiveBuilder
{
    public class Interactive : ViewModel.BaseViewModel
    {
        static bool GetInteractive { get; set; }
        static Interactive()
        {
            InteractiveLoad();
        }

        public static class InteractiveSets
        {
            public static string ActiveSet { get; set; }

            public static ObservableCollection<InteractiveSet> InteractiveSetList { get; set; } = new ObservableCollection<InteractiveSet>();

            static InteractiveSets()
            {
                Interactive.GetInteractive = true;
            }
            public class InteractiveSet
            {

                public string Title { get; set; }
                public ObservableCollection<InteractiveCommand> InteractiveSetCommandsList { get; set; } = new ObservableCollection<InteractiveCommand>();
                static InteractiveSet()
                {
                    Interactive.GetInteractive = true;
                }

                public class InteractiveCommand : ViewModel.BaseViewModel
                {
                    static InteractiveCommand()
                    {
                        Interactive.GetInteractive = true;
                    }

                    private string _Title;
                    public string Title
                    {
                        get
                        {
                            return _Title;
                        }
                        set
                        {
                            _Title = value;
                            OnPropertyChanged(nameof(Title));
                        }
                    }

                    //Payment
                    public CommandPayment CommPayment { get; set; }
                    public class CommandPayment:ViewModel.BaseViewModel
                    {
                        private string _Type;
                        public string Type 
                        { 
                            get
                            {
                                return _Type;
                            }
                            set
                            {
                                _Type = value;
                                OnPropertyChanged(nameof(Type));
                            }
                        }
                        private int _Price;
                        public int Price 
                        {
                            get
                            {
                                return _Price;
                            }
                            set
                            {
                                _Price = value;
                                OnPropertyChanged(nameof(Price));
                            }
                        }
                        public CommandPayment()
                        {
                            Type = "Donate";
                        }
                    }

                    //Command Type
                    public CommandType CommType { get; set; }
                    public class CommandType:ViewModel.BaseViewModel
                    {
                        private string _Type;
                        public string Type 
                        {
                            get
                            {
                                return _Type;
                            } 
                            set
                            {
                                _Type = value;
                                OnPropertyChanged(nameof(Type));
                            }
                        }

                        public string Variable { get; set; }
                        public string Text { get; set; }
                        public float Value { get; set; }
                        public CommandType()
                        {
                            Type = "Default";
                            Variable = "Default";
                        }
                        
                        public void SetDefault()
                        {
                            Type = "Default";
                            //Variable = "Default";
                        }
                    }

                    //Command Alert
                    private CommandAlert _CommAlert;
                    public CommandAlert CommAlert
                    {
                        get
                        {
                            return _CommAlert;
                        }
                        set
                        {
                            _CommAlert = value;
                            OnPropertyChanged(nameof(CommAlert));
                        }
                    }
                    public class CommandAlert : ViewModel.BaseViewModel
                    {
                        private string _ImageUri;
                        private string _MusicUri;
                        public bool Enable { get; set; }
                        public string ImageUri
                        {
                            get => _ImageUri;
                            set
                            {
                                _ImageUri = value;
                                OnPropertyChanged("ImageUri");
                            }
                        }
                        public string Voice { get; set; }
                        public int VoiceVolume { get; set; }
                        public string Text { get; set; }
                        public string MusicUri
                        {
                            get => _MusicUri;
                            set
                            {
                                _MusicUri = value;
                                OnPropertyChanged("MusicUri");
                            }
                        }
                        public int MusicVolume { get; set; }

                        public CommandAlert()
                        {
                            ImageUri = "";
                            Voice = "off";
                            Text = "";
                            MusicUri = "";
                        }
                    }

                    //Command Memory Read
                    public ObservableCollection<CommandMemoryRead> CommMemoryReads { get; set; } = new ObservableCollection<CommandMemoryRead>();
                    public class CommandMemoryRead:ViewModel.BaseViewModel
                    {
                        private string _Variable;
                        public string Variable 
                        { 
                            get
                            {
                                return _Variable;
                            }
                            set
                            {
                                _Variable = value;
                                OnPropertyChanged(nameof(Variable));
                            }
                        }
                        public string Text { get; set; }
                        public float Value { get; set; }
                        public CommandMemoryRead()
                        {
                            Variable = "Default";
                        }
                        public void SetDefault()
                        {
                            Variable = "Default";
                        }
                    }

                    //Command Memory Write
                    public ObservableCollection<CommandMemoryWrite> CommMemoryWrites { get; set; } = new ObservableCollection<CommandMemoryWrite>();
                    public class CommandMemoryWrite:ViewModel.BaseViewModel
                    {
                        private string _Variable;
                        public string Variable 
                        { 
                            get
                            {
                                return _Variable;
                            }
                            set
                            {
                                _Variable = value;
                                OnPropertyChanged(nameof(Variable));
                            }
                        }
                        public string Text { get; set; }
                        public float Value { get; set; }
                        public string Type { get; set; }

                        public CommandMemoryWrite()
                        {
                            Text = "";
                            Variable = "Default";
                            Type = "Write";
                        }
                        public void SetDefault()
                        {
                            //Text = "";
                            Variable = "Default";
                            //Type = "Write";
                        }
                    }

                    //Command Barrels
                    public ObservableCollection<Barrel> CommBarrels { get; set; } = new ObservableCollection<Barrel>();
                    public class Barrel
                    {
                        public string Text { get; set; } = "";

                        public ObservableCollection<Choice> BarrelChoices { get; set; } = new ObservableCollection<Choice>();
                        public class Choice: ViewModel.BaseViewModel
                        {
                            private string _ChoiceImageUri="";
                            public string ChoiceImageUri 
                            { 
                                get
                                {
                                    return _ChoiceImageUri;
                                }
                                set
                                {
                                    _ChoiceImageUri = value;
                                    OnPropertyChanged(nameof(ChoiceImageUri));
                                }
                            }

                            private float _Chance=100;
                            public float Chance 
                            { 
                                get
                                {
                                    return _Chance;
                                }
                                set
                                {
                                    _Chance = value;
                                    Task.Run(()=> OnPropertyChanged(nameof(Chance)));
                                } 
                            }

                            public ObservableCollection<CommandMemoryWrite> ChoiceMemoryWrites { get; set; } = new ObservableCollection<CommandMemoryWrite>();
                        
                        }
                    }


                }
            }
        }

        public static class InteractiveVariables
        {
            public static float StandartValue { get; set; }
            public static ObservableCollection<InteractiveVariable> InteractiveVariablesList { get;set; } = new ObservableCollection<InteractiveVariable>();

            static InteractiveVariables()
            {
                Interactive.GetInteractive = true;
            }

            public class InteractiveVariable : IDataErrorInfo
            {
                public string Variable { get; set; }
                public float СalibrationValue { get; set; }

                public string this[string columnName]
                {
                    get
                    {
                        string error = String.Empty;
                        switch (columnName)
                        {
                            case "Variable":
                                {
                                    if (InteractiveVariablesList.Where(obj => obj.Variable == Variable).Count() > 1 )
                                    {
                                        error = "Не должно быть двух переменных с одинаковыми именами";
                                    }
                                    else if(Variable == string.Empty)
                                    {
                                        error = "Имя переменной не может быть пустым";
                                    }
                                }
                                break;
                        }
                        return error;
                    }
                }
                public string Error
                {
                    get { throw new NotImplementedException(); }
                }
            }
        }

        public static class InteractiveCommandTypes
        {
            public static ObservableCollection<InteractiveCommandType> InteractiveCommandTypesList = new ObservableCollection<InteractiveCommandType>();

            static InteractiveCommandTypes()
            {
                Interactive.GetInteractive = true;
            }

            public class InteractiveCommandType : ViewModel.BaseViewModel,IDataErrorInfo
            {

                public string Type { get; set; }
                private string _Variable;
                public string Variable 
                {
                    get
                    {
                        return _Variable;
                    }
                    set
                    {
                        _Variable = value;
                        OnPropertyChanged(nameof(Variable));
                    }
                }
                public string Text { get; set; }
                public float Value { get; set; }

                public string this[string columnName]
                {
                    get
                    {
                        string error = String.Empty;
                        switch (columnName)
                        {
                            case "Type":
                                {
                                    if (InteractiveCommandTypesList.Where(obj => obj.Type == Type).Count() > 1)
                                    {
                                        error = "Не должно быть двух типов команд с одинаковыми именами";
                                    }
                                    else if(Type==string.Empty)
                                    {
                                        error = "Имя типа команды не может быть пустым";
                                    }    
                                }
                                break;
                        }
                        return error;
                    }
                }
                public string Error
                {
                    get { throw new NotImplementedException(); }
                }

                public void SetDefault()
                {
                    Variable = "Default";
                }
            }
        }


        public static void InteractiveLoad()
        {
            //Deserialize
            InteractiveSerialize interactivSerialize = new InteractiveSerialize();

            XmlSerializer formatter = new XmlSerializer(typeof(InteractiveSerialize));

            using (FileStream fs = new FileStream("Interactive.xml", FileMode.Open))
            {
                interactivSerialize = (InteractiveSerialize)formatter.Deserialize(fs);
            }

            //ActiveSet
            Interactive.InteractiveSets.ActiveSet = interactivSerialize.InteractiveSetsObject.ActiveSet;

            //InteractiveSets
            foreach (var set in interactivSerialize.InteractiveSetsObject.InteractiveSetList)
            {
                var currentSet = new InteractiveSets.InteractiveSet() { Title = set.Title };
                foreach (var comm in set.InteractiveSetCommandsList)
                {
                    //загружаем команду
                    currentSet.InteractiveSetCommandsList.Add(new InteractiveSets.InteractiveSet.InteractiveCommand()
                    {
                        Title = comm.Title,
                        CommPayment = new InteractiveSets.InteractiveSet.InteractiveCommand.CommandPayment() { Type = comm.CommPayment.Type, Price = comm.CommPayment.Price },
                        CommType = new InteractiveSets.InteractiveSet.InteractiveCommand.CommandType() { Type = comm.CommType.Type, Variable = comm.CommType.Variable, Text = comm.CommType.Text, Value = comm.CommType.Value },
                        //CommAlert=new InteractiveSets.InteractiveSet.InteractiveCommand.CommandAlert() { Enable=comm.CommAlert.Enable,  ImageUri = comm.CommAlert.ImageUri,  Voice = comm.CommAlert.Voice,  VoiceVolume = comm.CommAlert.VoiceVolume, Text=comm.CommAlert.Text, MusicUri=comm.CommAlert.MusicUri, MusicVolume=comm.CommAlert.MusicVolume}
                    });

                    //Загружаем алерт команды
                    if (comm.CommAlert != null)
                    {
                        currentSet.InteractiveSetCommandsList.Last().CommAlert = new InteractiveSets.InteractiveSet.InteractiveCommand.CommandAlert() { Enable = comm.CommAlert.Enable, ImageUri = comm.CommAlert.ImageUri, Voice = comm.CommAlert.Voice, VoiceVolume = comm.CommAlert.VoiceVolume, Text = comm.CommAlert.Text, MusicUri = comm.CommAlert.MusicUri, MusicVolume = comm.CommAlert.MusicVolume };
                    }

                    //Загружаем MemoryReads команды
                    if (comm.CommMemoryReads.Count > 0)
                    {
                        currentSet.InteractiveSetCommandsList.Last().CommMemoryReads = new ObservableCollection<InteractiveSets.InteractiveSet.InteractiveCommand.CommandMemoryRead>();
                        foreach (var commRead in comm.CommMemoryReads)
                        {
                            currentSet.InteractiveSetCommandsList.Last().CommMemoryReads.Add(new InteractiveSets.InteractiveSet.InteractiveCommand.CommandMemoryRead() { Variable = commRead.Variable, Text = commRead.Text, Value = commRead.Value });
                        }
                    }

                    //Загружаем MemoryWrites команды
                    if (comm.CommMemoryWrites.Count > 0)
                    {
                        currentSet.InteractiveSetCommandsList.Last().CommMemoryWrites = new ObservableCollection<Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.CommandMemoryWrite>();
                        foreach (var commWrite in comm.CommMemoryWrites)
                        {
                            currentSet.InteractiveSetCommandsList.Last().CommMemoryWrites.Add(new Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.CommandMemoryWrite() { Variable = commWrite.Variable, Text = commWrite.Text, Value = commWrite.Value, Type = commWrite.Type });
                        }
                    }

                    //Загружаем рулетку
                    if(comm.CommBarrels.Count>0)
                    {
                        //Загружаем барабаны
                        currentSet.InteractiveSetCommandsList.Last().CommBarrels = new ObservableCollection<InteractiveSets.InteractiveSet.InteractiveCommand.Barrel>();
                        foreach (var commBarrel in comm.CommBarrels)
                        {
                            currentSet.InteractiveSetCommandsList.Last().CommBarrels.Add(new InteractiveSets.InteractiveSet.InteractiveCommand.Barrel() { Text= commBarrel.Text});

                            //Загружаем варианты
                            foreach (var barrelChoice in commBarrel.BarrelChoices)
                            {
                                currentSet.InteractiveSetCommandsList.Last().CommBarrels.Last().BarrelChoices.Add(new InteractiveSets.InteractiveSet.InteractiveCommand.Barrel.Choice() { ChoiceImageUri= barrelChoice.ChoiceImageUri, Chance= barrelChoice.Chance });

                                //Загружаем MemoryWrite вариантов
                                foreach(var choiceMemoryWrite in barrelChoice.ChoiceMemoryWrites)
                                {
                                    currentSet.InteractiveSetCommandsList.Last().CommBarrels.Last().BarrelChoices.Last().ChoiceMemoryWrites.Add(new InteractiveSets.InteractiveSet.InteractiveCommand.CommandMemoryWrite() { Variable= choiceMemoryWrite.Variable, Text= choiceMemoryWrite.Text, Value= choiceMemoryWrite.Value, Type= choiceMemoryWrite.Type });
                                }
                            }
                        }
                    }
                }

                Interactive.InteractiveSets.InteractiveSetList.Add(currentSet);
            }

            //InteractiveVariables
            Interactive.InteractiveVariables.StandartValue = interactivSerialize.InteractiveVariablesObject.StandartValue;

            //Стандартная переменная нужная что бы в случае удаления всех переменных присваивалось хоть что то
            Interactive.InteractiveVariables.InteractiveVariablesList.Add(new InteractiveVariables.InteractiveVariable() { Variable = "Default", СalibrationValue = 0 });
            if (interactivSerialize.InteractiveVariablesObject.InteractiveVariablesList != null)
            {
                foreach (var var in interactivSerialize.InteractiveVariablesObject.InteractiveVariablesList)
                {
                    Interactive.InteractiveVariables.InteractiveVariablesList.Add(new InteractiveVariables.InteractiveVariable() { Variable = var.Variable, СalibrationValue = var.СalibrationValue });
                }
            }

            //InteractiveCommandTypes
            //Стандартный тип нужен что бы в случае удаления всех типов присваивалось хоть что то
            Interactive.InteractiveCommandTypes.InteractiveCommandTypesList.Add(new Interactive.InteractiveCommandTypes.InteractiveCommandType() { Variable = "Default", Type = "Default", Text = "Default", Value = 0 });
            if (interactivSerialize.InteractiveCommandTypesObject.InteractiveCommandTypesList != null)
            {
                foreach (var type in interactivSerialize.InteractiveCommandTypesObject.InteractiveCommandTypesList)
                {
                    Interactive.InteractiveCommandTypes.InteractiveCommandTypesList.Add(new InteractiveCommandTypes.InteractiveCommandType() { Variable = type.Variable, Type = type.Type, Text = type.Text, Value = type.Value });
                }
            }
        }
        public static void InteractiveSave()
        {

            InteractiveSerialize IntSerialize = new InteractiveSerialize();

            //ActiveSet
            IntSerialize.InteractiveSetsObject.ActiveSet = Interactive.InteractiveSets.ActiveSet;

            //InteractiveSets
            IntSerialize.InteractiveSetsObject.InteractiveSetList = new ObservableCollection<InteractiveSerialize.InteractiveSets.InteractiveSet>();

            //InteractiveSets
            foreach (var set in Interactive.InteractiveSets.InteractiveSetList)
            {
                var currentSet = new InteractiveSerialize.InteractiveSets.InteractiveSet { Title = set.Title };
                foreach (var comm in set.InteractiveSetCommandsList)
                {
                    //Заполняем текущий набор командой
                    currentSet.InteractiveSetCommandsList.Add(new InteractiveSerialize.InteractiveSets.InteractiveSet.InteractiveCommand()
                    {
                        Title = comm.Title,
                        CommPayment = new InteractiveSerialize.InteractiveSets.InteractiveSet.InteractiveCommand.CommandPayment() { Type = comm.CommPayment.Type, Price = comm.CommPayment.Price },
                        CommType = new InteractiveSerialize.InteractiveSets.InteractiveSet.InteractiveCommand.CommandType() { Type = comm.CommType.Type, Variable = comm.CommType.Variable, Text = comm.CommType.Text, Value = comm.CommType.Value },
                        //CommAlert = new InteractiveSerialize.InteractiveSets.InteractiveSet.InteractiveCommand.CommandAlert() { Enable = comm.CommAlert.Enable, ImageUri = comm.CommAlert.ImageUri, Voice = comm.CommAlert.Voice, VoiceVolume = comm.CommAlert.VoiceVolume, Text = comm.CommAlert.Text, MusicUri = comm.CommAlert.MusicUri, MusicVolume = comm.CommAlert.MusicVolume }
                    });

                    //Заполняем алерт команды
                    if (comm.CommAlert != null)
                    {
                        currentSet.InteractiveSetCommandsList.Last().CommAlert = new InteractiveSerialize.InteractiveSets.InteractiveSet.InteractiveCommand.CommandAlert() { Enable = comm.CommAlert.Enable, ImageUri = comm.CommAlert.ImageUri, Voice = comm.CommAlert.Voice, VoiceVolume = comm.CommAlert.VoiceVolume, Text = comm.CommAlert.Text, MusicUri = comm.CommAlert.MusicUri, MusicVolume = comm.CommAlert.MusicVolume };
                    }

                    //Заполняем MemoryReads команды
                    if (comm.CommMemoryReads.Count > 0)
                    {
                        currentSet.InteractiveSetCommandsList.Last().CommMemoryReads = new ObservableCollection<InteractiveSerialize.InteractiveSets.InteractiveSet.InteractiveCommand.CommandMemoryRead>();
                        foreach (var commRead in comm.CommMemoryReads)
                        {
                            currentSet.InteractiveSetCommandsList.Last().CommMemoryReads.Add(new InteractiveSerialize.InteractiveSets.InteractiveSet.InteractiveCommand.CommandMemoryRead() { Variable = commRead.Variable, Text = commRead.Text, Value = commRead.Value });
                        }
                    }


                    //Заполняем MemoryWrites команды
                    if (comm.CommMemoryWrites.Count > 0)
                    {
                        currentSet.InteractiveSetCommandsList.Last().CommMemoryWrites = new ObservableCollection<InteractiveSerialize.InteractiveSets.InteractiveSet.InteractiveCommand.CommandMemoryWrite>();
                        foreach (var commWrite in comm.CommMemoryWrites)
                        {
                            currentSet.InteractiveSetCommandsList.Last().CommMemoryWrites.Add(new InteractiveSerialize.InteractiveSets.InteractiveSet.InteractiveCommand.CommandMemoryWrite() { Variable = commWrite.Variable, Text = commWrite.Text, Value = commWrite.Value, Type = commWrite.Type });
                        }
                    }

                    //Загружаем рулетку
                    if (comm.CommBarrels.Count > 0)
                    {
                        //Загружаем барабаны
                        currentSet.InteractiveSetCommandsList.Last().CommBarrels = new ObservableCollection<InteractiveSerialize.InteractiveSets.InteractiveSet.InteractiveCommand.Barrel>();
                        foreach (var commBarrel in comm.CommBarrels)
                        {
                            currentSet.InteractiveSetCommandsList.Last().CommBarrels.Add(new InteractiveSerialize.InteractiveSets.InteractiveSet.InteractiveCommand.Barrel() { Text = commBarrel.Text });

                            //Загружаем варианты
                            foreach (var barrelChoice in commBarrel.BarrelChoices)
                            {
                                currentSet.InteractiveSetCommandsList.Last().CommBarrels.Last().BarrelChoices.Add(new InteractiveSerialize.InteractiveSets.InteractiveSet.InteractiveCommand.Barrel.Choice() { ChoiceImageUri = barrelChoice.ChoiceImageUri, Chance = barrelChoice.Chance });

                                //Загружаем MemoryWrite вариантов
                                foreach (var choiceMemoryWrite in barrelChoice.ChoiceMemoryWrites)
                                {
                                    currentSet.InteractiveSetCommandsList.Last().CommBarrels.Last().BarrelChoices.Last().ChoiceMemoryWrites.Add(new InteractiveSerialize.InteractiveSets.InteractiveSet.InteractiveCommand.CommandMemoryWrite() { Variable = choiceMemoryWrite.Variable, Text = choiceMemoryWrite.Text, Value = choiceMemoryWrite.Value, Type = choiceMemoryWrite.Type });
                                }
                            }
                        }
                    }
                }

                IntSerialize.InteractiveSetsObject.InteractiveSetList.Add(currentSet);
            }

            //InteractiveVariables
            IntSerialize.InteractiveVariablesObject.StandartValue = Interactive.InteractiveVariables.StandartValue;

            foreach (var var in Interactive.InteractiveVariables.InteractiveVariablesList)
            {
                if (var.Variable != "Default")
                {
                    IntSerialize.InteractiveVariablesObject.InteractiveVariablesList.Add(new InteractiveSerialize.InteractiveVariables.InteractiveVariable() { Variable = var.Variable, СalibrationValue = var.СalibrationValue });
                }
            }

            //InteractiveCommandTypes
            foreach (var type in Interactive.InteractiveCommandTypes.InteractiveCommandTypesList)
            {
                if (type.Type != "Default")
                {
                    IntSerialize.InteractiveCommandTypesObject.InteractiveCommandTypesList.Add(new InteractiveSerialize.InteractiveCommandTypes.InteractiveCommandType() { Variable = type.Variable, Type = type.Type, Text = type.Text, Value = type.Value });
                }
            }


            //Serialize
            XmlSerializer formatter = new XmlSerializer(typeof(InteractiveSerialize));
            using (FileStream fs = new FileStream("Interactive.xml", FileMode.Create))
            {
                formatter.Serialize(fs, IntSerialize);
            }

        }
    }

    [Serializable]
    [XmlRoot("Interactive")]
    public class InteractiveSerialize
    {

        [XmlElement("InteractiveSets")]
        public InteractiveSets InteractiveSetsObject { get; set; } = new InteractiveSets();

        public class InteractiveSets
        {
            [XmlAttribute]
            public string ActiveSet { get; set; }

            [XmlElement("InteractiveSet")]
            public ObservableCollection<InteractiveSet> InteractiveSetList { get; set; }// = new ObservableCollection<InteractiveSet>();
            public class InteractiveSet
            {
                [XmlAttribute]
                public string Title { get; set; }

                [XmlElement("InteractiveCommand")]
                public ObservableCollection<InteractiveCommand> InteractiveSetCommandsList { get; set; } = new ObservableCollection<InteractiveCommand>();

                public class InteractiveCommand
                {
                    [XmlAttribute]
                    public string Title { get; set; }

                    [XmlElement("CommandPayment")]
                    public CommandPayment CommPayment { get; set; }
                    public class CommandPayment
                    {
                        [XmlAttribute]
                        public string Type { get; set; }
                        [XmlText]
                        public int Price { get; set; }
                    }

                    [XmlElement("CommandType")]
                    public CommandType CommType { get; set; }

                    public class CommandType
                    {
                        [XmlAttribute]
                        public string Type { get; set; }
                        [XmlAttribute]
                        public string Variable { get; set; }
                        [XmlAttribute]
                        public string Text { get; set; }
                        [XmlText]
                        public float Value { get; set; }
                    }
                    [XmlElement("CommandAlert")]
                    public CommandAlert CommAlert { get; set; }
                    public class CommandAlert
                    {
                        [XmlAttribute]
                        public bool Enable { get; set; }
                        [XmlAttribute]
                        public string ImageUri { get; set; }
                        [XmlAttribute]
                        public string Voice { get; set; }
                        [XmlAttribute]
                        public int VoiceVolume { get; set; }
                        [XmlAttribute]
                        public string Text { get; set; }
                        [XmlAttribute]
                        public string MusicUri { get; set; }
                        [XmlAttribute]
                        public int MusicVolume { get; set; }

                        public CommandAlert()
                        {
                            Voice = "off";
                        }
                    }
                    //Command Memory Read
                    [XmlArray("CommandMemoryReads")]
                    public ObservableCollection<CommandMemoryRead> CommMemoryReads { get; set; }
                    public class CommandMemoryRead
                    {
                        [XmlAttribute]
                        public string Variable { get; set; }
                        [XmlAttribute]
                        public string Text { get; set; }
                        [XmlText]
                        public float Value { get; set; }
                    }

                    //Command Memory Write
                    [XmlArray("CommandMemoryWrites")]
                    public ObservableCollection<CommandMemoryWrite> CommMemoryWrites { get; set; }
                    public class CommandMemoryWrite
                    {
                        [XmlAttribute]
                        public string Variable { get; set; }
                        [XmlAttribute]
                        public string Text { get; set; }
                        [XmlText]
                        public float Value { get; set; }
                        [XmlAttribute]
                        public string Type { get; set; }
                    }

                    //Command Barrels
                    [XmlArray("CommandBarrels")]
                    public ObservableCollection<Barrel> CommBarrels { get; set; } = new ObservableCollection<Barrel>();
                    public class Barrel
                    {
                        [XmlAttribute]
                        public string Text { get; set; }

                        [XmlElement("Choice")]
                        public ObservableCollection<Choice> BarrelChoices { get; set; } = new ObservableCollection<Choice>();
                        public class Choice
                        {
                            [XmlAttribute]
                            public string ChoiceImageUri { get; set; }

                            [XmlAttribute]
                            public float Chance { get; set; } = 0;
                            [XmlElement("ChoiceMemoryWrite")]
                            public ObservableCollection<CommandMemoryWrite> ChoiceMemoryWrites { get; set; } = new ObservableCollection<CommandMemoryWrite>();

                        }
                    }
                }
            }
        }

        [XmlElement("InteractiveVariables")]
        public InteractiveVariables InteractiveVariablesObject { get; set; } = new InteractiveVariables();

        public class InteractiveVariables
        {
            [XmlAttribute]
            public float StandartValue { get; set; }

            [XmlElement("InteractiveVariable")]
            public ObservableCollection<InteractiveVariable> InteractiveVariablesList { get; set; } = new ObservableCollection<InteractiveVariable>();

            public class InteractiveVariable
            {
                [XmlAttribute]
                public string Variable { get; set; }
                [XmlAttribute]
                public float СalibrationValue { get; set; }
            }
        }

        [XmlElement("InteractiveCommandTypes")]
        public InteractiveCommandTypes InteractiveCommandTypesObject { get; set; } = new InteractiveCommandTypes();
        public class InteractiveCommandTypes
        {


            [XmlElement("InteractiveCommandType")]
            public ObservableCollection<InteractiveCommandType> InteractiveCommandTypesList { get; set; } = new ObservableCollection<InteractiveCommandType>();

            public class InteractiveCommandType
            {

                [XmlAttribute]
                public string Type { get; set; }
                [XmlAttribute]
                public string Variable { get; set; }
                [XmlAttribute]
                public string Text { get; set; }
                [XmlText]
                public float Value { get; set; }
            }
        }


    }
}
