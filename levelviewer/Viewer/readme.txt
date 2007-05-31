GK3 Scene Viewer
----------------
Version 0.1.0
Released May 27, 2007
Licensed under the GNU GPL (see license.txt)
http://gk3tools.sourceforge.net

=== INSTRUCTIONS ===
When you run the viewer it will ask for a path to a .brn file. This should probably be core.brn, but you could give it another barn if you wanted. Next the viewer will read the barn file and show a list of scene files to load.

Now that the scene is loaded you can use the arrow keys to move the camera and the mouse to look around. Press "L" to toggle lightmaps, press "T" to enable texture mapping, "F" to enable flat 


=== TROUBLESHOOTING ===
1. Opening a barn file at startup shows an error message that says "You must provide a valid barn file."

-- This means that an invalid barn was given to the browser, or the viewer couldn't read the barn file.

2. Loading a scene shows an error message that says "Unable to load [blah], possibly because the required files are inside barn files that the viewer couldn't find."

-- This means that you opened core.brn and either the scene file or the .bsp file that goes with the scene file was located in a barn that the viewer couldn't find. Try a different scene, or try adding additional .brn files into the same directory as core.brn.

3. Pressing "F" to enable flat shading everything goes white.

-- There's no shading being done on the polygons of the room. You'll either need to enable lightmaps (with "L") or switch to color shading (with "C") to actually make out anything in the room.


=== KNOWN ISSUES ===
1. Alpha testing doesn't work when lightmaps are enabled
2. Lightmap coordinates aren't 100% figured out yet

=== TODO ===
1. Load models
2. Add optional camera bounds

=== CHANGE LOG ===
0.0.1 - May 27, 2007
    - Initial release