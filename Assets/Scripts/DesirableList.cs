/*
 * 
Every 2 months, go over classes and reorganize
rename them
make a diagram of their relationships
Utility classes should not be relying on any other classes, they should just be computation algorithms

----------- To Do for TD: -----------
Add in town hall including its core block
Add in Block Damage
Add in block Damage Gui - red health bar for blocks
Add in spawn zones - just one - 30 blocks away from town hall
Add in orders for bots to destroy the town hall
bots behaviour - attack blocks
Add in turrets - animate them
Building times for different block structures - with the zone being a no enter zone until its finished building
Highscore screen when the town hall dies - and close the level


  ---------- To ReDo: -------------
	Remake turrets and turret placement
	Redo ai, basic shooting, moving to new positions
	redo building generation
	redo block structures
	Core Block
  ---------- To Do: -------------
 OnDestroyBlockOfType Event for worlds
 OnStepOnBlock Event for characters
 OnMoveToNewChunk event for characters
 OnItemsChange Event for characters linked with Saving

 Preview placement mesh for block structures (Requirement: BlockStructures)
---------------------


Each block type will have a block data
I need to pass in the block data into the rendering algorithm so it creates on or the other

Biome map
When you enter a town, it should say -you have enter x town - at the top as a systems message
all the ingame gui should be toggleable in options



--Model Editing Section--
Export to 3dsMax option - bones and all

Bone Editor		-
	Top, Front, Left views - Left click and drag to position bones
Chunk Editor	- Each chunk to have their own textures - uses same rules as the terrain
Texture Editor	- 
Animation Editor- Position bones along time slider
Mesh Destruction Animation
Subdivision of block in voxel engine - and changing of resolution at a distance
Created pieces of broken up cube


Spells to charge while holding the left click and disperse when released



Render Preview for placing Blocks
Render preview for placing Block Structures
Effects to show on enemy - Ie particle effects for burn and slow
Voxel Physics: Make one Object for physics, which controls its movements, the other kinametic one, will only interact with players
ShipCore	- when activated, camera goes on it - press q/e to shift between cameras
CameraCube

Plane with animator:
Animations, transitioning, after stepping on a block, for last 10 blocks - dissapears after 3 seconds
Destruction animation for blocks destroying - with health bar for blocks - they will regenerate up to 100% after a while
BlockDestruction - holds block position, health, health max, defence etc

Utilities Coding:
Move the position of the model
Editing building prefabs inside a window
Add blocks
Remove blocks
--- basically i will need to spawn another world, have it small, and render them all at the position of the 3d gui texture ---



Sound:
make one music theme song, and also one song for level
Make sound effects for enemies


better textures - with normal maps
better shaders for textures



More Props for buildings
-----------------------------
Section : Block Engine
desc: Anything related more to the core of the blocks, rather then the designing element
-----------------------------
Voxels:
Subdivision on blocks - the closer ones to be .5 size, in between (2 chunks away) to be 1 size, and anything past 6 chunks to decrease by 1 subdivision each time
Smooth Terrain blocks
Lighting for blocks - Colour variable for blocks - with smoothed lighting

Have props load in, like barrels etc in chunks
Have bots load/save in and out of chunks

Add in ability to modify sides of blocks - ie model data
	After this, add in the culling ability. The culling rules will be based on if 2 polygons overlap each other. 
	For blocks this will work easily. For others I will need to check if one side is smaller, if it is, remove only the smaller side
	These rules will be based on every possible pair. Ie if there is 2 slopes and a block, it will be: block x block, block x slope1, block x slope2, slope1 x slope2.
	The idea behind this is merely making everything precalculated
	It also gives the option of flexability when adding new meshes into the voxel engine
	Then make a BlockModel which has a reference to the model data for each side  - SideModel[6] - which will merely be added to the chunk when its loading
	After I create this culling mechanism I can generate heaps of weird shapes and add them in

Need a combined mesh block that goes accross block spaces
Possibly even animates and chunks block spaces - ie like an elevator
	possible solution: have another mesh in the chunk that can animate, on for each movement as ill need to move the bones/physics 
	- so i guess another object for each animation 
	- the blocks will all link to that object

Rotate blocks when placing them
	- will need the ability to change the sides of the models
	- this will require more batching of model culling
	
Spaceships made from voxels

Monster Spawning in dungeons
	- need monsters to be saved in blocks

loading new chunks still lags the rendering - this is due to the mesh colliders - turning it off will reveal this 
	- i need this managed better 1 every 2 frames or something

Building Physics Constraints like Space Engineers
Instead of monsters randomly spawning, generate population sizes, and save it into the chunks
save the characters when chunks load/unload
disable character movement based on camera position - if they out of range disable them

-----------------------------
Section : Block Structure Generation
desc: The generation of structures, like buildings or dungeons or towns
-----------------------------
Tall towers like in wind waker, for the water, ladder leading to the top of them - its a diamond shape/or circle shape platform with a diamond shape roof - with some enemies and a chest appears when you defeat them
Castle towers - its the //even or odd blocks placed at the top
Castle - castle towers on corners and big hall in centre
Nexus - basically a diamond shape thing - needs a red crystal texture - these blocks need to have lots of hp - will be used in team fotress mode

AStar for room to room in dungeons
Room Order in dungeons - with monsters difficulty to increase / puzzle rooms

-----------------------------
Section : Block Mechanics
desc: anything related to the voxel data
-----------------------------
Door blocks - activate with redstone to open
Invisible block - has collisions but you can't see it. Use a special spell to 'show it' it, the render data will be Added to a seperate mesh
Secret block - you can walk through passages, the outside of these blocks will be rendered (like water culling), but the collision data will not be added
Brick block - smash it with a hammer and it will animate (expand/deflate - wobble), and release coins
Ladders - test in fps level first - when collide with a side of them you can walk up the blocks
Torches - with emmision light
Redstone - that creates logic blocks around it - have BlockLogic with an additional byte used for its logic gate
Deformed blocks - ones like OOR online, slightly deformed
Moving Grass Model
Tree Model
Gravity Core
Treasure chests - and spawning in dungeons
	animation -Treasure chests to open up
	^ program a treasure chest in voxel blockstructure
	Use bones on top of chest mesh to open and close it
	A block item to have an inventory in it
Make basic chivel example
Have 2 slopes, if i click on either corner it becomes one slope or the other
-----------------------------
Section : Combat Mechanics
desc: anything to do with spells, or consumables, or weapons, or armor
----------------------------

Add in spell charging, hold down the button and release to fire
Flying cube to charge and fire lasers at targets
	-LaserBeams as an attack (rather then a projectile, its a long beam

----------------------------
Section : GUI
desc: All of the interfaces etc
----------------------------
Body Creation screen
	- add in bones
	- premade human bones for most characters
	
Character creation screen
	- name character
	- which models for which body parts - click on a body part and then click on a model
	- or edit the models right there
World Creation Screen
	- Edit the height map for each biome
	- Edit biome creation variables - shape/size etc	- base shape will be a hexagon for biomes
	

Load Game Screen			- maybe just an icon somewhere that says how many chunks are loading, just for the intial load
Menu screen to fade between different NPC's
Perhaps they are on different worlds?
just have it randomly switch every 60 seconds to a new npc
perhaps show some more GUI information for the bot

Texture Editor

Model Viewer Extras:
	3D icon for the mode, like 3ds max, one for move, rotate, edit blocks, scale
Server Admin Tools:
Spell Editor	- edit their properties
Blocks Editor 	- edit their properties
Item Editor		- Same

Gear Gui Screen - placement of 'helmet'
Model screen to render character models
Window around Chatbox when in gui/pause screen
macro creator
macros to enter text into chatbox

Gui for Genetics of character
Open up another players item gui
party gui - a list of characters in your party (includes summons)

MapGui to:
	load any saved parts of the map / explored parts - keeps the rest as grey
	if I enter a new chunk, load that onto the map gui saved textures - check every 3 seconds if i enter a new chunk
6/9/2015:
DungeonCreator Additions:
	full screen mode
	add in a load/save system to block structures
	gui things to edit them
===Done===
Model Viewer
Moveable/Closeable windows - / Resizable

----------------------------
Section : Characters
desc: Mostly to do with the feel of the characters animations/sounds/stories
----------------------------
3d canvas for health/name over players heads

^ first make it so i can open up other players inventories

Characters:
Dynamic death animations - rag doll
Dynamic walk cycle - position bones at certain points
Need the bones attached using joints, merely stop the scripted positioning after it dies and it will fall
This way in fights the player make have its legs hit and fall over
Hands model for first person view

Randomly create their names from a list - and/or make a name generator

make 5 different enemy types - with custom: spells - movement - item drops
5 Different Character Designs - for one dungeon
each one to be a different chactacter - with different attack styles - but the same theme

i.e. Water Temple:
All of the creatures will have ruin like symbols that glow
The temple itself will be painted like this
	Squid like creeps in the water
	Bat like creatures in the cave, that goes deeper into the water
	Shark Humanoid Creatures
	Slime like creatures - similar to zeldas slimes
	Boss - Similar to Gyradose, its parts can move on their own


Genetics to alter textures on characters
Genetics to alter aggressiveness and strength
strength to alter model - blend shapes for character strength / fatness
Customizable clothing for character models
----------------------------
Section : Weather
desc: Lighting/particles basically, anything to do with the feeling of weather systems
----------------------------
snow lags it up a bit - probaly the collisions
Add in snow/wind etc to only certain biomes
rain
puddles
reflective water
Sun Cycles with shadows
ambient lighting
atmosphere on different planets depending on their humidity etc
Day/night cycle
Volumetric Procedural Clouds
Rain Effects
----------------------------
Section : Model Editing
desc: The main functions behind the model editor
----------------------------
Slicing cube
Crushing cube
Custom Bone Sizes
Create bone tool for a skeletal mesh
Select bone tool
Edit bone positions tool
Edit bone sizes tool (should make verticies bigger)
----------------------------
Section : Texture Editing
desc: all of them juicy pixel editing abilities
----------------------------
A list of the block textures
a block texture window
a block texture tool window

tools:
	place pixel
	place pixel structure - like paint brush tools
	select pixel
	select pixels - rectangle select tool for now
	cntrl + c = copy pixels
	Move selected pixels
	delete selected pixels
----------------------------
Section : Game Modes
desc: A list of game modes with rules unique to them
----------------------------
DeathMatch:
	Round Ends when all players are dead - multiple rounds per play through - map switches at the end
	Work on death match mode
	show scores at the end
	loads in the death match
	make it a dungeon level with rooms connect up, and hallways - basic like that - maybe 10 rooms
	ie don't load the map
	start in spawn point
	have particle effects for when spawn

Team DeathMatch:
	The same as above but players are in teams and start at a 'team base'
King of the hill:
	Points awarded to the team who secure a base the longest. There will be one base on this map
Timed match:
	Points are awarded to kills
Dungeon Mode:
	X amount of floors
	Coop between players
	Enemies spawn in dungeon rooms
	
Options:
	Friendly fire can be turned on so friendlys can harm each other
	Leveling can be turned on, so kills reward EXP
	Gain Spells can be turned on, so players can gain spells when they level up
Map Generation options:
	Size	- can be unlimited
	LoadOnce - if the map is a suitable size, it will just load one and players will not be able to leave the area - this may help with jagginess
	Biome settings 	- if the main building has an exit players can wander the lands - this should be limited for enjoyable gameplay
All modes will have a 'match history' to view
----------------------------
Section : Bot AI - (Bot Movement class)
desc: related to the movement of the bot
----------------------------
Render spheres for movement paths
summon swarm - summons 5 flying drones that fly flock together and attack the same targets - flocking algorithm is already implemented
For flying bots, have their flight use energy so sometimes they have to sleep - 1 energy per 1 minutes of flying
Have flying bots fly close to the ground when wandering - at least for an option - just use raytrace in gravity direction to find ground - if no ground below add some force that way
when pressing alt, make it so i can select multiple way points for bots
----------------------------

Possible efficiency improvements:
Recycle the GameObjects
    put them in a Stack
    Set Renderer.Enabled to false
    pop the stack when you need an object, if the stack is empty you create a new GO.


  ----------Done: -------------
Rotate the model
Effects Icons
Stats to add when pressing + / -
Teleport Spell
Map Gui
Item drops on character deaths
Subdivision of plane
fix the lag when switching items
add in a dungeon creator gui:
	the texture to view it
	left click to regenerate
	
editing models works perfectly - no lag - added in threading and batching for the mesh per block

Lags when Switching item, Placing item in gui - this was due to all the updates of the textures - this should not be done very often

Placing blocks still lags .1 of a second - fixed using threading although its a bit glitchy accross chunks - have not properly tested
multiplayer - hosting/finding to work
bombs to destroy blocks
bombs to have their own particle effect
bombs particles to be accurate to the explosion size

Bedrock now cannot be destroyed

Bad Bugs:
Fix chunk bug, where updating a chunk doesnt update the one next to it. This is new since I used threads for chunk loading.
Multiplayer - characters that join don't have the right player index set
	- fix : debug all players loaded for a client - check their indexes - if the server player is just loaded after ... get the player indexes from the server

 */