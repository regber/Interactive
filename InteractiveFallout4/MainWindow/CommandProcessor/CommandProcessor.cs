using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Interactive = InteractiveFallout4.InteractiveBuilder.Interactive;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;

namespace InteractiveFallout4.MainWindow.CommandProcessor
{
    class CommandProcessor
    {
        public static bool commandInAction = false;

        bool commandConditionsCompleteOnCommandType = false;

        bool commandConditionsCompleteOnCommandTypeMemoriesRead = false;

        private static List<Tuple<Interactive.InteractiveSets.InteractiveSet.InteractiveCommand, Newtonsoft.Json.Linq.JObject>> currentCommandList;
        private static Tuple<Interactive.InteractiveSets.InteractiveSet.InteractiveCommand, Newtonsoft.Json.Linq.JObject> currentCommandObject;
        private static Interactive.InteractiveSets.InteractiveSet.InteractiveCommand currentCommand;
        private static Interactive.InteractiveSets.InteractiveSet.InteractiveCommand.CommandAlert currentAlert;
        private static Newtonsoft.Json.Linq.JObject currentMessage;

        static CommandProcessor()
        {
            OldAlertWindow.AlertWindow.AlertComplited += AlertComplited;
            InteractiveRoulette.InteractiveRouletteWindow.GetWindow().RouletteAnimationEnd += RouletteAnimationEnd;
        }
        public CommandProcessor()
        {
            System.Threading.Thread.CurrentThread.SetApartmentState(System.Threading.ApartmentState.STA);
        }

        private static void RouletteAnimationEnd(object sender, List<int> argum)
        {
            //Запускаем команду после окончания рулетки
            StartCommandProcessing(argum);
        }

        private static void AlertComplited(object sender, EventArgs e)
        {
            //если команда не содержит барабаны для проигрывания в рулетке, то запускаем команду, иначе запускаем рулетку
            if (currentCommand != null)//костыль, чтобы завершение алерта без команды не пыталось запустить команду, т.к. событие реагирует на любоей завершение event'a в том числе и из других окон
            {
                if (currentCommand.CommBarrels.Count() == 0)
                {
                    StartCommandProcessing();
                }
                else
                {
                    StartRouletteProcessing();
                }
            }

        }

