   M S B A M L   `   `   `  ����N  JInteractiveFallout4, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullc6clr-namespace:InteractiveFallout4.Common.DialogWindows(InteractiveFallout4.Common.DialogWindows  5   	   H    BInteractiveFallout4.Common.DialogWindows.RenameCommandDialogWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Hlocal6clr-namespace:InteractiveFallout4.Common.DialogWindows     5   	     � Title$  DialogWindow��5   	   $	��100��6   $	��400��6+    � WindowStartupLocation$ CenterScreen=�67    � DataContext 5	      Q  KInteractiveFallout4.Common.DialogWindows.RenameCommandDialogWindowViewModel  5
   
   65    � 	Resources 5         textValidationError �� 
TargetType �� Triggers P� Property P� Value �� Property	 �� Value
 �� RelativeSource �� ModeC  =InteractiveFallout4.Common.DialogWindows.ActualWidthConverter  ActualWidthConverter��%�   &	        &	 �     �� 5   
   " ��6,    5      P� 5       �@=�HasError . . �5      ��  �@�ToolTip	 61   �� *%#(Validation.Errors)[0].ErrorContent+
 �� $
 Self=�5      5        5   
   6D   5      .��5      �  � RowDefinitions 5   
   �� 5      6   �� 5      6   .+�5   
   �� $��0��6    �� Orientation$ 
Horizontal=�6"   .+�5      �� $��Center=�6   $��Название:��63   6F   �� 5      #��[  6   $��Center=�6C   ��6^   �� 
 ��  �� AncestorType$
 GridL�$ FindAncestor=� �� 	Converter# [  �� Path$ ActualWidth�.��6�   .��5       �� $ NewObjectName�6    �� UpdateSourceTrigger$ PropertyChanged=�60    �� Mode$ TwoWay=�6V    �� ValidationRules 5!      5  /System.Windows.Controls.DataErrorValidationRule  5"      64   5$      5%      5&      � 5'   
   $��0 5 0 0q�6   $��1��6    .+�5(      �� $
��Ок��6   $	��100��6�   $
��Left=�6�   ��6"   �� *AcceptButtonCommand+��6J   �� 
 �� $% local:RenameCommandDialogWindowL�$ FindAncestor=�6�   �� 5)      $��Отмена��6   $	��100��6�   $��Right=�6�   ��6&   �� * CloseRenameDialogWindowCommand+��6[   �� 
 �� $% local:RenameCommandDialogWindowL�$ FindAncestor=�6  5*      5+      5,      