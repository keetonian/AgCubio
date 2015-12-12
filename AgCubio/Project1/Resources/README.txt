AgCubio Client and server

By:
Daniel Avery
Keeton Hodgson

Current state of the project:

-----Database----- 12/11/15

AgCubio now updates a database (and in turn a webpage) on the death of a player

Front-end
	The following browser entries work with AgCubio:
		http://[ipaddress]:11100/players - shows the game stats for every player
		http://[ipaddress]:11100/games?player=[playername] - shows stats for a single player for all games the player was in
		http://[ipaddress]:11100/eaten?id=[gameid] - shows elaborated stats, including list of other players eaten, for a single player game
	In addition, entries on all webpages link to the other corresponding web pages

	Invalid browser requests to the server link to an error page, which links to the main “High Scores” page (first above)
	Invalid player names or id’s in corresponding browser entries link to a webpage with an empty table signifying no available info.
		This page links back to the main page
	All webpages are cleanly designed and tables look nice

Back-end
	Database uses two tables: Players (most stats) and Eaten (eaten players). This seemed like better practice than using one table and appending
		eaten player names in a long string that might get concatenated depending on allocated DB column space
	Database tables are reset when a server is closed and opened again.
	Stats values for a player are obtained from both the World class and the Server class, depending on where stats are more accessible. Stats are
		compiled in the server and added to the database upon player socket disconnection
	In Network, the Send method was modified with an optional parameter to distinguish between game server and web server sends (callback changes)
	A custom html-generating static class in Server aids with creating webpages, also done in server based on the query received in the web browser


-----Server-----

Current most up-to-date server is in the Server branch.

This project is only close to completion due to hours spent on compatibility. It took a lot of time and study to figure out how to make our client and the class client
work on both versions of the server on remote and local hosts. If we did not spend that time on that problem, more game mechanics would have been implemented.

We fixed several problems with our client code and are working on implementing everything in our server.


Implemented Features:
	Thread safety (locking)
	Client and server connections and communications
	Graceful socket disconnection and removal
	Movement using vector math
	Movement contained within world (invisible barriers - slide past)
	Eating food and players (slow algorithm, but insufficient time for optimizations)
	Attrition (players lose mass over time)
	Randomly generated food with random but vibrant color
	Players spawn at random starting coordinates where they won't be eaten
	Viruses spawn at random starting coordinates where they won't split players
	Start uid counter at 1 so that no player can have a uid of 0 and a teamid of 0 (give all cubes to his team)
	Unique uid's and uid recovery/reuse
	Food cubes: there are cubes of mass 1xfoodmass, 2x and 3x (increasing rarity)
	If you have split the max amount, you can eat viruses with no ill effects: they are simply very big food items
	Split inertia and remerging
	Little/no overlap of split cubes - slide past
	Player splits - each player cube with enough mass divides evenly in two, not compromising max split count
	Virus splits - produces max number of split cubes possible without compromising player start mass and max split count
		split cubes have purposefully uneven distribution of mass, and split in all directions randomly

	Military Viruses: red viruses that move in a set pattern
		Limited to a specific area (randomly chosen)
		Move in a four-leaf clover shape
		Eat food and viruses
		Eat player cubes if players are smaller; otherwise split player cubes

	World: 
		Contains the state of the simulation. Most computations happen in the world class.
		Reads an xml file of parameters (including new/custom parameters), located in the Resources folder

	Server: 
		Updates the world at a steady 'heartbeat' rate gained from the world
		Async connection requests

	Client(new):
		Center of mass frame of reference
		Circle grid background, drawn from center (maintains relative cube positions as player size changes)

*Note: Absorbtion distance delta was unused due to ambiguity on meaning and a preferred alternative - a larger cube eats
	a smaller cube when the larger overlaps the smaller's center

-------------------------------------------------------------


