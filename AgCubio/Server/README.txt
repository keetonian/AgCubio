Server Readme

Server maintains state of the world.

1.main - the main function which will build a new world and start the server.


2.start - populate the initial world (with food), set up the heartbeat of the program, and await network client connections. (Note: I suggest using a timer object for the heartbeat.)


3.handle new client connections - (note this is most likely used as the callback function required by the networking code parameters). This code should set up a callback to receive a players name and then request more data from the connection.


4.receive the player name - this function should create the new player cube (and of course update the world about it) and store away all the necessary data for the connection to be used for further communication. It should also set up the callback for handling move/split requests and request new data from the socket. Finally it should send the current state of the world to the player.

Note: the current server exemplar adds the player to the world before the initial state is sent. the initial state can take seconds to arrive, thus the player's cube can be eaten, or even move, while this is happening. An excellent solution will avoid this (i.e., the player's game only really starts after all the data has been sent.


5.handle data from the client - this data should be either (move,x,y) or (split,x,y). In either case you should handle the request.

Note: when move,x,y does not mean move the cube to x,y, but move the cube toward x,y.

Note: this code should be the callback used by the networking code when it receives data.


6.update - this is the main "heart" of the game. During each update event, the server should :
1.Grow new food
2.Handle Players eating food or other players
3.Handle player cube attrition (and other things, like food growth)
4.Handle sending the current state of the world to every client (new food, player changes, etc.).

If a client has disconnected since the last update, this should be cleaned up here.

Additionally, you could instrument your server so that if you send the magic name: "observer", no player cube is created.










TODO:
The World Class

The world represents the "state" of the simulation. On the server side the world's functionality will be greatly expanded to be responsible for tracking all objects in the game as well as providing all the game mechanics.

The world needs at least the following new functionality. Please note that some of the functionality will be optional for those who are looking for an "excellent" category grade. Good solutions will not require it.

Modifications for the Server

1.Constants:
1.Width, Height - the game size of the world
2.Heartbeats per second - how many updates the server should attempt to execute per second. Note: adequate work will simply update the world as fast as possible.
3.Top speed - how fast cubes can move when small
4.Low speed - how fast the biggest cubes move
5.Attrition rate - how fast cubes lose mass
6.Food value - the default mass of food
7.Player start mass - the default starting mass of the player.
8.Max food - how many food to maintain in the world. Server should update one food per heartbeat if below this.
9.Minimum split mass - players are not allowed to split if below this mass
10.Maximum split distance - how far a cube can be "thrown" when split
11.Maximum splits - how many total cubes a single player is allowed. Note: our test server does not implement this. Try setting it to around 10-20.
12.Absorb distance delta - how close cubes have to be in order for the larger to eat the smaller

Excellent Work: the constants should be set to read only variables and set by parsing a game state XML file.


2.Splitting

The server must handle split nodes. I suggest a separate data structure from the main cubes list which stores those cubes which are part of a split. Alternatively, you could augment your cube data structure to store this info.

When a split request comes from a client, all cubes that belong to a given player and have sufficient mass, are split (respecting the maximum splits constant). The split happens along the line from each node to the requested position and distance should have some ratio to the size of the cube. Excellent work will make this a "tweakable" value in order to optimize game play (i.e., splitting too far makes the game less fun (for others) and splitting too short makes the game less fun (for the player).

Timer: When a cube splits it should have a time set. The cube should be marked as not being allowed to merge until the time elapses.

Momentum: When a cube splits, it should not immediately jump to the final "split point", but should instead have a momentum that moves it smoothly toward that spot for a short period of time.


3.Absorbing (aka eating)

Any time a larger cube significantly overlaps a smaller cube, the smaller cube should be absorbed (all mass transferred) into the bigger cube and the smaller cube's demise should be broadcast to all clients and the world updated.

Note: if the eaten cube is part of a "split", then you must update any data structures tracking this information.

IMPORTANT Note: if the eaten cube is part of a "split" AND is the original cube of the player, PRESERVE the unique id of the cube! The easiest way to do this is to swap the unique id of the original (now eaten) cube with one of its team cubes. (If there are no team cubes, then it is game over for the player.)


4.Moving

In the absence of a move command, cubes for a player should not be moved. If a move command comes from the client, all cubes associated with the player should move toward that spot.

Overlapping: Split cubes are not allowed to overlap (by more than a little bit) until their split time is elapsed. How you choose to implement this is up to you but will have a big impact on the "look and feel" of the game. For example: if you simply don't move cubes that are overlapped, you will see cubes "trapped/stopped" when they accidentally get set to overlap while splitting. If you simply don't move cubes that would overlap because of the move, you will find your cubes "freeze up" as they all try to go to the same spot.

 The Edge of the WorldYou should not allow cubes to leave the world. Some overlap (up to say 30% of the width of the cube is allowable


5.Food

One food should be randomly generated and placed on the world per heartbeat. Excellent work will make this amount easily variable (via a constant) so that game play tweaking can occur.

Food Eating Functionality Requirement: Any time a player is on top of a food, the player's mass should be increased by the size of the food and the food destroyed. Note: anytime any cube (food/player) is destroyed, a "final message" needs to be sent to every client with the cube mass set to 0.

Note: how will implement the above is up to you but we expect a easily understandable algorithm that is reasonably efficient. For example, you could check every food cube vs. every player cube every heartbeat of the game. Do you think this is a reasonable algorithm? Your README should discuss your choice of implementation.

Another possible way to show excellence is to allow food to randomly grow larger (mass++). This should not happen often and should be tweakable by a parameter to improve the game experience.


6.Viruses

Viruses implementation is not required for a passing grade, but to get higher scores, you must add this new game play mechanic to the game.

A virus should be a large green cube. Any player that would "eat" the virus will instead be "exploded" and the virus destroyed.

Exploded means that the players cube is randomly broken up. The algorithm you choose for doing this is up to you. The net result is that one cube becomes a few larger cubes and a number of smaller cubes.

Viruses should randomly spawn on the world (but not on top of a player). The rate and size of the virus "infestation" should be controllable by input parameters.

Document your virus strategy in your README.


7.Attrition

At each heartbeat of the game every player cube should lose some portion of its mass. Larger cubes should lose mass faster than smaller cubes. Cubes less than some mass (say 200) should not lose mass. Cubes less than some mass (say 800) should only lose mass very slowly. Cubes above 800 should rapidly start losing mass. (Again this should be tweakable).


8.Server as Exemplar

Note: any functionality that is present in the sample server should be preserved even if not mentioned in this specification. That being said, if there is functionality you wish to modify, you may do so but must document it in the README. (For example: it is not stated here that food should have random colors, but you need to do this. If you, as the game designer, decided all food should be red, then you could do so, but you would need to document this and defend the decision.)

