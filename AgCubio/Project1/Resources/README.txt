Server Problems:
1. Defines cube size as being too big, therefore, our resizing and drawing does not look optimal. We will need to fix the server code.
2. Movements: can be jerky, way too fast. Server controls speed, so the speed on that side needs to be lessened.
3. Player starting size is much too big.

November 13, 2015:
Problems to still work on:
	// 1. Exiting gracefully when connection is lost
	2. Getting the correct mouse position in the resized screen when the cube has split
	/3. Draw things on a scale that looks good.
	4. Status values (Implement a timer for refresh speed, smooth things over.) (FPS, mass, scoreboard, etc)
	// 5. Allow to retry connecting after a failed connection.
	/6. Endgame scenario: provide some statistics, allow for player to play again
	7. Update only when the scene changes
	8. NETWORK: SendCallBack needs to be implemented.

	November 14, 2015:
	Problems fixed:
		1. Exiting gracefully when connection is lost, also prompts player to play again.
		2. Scaling looks ok and does well when player splits, but player positions after a split are still off.
		3. Retry implemented for a failed connection attempt.