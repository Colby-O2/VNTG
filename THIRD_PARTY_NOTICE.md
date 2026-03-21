Only the listed files are included from the original projects; other files in the original repository are not part of this project.



## 1. [PSXLighting_URP.hlsl](Shaders/HLSL/PSXLighting_URP.hlsl) (Modified from CustomLighting.hlsl)

- Original Author: Codrin-Mihail

- Original File: [CustomLighting.hlsl](https://github.com/Kodrin/URP-PSX/blob/master/URP-PSX/Assets/Shaders/HLSL/CustomLighting.hlsl)

- Source: https://github.com/Kodrin/URP-PSX

- License: MIT

- Modifications by Colby-O:

  - Integrated texel snapping logic	
  
  - Added support for Unlit shaders
  
  - Refactored MainLight to handle lighting calculations internally
  
  - Implemented URP LIGHT\_LOOP macros for additional lights, supporting Forward+ and Deferred Rendering in addition to Forward Rendering which was originally supported
  
  - Fixed shadows for additional lights and added custom shadow tinting and distance cutoffs
  
  - Added light cookie support
  
  - Refactored overall code structure



MIT License



Copyright (c) 2020 Codrin-Mihail



Permission is hereby granted, free of charge, to any person obtaining a copy

of this software and associated documentation files (the "Software"), to deal

in the Software without restriction, including without limitation the rights

to use, copy, modify, merge, publish, distribute, sublicense, and/or sell

copies of the Software, and to permit persons to whom the Software is

furnished to do so, subject to the following conditions:



The above copyright notice and this permission notice shall be included in all

copies or substantial portions of the Software.



THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR

IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,

FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE

AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER

LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,

OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE

SOFTWARE.

