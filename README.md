# **0 - Table of Cotents**

### [1 - Tessellation & Wireframe](#tess)
* #### [1.1 - Demo](#tess_demo)
* #### [1.2 - Flowchart](#tess_fc)
* #### [Reference](#tess_ref)

### [2 - Geometry Grass](#geograss)
* #### [2.1 - Demo](#geograss_demo)
* #### [Reference](#geograss_ref)

### [3 - Droplet Effect](#dropletFX)
* #### [3.1 - Demo](#dropletFX_demo)
* #### [3.2 - Flowchart](#dropletFX_fc)
* #### [Reference](#dropletFX_ref)

---

# 1 - Tessellation&Wireframe <a name="tess"></a>

## 1.1 - Demo <a name="tess_Demo"></a>
<p align="center">
  <img src="https://user-images.githubusercontent.com/79186991/230733377-5270dc49-1dab-4f82-9dc3-2bbf4087a013.gif" width="70%" height="70%">
  <br> > Tessellation Shader
</p>

## 1.2 - Flowchart <a name="tess_fc"></a> 
![Tessellation Shader](https://user-images.githubusercontent.com/79186991/233772762-b9bfa527-97b9-4e4d-800a-a1d0b3aae74c.png)

> ## Reference <a name="tess_ref"></a> 
> 1. [shaders-botw-grass/BotWGrass.shader at main · daniel-ilett/shaders-botw-grass](https://github.com/daniel-ilett/shaders-botw-grass/blob/main/Assets/Shaders/BotWGrass.shader) 
> 2. [Catlike-coding-Tessellation](https://catlikecoding.com/unity/tutorials/advanced-rendering/tessellation/)  
> 3. [Catlike-coding-Flat and Wireframe Shading](https://catlikecoding.com/unity/tutorials/advanced-rendering/flat-and-wireframe-shading/)

---

# 2 - Geometry Grass  <a name="geograss"></a>
- [x] \<VFX> Collider Interactor - characters own collision volume against the grass.
- [x] \<VFX> Wind FX
- [x] \<EDT> Grass painting tool
- [x] \<OPT> Culling based on distance - discard pixels when they are distant from camera.
- [ ] \<OPT> Pre-Z

## 2.1 - Demo <a name="geograss_demo"></a>
<p align="center">
  <img src="https://user-images.githubusercontent.com/79186991/232084887-e96e8136-25ef-41df-96d8-51203b39a56e.gif" width="100%" height="100%">
  <br> > Geograss
</p>

<img src="https://user-images.githubusercontent.com/79186991/233769617-cd639b01-596d-4fea-a9eb-46e34a460414.gif" width="50%" height="50%" alt="tool"> 
  > Grass Painting Tool

> ## Reference <a name="geograss_ref"></a>
> 1. [Unity Grass Geometry Shader Tutorial at Roystan](https://roystan.net/articles/grass-shader/) 
> 2. [GitHub - daniel-ilett/shaders-botw-grass: A Legend of Zelda: Breath of the Wild-style grass shader](https://github.com/daniel-ilett/shaders-botw-grass) 
> 3. [自定义Unity Terrain材质来刷草-Part 2](https://zhuanlan.zhihu.com/p/437102341) 
> 4. [Youtube | Unity | I made an Interactive Grass Shader + Tool](https://www.youtube.com/watch?v=xKJHL8nQiuM) 
> 5. [Mesh Texture painting in Unity Using Shaders](https://shahriyarshahrabi.medium.com/mesh-texture-painting-in-unity-using-shaders-8eb7fc31221c) 

---

# 3 - Droplet Effect  <a name="dropletFX"></a>

## 3.1 - Demo <a name="dropletFX_demo"></a>
<p align="center">
  <img src="https://user-images.githubusercontent.com/79186991/232098318-356d769a-4ef7-4e9c-9a6a-3f485213374a.gif" width="100%" height="100%">
  <br> > Droplet FX
</p>

## 3.2 - Flowchart <a name="dropletFX_fc"></a>
![Case - medium](https://user-images.githubusercontent.com/79186991/233773569-53676c57-8245-4be6-b686-9ac57a637cd5.png)

> ## Reference <a name="dropletFX_ref"></a>
> 1. [Youtube | Making a rainy window in Unity - Part 1](https://www.youtube.com/watch?v=EBrAdahFtuo&t=2077s) 
> 2. [Youtube | Making a rainy window in Unity - Part 2](https://www.youtube.com/watch?v=0flY11lVCwY&t=17s) 

---

# 4 - Double-layered BRDF - a Practice of Rainy Ground Rendering <a name="rainyGround"></a>
