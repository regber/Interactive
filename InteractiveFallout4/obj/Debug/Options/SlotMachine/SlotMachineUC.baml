   M S B A M L   `   `   `  ����N  JInteractiveFallout4, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nulla5clr-namespace:InteractiveFallout4.Options.SlotMachine'InteractiveFallout4.Options.SlotMachine  5      ]3clr-namespace:InteractiveFallout4.Common.Converters%InteractiveFallout4.Common.Converters  5      ;    5InteractiveFallout4.Options.SlotMachine.SlotMachineUC   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Glocal5clr-namespace:InteractiveFallout4.Options.SlotMachine     5      J
Converters3clr-namespace:InteractiveFallout4.Common.Converters     5        � 	Resources  5
      ;  5InteractiveFallout4.Common.Converters.EnableConverter   EnableConverterD  >InteractiveFallout4.Options.SlotMachine.SlotMachineUCViewModel  SlotMachineUCViewModelE  ?InteractiveFallout4.Common.Converters.AlterationNumberConverter  AlterationNumberConverter  validationFailed �� Orientation  textBoxInError �� 
TargetType �� Triggers P� Property P� Value �� Property �� Value �� Self	 �� RelativeSource
 �� Path��%  &	        &	      &	 &     &	 9     &	 /      5   
   6?     5   
   6H     5   
   6S   �� 5   
   .R�5      �� $ 
Horizontal=�6   .+�5      �� $��0 5 0 5q�6   
���  ��$��1q�6<   ._�5      �� $��Center=�60   $��Center=�6M   6j   5      �� 5      
���  ��$��28(�6-   !6>   5      5      �� 5   
   " ��6'    5      P� 5       �@=�HasError . . �5      ��  �@�ToolTip 60   �� #	 Z $)
 #(Validation.Errors)[0].ErrorContent�5      5      5      .��5!      � 
�������.+�5"   
   �� $'��!Настройки рулетки��6   .��5#      �  � RowDefinitions 5$      �� 5%      $��80 �6$   62   �� 5&      $	��230 �6$   63   �� 5'      6&   .+�5)      �� $��0��6   $ Vertical=�6*   .+�5*      �� $��40��6!   $ 
Horizontal=�6-   .+�5+      �� $p��jВключитиь зависимость кол-ва барабанов от уровня донатов:��6$   $	��200��6e   $
��Wrap=�6q   $��Center=�6�   6�   �� 5,      $��Center=�6#      
EnableList#��Z 6>   $	��100��6�    �� SelectedItem 6   ��    Enable#
 Z  �� 	Converter# [  6  5-      �� 5.      $��40��6!   $ 
Horizontal=�6-   .+�5/      �� $��Цвет задника��6!   $	��100��68   $��4q�6D   ��6O   �� (   !SlotMachineColorWindowButtonClick �� Source# Z 6�   50      51      O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem 52      R� 6   9system&clr-namespace:System;assembly=mscorlib     6    �� MaxValue �� AlternationCount# Z 6Q    � DataContext# [ 6�   $��1��6�   ��6�   �� *BarrelsTreeViewData+��53      �� 54      .R�55      �� 
���   �$��2q�68   ._�56   "   �� $ 
Horizontal=�6-   
�������.+�57   &   �� ��60   �� 	 ��  �� Mode$ FindAncestor=� �� AncestorType$ TreeViewItemL�$%
 (ItemsControl.AlternationIndex)� �� StringFormat$ Барабан №{0}��# [ 6  �� 58   &   #��[ 6.   $	��100��6V   $��2q�6b    =� ErrorTemplate# [ 6m   .��6�   .��59   *   �� $
 Price�62    �� ValidationRules 5:   .   5  /System.Windows.Controls.DataErrorValidationRule  5;   2   5=   +   5>   '   5?   #   5@      5A      5C      �� 5D      $��2��6   .+�5E      �� $��12(�6    $����Числа должны идти по возврастанию и быть больше нуля(кроме первого барабана, он всегда 0)��6.   5F      5G      5H      5I      5J      5K      