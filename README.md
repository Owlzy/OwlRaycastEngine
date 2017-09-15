# OwlRaycastEngine
Basic MonoGame raycast engine

Provides a few simple classses to raycast a pseudo 3D world.

Largely built around the same tutorial everyone uses:- 
http://lodev.org/cgtutor/raycasting.html

Moved accross to use modern drawing methods with a bit of OOP to keep things neater.  
Would provide a nice start to anyone wanting to build this kind of engine.

Also includes the ability to renderer multiple levels.

A good exercise if wanted to extend the engine would be to implement binary space partioning to improve performance.
Currently walls on other levels that are not visible are actually getting rendered behind those you can see!

Sprite casting is part implemented, and I will likely add this shortly. Other than that I will clean up the code in parts, but will leave it to the community 
if anyone is intrestred in improving it. 

![alt text](https://github.com/Owlzy/OwlRaycastEngine/blob/master/screenshot.PNG?raw=true)
