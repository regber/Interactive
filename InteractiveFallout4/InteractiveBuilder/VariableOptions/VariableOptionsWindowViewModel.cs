using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace InteractiveFallout4.InteractiveBuilder.VariableOptions
{
    class VariableOptionsWindowViewModel
    {
        private bool variableVerifPassed { get; set; }// = new PropertyPath(typeof(VariableOptionsWindowViewModel).GetProperty("_SaveAccept"));
        public static PropertyPath StandartValue { get; set; } = new PropertyPath(typeof(Interactive.InteractiveVariables).GetProperty("StandartValue"));

        public ObservableCollection<Interactive.InteractiveVariables.InteractiveVariable> InteractiveVariablesList { get; set; } = Interactive.InteractiveVariables.InteractiveVariablesList;


        public ViewModel.Command Delete
        {

            get
            {
                return new ViewModel.Command(obj =>
                {
                    var currentVariable = (Interactive.InteractiveVariables.InteractiveVariable)obj;

                    
                    //Сбрасываем все удаляемые переменные на стандартное значение
                    foreach (var set in Interactive.InteractiveSets.InteractiveSetList)
                    {
                        //Command
                        foreach (var comm in set.InteractiveSetCommandsList)
                        {

                            //CommandMemoryReads
                            foreach (var read in comm.CommMemoryReads)
                            {
                                if(read.Variable== currentVariable.Variable)
                                {
                                    read.SetDefault();
                                }
                            }

                            //CommandMemoryWrites
                            foreach (var write in comm.CommMemoryWrites)
                            {
                                if (write.Variable == currentVariable.Variable)
                                {
                                    write.SetDefault();
                                }
                            }
                            
                            //Barrels
                            foreach(var barrel in comm.CommBarrels)
                            {
                                //Choice
                                foreach(var choice in barrel.BarrelChoices)
                                {
                                    foreach(var write in choice.ChoiceMemoryWrites)
                                    {
                                        if(write.Variable== currentVariable.Variable)
                                        {
                                            write.SetDefault();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    //CommandType
                    foreach(var type in Interactive.InteractiveCommandTypes.InteractiveCommandTypesList)
                    {
                        if(type.Variable== currentVariable.Variable)
                        {
                            type.SetDefault();
                        }
                    }


                    InteractiveVariablesList.Remove(currentVariable);
                });
            }

        }

        public ViewModel.Command AddNewVariable
        {
            get
            {
                return new ViewModel.Command(obj =>
                {
                    string NewVariable = "New Variable №";
                    int number = 0;

                    while (Interactive.InteractiveVariables.InteractiveVariablesList.Any(obje => obje.Variable == NewVariable + number))
                    {
                        number++;
                    }
                    Interactive.InteractiveVariables.InteractiveVariablesList.Add(new Interactive.InteractiveVariables.InteractiveVariable() { Variable = NewVariable + number });

                });
            }
        }

        public ViewModel.Command SaveChangeVariable
        {
            get
            {

                return new ViewModel.Command(obj =>
                {
                    variableVerifPassed = true;
                    foreach (var var in Interactive.InteractiveVariables.InteractiveVariablesList)
                    {
                        if (InteractiveVariablesList.Where(o => o.Variable == var.Variable).Count() > 1)
                        {
                            variableVerifPassed = false;
                        }
                    }

                    if (variableVerifPassed)
                        Interactive.InteractiveSave();
                    else
                        MessageBox.Show("Несколько переменных имеют одинаковое название, сохранить переменные можно будет только после исправления ошибки.");
                });
            }
        }
    }
}