        //////Загрузка методов библиотеки kernel32.dll необходимых для работы с памятью и процессами///
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, Int64 lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, ulong lpBaseAddress, byte[] lpBuffer, long dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(int hProcess, ulong lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        ////////////////////////////////////////////////////////////////////////

        //Установить доступность процесса для:
        const int PROCESS_WM_READ = 0x0010;//Чтения
        const int PROCESS_VM_WRITE = 0x0020;//Записи
        const int PROCESS_VM_OPERATION = 0x0008;//Операций?(нигде не использовал)
        const int PROCESS_ALL_ACCESS = 0x1F0FFF;//Полный доступ
        const int PROCESS_QUERY_LIMITED_INFORMATION = 0x00001000;
        const int PROCESS_SET_INFORMATION = 0x0200;

        //Игра для которой меняются переменные
        public static string GameName = "Fallout4";

        //Поиск процесса и присвоение его объекту "process"
        static Process gameProcess;// = Process.GetProcessesByName(GameName).FirstOrDefault();
        //получение дискриптора/указателя процесса?
        static IntPtr gameProcessHandle;// = OpenProcess(PROCESS_ALL_ACCESS, false, process.Id);
        //Проверка запущена ли игра
        public bool processGameRun = false;
        private bool CheckGameProcessRun()
        {
            //Очищаем процесс перед его перезаписью
            gameProcess = null;
            //Поиск процесса и присвоение его объекту "process"
            gameProcess = Process.GetProcessesByName(GameName).FirstOrDefault();

            if (gameProcess != null)
            {
                return true;

            }
            else
            {
                return false;
            }
        }
        private static bool СhechMemoryValueConcur(ulong MemoryAddress, float comparableValue, IntPtr gameProcessHandle)
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
                if (comparableValue == BitConverter.ToSingle(valueAtAdressVariable, 0))
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

        public void StartCommand(List<Tuple<Interactive.InteractiveSets.InteractiveSet.InteractiveCommand, Newtonsoft.Json.Linq.JObject>> commandsList)
        {
            commandInAction = true;//текущее состояние запуска команды, команда в процессе выполнения
            currentCommandList = commandsList;

            //Проверяем есть ли в очереди команды, а также проверяем запущена ли игра
            if (commandsList.Count > 0 && CheckGameProcessRun())
            {
                foreach (var commandObject in commandsList)
                {
                    //Записываем информацию по текущей команде 
                    currentCommandObject = commandObject;
                    currentCommand = currentCommandObject.Item1;
                    currentAlert = currentCommandObject.Item1.CommAlert;
                    currentMessage = commandObject.Item2;


                    //Заполняем данные по адресу и значению из типа команды
                    ulong memmoryReadOnCommandTypeAdress = CalibrationProcessor.CalibrationProcessor.checkedVariableAddreses[currentCommand.CommType.Variable];
                    float memmoryReadOnCommandTypeValue = currentCommand.CommType.Value;

                    //Проверяем равно ли значение из типа команды со сначением найденным по адресу указанному в типе команды
                    commandConditionsCompleteOnCommandType = СhechMemoryValueConcur(memmoryReadOnCommandTypeAdress, memmoryReadOnCommandTypeValue, gameProcessHandle);

                    commandConditionsCompleteOnCommandTypeMemoriesRead = true;

                    if (currentCommand.CommMemoryReads.Count() != 0)
                    {
                        foreach (var memmoryRead in currentCommand.CommMemoryReads)
                        {
                            if (СhechMemoryValueConcur(CalibrationProcessor.CalibrationProcessor.checkedVariableAddreses[memmoryRead.Variable], memmoryRead.Value, gameProcessHandle) != true)
                            {
                                commandConditionsCompleteOnCommandTypeMemoriesRead = false;
                                break;
                            }
                        }
                    }

                    //Если команда прошла проверки по значениям, обрываем цикл и переходим к обработкуе программы, иначе проверяем если было проверено последнее значение листа,
                    //а команда так и не найдена то завершаем обработку и переводим commandInAction в положение false для дальнейшей обработки команд и алертов в MessageProcessor
                    if (commandConditionsCompleteOnCommandType && commandConditionsCompleteOnCommandTypeMemoriesRead)
                    {
                        break;
                    }
                    else if (commandObject == commandsList.Last())
                    {
                        commandInAction = false;//текущее состояние запуска команды, команда НЕ в процессе выполнения
                    }
                }

                //Если проверенные значения из памяти равны заданным то запускаем команду
                if (commandConditionsCompleteOnCommandType && commandConditionsCompleteOnCommandTypeMemoriesRead)//Сюда прописать проверку условий из команды
                {
                    //Если интерактив был вызван из чата
                    if (currentMessage != null)
                    {
                        //MessageBox.Show("comanda s alertom");

                        //Если в команде есть алерт он включен и окно алертов видимо, то запускаем алерт из команды, по завершению которого вызываем команду
                        if (currentCommand.CommAlert != null && currentCommand.CommAlert.Enable && MainWindowViewModel.AlertsWindowThread != null && OldAlertWindow.AlertWindow.GetWindow().IsVisible/* && OldAlertWindow.AlertWindow.GetWindow().IsVisible*/)
                        {
                            //Запускаем алерт после которого будет запущена сама команда
                            StartAlertProcessing();
                        }
                        else//Если команда была вызвана из чата но алерт в команде отключен или окно AlertWindow скрыто то запускаем команду как обычно, либо с рулеткой если она есть, либо без нее
                        {
                            //MessageBox.Show("ne pokazivat' alert");
                            //Если в команде есть рулетка
                            if (currentCommand.CommBarrels.Count() != 0)
                            {
                                //Вызываем запуск рулетки в слот машине
                                StartRouletteProcessing();
                            }
                            else//Если в команде нет рулетки
                            {
                                StartCommandProcessing();
                            }
                        }
                    }
                    else//Если интерактив был вызван стримером через консоль
                    {
                        //Если в команде есть рулетка
                        if (currentCommand.CommBarrels.Count() != 0)
                        {
                            //Вызываем запуск рулетки в слот машине
                            StartRouletteProcessing();
                        }
                        else//Если в команде нет рулетки
                        {
                            StartCommandProcessing();
                        }
                    }
                }
            }
            else
            {
                commandInAction = false;//текущее состояние запуска команды, команда НЕ в процессе выполнения
            }


        }


        private static void StartCommandProcessing()
        {

            //Переменные
            try
            {
                gameProcessHandle = OpenProcess(PROCESS_ALL_ACCESS, false, gameProcess.Id);
            }
            catch
            {
                //тупо для избежания ошибки
            }
            Int64 processBaseAddress = (Int64)gameProcess.MainModule.BaseAddress;//Получаем базовый адрес эксешника игры
            int bytesRead = 0;//хз для чего
            int bytesWritten = 0;//хз для чего
            byte[] injectionValue;//значение которое будет инжектироваться в память
            ulong finalPointAddress = 0;
            byte[] bufferAddress = new byte[8]; //Каждый символ в строковой переменной занимает 2 байта!, числовое значение int64 занимает 8 байт?(//'Hello World!' takes 12*2 bytes because of Unicode)
            byte[] valueFromAdressMemmoryCell = new byte[8];//Значение переменной найденной по указанному адресу

            //Если в текущей команде есть запись в память то записываем значения из него
            if (currentCommand.CommMemoryWrites.Count() != 0)
            {
                foreach (var memoryWrite in currentCommand.CommMemoryWrites)
                {
                    //Если в массив с адресами памяти не является пустым(проверяем нулевой элемент т.к. команда Split создает массив с одним значением независимо от того было ли что то в строке или нет)
                    if (CalibrationProcessor.CalibrationProcessor.checkedVariableAddreses[memoryWrite.Variable]!=0)
                    {
                        //Запись финального адреса искомой переменной в которой будет меняться значение
                        finalPointAddress = CalibrationProcessor.CalibrationProcessor.checkedVariableAddreses[memoryWrite.Variable];

                        //Конвертируем значение с плавующей точкой в массив байтов
                        injectionValue = BitConverter.GetBytes(Convert.ToSingle(memoryWrite.Value));
                        

                        //Запись в память поверх старого значения(Write)
                        if (memoryWrite.Type == "Write")
                        {
                            //Записываем значение в указанную ячейку памяти
                            WriteProcessMemory((int)gameProcessHandle, finalPointAddress, injectionValue, injectionValue.Length, ref bytesWritten);
                        }

                        //Запись в память суммы того значения что было в ячейки памяти и того что передаем в ячейку памяти(GetAddAndWrite)
                        if (memoryWrite.Type == "GetAddAndWrite")
                        {
                            valueFromAdressMemmoryCell = new byte[8];
                            //Чистаем значение из памяти
                            ReadProcessMemory((int)gameProcessHandle, finalPointAddress, valueFromAdressMemmoryCell, valueFromAdressMemmoryCell.Length, ref bytesRead);

                            //Складываем значение прочитанное из памяти с передаваемым значением
                            injectionValue = BitConverter.GetBytes(BitConverter.ToSingle(valueFromAdressMemmoryCell, 0) + BitConverter.ToSingle(injectionValue, 0));

                            //Записываем полученное значение в указанную ячейку памяти
                            WriteProcessMemory((int)gameProcessHandle, finalPointAddress, injectionValue, injectionValue.Length, ref bytesWritten);
                        }

                        //Запись в память произведения того значения что было в ячейки памяти и того что передаем в ячейку памяти(GetMultipleAndWrite)
                        if (memoryWrite.Type == "GetMultipleAndWrite")
                        {
                            valueFromAdressMemmoryCell = new byte[8];
                            //Чистаем значение из памяти
                            ReadProcessMemory((int)gameProcessHandle, finalPointAddress, valueFromAdressMemmoryCell, valueFromAdressMemmoryCell.Length, ref bytesRead);

                            //Перемножаем значение прочитанное из памяти с передаваемым значением
                            injectionValue = BitConverter.GetBytes(Convert.ToSingle(BitConverter.ToSingle(valueFromAdressMemmoryCell, 0) * BitConverter.ToSingle(injectionValue, 0)));

                            //Записываем значение в указанную ячейку памяти
                            WriteProcessMemory((int)gameProcessHandle, finalPointAddress, injectionValue, injectionValue.Length, ref bytesWritten);
                        }

                        //Запись в память деления того значения что было в ячейки памяти и того что передаем в ячейку памяти(GetDevideAndWrite)
                        if (memoryWrite.Type == "GetDevideAndWrite")
                        {
                            valueFromAdressMemmoryCell = new byte[8];
                            //Чистаем значение из памяти
                            ReadProcessMemory((int)gameProcessHandle, finalPointAddress, valueFromAdressMemmoryCell, valueFromAdressMemmoryCell.Length, ref bytesRead);

                            //Делим значение прочитанное из памяти на передаваемое значение
                            injectionValue = BitConverter.GetBytes(Convert.ToSingle(BitConverter.ToSingle(valueFromAdressMemmoryCell, 0) / BitConverter.ToSingle(injectionValue, 0)));

                            //Записываем значение в указанную ячейку памяти
                            WriteProcessMemory((int)gameProcessHandle, finalPointAddress, injectionValue, injectionValue.Length, ref bytesWritten);
                        }
                    }
                }
            }

            

            //Удаляем команду из очереди
            currentCommandList.Remove(currentCommandObject);

            //очищаем переменные
            currentCommandObject = null;
            currentCommand = null;
            currentAlert = null;
            currentMessage = null;

            commandInAction = false;//текущее состояние запуска команды, команда НЕ в процессе выполнения


        }
        private static void StartCommandProcessing(List<int> rouletteResult)
        {
            //Переменные
            try
            {
                gameProcessHandle = OpenProcess(PROCESS_ALL_ACCESS, false, gameProcess.Id);
            }
            catch
            {
                //тупо для избежания ошибки
            }
            Int64 processBaseAddress = (Int64)gameProcess.MainModule.BaseAddress;//Получаем базовый адрес эксешника игры
            int bytesRead = 0;//хз для чего
            int bytesWritten = 0;//хз для чего
            byte[] injectionValue;//значение которое будет инжектироваться в память
            ulong finalPointAddress = 0;
            byte[] bufferAddress = new byte[8]; //Каждый символ в строковой переменной занимает 2 байта!, числовое значение int64 занимает 8 байт?(//'Hello World!' takes 12*2 bytes because of Unicode)
            byte[] valueFromAdressMemmoryCell = new byte[8];//Значение переменной найденной по указанному адресу

            //Если в команде есть барабан
            if (currentCommand.CommBarrels.Count()!=0)
            {
                //int barrelNumber = 0;
                //Проходим все барабаны в команде
                for (int k = 0; k < rouletteResult.Count(); k++)
                {
                    //Если номер текущего барабана не превышает текущего количества доступных барабанов согласно донату
                    //if (k + 1 <= GetCurrentCountBarrels() && (k + 1) <= winPositionInBarrels.Count()) /* (k + 1)<= winPositionInBarrels.Count()-косталь не позволяющий зависнуть проге при сплоном донате когда меняется число барабанов*/
                    //{
                        if (rouletteResult[k] != -1)//Если выйгрышный вариант для данного барабана не равен -1(-1 принимается если барабан настроен не правильно)
                        {
                            //Перебираем текущий барабан и выбираем из его выпавшего "Варианта" запись в память
                            foreach (var memoryWrite in currentCommand.CommBarrels.ToArray()[k].BarrelChoices.ToArray()[rouletteResult[k]].ChoiceMemoryWrites)
                            {

                                //Если в массив с адресами памяти не является пустым(проверяем нулевой элемент т.к. команда Split создает массив с одним значением независимо от того было ли что то в строке или нет)
                                if (CalibrationProcessor.CalibrationProcessor.checkedVariableAddreses[memoryWrite.Variable]!=0/* InternalCheckedVariableAddreses[memoryWrite.Attribute("Variable").Value] != 0*/)
                                {
                                    //Запись финального адреса искомой переменной в которой будет меняться значение, а она как уже было ранее равна предпоследнему адресу найденного как ссылка плюс последнее смещение в массиве addressOffset
                                    finalPointAddress = CalibrationProcessor.CalibrationProcessor.checkedVariableAddreses[memoryWrite.Variable];

                                    injectionValue = BitConverter.GetBytes(Convert.ToSingle(memoryWrite.Value));

                                    //Запись в память поверх старого значения
                                    if (memoryWrite.Type == "Write")
                                    {
                                        //Записываем значение в указанную ячейку памяти
                                        WriteProcessMemory((int)gameProcessHandle, finalPointAddress, injectionValue, injectionValue.Length, ref bytesWritten);
                                    }
                                    //Запись в память суммы того значения что было в ячейки памяти и того что передаем в ячейку памяти(GetAddAndWrite)
                                    if (memoryWrite.Type == "GetAddAndWrite")
                                    {
                                        valueFromAdressMemmoryCell = new byte[8];
                                        //Чистаем значение из памяти
                                        ReadProcessMemory((int)gameProcessHandle, finalPointAddress, valueFromAdressMemmoryCell, valueFromAdressMemmoryCell.Length, ref bytesRead);

                                        //MessageBox.Show(BitConverter.ToSingle(injectionValue, 0) +"/"/*+ BitConverter.ToDouble(valueFromAdressMemmoryCell, 0)*/);
                                        injectionValue = BitConverter.GetBytes(BitConverter.ToSingle(valueFromAdressMemmoryCell, 0) + BitConverter.ToSingle(injectionValue, 0));

                                        //Записываем значение в указанную ячейку памяти
                                        WriteProcessMemory((int)gameProcessHandle, finalPointAddress, injectionValue, injectionValue.Length, ref bytesWritten);
                                    }

                                    //Запись в память произведения того значения что было в ячейки памяти и того что передаем в ячейку памяти(GetMultipleAndWrite)
                                    if (memoryWrite.Type == "GetMultipleAndWrite")
                                    {
                                        valueFromAdressMemmoryCell = new byte[8];
                                        //Чистаем значение из памяти
                                        ReadProcessMemory((int)gameProcessHandle, finalPointAddress, valueFromAdressMemmoryCell, valueFromAdressMemmoryCell.Length, ref bytesRead);

                                        //MessageBox.Show(BitConverter.ToSingle(injectionValue, 0) +"/"/*+ BitConverter.ToDouble(valueFromAdressMemmoryCell, 0)*/);
                                        injectionValue = BitConverter.GetBytes(Convert.ToSingle(BitConverter.ToSingle(valueFromAdressMemmoryCell, 0) * BitConverter.ToSingle(injectionValue, 0)));

                                        //Записываем значение в указанную ячейку памяти
                                        WriteProcessMemory((int)gameProcessHandle, finalPointAddress, injectionValue, injectionValue.Length, ref bytesWritten);
                                    }

                                    //Запись в память деления того значения что было в ячейки памяти и того что передаем в ячейку памяти(GetDevideAndWrite)
                                    if (memoryWrite.Type == "GetDevideAndWrite")
                                    {
                                        valueFromAdressMemmoryCell = new byte[8];
                                        //Чистаем значение из памяти
                                        ReadProcessMemory((int)gameProcessHandle, finalPointAddress, valueFromAdressMemmoryCell, valueFromAdressMemmoryCell.Length, ref bytesRead);

                                        //MessageBox.Show(BitConverter.ToSingle(injectionValue, 0) +"/"/*+ BitConverter.ToDouble(valueFromAdressMemmoryCell, 0)*/);
                                        injectionValue = BitConverter.GetBytes(Convert.ToSingle(BitConverter.ToSingle(valueFromAdressMemmoryCell, 0) / BitConverter.ToSingle(injectionValue, 0)));

                                        //Записываем значение в указанную ячейку памяти
                                        WriteProcessMemory((int)gameProcessHandle, finalPointAddress, injectionValue, injectionValue.Length, ref bytesWritten);
                                    }
                                }
                            }
                        }
                }
            }

            //Если в текущей команде есть запись в память то записываем значения из него
            if (currentCommand.CommMemoryWrites.Count() != 0)
            {
                foreach (var memoryWrite in currentCommand.CommMemoryWrites)
                {
                    //Если в массив с адресами памяти не является пустым(проверяем нулевой элемент т.к. команда Split создает массив с одним значением независимо от того было ли что то в строке или нет)
                    if (CalibrationProcessor.CalibrationProcessor.checkedVariableAddreses[memoryWrite.Variable] != 0)
                    {
                        //Запись финального адреса искомой переменной в которой будет меняться значение
                        finalPointAddress = CalibrationProcessor.CalibrationProcessor.checkedVariableAddreses[memoryWrite.Variable];

                        //Конвертируем значение с плавующей точкой в массив байтов
                        injectionValue = BitConverter.GetBytes(Convert.ToSingle(memoryWrite.Value));


                        //Запись в память поверх старого значения(Write)
                        if (memoryWrite.Type == "Write")
                        {
                            //Записываем значение в указанную ячейку памяти
                            WriteProcessMemory((int)gameProcessHandle, finalPointAddress, injectionValue, injectionValue.Length, ref bytesWritten);
                        }

                        //Запись в память суммы того значения что было в ячейки памяти и того что передаем в ячейку памяти(GetAddAndWrite)
                        if (memoryWrite.Type == "GetAddAndWrite")
                        {
                            valueFromAdressMemmoryCell = new byte[8];
                            //Чистаем значение из памяти
                            ReadProcessMemory((int)gameProcessHandle, finalPointAddress, valueFromAdressMemmoryCell, valueFromAdressMemmoryCell.Length, ref bytesRead);

                            //Складываем значение прочитанное из памяти с передаваемым значением
                            injectionValue = BitConverter.GetBytes(BitConverter.ToSingle(valueFromAdressMemmoryCell, 0) + BitConverter.ToSingle(injectionValue, 0));

                            //Записываем полученное значение в указанную ячейку памяти
                            WriteProcessMemory((int)gameProcessHandle, finalPointAddress, injectionValue, injectionValue.Length, ref bytesWritten);
                        }

                        //Запись в память произведения того значения что было в ячейки памяти и того что передаем в ячейку памяти(GetMultipleAndWrite)
                        if (memoryWrite.Type == "GetMultipleAndWrite")
                        {
                            valueFromAdressMemmoryCell = new byte[8];
                            //Чистаем значение из памяти
                            ReadProcessMemory((int)gameProcessHandle, finalPointAddress, valueFromAdressMemmoryCell, valueFromAdressMemmoryCell.Length, ref bytesRead);

                            //Перемножаем значение прочитанное из памяти с передаваемым значением
                            injectionValue = BitConverter.GetBytes(Convert.ToSingle(BitConverter.ToSingle(valueFromAdressMemmoryCell, 0) * BitConverter.ToSingle(injectionValue, 0)));

                            //Записываем значение в указанную ячейку памяти
                            WriteProcessMemory((int)gameProcessHandle, finalPointAddress, injectionValue, injectionValue.Length, ref bytesWritten);
                        }

                        //Запись в память деления того значения что было в ячейки памяти и того что передаем в ячейку памяти(GetDevideAndWrite)
                        if (memoryWrite.Type == "GetDevideAndWrite")
                        {
                            valueFromAdressMemmoryCell = new byte[8];
                            //Чистаем значение из памяти
                            ReadProcessMemory((int)gameProcessHandle, finalPointAddress, valueFromAdressMemmoryCell, valueFromAdressMemmoryCell.Length, ref bytesRead);

                            //Делим значение прочитанное из памяти на передаваемое значение
                            injectionValue = BitConverter.GetBytes(Convert.ToSingle(BitConverter.ToSingle(valueFromAdressMemmoryCell, 0) / BitConverter.ToSingle(injectionValue, 0)));

                            //Записываем значение в указанную ячейку памяти
                            WriteProcessMemory((int)gameProcessHandle, finalPointAddress, injectionValue, injectionValue.Length, ref bytesWritten);
                        }
                    }
                }
            }


            //Удаляем команду из очереди
            currentCommandList.Remove(currentCommandObject);

            //очищаем переменные
            currentCommandObject = null;
            currentCommand = null;
            currentAlert = null;
            currentMessage = null;

            commandInAction = false;//текущее состояние запуска команды, команда НЕ в процессе выполнения
        }
        private static void StartRouletteProcessing()
        {
            int currentBarrelCount;

            //Если количество барабанов от уровня донатов включено, то определяем количество, иначе задаем максимально возможное.
            if (Options.Options.SlotMachine.Enable)
            {
                currentBarrelCount = Options.Options.SlotMachine.Barrels.Where(barrel => barrel.Price <= DonateStatistics.DonateStatistics.CurrentDonateAmount).Count();
            }
            else
            {
                currentBarrelCount = 7;
            }

            //Если количество барабанов превышает максимально возможное для данной команды, то выбираем максимально возможное для данной команды количество барабанов
            if(currentBarrelCount>currentCommand.CommBarrels.Count())
            {
                currentBarrelCount = currentCommand.CommBarrels.Count();
            }


            //Вызываем запуск рулетки в слот машине, передав комманду, а так же количество барабанов
            InteractiveRoulette.InteractiveRouletteWindow.GetWindow().Dispatcher.Invoke(() =>
            {
                InteractiveRoulette.InteractiveRouletteWindow.GetWindow().StartRoulette(currentCommand, currentBarrelCount);
            });
        }
        private static void StartAlertProcessing()
        {
            OldAlertWindow.AlertWindow.GetWindow().Dispatcher.Invoke(() =>
            {
                OldAlertWindow.AlertWindow.StartAlert(currentCommand.CommAlert, currentMessage);
            });
        }
    }
}
