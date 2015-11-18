Server Problems:
1. Defines cube size as being too big, therefore, our resizing and drawing does not look optimal. We will need to fix the server code.
2. Movements: can be jerky, way too fast. Server controls speed, so the speed on that side needs to be lessened.
3. Player starting size is much too big.

November 13, 2015:
Problems to still work on:
	// 1. Exiting gracefully when connection is lost
	// 2. Getting the correct mouse position in the resized screen when the cube has split
	// 3. Draw things on a scale that looks good.
	// 4. Status values (, smooth things over.) (FPS, mass, scoreboard - top 3, etc)
	// 5. Allow to retry connecting after a failed connection.
	// 6. Endgame scenario: provide some statistics, allow for player to play again
	// 7. Update only when the scene changes- our scene is constantly changing because of scaling, attrition.
	// 8. NETWORK: SendCallBack needs to be implemented.

	November 14, 2015:
	Problems fixed:
		1. Exiting gracefully when connection is lost, also prompts player to play again.
		2. Scaling looks ok and does well when player splits, but player positions after a split are still off.
		3. Retry implemented for a failed connection attempt.

	November 16
	Tasks:
		1. Add in a play time counter for how long a player survives.
		2. Keeton - center everything/make look nice/background/consistency when window is resized, game over, etc

------------

Bugs(/Features!):
	Mouse coordinates are a little off for non-fullscreen window and split cubes
	Split cubes focus on one cube instead of center of mass
	Splitting does not resize the screen, and some player cubes may go offscreen
	Food may sometimes be left onscreen after being eaten
	Split cubes not always drawn correctly, may overlap food

Design decisions:
	Have player cube follow previous mouse coordinates when the mouse leaves the window
	Give food an extra scaling factor so it is always visible

