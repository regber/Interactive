   M S B A M L   `   `   `  ����N  JInteractiveFallout4, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:InteractiveFallout4.OptionsInteractiveFallout4.Options  5   	   /    )InteractiveFallout4.Options.OptionsWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ;local)clr-namespace:InteractiveFallout4.Options     5   	     � Title$  Настройки��5   	   $	��450��6   $	��800��6(    � 
ResizeMode$ NoResize=�64    � WindowStartupLocation$ CenterScreen=�6J    � 	Resources 5      2  ,InteractiveFallout4.Options.OptionsViewModel   OptionsViewModel/  )InteractiveFallout4.Options.TreeViewModel  TreeViewModel��%:   &	        &	        5   
   6<     5   
   66   5      .��5      � -   5        $��Grid_OptionsWindow��5       � ColumnDefinitions 5   
   �� 5      $	��200 �6   6-   �� 5      6!    � RowDefinitions 5   
   �� 5      6   �� 5      $��50 �6   6*   .+�5   
   R�  � DataContext# [ 6   $��0��6e   $��18(�6u   ��6@   �� *TreeViewData+��5      �� 5       �� 
TargetType" Q�6   .�5       ��  �� Property �@Q�
IsSelected	 �� Value	 63   �� *
IsSelected+
 �� Mode$
 TwoWay=�5!      ��5$      �� 5%       �� ItemsSource 6+   �� *
SubItems+.R�5&      �� ��6    �� *Title+5'      5)      � 5*   
   $��1��6   .+�5+      �� ��6   ��  �� Source# [   �� Path$ 	OptionsUC�6z   5,      �� 5.   
   # [  6    �� Orientation$ 
Horizontal=�6E   $��1��6^   $��1��6k   $��Right=�6{   .+�5/      �� $
��Ок��6   $��30��6�   $	��100��6�   $��0 0 10 0q�6�   ��6"   �� *AcceptButtonClick+��6H   ��  �� RelativeSource ��  �� AncestorType" � �� Mode$ FindAncestor=�6�   �� 50      $��Отмена��6   $��30��6�   $	��100��6�   $��0 0 10 0q�6�   ��6&   �� *CancelButtonClick+��6L   ��  �� " �$ FindAncestor=�6�   51      53      54      