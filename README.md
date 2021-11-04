# Sprite-Animator
A simple sprite animator with spritesheet import and z-sorting

An experiment to create my own 2D sprite renderer in C#. Supports smooth animation using deltatime and FPS targeting, spritesheet importing, simple alpha blending, z-sorting / depth sorting, and a simple state machine.

* The images are drawn using Graphics.DrawImage() with specified image positions and sizes in the spritesheet. Unfortunately this process is slow when drawing hundreds of images. I will update this later to save image frames instead of positions, and draw the frames themselves instead of cutting the spritesheet on every draw event for every image.

On line 66, the alpha scaling factor is 50%. This can be changed to 100% to remove the alpha blending
ZLayerAlpha.cs is a standalone version of the z-sorting algorithm
Sprite sheet not included

![image](https://user-images.githubusercontent.com/61665584/140416682-0f2d0550-e895-430e-8fc7-009622fd0c1c.png)
![image](https://user-images.githubusercontent.com/61665584/140416771-0adbbb92-7ac9-451c-8df4-9b270289a9db.png)
