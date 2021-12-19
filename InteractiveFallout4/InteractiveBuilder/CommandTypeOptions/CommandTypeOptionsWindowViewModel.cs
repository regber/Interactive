using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;


namespace InteractiveFallout4.InteractiveBuilder.CommandTypeOptions
{
    class CommandTypeOptionsWindowViewModel
    {
        public static ObservableCollection<Interactive.InteractiveCommandTypes.InteractiveCommandType> InteractiveCommandTypesList { get; set; } = Interactive.InteractiveCommandTypes.InteractiveCommandTypesList;
        public static ObservableCollection<Interactive.InteractiveVariables.InteractiveVariable> InteractiveVariablesList { get; set; } = Interactive.InteractiveVariables.InteractiveVariablesList;

        public ViewModel.Command Delete
        {

            get
            {
                return new ViewModel.Command(obj =>
                {
                    var currentType = ((Interactive.InteractiveCommandTypes.InteractiveCommandType)obj);

                    foreach(var set in Interactive.InteractiveSets.InteractiveSetList)
                    {
                        foreach(var command in set.InteractiveSetCommandsList)
                        {
                            if (command.CommType.Type == currentType.Type)
                            {
                                command.CommType.SetDefault();
                            }
                        }

                    }

                    Interactive.InteractiveCommandTypes.InteractiveCommandTypesList.Remove(currentType);
                });
            }

        }

        public ViewModel.Command AddNewType
        {
            get
            {
                return new ViewModel.Command(obj =>
                {
                    string NewType = "New Type №";
                    int number = 0;

                    while (Interactive.InteractiveCommandTypes.InteractiveCommandTypesList.Any(obje => obje.Type == NewType + number))
                    {
                        number++;
                    }
                    Interactive.InteractiveCommandTypes.InteractiveCommandTypesList.Add(new Interactive.InteractiveCommandTypes.InteractiveCommandType() { Type = NewType + number , Variable="Default"});

                });
            }
        }

        public ViewModel.Command SaveChangeTypes
        {
            get
            {

                return new ViewModel.Command(obj =>
                {
                    bool variableVerifPassed = true;
                    foreach (var var in Interactive.InteractiveCommandTypes.InteractiveCommandTypesList)
                    {
                        if (InteractiveCommandTypesList.Where(o => o.Type == var.Type).Count() > 1)
                        {
                            variableVerifPassed = false;
                        }
                    }

                    if (variableVerifPassed)
                        Interactive.InteractiveSave();
                    else
                        MessageBox.Show("Несколько типов имеют одинаковое название, сохранить типы можно будет только после исправления ошибки.");
                });
            }
        }

    }
}
