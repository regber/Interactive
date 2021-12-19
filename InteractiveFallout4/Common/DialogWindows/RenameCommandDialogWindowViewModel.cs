using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.ComponentModel;
using InteractiveFallout4.InteractiveBuilder;
using System.Windows.Data;


namespace InteractiveFallout4.Common.DialogWindows
{
    class RenameCommandDialogWindowViewModel:ViewModel.BaseViewModel, IDataErrorInfo
    {
        public Interactive.InteractiveSets.InteractiveSet.InteractiveCommand currentCommand=new Interactive.InteractiveSets.InteractiveSet.InteractiveCommand();
        public static string InitalName;
        public string NewObjectName
        {
            get
            {
                return currentCommand.Title;
            }
            set
            {
                currentCommand.Title = value;
                OnPropertyChanged(nameof(NewObjectName));
            }
        }

        public string this[string columnName]
        {
            get
            {
                string error = String.Empty;
                if (NewObjectName !=null)
                {
                    
                    switch (columnName)
                    {
                        case "NewObjectName":
                            {
                                if(Interactive.InteractiveSets.InteractiveSetList.Where(o=>o.Title== Interactive.InteractiveSets.ActiveSet).First().InteractiveSetCommandsList.Where(obj=> obj.Title== NewObjectName).Count()>1)
                                {
                                    Verificated = false;
                                    error = "Не должно быть двух комманд с одинаковыми именами";
                                }
                                else
                                {
                                    Verificated = true;
                                }
                            }
                            break;
                    }
                    
                }
                return error;
            }
        }
        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public bool Verificated { get; set; }

        public ViewModel.Command AcceptButtonCommand
        {
            get
            {
                return new ViewModel.Command(obj=>
                {
                    if(!Verificated)
                    {
                        currentCommand.Title = InitalName;
                    }
                    ((RenameCommandDialogWindow)obj).Close();
                });
            }
        }
        public ViewModel.Command CloseRenameDialogWindowCommand
        {
            get
            {
                return new ViewModel.Command(obj =>
                {

                    currentCommand.Title = InitalName;
                    ((RenameCommandDialogWindow)obj).Close();
                });
            }
        }
    }


    class ActualWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double)value-60;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException("Not implemented.");
        }
    }
}
