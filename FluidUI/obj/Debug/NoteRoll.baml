   M S B A M L   `   `   `  ����B  >FluidUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null!clr-namespace:FluidUIFluidUI  5          FluidUI.NoteRoll   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      'localclr-namespace:FluidUI     5      $��489.433��5      $��775.022��60     � UseLayoutRounding  .  J� SnapsToDevicePixels . .��5
      ]� .+�5   
   � -   5         �Name$ topGrid��5      $	��Top=�6    $��0��65   $y�Hidden=�6@   $��Visible=�6T   $��Disabled=�6�   .+�5         FluidUI.EditorPanel  -   5          Name$ envelopePanel��5   $   $
��Auto��6;   $
��Auto��6I   5      � 5   
   -   5        $ 
bottomGrid��5      $��Bottom=�6#   .+�5      �� -   5         ��Name$ renderStatus��5      $��20��6*   $	��Top=�66   ���	#FF1B1B1B$
��Left=�6e   $��77��6�    J� RenderTransformOrigin$ 0.5,0.5&�6�   $�� ��6�   
�������6�   �� 5      -   5         ��Name$ timeBarScroller��5      $��Visible=�64   $��Hidden=�6\   $��	76,0,18,0q�6�   $��47��6�   $	��Top=�6�   .��5      � .+�5      �� -   5         ��Name$ timeBar��5   !   $��50��62   $	��Top=�6>   ���	#FF1B1B1B$��Disabled=�6m   $
��3903��6�    �� Orientation$ 
Horizontal=�6�   $��Center=�6�   �� 5      -   5        	 ��Name$	 timeBarSlider��5      $��50��64   $	��Top=�6@   
 J� Opacity$
 1Q�6X   ���	#001B1B1B$��Disabled=�6{   $
��3903��6�   $��Center=�6�   6�   5      5      �� 5      -   5        $
 fred��5      $
��Left=�6)   $��94��6D   $��	0,20,0,17q�6O   .��5      �� -	   5        $ 
pianoPanel��5      
�������$
��Left=�6H   $
��1728��6c   $	��Top=�6q   $��76��6�   5      �� 5      -
   5        $ scroller��5      $��Visible=�6-   $��	76,20,0,0q�6U   .��5      � .+�5      �� -   5        $	 	bkgCanvas��5      $
��1728��60   $
��3954��6>   $��0,2,0,-2q�6�   ��5      !� 5       j� TileMode$
 Tile=�6*    j� Viewport$ 0,0,0.5,0.5�6:    !� Visual 5   "   �� 5   &   5!      5#      �� 5$      -   5        $	 overlayCanvas��5$      $
��1728��64   $
��3954��6B   6O   5%      5&      �� 5'      -   5         ��Name$ dataDisp��5'      $��data box��6(   ���	#FF0074FF
���   �$	��157��6b   $	��Top=�6o    �� TextWrapping$
 Wrap=�6�   $��	0,47,18,0q�6�   $��Right=�6�   $	��139��6�   $��Consolas)�6�   ��6�   Z� �� 5(      $��21��6   $	��Top=�6    ���	#FF1B1B1B$ 0.5,0.5&�6O   $�� ��6o   
�������$��Right=�6�   $��18��6�   5)      5*      5+      