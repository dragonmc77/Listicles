   M S B A M L   `   `   `  ����D  @Listicles, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:Listicles	Listicles  5          Listicles.AddToListicle   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35G CPlaynite.SDK, Version=6.9.0.0, Culture=neutral, PublicKeyToken=null\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      )localclr-namespace:Listicles     5      $	��384��5      .��5      �   � RowDefinitions  5	   
   �� 5
      �� 5      �� 5      �� 5      �� 5      �� 5       � ColumnDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� $��0��6   $��3��6   $��5,0,0,0q�61   .��6B   ><Create a new listicle or pick an existing one from the list.6�   	� 5   
   -   5         	�Name$ rdoNew��5      $��1��6&   $��2��63   $��0��6D   $��5,0,0,0q�6T   �� 5   
   $��Title��6   $��1��6    $��1��6-   �� 5   
   -   5         ��Name$ txtNewListicleTitle��5      $��3q�6/   $��1��6:   $��2��6G   6W   �� 5   
   $
��Link��6   $��2��6   $��1��6,   �� 5   
   -   5        $ txtNewListicleLink��5      $��3q�6.   $��2��69   $��2��6F   6V   �� 5   
   $��3��6   $��3q�6!   $��3��6,   	� 5   
   -   5        $ rdoExisting��5      $��4��6+   $��0��68   $��5,0,0,0q�6H   �� 5   
   -   5        $��cmbListicles��5       �� DisplayMemberPath$ Title��6I    �� SelectedValuePath$ Id��6c   $��4��6�   $��1��6�   $��2��6�   $��3q�6�   ��6'   �� *	Listicles+6�   � 5   
   $��5��6   $��0��6   $��3��6,     5       �� 5!       5#      �� 5$      �� 5%      �� 5&      �� 5'      .+�5)      �� -   5         ��Name$ btnOK��5)      $��OK��6$   $��5q�61   $��3q�6<    �� 	IsDefault . $��2��6Y   6i   �� 5*      $��Cancel��6   $��5q�6&   $��3q�61    �� IsCancel . $��3��6M   5+      5,      5-      