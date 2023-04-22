# **0 - Table of Cotents**

### [1 - Tessellation & Wireframe](#tess)
* #### [1.1 - Demo](#tess_demo)
* #### [1.2 - Flowchart](#tess_nt)
* #### [Reference](#tess_ref)

### [2 - Geometry Grass](#geograss)
* #### [2.1 - Demo](#geograss_demo)
* #### [Reference](#geograss_ref)

### [3 - Droplet Effect](#dropletFX)
* #### [3.1 - Demo](#dropletFX_demo)
* #### [3.2 - Flowchart](#dropletFX_nt)
* #### [Reference](#dropletFX_ref)

### [4 - Double-layered BRDF - a Practice of Rainy Ground Rendering](#rainyground)
* #### [4.1 - Demo](#rainyground_demo)
* #### [4.2 - Notes](#rainyground_nt)
* #### [Reference](#rainyground_ref)

---

# 1 - Tessellation&Wireframe <a name="tess"></a>

## 1.1 - Demo <a name="tess_demo"></a>
<p align="center">
  <img src="https://user-images.githubusercontent.com/79186991/230733377-5270dc49-1dab-4f82-9dc3-2bbf4087a013.gif" width="70%" height="70%">
  <br> > Tessellation Shader
</p>

## 1.2 - Flowchart <a name="tess_nt"></a> 
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

<p align="center">
  <img src="https://user-images.githubusercontent.com/79186991/233769617-cd639b01-596d-4fea-a9eb-46e34a460414.gif"  height="330" alt="tool"> 
  <img src="https://user-images.githubusercontent.com/79186991/233777662-ae1a2857-d731-4432-aafa-bfa5e745eb85.gif" height="330"> 
  <br> > Grass Painter Tool & Collider Interactor
</p>



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

## 3.2 - Flowchart <a name="dropletFX_nt"></a>
![Case - medium](https://user-images.githubusercontent.com/79186991/233773569-53676c57-8245-4be6-b686-9ac57a637cd5.png)

> ## Reference <a name="dropletFX_ref"></a>
> 1. [Youtube | Making a rainy window in Unity - Part 1](https://www.youtube.com/watch?v=EBrAdahFtuo&t=2077s) 
> 2. [Youtube | Making a rainy window in Unity - Part 2](https://www.youtube.com/watch?v=0flY11lVCwY&t=17s) 

---

# 4 - Double-layered BRDF - a Practice of Rainy Ground Rendering <a name="rainyground"></a>
- [x] \<VFX> Ripple FX
- [x] \<VFX> Water level go-up

## 4.1 - Demo <a name="rainyground_demo"></a>
<p align="center">
  <img src="https://user-images.githubusercontent.com/79186991/233775740-80ca6f81-e30f-4af6-b20a-7ad97ef733c6.gif" width="100%" height="100%">
  <br> > Rainy Ground
</p>
<p align="left">
  <img src="https://user-images.githubusercontent.com/79186991/233776939-22c30b28-0232-459d-b3de-af0abdc307ae.gif" width="50%" height="50%">
  <br> > Water Level Go-up
</p>

## 4.2 - Note <a name="rainyground_nt"></a>
![Case - medium](https://user-images.githubusercontent.com/79186991/233775919-eee8bc52-c246-4061-bc01-3a30c945d9ce.png)
![PBR](https://user-images.githubusercontent.com/79186991/233776072-30ea3e3b-bfc0-4fda-92c9-8227c40e915f.png)

> ## Reference <a name="rainyground_ref"></a>
> 1. [如何在Unity中造一个PBR Shader轮子](https://zhuanlan.zhihu.com/p/68025039)
> 2. [URP管线的自学HLSL之路 第三十七篇 造一个PBR的轮子](https://www.bilibili.com/read/cv7510082)
> 3. [【基于物理的渲染（PBR）白皮书】（一） 开篇：PBR核心知识体系总结与概览](https://zhuanlan.zhihu.com/p/53086060) 
> 4. [Water drop 2b – Dynamic rain and its effects](https://seblagarde.wordpress.com/2013/01/03/water-drop-2b-dynamic-rain-and-its-effects/) 
> 5. [Unity Shader 实现雨天的水面涟漪效果](https://zhuanlan.zhihu.com/p/83219238)
> 6. [Unity Shader 水体渲染](https://zhuanlan.zhihu.com/p/179249031)