12/5/15 : Needs work :
	Splitting - moving in a decreasing speed towards a position, merging back together after a set time (RESOLVED)
	Viruses - split player (RESOLVED, beautifully)
	Optimization of other algorithms (most are slow currently, but work at the small scale)

	--- Polishing code ---

	Splitting: a bug where sometimes split cubes are not put into our data structure for tracking them (RESOLVED)
	Collisions code: much of it is the same, can be put into helper methods (SIMPLIFIED)
	bug: multiple players: eating cubes, sometimes a player cube gets left on the screen even after it is consumed
		This happens when the main player cube gets eaten and the player has other split cubes still. Work in progress.
	
	

	TESTING:
		It seems that for most purposes, testing is very tricky and hard to implement, as edge cases are most easily reached by players testing the code instead of code testing the code.
		We talk about each new implementation and draw/diagram it out, then test it several times by running the server and multiple clients.

	



	NOTE: most bug fixes have been documented in GitHub by committing data and commenting on what was done.



----------------

::Client Assignment (LEGACY)

Bugs(/Features!):
		Mouse coordinates are a little off for non-fullscreen window and split cubes, but for fullscreen it is fine.
		Split cubes focus on one cube instead of center of mass
		Splitting does not resize the screen, and some player cubes may go off the screen
		Food may sometimes be left onscreen after being eaten (rare bug)
(fixed)	Split cubes and cubes of other players not always drawn correctly, may overlap food (offset too far down and to the right)
(fixed)	Sometimes get a big red X through screen, not sure what that error means (very rare)
(fixed)	Sometimes get a disposed object exception that the server thread can’t access the socket after a player restarts a game after he dies.
		JSON parse errors after a player dies: Sometimes happens, but hasn't for a while. Perhaps we fixed the bug.


Design decisions:
	Have player cube follow previous mouse coordinates when the mouse leaves the window.
	Player stats at the end: we decided to only show the mass and the play time.
	Give food an extra scaling factor so it is always visible- may need to change when we create our own server.
	FPS- as fast as possible, because of our scaling algorithm: we are constantly scaling, and we like the look of how the food 
		shimmers and changes size according to player size, which is constantly changing due to attrition. We may slow it down later,
		but for now, the frames are working well and look cool as they refresh between 50-120 fps. It is shown in the upper-left
		hand side of the play window with the current player mass.
	Labels and text automatically center themselves.
	Names of cubes wrap inside of the cube, are centered, and the font size resizes depending on string length and cube size. 
		Thus, single letters are very big, and sentences are small, but all is usually displayed.
	Cubes: colored with the supplied color and its inverse, looks cool.
	Gives pop-up option to retry connecting if a connection to a valid IP address fails.
	The World class doesn't do much yet beyond store cube data, but I believe we will find more use for it in the next assignment.
	Network: SendCallBack method wasn't needed to send extra data, all data was well managed in the original Send method.
	Network: Buffer size: we used a big buffer size to allow us to read a lot of data all at once.
		The buffer dumps the data into a string
		The callback function appends the string into a stringbuilder, then parses it at the newline characters
		This implementation gets rid of partial data or unfinished strings that cause JSON deserialization errors.
	Background: Went with a neutral, almost unnoticeable color. Made a custom background, but did not figure out how to change it to play the game.
		We could implement it to where the player could choose a background color.

	(UPDATE: PS8)
	Background now has a grid of circles, to continue with the opposing theme towards agario (background of squares)
	Placement of cubes and scaling algorithm patched up, easier to test now that we control the server and know where things should be.


Server Problems:
	1. Defines cube size as being too big, therefore, our resizing and drawing does not look optimal. We will need to fix the server code.
	2. Movements: can be jerky, way too fast. Server controls speed, so the speed on that side needs to be lessened.
	3. Player starting size is much too big.
	4. World size is probably too small.




------------
Some final work notes:

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
		1. Add in a play time counter for how long a player survives- Complete.
		2. Center everything/make look nice/background/consistency when window is resized, game over, etc

------------