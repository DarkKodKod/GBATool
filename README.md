# GBATool
Tool written in C# and WPF using the MVVM design pattern to create and manage asset for a Game Boy Advance game by Felipe Reinaud. Resources files are in TOML format, [https://github.com/toml-lang/toml](https://github.com/toml-lang/toml). 

### Table of Content  
[1. Overview](#Overview)   
[1.1. Menu](#Menu)   
[1.2. Toolbar](#Toolbar)   
[2. Getting started](#Gettingstarted)   
[2.1 Project Properties](#ProjectProperties)   
[2.2 Create project elements](#CreateElements)   
[3. Managing resources](#ManagingResources)   
[3.1. Tile Sets](#TileSets)   
[3.2. Banks](#Banks)   
[3.3. Characters](#Characters)   
[3.4. Palettes](#Palettes)   
[4. Building the project](#Buildingtheproject)   
[4.1. Building for Butano](#Buildingforbutano)   

<a name="Overview"/>

## 1. Overview

The purpose of this tool is to provide a way to create and manage Game Boy Advance assets that is game agnostic. This means that the tool will not restrict what can be created with the original hardware. Assets can have any configuration, and the software/game will add the necessary limitations for them to run properly. Another goal is to have a single executable file that runs without depending on external files, like DLLs. This executable is self-contained, which is why it is so large. With this tool, you can import sprite sheets and organize images to create character blocks, palettes, character animations, and more. For now, the current features are all I need for my game, but if you would like to contribute to expanding its capabilities to export different file formats, please let me know.

![](/Images/gbatool.png)

<a name="Menu"/>

### 1.1 Menu

![](/Images/menu.png)

#### File
![](/Images/file_menu.png)

From File is possible to create a new project or a new element like a [Tile Sets](#TileSets) or a [Character](#Characters).

* A new project will have the extension **.proj** which is internaly a [TOML](https://github.com/toml-lang/toml) format and the name is given by the user. It will also create the folders **Banks**, **Palettes**, **Characters** and **TileSets**. 

* You can open an existing project, where the **.proj** file exist.

* Close the current project.

* Import any image from these formats: *.png, .bmp, .gif, .jpg, .jpeg, .jpe, .jfif, .tif, .tiff, .tga*. The image will reduce the colors with a Palette Quantizer algorithm to match the number of colors the NES can reproduce. More about this topic in the [Getting started](#Gettingstarted).

* Recent project will contain all previous opened projects.

#### Edit
![](/Images/edit_menu.png)

Undo/Redo, Copy, Paste, Duplicate and Delete only affects the project's elements like a [Character](#Characters) element.

#### Project
![](/Images/project_menu.png)

* Project Properties is where is possible to reconfigure the project settings after the project is created. There are actually more options here change than when the project is created. For more information read the [Getting started](#Gettingstarted) section.

* Build Project will create and export all maps, characters and pattern tables in the selected output folder. More on that in [Building the project](#Buildingtheproject) section.

#### Help
![](/Images/help_menu.png)

View Help redirects to this page in github.com.

<a name="Toolbar"/>

### 1.2 Toolbar

![](/Images/toolbar.png)

Toolbar Has the option to create a new project (explained in [Getting started](#Gettingstarted) section) or open an existing project, undo or redo (not recommended to use) and build project. The last one is explained in the [Building the project](#Buildingtheproject) section.

<a name="Gettingstarted"/>

## 2. Getting started

Once GBATool is opened for the first time, it will create in the root of the executable, the file **config.toml**, but only if some of the configuration of the tool changes like the window size.

![](/Images/newproject.png)

To craete a new project click File > New > New Project (Ctrl + Shift + N) or ![](/Images/newproject_toolbar.png) in the toolbar. From there is possible to name the project and a location. After the button *Create project* is pressed it will create at the specified location the file *name of the project*.proj in [TOML](https://github.com/toml-lang/toml) format, and the folders *Banks*, *Characters*, *TileSets*.

GBATool will always open the last opened project. 

<a name="ProjectProperties"/>

### 2.1 Project Properties

At any time is possible to change the project configurations from the menu Project > Project Properties...

![](/Images/projectproperties.png)

From Project properties it is possibled to set up the output format for each type of asset. The Game Boy Advance header can exported in assembly language, same as the palettes and the character's animation data. The Screen Blocks are exported in binary format for example. The *Game Properties* is the information that is used to generate the Game Boy Advance header. The *Sprite Pattern Format* can be 1D or 2D. This is how the internal character blocks are arranged when exported.

<a name="CreateElements"/>

### 2.2 Create project elements

![](/Images/newelement.png)

Creating elements is possible from the menu File > New > New Element (Ctrl + N) or right click on any root folder to open the context menu and select *Create New Element*.

There are only four type of elements to create, [Tile Sets](#TileSets), where you can import a new image and change its pixels, [Banks](#Banks), where you can create banks of any size or NES pattern tables using the [Tile Sets](#TileSets) as input, [Palettes](#Palettes), are the actual colors for any sprite used by the caracteres, [Characters](#Characters), is an element created by [Banks](#Banks) as its input and there, it is possible to create meta sprites and animations. It is really important to understand that all of the links between elements are just references to each other. For example: the bank pattern table could use different tile sets but if one of those tile sets changes something it will also change the tile inside the pattern table and immediately in the character or map that is using that specific bank.

![](/Images/tree.png)

Is possible to create folders inside each root folder and move elements of the same type to any sub folder by just dragging the element. Each element including folders has a context menu with the right click.

To start creating assets for the Game Boy Advance, the very first thing to create is the Tile Set explained in the section below.

<a name="ManagingResources"/>

## 3. Managing resources

<a name="TileSets"/>

### 3.1 Tile Sets

Tile Sets are the basic element to start constructing Game Boy Advance assets but they are not exported directly, they are only used to build [Banks](#Banks), those can be exported from this tool. This is explained more in depth in the [Building the project](#Buildingtheproject) section. Tile Sets are images from these formats: *.png, .bmp, .gif, .jpg, .jpeg, .jpe, .jfif, .tif, .tiff, .tga*. Once is imported, it is possible to define sprite areas in all supported sizes like 8x8, 16x16, 32x32, 64x64, 16x8, 32x8, 32x16, 64x32, 8x16, 8x32, 16x32 and 32x64. This sprites are later used to construct a character meta sprite including its animation.

![](/Images/Axl.png)

Let´s use this image from Final Fight One as an example for later steps.

![](/Images/importimage.png)

There are two ways to import a new image to a *Tile Set* element, first one is to use File > Import > Image... (Ctrl + I). This will create a *Tile Set* element with the name of the image. The second way to import an image is to create a *Tile Set* element, explained the the [Getting started](#Gettingstarted) section and then click over the new element and then click the *tree dots* button on the top part of the element window to browse your computer for an image.

All images after being imported will create if it doesn't exist already a folder name **Images** in the project root directory and I will copy the new imported image there with the extension *.bmp*.

![](/Images/importedimage.png)

After the image is imported it is possible to zoom in/out with the mouse's scroll wheel and you can pick one of the crop sizes to select what part of the entire image you want to create a sprite of that. Then an sprite will appear in the box to the right. There is possible to delete it if not needed.

![](/Images/sprites_alias.png)

The field **Alias** is assigned automatically to any new sprite created. The alias can be changed to any string value. This is later used by the [Banks](#Banks) to ideantify the sprites if is needed.

<a name="Banks"/>

### 3.2 Banks

Banks are tiles grouped together. Is possible to have banks arraged in 1D or 2D (When the checkbox **Is Background** is checked then it will always be 1D arragement). These charblocks can be used either for background or sprites. The source of these banks are constructed from [Tile Sets](#TileSets). This will form a link inside the banks to each **Tile Set** used. If a Tile Set changes its tiles, it is renamed or removed, it will automatically update the bank. From here is possible to generate a [Palette](#Palette) object but these are not linked together. The link is done in the [Character](#Character) object. If your bank is using more than 16 colors then it is important to check the checkbox 256 colors, then when a palette is constructed, it will create palettes objects as much as needed up to 16 palette objects. To have the first color of the palette with the transparency color, click on any sprite region here and by pressing the button **Obtain Transparent Color** it will store the very first pixel of that sprite region.

![](/Images/bank.png)

When building the project, see: [Building the project](#Buildingtheproject), it will generate bank files for each bank object in the project.

<a name="Characters"/>

### 3.3 Characters

Characters are created by using banks. The tiles from this bank will be stored as a link to them, if one of those banks changes, it is renamed or deleted it will automatically updates the character.

![](/Images/emptyCharacter.png)

Press the plus button in the tab to create a new animation.

![](/Images/character.png)

From here it is possible to create frames for the animation. Clicking the plus button will create a new frame of the animation. When there is more than one frame, the play button, stop, pause, previous frame and next frame are available.

Here is also possible to set the animation speed. This value is in seconds per frame and this is also used when building the project to be used in the output file. More details explained in [Building the project](#Buildingtheproject) section.

![](/Images/edit_frame.png)

Scene position area is just for export the correct position only, **Relative Origin** are the yellow lines, this is used to calculate the origin where every x and y position is calculated so it is used as the 0,0 where the lines are. **Vertical Axis** is the red line and it is used as the center of the sprite. It is used to calculate the flip position of all the sprites and **Base** it is the position for the feets. This is the only one that it is exported because it could be used as a visual feet position for somme collisions or z sorting for example.

Demo of the sprites in motion from Mesen's Sprite Viewer.

![](/Images/demo1.gif)

<a name="Palettes"/>

### 3.4 Palettes

Here it is just simply, a 16 colors palette where is possible to pick and change the colors. This palettes are referenced by name for the [Characters](#Characters). It is required to link the palette with a character so the character can be exported.

![](/Images/palette.png)

<a name="Buildingtheproject"/>

## 4. Building the project

![](/Images/build.png)

Building the project will create a bunch of files in the output directory:

+ For each [Bank](#Banks) element, it will generate a .bin file.
+ An .asm file containing all the [Palette](#Palettes) elements called **palettes.asm**.

Example of the assembly output file for Palettes:
```
; This file is auto generated!

; Color format: RGB555 0BBBBBGGGGGRRRRR

    align 32
palette_axl:
    db 0xF7,0x67,0xAD,0x04,0x1A,0x3E,0x33,0x25,0xDF,0x56,0x6E,0x0C,0x12,0x0D,0xAD,0x31,0x29,0x21,0xA5,0x10,0x28,0x04,0xFF,0x56,0x9C,0x63,0x73,0x46,0xF7,0x56,0x00,0x00
palette_guy:
    db 0x00,0x42,0x31,0x04,0x1D,0x00,0x35,0x04,0x29,0x04,0xCD,0x04,0xBF,0x07,0x93,0x19,0x5E,0x02,0xF7,0x7A,0xFF,0x7F,0x17,0x2A,0x7E,0x2E,0xFF,0x36,0x7F,0x01,0x00,0x00
```
 
+ A header file.
+ Character files for their animations.

<a name="Buildingforbutano"/>

### 4.1. Building for Butano

Go to project properties from the menu or by pressing Ctrl+P to open the Project Properties dialog and set Butano for **Palettes**, **Characters**, **Screenblock**. For **Header** set it to None.

![](/Images/projectproperties.png)

Set the cpp and header folder to the correct place on your folder and then after building the project by pressing F5 or clicking Build Project from the menu.

This will generate cpp and header files with classes to handle meta sprites and its animations.

Here as an example the exported class is called **gbatool::Guy**, This one is a class that act as a meta sprite with its own sprite that can all be animated by calling the method, **load_animation**. The animation IDs are defined inside the class and those comes from the **GBATool** itself when you create [Characters](#Characters).

Header file example:
```
class title : public scene
{
public:
    title(bn::camera_ptr camera);
    void update();

private:
    bn::unique_ptr<gbatool::Guy> _player;
}
```

cpp file example:
```
title::title(bn::camera_ptr camera)
{
    _player.reset(new gbatool::Guy());
    _player->set_camera(camera);
    _player->load_animation(gbatool::Guy::AnimationID::WALKING);
    _player->set_position(bn::fixed_point(0, 0));
}

void title::update()
{
    _player->update_animation();
}
```


