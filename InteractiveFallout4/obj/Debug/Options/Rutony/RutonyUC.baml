   M S B A M L   `   `   `  ����N  JInteractiveFallout4, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullW0clr-namespace:InteractiveFallout4.Options.Rutony"InteractiveFallout4.Options.Rutony  5      1    +InteractiveFallout4.Options.Rutony.RutonyUC   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Blocal0clr-namespace:InteractiveFallout4.Options.Rutony     5        � 	Resources  5	      0  *InteractiveFallout4.Options.Rutony.AuthMD5   Authentication:  4InteractiveFallout4.Options.Rutony.RutonyUCViewModel  RutonyUCViewModel��%:   &	        &	        5
   
   61     5   
   6>   5      .��5      � 
�������.+�5   
   �� $��Настройки Rutony��6   $��5 5 5 0q�6-   .��5      ��  �� Orientation$ Vertical=�6   $	��Top=�60   .+�5      �� $ 
Horizontal=�6   .+�5      �� $��Подключен:��6    $��Center=�62   $��	10 10 0 0q�6M   $��70��6`   6p   �� 5      $��Center=�6e   $��	14 10 0 0q�6�    d� 	IsChecked 6   ��    Enable �� Path# Z 6�   5      �� 5      $ 
Horizontal=�6   .+�5      �� $��MD5 ключ:��6    $��Center=�61   $��10 5 0 5q�6L   $��70��6^   6n   �� 5      -   5        $��
MD5TextBox��5      $	��360��6p   $��10 5 0 5q�6|   $��Center=�6�   ��60   ��    MD5Key# Z 6�   �� 5       � DataContext# [ 6   $��10 5 0 5q�6N   $��Center=�6`   $!��Проверить ключ��6|   ��6�   �� *CheckRutonyKeyButtonClick+��6�   ��  �� ElementName$ 
MD5TextBox��$
 Text�6  � 5      # [ 6   $��Slide=�6M    � 	StaysOpen .  $��Bottom=�6�   	 � AllowsTransparency	 . 
 � PlacementTarget
 6x   �� $ 
MD5TextBox��.��5      �� # [ 6$   ���	#FFA5E2DA��6U   �� *PopUpMessage+6�   ��5      �� 5       �� 
TargetType" �6$   .�5   "   ��  �� Property � �� �� Value 6;   �� *IsPopUpShow+ �� Mode$ TwoWay=�6h   5      5!      5#      �� 5$      $����* Обратите внимание что при одновременном подключении Rutony и площадок через API, информация от площадок и от Rutony может задвоиться.��6   $
��Wrap=�6�   $��10 10 10 10q�6�   6�   5%      5&      5(      5)      