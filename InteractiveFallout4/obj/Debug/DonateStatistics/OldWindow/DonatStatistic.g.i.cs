﻿#pragma checksum "..\..\..\..\DonateStatistics\OldWindow\DonatStatistic.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "5C018224DB2BFDAC47D2BA6E1E177AB1BBBC9D57F0CA26A9F2649B937AB8EFB8"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using InteractiveFallout4.DonatStatistic;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace InteractiveFallout4.DonatStatistic {
    
    
    /// <summary>
    /// DonatStatisticWindow
    /// </summary>
    public partial class DonatStatisticWindow : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 20 "..\..\..\..\DonateStatistics\OldWindow\DonatStatistic.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Calendar mainCalendar;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\..\DonateStatistics\OldWindow\DonatStatistic.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock sumTextBlock;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\..\DonateStatistics\OldWindow\DonatStatistic.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox displayModeComboBox;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\..\DonateStatistics\OldWindow\DonatStatistic.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox PlatformComboBox;
        
        #line default
        #line hidden
        
        
        #line 38 "..\..\..\..\DonateStatistics\OldWindow\DonatStatistic.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox DonatComboBox;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\..\..\DonateStatistics\OldWindow\DonatStatistic.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox DonatersComboBox;
        
        #line default
        #line hidden
        
        
        #line 54 "..\..\..\..\DonateStatistics\OldWindow\DonatStatistic.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel displayColumnsStackPanel;
        
        #line default
        #line hidden
        
        
        #line 74 "..\..\..\..\DonateStatistics\OldWindow\DonatStatistic.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel datesColumnStackPanel;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/InteractiveFallout4;component/donatestatistics/oldwindow/donatstatistic.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\DonateStatistics\OldWindow\DonatStatistic.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 8 "..\..\..\..\DonateStatistics\OldWindow\DonatStatistic.xaml"
            ((InteractiveFallout4.DonatStatistic.DonatStatisticWindow)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            this.mainCalendar = ((System.Windows.Controls.Calendar)(target));
            
            #line 20 "..\..\..\..\DonateStatistics\OldWindow\DonatStatistic.xaml"
            this.mainCalendar.SelectedDatesChanged += new System.EventHandler<System.Windows.Controls.SelectionChangedEventArgs>(this.Calendar_SelectedDatesChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.sumTextBlock = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.displayModeComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 26 "..\..\..\..\DonateStatistics\OldWindow\DonatStatistic.xaml"
            this.displayModeComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.ChangeSelectionOptions);
            
            #line default
            #line hidden
            return;
            case 5:
            this.PlatformComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 32 "..\..\..\..\DonateStatistics\OldWindow\DonatStatistic.xaml"
            this.PlatformComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.ChangeSelectionOptions);
            
            #line default
            #line hidden
            return;
            case 6:
            this.DonatComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 38 "..\..\..\..\DonateStatistics\OldWindow\DonatStatistic.xaml"
            this.DonatComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.ChangeSelectionOptions);
            
            #line default
            #line hidden
            return;
            case 7:
            this.DonatersComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 42 "..\..\..\..\DonateStatistics\OldWindow\DonatStatistic.xaml"
            this.DonatersComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.ChangeSelectionOptions);
            
            #line default
            #line hidden
            return;
            case 8:
            this.displayColumnsStackPanel = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 9:
            this.datesColumnStackPanel = ((System.Windows.Controls.StackPanel)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
