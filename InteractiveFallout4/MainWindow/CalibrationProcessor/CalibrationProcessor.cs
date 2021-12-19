using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace InteractiveFallout4.MainWindow.CalibrationProcessor
{
    [StructLayout(LayoutKind.Sequential)]
    struct SYSTEM_INFO
    {
        public ushort processorArchitecture;
        ushort reserved;
        public uint pageSize;
        public IntPtr minimumApplicationAddress;
        public IntPtr maximumApplicationAddress;
        public IntPtr activeProcessorMask;
        public uint numberOfProcessors;
        public uint processorType;
        public uint allocationGranularity;
        public ushort processorLevel;
        public ushort processorRevision;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MEMORY_BASIC_INFORMATION
    {
        public UIntPtr BaseAddress;
        public UIntPtr AllocationBase;
        public uint AllocationProtect;
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }
    //[StructLayout(LayoutKind.Sequential)]
    struct MEMORY_BASIC_INFORMATION64
    {
        //Пояснения http://nmdsoft.blogspot.com/2010/10/memorybasicinformation.html
        public ulong BaseAddress; //Указатель на базовый адрес региона страниц. 
        public ulong AllocationBase;//Указатель на базовый адрес диапазона страниц ассигнован функцией VirtualAlloc. Страница, на которую указывает член BaseAddress, содержится в пределах этого диапазона распределения.
        public int AllocationProtect;//Выбор защиты памяти, когда область была первоначально ассигнована. Этот участник может быть одной из констант защиты памяти или 0, если у гостя нет доступа.
        public int __alignment1;
        public ulong RegionSize;//Размер в регионе, начиная с базового адреса, в которой все страницы имеют идентичные атрибуты, в байтах.  
        public int State;//Состояние страницы в регионе. Этот член может быть одним из следующих значений. 
        public int Protect;//    Защита доступа страницы в регионе. Этот член является одним из значений, перечисленных для элемента AllocationProtect. 
        public int Type;
        public int __alignment2;
    }
    class CalibrationProcessor
    {
        //public static bool CalibrationOnAction = false; 

        //Свойство указывающее активировано ли обывание калибровки или нет
        private static bool _IsCalibrationAborted = false;
        public static bool IsCalibrationAborted
        {
            get
            {

                var isMainWindowActive = System.Windows.Application.Current.Dispatcher.Invoke<bool>(() =>
                {
                    return System.Windows.Application.Current.MainWindow.IsActive;
                });

                //Если окно выбрано и нажат Esc останавливаем калибровку
                return (isMainWindowActive == true && Keyboard.IsKeyDown(Key.Escape) == true) ? true : false;
            }
            set
            {
                _IsCalibrationAborted = value;
            }
        }

        //Словарь с действующими адресами переменных
        public static Dictionary<string, ulong> checkedVariableAddreses = new Dictionary<string, ulong>();

        //////Загрузка методов библиотеки kernel32.dll необходимых для работы с памятью и процессами///

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, Int64 lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(int hProcess, ulong lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        //для чтения регионов
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern void GetSystemInfo(ref SYSTEM_INFO Info);
        [DllImport("kernel32.dll")]
        static extern int VirtualQueryEx(IntPtr hProcess, ulong lpAddress, ref MEMORY_BASIC_INFORMATION64 lpBuffer, ulong dwLength);
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, ulong lpBaseAddress, byte[] lpBuffer, long dwSize, ref int lpNumberOfBytesRead);

        ////////////////////////////////////////////////////////////////////////
        ///

        public static bool CheckGameVariableValue()
        {


            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                ((MainWindowViewModel)(System.Windows.Application.Current.MainWindow.DataContext)).SendMessageToChatRichTextBox("Поиск переменных..." + Environment.NewLine);
            });

            //Перед заполнением словаря очищаем его
            checkedVariableAddreses.Clear();

            //Открываем файл XML
            //XDocument xDocumentForVariable = XDocument.Load(XMLSuckersPath);
            //Выбираем свойства переменных из открытого XML файла
            //IEnumerable<XElement> xElementVariables = xDocumentForVariable.Root.Element("SuckersOptions").Element("SuckerVariables").Elements("SuckerVariable");
            //Стандартное значение
            string standartValue = InteractiveBuilder.Interactive.InteractiveVariables.StandartValue.ToString();
            //Массив с адресом ячейки
            //Int64[] memoryCellAddressArray;
            //Лист потенциальных переменных
            List<ulong> potentialVariableAddresses = new List<ulong>();

            /////Переменные для чтения регионов///////

            const int PROCESS_QUERY_INFORMATION = 0x0400;
            const int PROCESS_WM_READ = 0x0010;

            ulong currentAddress;//текущий адрес
            ulong maxAddress;//маскимально возможный адрес

            SYSTEM_INFO SysInfo = new SYSTEM_INFO();

            MEMORY_BASIC_INFORMATION64 memoryBaseInfo64 = new MEMORY_BASIC_INFORMATION64();

            //Информация по регионам
            Dictionary<ulong, ulong> RegionInformation = new Dictionary<ulong, ulong>();//RegionInformation<адрес региона,размер региона>

            //Масив задач для асинхронного поиска адресов переменных
            Task[] tasks;
            //////////////////////////////////////////

            //Поиск процесса и присвоение его объекту "process"
            Process gameProcessForVariable;
            //Проверка запущена ли игра
            bool processGameRun = false;

            //Проверка стандартных значений игровых переменных
            bool checkGameVariable;

            string GameName = "Fallout4";

            //Очищаем процесс перед его перезаписью
            gameProcessForVariable = null;
            //Поиск процесса и присвоение его объекту "process"
            gameProcessForVariable = Process.GetProcessesByName(GameName).FirstOrDefault();

            if (gameProcessForVariable != null)
            {
                processGameRun = true;
                checkGameVariable = true;
            }
            else
            {
                //MessageBox.Show("Необходимо запустить игру.");
                processGameRun = false;
                checkGameVariable = false;
            }

            if (processGameRun)
            {
                //chatTextBox.AppendText("Поиск адресов потенциальных переменных!" + Environment.NewLine);

                #region memory block

                GetSystemInfo(ref SysInfo);//получаем информацию о системе

                maxAddress = (ulong)SysInfo.maximumApplicationAddress;//максимальный адрес возможный в системе
                currentAddress = (ulong)SysInfo.minimumApplicationAddress;//текущий адрес устанавливаем минимально возможным

                //открытие процесса с требуемым уровнем доступа
                IntPtr processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_WM_READ, false, gameProcessForVariable.Id);

                //Перебираем регионы и записываем их в таблицу
                while (currentAddress < maxAddress)
                {
                    //Считываем данные региона
                    VirtualQueryEx(processHandle, currentAddress, ref memoryBaseInfo64, (ulong)Marshal.SizeOf(memoryBaseInfo64));
                    //Console.WriteLine("Address: " + memoryBaseInfo.BaseAddress.ToString("X8") + " Type: " + TypeToString(memoryBaseInfo.Type) + "\t\t" + " Protect: " + ProtectToString(memoryBaseInfo.Protect));

                    //Записываем данные региона если Protect равен 1(Read) или 4(Read/Write)
                    if (memoryBaseInfo64.Protect == 1 || memoryBaseInfo64.Protect == 4)
                    {
                        RegionInformation.Add(currentAddress, memoryBaseInfo64.RegionSize);
                    }

                    //Переключаем адрес текущего региона на следующий регион
                    currentAddress += memoryBaseInfo64.RegionSize;
                }

                #endregion memory block

                #region MemoryRead

                //Создаем массив задач по размеру равный кол-ву найденных регионов
                tasks = new Task[RegionInformation.Keys.Count];

                int i = 0;
                foreach (KeyValuePair<ulong, ulong> region in RegionInformation)
                {
                    tasks[i] = Task.Run(() => ReadRegionMemory((int)processHandle, region.Key, region.Value, float.Parse(standartValue), ref potentialVariableAddresses));
                    i++;
                }

                //Ждем завершения потоков в которых заполняется "Лист потенциальных переменных"
                Task.WaitAll(tasks);
                foreach (Task task in tasks)
                {
                    //Освобождаем русурсы занятые потоками
                    task.Dispose();
                }

                //Освобождаем память занятую записанными ранее регионами памяти
                RegionInformation.Clear();
                GC.Collect();

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ((MainWindowViewModel)(System.Windows.Application.Current.MainWindow.DataContext)).SendMessageToChatRichTextBox("Поиск завершен, для завершения калибровки запустите калибровку в игре!(Esc прервать калибровку)" + Environment.NewLine);
                });
                #endregion MemoryRead

                //Запускаем калибровку
                MemoryValueСalibration(ref checkedVariableAddreses, potentialVariableAddresses, gameProcessForVariable);

                //Если количество переменных в результате отличается от необходимого кол-ва тор возвращаем из метода калибровки false, что указывает программе что калибровка прервана.
                if (checkedVariableAddreses.Keys.Count != InteractiveBuilder.Interactive.InteractiveVariables.InteractiveVariablesList.Count() - 1)
                {
                    checkGameVariable = false;

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        ((MainWindowViewModel)(System.Windows.Application.Current.MainWindow.DataContext)).SendMessageToChatRichTextBox("Калибровка прервана!" + Environment.NewLine);
                    });
                }
                else
                {
                    //если калибровка завершена включаем консоль с командами
                    MainWindowViewModel.enableCommandConsole = true;

                    //добавляем в словарь стандартную переменную
                    checkedVariableAddreses.Add("Default", 0);

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        ((MainWindowViewModel)(System.Windows.Application.Current.MainWindow.DataContext)).SendMessageToChatRichTextBox("Калибровка завершена, интерактив к игре подключен!" + Environment.NewLine);
                    });
                }






            }

            return checkGameVariable;
        }
        static void MemoryValueСalibration(ref Dictionary<string, ulong> checkedVariableAddreses, List<ulong> potentialVariableAddresses, Process gameProcessForVariable)
        {

            ObservableCollection<InteractiveBuilder.Interactive.InteractiveVariables.InteractiveVariable> InteractiveVariable = InteractiveBuilder.Interactive.InteractiveVariables.InteractiveVariablesList;

            const int PROCESS_ALL_ACCESS = 0x1F0FFF;//Полный доступ
            IntPtr gameProcessHandle = OpenProcess(PROCESS_ALL_ACCESS, false, gameProcessForVariable.Id);



            int bytesWritten = 0;//хз для чего(Кол-во записанных байт?)
            byte[] injectionValue;//значение которое будет инжектироваться в память
            byte[] bufferAddress = new byte[8]; //Каждый символ в строковой переменной занимает 2 байта!, числовое значение int64 занимает 8 байт?(//'Hello World!' takes 12*2 bytes because of Unicode)
            injectionValue = BitConverter.GetBytes((float)0);

            //CalibrationOnAction = true;
            while (checkedVariableAddreses.Keys.Count != InteractiveVariable.Count() - 1 && !IsCalibrationAborted)//-1 для исключения Default переменной
            {
                foreach (ulong potentialAddress in potentialVariableAddresses)
                {
                    for (int i = 1; i < InteractiveVariable.Count(); i++)
                    {
                        //messa
                        if (СhechMemoryValueConcur(potentialAddress, InteractiveVariable[i].СalibrationValue.ToString(), gameProcessHandle))
                        {
                            //Если в проверенных переменных еще нет проверяемой переменной
                            if (checkedVariableAddreses.ContainsKey(InteractiveVariable[i].Variable) == false)
                            {
                                checkedVariableAddreses.Add(InteractiveVariable[i].Variable, potentialAddress);

                                WriteProcessMemory((int)gameProcessHandle, potentialAddress, injectionValue, injectionValue.Length, ref bytesWritten);

                                //mainForm1.chatTextBox.Invoke(new Action(() => { mainForm1.chatTextBox.AppendText("Переменная " + xElementVariables.ToArray()[i].Attribute("Variable").Value + " найдена!" + Environment.NewLine); }));
                            }
                        }
                    }
                }
            }
            //CalibrationOnAction = false;

        }
        private static bool СhechMemoryValueConcur(ulong MemoryAddress, string comparableValue, IntPtr gameProcessHandle)
        {
            ulong adressVariableInMemory = MemoryAddress;//Конечный адрес ячейки памяти для необходимой переменной
            byte[] valueAtAdressVariable = new byte[8];//Значение переменной найденной по указанному адресу

            int bytesRead = 0;//хз для чего(Кол-во прочитанных байт?)

            //Команда проходит проверку по значению переменной по адресу типа команды
            bool commandConditionsCompleteOncommandType = false;

            //Если строка адреса не пустая то проверяем значение в ней, а также если значение переменной не пусто
            if (MemoryAddress != 0)
            {
                //Чистаем значение из памяти
                ReadProcessMemory((int)gameProcessHandle, adressVariableInMemory, valueAtAdressVariable, valueAtAdressVariable.Length, ref bytesRead);

                //Проверяем условия из типа команды
                if (comparableValue == BitConverter.ToSingle(valueAtAdressVariable, 0).ToString())
                {
                    commandConditionsCompleteOncommandType = true;
                }
            }
            else//Если строка адреса пустая то принимаем проверку по адресу из типа команды атоматически пройденной
            {
                commandConditionsCompleteOncommandType = true;
            }

            return commandConditionsCompleteOncommandType;
        }

        static void ReadRegionMemory(int processHandle, ulong RegionAddress, ulong RegionSize, float standartValue, ref List<ulong> potentialVariableAddresses)
        {
            try
            {
                byte[] numberArray = new byte[4];
                numberArray = BitConverter.GetBytes(standartValue);
                byte[] buffer = new byte[RegionSize];
                int byteStartIndex = 0;
                ReadProcessMemory(processHandle, RegionAddress, buffer, buffer.Length, ref byteStartIndex);

                byte[] searchByte = new byte[4];

                for (ulong i = 0; i < RegionSize - 3; i++)
                {
                    searchByte[0] = buffer[i];
                    searchByte[1] = buffer[i + 1];
                    searchByte[2] = buffer[i + 2];
                    searchByte[3] = buffer[i + 3];

                    if (BitConverter.ToSingle(searchByte, 0) == standartValue)
                    {
                        potentialVariableAddresses.Add(RegionAddress + i);
                    }
                }
            }
            catch
            {

            }
        }
    }
}
