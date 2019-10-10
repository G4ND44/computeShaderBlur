Compute Shader Blur for Unity
===============================

Optimized blur for Unity using compute shaders with some examples.
Works only for devices supporting compute shaders.

![preview](https://i.imgur.com/C7tJDHD.png)

Static Blur 
-------------
Blur with kernel caculated on start

![static blur](staticBlur.gif)

Dynamic Blur 
-------------
Blur with kernel caculation on gpu in realtime

![dynamic blur](dynamicBlur.gif)

Pie Menu 
-------------
Example of use of dynamic Blur combined with postprocess

![pie menu](pieMenu.gif)


Limitations and problems
-------------

- No support fot LWRP
- Not working corretly on Android
- Need some optymalization for mobile

-------------
Based on "Intruduction to 3D Game Programming With DirectX 11" by Frank D. Luna
AWP rifle by RGI https://www.youtube.com/channel/UCqpUhXDO7-6oE6-OLRQojng
