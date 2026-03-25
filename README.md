# VNTG Shader Pack for Unity 6 URP

VNTG is a collection of PSX-inspired and CRT post-processing shaders built for Unity’s Universal Render Pipeline. These shaders were developed across multiple game jams and I hope to continue expanding on these in the future. I hope other developers can find it useful in their own projects.

---

## Installation

Download the proper `.unitypackage` file from [HERE](https://github.com/Colby-O2/VNTG/tree/downloads) and drop it into the `Assets` folder of your project. For rendering features, you can use the included VNTG renderer or add the PSX and CRT rendering features to your own. All settings can be found in the Global Volume.

---

## Links
- [Playable Demo](https://colby-o.itch.io/vntg-shaders)
- [GitHub](https://github.com/Colby-O2?tab=repositories)  
- [Discord](https://discord.gg/8XhAPdCuUU)  

Email: cokeefe919@gmail.com

---

## PSX PBR Material

**Custom lighting with four lighting models (Unlit, Lit, Texel Lit, and Vertex Lit).**  
**Compatible with Forward, Forward+, Deferred, and Deferred+ rendering.**

![PSX PBR Material Example](https://github.com/Colby-O2/VNTG/blob/master/Videos/Light.gif)

**Downsample textures to a set resolution**  
![Downsample Textures](https://github.com/Colby-O2/VNTG/blob/master/Videos/Texture.gif)

**Reduced Color Depth**  
![Reduced Color Depth](https://github.com/Colby-O2/VNTG/blob/master/Videos/ColorPrecision.gif)

**Snaps vertices to a customizable resolution**  
![Vertex Snapping](https://github.com/Colby-O2/VNTG/blob/master/Videos/Vertex.gif)

**Affine Texture Wrapping**  
![Affine Texture Wrapping](https://github.com/Colby-O2/VNTG/blob/master/Videos/Affine.gif)

**Vertex Color Support**  
![Vertex Color Support](https://github.com/Colby-O2/VNTG/blob/master/Videos/VertexColor.gif)

**Alpha Clipping Support**  
![Alpha Clipping](https://github.com/Colby-O2/VNTG/blob/master/Videos/AlphaClipping.png)

**Terrain Support (Unity 6.3 or higher)**  
![Terrain Support](https://github.com/Colby-O2/VNTG/blob/master/Videos/Terrain.gif)

---

## PSX Renderer Feature

**Full Screen Pixelation as well as full screen color depth control**  
![Full Screen Pixelation](https://github.com/Colby-O2/VNTG/blob/master/Videos/Pixelation.gif)

**Dithering with 9 preset patterns**  
with Additive and Multiplicative dither modes. (only multiplicative is shown below)<br>
Options for pixel perfect dither or a custom scale
![Dithering Example](https://github.com/Colby-O2/VNTG/blob/master/Videos/Dither.gif)

**Customizable Stylized Fog**  
![Stylized Fog](https://github.com/Colby-O2/VNTG/blob/master/Videos/Fog.gif)

---

## CRT Renderer Feature

**Screen Curvature and Vintage effect**  
![Screen Curvature](https://github.com/Colby-O2/VNTG/blob/master/Videos/ScreenBend.gif)

**Simulated CRT rendering with Interlaced rendering, pixel decay, and customizable refresh rate and screen resolution**  
![CRT Simulation](https://github.com/Colby-O2/VNTG/blob/master/Videos/Interlaced.gif)

**Scanlines, Noise, Chromatic Aberration, Smear, and other VHS artifacts**  
![VHS Artifacts](https://github.com/Colby-O2/VNTG/blob/master/Videos/VHS.gif)

**Color & Post-Processing Controls**  
![Post-Processing Controls](https://github.com/Colby-O2/VNTG/blob/master/Videos/CRT.gif)

**Subpixels with four display modes (OFF, Shadow Mask, Aperture Grille, and Spot Mask)**  
![Subpixel Modes](https://github.com/Colby-O2/VNTG/blob/master/Videos/Subpixels.gif)

---

## Compatibility

- Unity 6+ (6000.x and newer)  
- URP support  
- Render Graph Supported  
- Desktop & WebGL Fully Supported

---

## Example Uses

Some projects where I've used VNTG or older versions of these shaders:

- [Wrong Floor](https://colby-o.itch.io/wrong-floor)
- [The Motel](https://colby-o.itch.io/the-motel)  
- [Code Black](https://colby-o.itch.io/code-black)

---

## Credits

**Collaborators**
 - **SmokeyTheKat**: A huge thank you for providing the custom icons, tree models, tree textures, and essential utility functions for the example scene. [GitHub](https://github.com/SmokeyTheKat) | [Itch](https://smoekythekittycat.itch.io/)

**Technical Credits**
 - [**Codrin-Mihail**](https://github.com/Kodrin/URP-PSX/blob/master/URP-PSX/Assets/Shaders/HLSL/CustomLighting.hlsl): Base lighting logic (MIT License & changes documented in [HERE](https://github.com/Colby-O2/VNTG/blob/master/THIRD_PARTY_NOTICE.md)).
 - [**GreatestBear**](https://discussions.unity.com/t/the-quest-for-efficient-per-texel-lighting/700574): Per-texel snapping logic and math.
---

## License

VNTG Shader Pack is released under **The Unlicense**. You may freely use, modify, and redistribute these shaders, including in commercial projects. While credit is not required, it’s greatly appreciated!
