   M S B A M L   `   `   `  ����  FolderParser    FolderParser.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  FolderParser��5   	   $	��450��6   $	��700��6+   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5	      $��* �6   .+�5   
   � $��0��6   $��10,10,10,10q�6    5      �� 5      $��5 �6    �� 5      $
��Auto �6    �� 5      $��5 �6    �� 5      $
��Auto �6     � ColumnDefinitions 5      �� 5      $
��Auto �6#   �� 5      $��10 �6#   �� 5      $��* �6#   �� 5      $��10 �6#   �� 5      $
��Auto �6#   �� 5      $��10 �6#   �� 5      $
��Auto �6#   .+�5      �� $��1��6   $��0��6%   $��XML file name: ��65      TextBlockStyle#��[  6L   �� 5      -   5         ��Name$ m_xmlFileName��5      $��1��6-   $��2��6:   $��Stretch=�6w     	FontStyle#��[ 6�   ��6J   �� *OutputFileName+ �� Mode$ TwoWay=�6�   �� 5      -   5         ��Name$ m_btnBrowseXML��5      $��1��6.   $��4��6;   $��
_Browse...��6K     ButtonStyle#��[ 6`   6�   �� 5      $��3��6   $��0��6&   $��Specify root folder: ��66   #��[  6S   �� 5      -   5        $ m_folderName��5      $��3��6,   $��2��6:   $��Stretch=�6s   #��[ 6�   ��6J   �� *
InitFolder+$ TwoWay=�6�   �� 5       -   5        $ m_btnBrowseFolder��5       $��3��60   $��4��6>   $��
_Browse...��6N   #��[ 6c   6�   �� 5!      -   5        $ 
m_btnStart��5!      $��1��6)   $��3��67   $��6��6H   $��_Start!��6X   #��[ 6j   6�   5"      R� 5#   
   -   5         R�Name$ 
m_treeView��5#      $��1��6'     TreeViewStyle#��[ 64   5$      5%      