## Palette Importing & Creation

A palette can be imported from the following file formats (simply drag the file into the asset folder somewhere):

* **.ase** — Adobe Photoshop ASE file
* **.gpl** — GIMP Palette file
* **.hex** — Hex color list
* **.txt** — Paint.NET palette format
* **.pal** — PAL file (supports both JASC and RPAL formats)

### Creating a Palette

You can create a palette in several ways:

* **Manually:**
  Right-click in the Project window and select:
  `Create → VNTG → Palette Asset`

* **From a Texture:**
  Right-click on a texture and select:
  `Create → VNTG → Convert Texture To Palette Asset`

### Palette Settings

Within a Palette Asset, you can adjust the **color distance metric** used to generate the LUT (Look-Up Table). This setting affects how colors are matched and applied.

The default metric is **CIE76**, but depending on your palette and desired visual style, other metrics may produce better results. Experimenting with different options is recommended.
