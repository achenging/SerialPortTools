   M S B A M L   `   `   `  ����J  FSerialPortTools, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null1clr-namespace:SerialPortToolsSerialPortTools  5   	   '    !SerialPortTools.LineSettingWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   /localclr-namespace:SerialPortTools     5   	   -   5          � Title$  换行设置��5   	   $	��150��6   $	��300��6#   .��5	      �  � RowDefinitions 5
   
   �� 5      $��* �6    � ColumnDefinitions 5   
   �� 5      $��* �6   .+�5   
   �� $��换行符类型��6   .��5      ��  �� Orientation$ Vertical=�6   .+�5      	� -   5        $��rbCRLF��5      $��回车换行(\r\n)��6,   $��5q�6A    d� 	IsChecked$
 TrueY�6u   	� 5      -   5        $
��rbCR��5      $��
回车(\r)��6*   $��5q�6;   6F   	� 5      -   5        $
��rbLF��5      $��
换行(\n)��6+   $��5q�6<   6G   5      5      5      5      