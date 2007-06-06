GK3 Scene Viewer
----------------
Version 0.2.0
Released June 5, 2007
Licensed under the GNU GPL (see license.txt)
http://gk3tools.sourceforge.net

=== INSTRUCTIONS ===
The first thing you should do is make sure the search paths are set up correctly. You do that by clicking "File->Edit search paths." By default the directory that the viewer is run from will automatically be added as a search path, but unless the files you want to view are within this directory you'll need to add to the search path. To add a .brn file click the "Add barn" button. To add a directory click the "Add path" button.

Now you're ready to load a scene. Click "File->Open .scn," and the viewer will show a list of .scn files it can find (.scn files must be in one of the folders or .brn files listed as a search path).

Now that the scene is loaded you can use the mouse to look around and move. To look left and right just hold the left mouse button and move the mouse horizontally. To move forwards and backwards hold the left mouse button and move the mouse up and down. To adjust the camera's pitch hold the Shift key while moving the mouse up and down. Holding the left and right mouse buttons together while moving the mouse can adjust the height of the camera and move the camera from side to side.


=== TROUBLESHOOTING ===
1. Opening a barn file at startup shows an error message that says "You must provide a valid barn file."

-- This means that an invalid barn was given to the browser, or the viewer couldn't read the barn file.

2. Loading a scene shows an error message that says "Unable to load [blah], possibly because the required files are inside barn files that the viewer couldn't find."

-- This means that you opened core.brn and either the scene file or the .bsp file that goes with the scene file was located in a barn that the viewer couldn't find. Try a different scene, or try adding additional .brn files into the same directory as core.brn.

3. Turning off lightmapping and texturing makes everything go white.

-- There's no shading being done on the polygons of the room. You'll need to enable either lightmaps or texturing to make anything out in the scene.


=== KNOWN ISSUES ===
1. Alpha testing doesn't work when lightmaps are enabled

=== TODO ===
1. Remember search path settings
2. Load models
3. Add optional camera bounds

=== CHANGE LOG ===
0.1.0 - May 27, 2007
    - Initial release

0.2.0 - June 5, 2007
   - Got lightmapping to work
   - UI improvements