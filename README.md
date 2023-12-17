---
README.md is no more updated.
For more detailed demo pls move to [https://krusssssbooom.artstation.com/](https://krusssssbooom.artstation.com)
---
Unity 2022.2.12f1
---

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

### [5 - Realistic Character Rendering](#rcr)
* #### [5.1 - Skin](#rcr_skin)
  * ##### [5.1.1 - Demo](#rcr_skin_demo)
  * ##### [5.1.2 - Notes](#rcr_skin_nt)
  * ##### [5.1.3 - Reference](#rcr_skin_ref)
* #### [5.2 - Hair](#rcr_hair)
  * ##### [5.2.1 - Demo](#rcr_hair_demo)
  * ##### [5.2.2 - Notes](#rcr_hair_nt)
  * ##### [5.2.3 - Reference](#rcr_hair_ref)

### [6 - 2D Frame Animation](#2danimation)
* #### [6.1 - Demo](#2danimation_demo)
* #### [6.2 - Notes](#2danimation_nt)
* #### [Reference](#2danimation_ref)

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
  <img src="https://user-images.githubusercontent.com/79186991/233769617-cd639b01-596d-4fea-a9eb-46e34a460414.gif"  width="34%"> 
  <img src="https://user-images.githubusercontent.com/79186991/233777662-ae1a2857-d731-4432-aafa-bfa5e745eb85.gif" width="60%"> 
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
<p align="center">
  <img src="https://user-images.githubusercontent.com/79186991/233776939-22c30b28-0232-459d-b3de-af0abdc307ae.gif" width="70%" >
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

---

# 5 - Realistic Character Rendering <a name="rcr"></a>

## 5.1 - Skin <a name="rcr_skin"></a>
- [x] \<VFX> BSSRDF - Pre-integrated Subsurface Scattering
- [x] \<VFX> Specular BRDF - by Kelemen/Szirmay-Kalos
- [x] \<VFX> non-physics-based BTDF - by Colin Barré-Brisebois/ Marc Bouchard 
- [x] \<VFX> Filmic tonemapping 

### 5.1.1 - Demo <a name="rcr_skin_demo"></a>
<p align="center">
  <img src="https://user-images.githubusercontent.com/79186991/235362825-debf6777-6e92-41e1-8bb5-ddb02e7df61a.png" width="100%" height="100%">
  <br> > Skin Rendering
</p>

<p align="center">
  <img src="https://user-images.githubusercontent.com/79186991/235362943-2ed63645-587f-4ec5-8544-8c73692e3d60.png" width="49%">
  <img src="https://user-images.githubusercontent.com/79186991/235363005-543d969c-46ca-462c-ac0c-65107864b060.png" width="49%">
  <br> > PISS BSSRDF (left) & Common BRDF (right)
</p>

### 5.1.2 - Note <a name="rcr_skin_nt"></a>
![PISS](https://user-images.githubusercontent.com/79186991/235363366-3d5b4c7b-4c80-41ae-8529-64fe9d9f5dc6.png)

> ### 5.1.3 Reference <a name="rcr_skin_ref"></a>
> 1. [Pre-Integrated Skin Shading 的常见问题解答](https://zhuanlan.zhihu.com/p/384541607)
> 2. [Pre-Integrated Skin Shading 数学模型理解](https://zhuanlan.zhihu.com/p/56052015)
> 3. [Pre-Integrated Skin Shading实现笔记](https://zhuanlan.zhihu.com/p/60343513) 
> 4. [Simon's Tech Blog - Pre-Integrated Skin Shading](http://simonstechblog.blogspot.com/2015/02/pre-integrated-skin-shading.html) 
> 5. [Adam Chen - Skin Rendering](https://progmdong.github.io/2019-02-03/Skin_Rendering/)
> 6. [SIGGRAPH 2011- Pre-Integrated Skin Shading](http://advances.realtimerendering.com/s2011/Penner%20-%20Pre-Integrated%20Skin%20Rendering%20(Siggraph%202011%20Advances%20in%20Real-Time%20Rendering%20Course).pptx) 
> 7. GPU Pro 2_ Advanced Rendering Techniques_Engel W. (Ed.)
> 8. [GPU Gems 3 | Chapter 14. Advanced Techniques for Realistic Real-Time Skin Rendering](https://developer.nvidia.com/gpugems/gpugems3/part-iii-rendering/chapter-14-advanced-techniques-realistic-real-time-skin) 
> 9. [A Microfacet Based Coupled Specular-Matte BRDF Model with Importance Sampling](http://www.hungrycat.hu/microfacet.pdf)
> 10. [Alan Zucconi | Fast Subsurface Scattering in Unity (Part 1)](https://www.alanzucconi.com/2017/08/30/fast-subsurface-scattering-1/) 
> 11. [Alan Zucconi | Fast Subsurface Scattering in Unity (Part 2)](https://www.alanzucconi.com/2017/08/30/fast-subsurface-scattering-2/)
> 12. [GDC 2011 – Approximating Translucency for a Fast, Cheap and Convincing Subsurface Scattering Look](https://colinbarrebrisebois.com/2011/03/07/gdc-2011-approximating-translucency-for-a-fast-cheap-and-convincing-subsurface-scattering-look/) 
> 13. [GPU Gems | Chapter 16. Real-Time Approximations to Subsurface Scattering](https://developer.nvidia.com/gpugems/gpugems/part-iii-materials/chapter-16-real-time-approximations-subsurface-scattering) 

## 5.2 - Hair <a name="rcr_hair"></a>
- [x] \<VFX> BRDF - by ATI Research's improvement on Kajiya-Kay Model
- [x] \<VFX> Sorting Order - pass1 preZ & pass2 light calculation

### 5.2.1 - Demo <a name="rcr_hair_demo"></a>
![IMG_1887]()
<p align="center">
  <img src="https://github.com/SelfishKrus/Krus_TA_Library/assets/79186991/fd6c25d0-4082-4ccc-a7e7-724eb8dbf2e5" width="100%" height="100%">
  <br> > BRDF by ATI Research
</p>

### 5.2.2 - Note <a name="rcr_hair_nt"></a>
![hair](https://github.com/SelfishKrus/Krus_TA_Library/assets/79186991/db4a561b-78c0-44e9-b324-d9a44667faf3)

> ### 5.2.3 Reference <a name="rcr_hair_ref"></a>
> 1. [Chapter 23. Hair Animation and Rendering in the Nalu Demo](https://developer.nvidia.com/gpugems/gpugems2/part-iii-high-quality-rendering/chapter-23-hair-animation-and-rendering-nalu-demo)
> 2. [角色渲染技术——毛发及其他](https://zhuanlan.zhihu.com/p/27313644)
> 3. [Hair Rendering and Shading | ATI Research](https://web.engr.oregonstate.edu/~mjb/cs557/Projects/Papers/HairRendering.pdf)

--- 

# 6 - 2D Frame Animation <a name="2danimation"></a>
- [x] \<VFX> Frame Animation - sample a texture atlas by indexes
- [x] \<VFX> UV Rolling

## 6.1 - Demo <a name="2danimation_demo"></a>
<p align="center">
  <img src="https://user-images.githubusercontent.com/79186991/235449858-c1cfd0a7-e3a7-4300-9fd3-02f261b34d71.gif" width="100%" height="100%">
  <br> > Frame Animation
</p>

## 6.2 - Note <a name="2danimation_nt"></a>
![2danimationnote](https://user-images.githubusercontent.com/79186991/235450017-1d07dd5b-d070-41a9-9b04-e7b000e8eaf7.png)

> ## Reference <a name="2danimation_ref"></a>
> 1. Unity Shader 入门精要

--- 

# 7 - 
