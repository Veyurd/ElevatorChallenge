# ElevatorChallenge
DVT Elevator Challenge


**Instructions**

Run the app, input the number of elevators in the system and the maximum floor available in the building.


_Assumptions_

In the current implementation, there is a single type of elevator _"StandardElevator"_

We will assume all persons have the same weight (for occupancy calculation), that each stop is instantaneous (all persons enter and exit the elevator instantly).

I have also assumed the building has no underground floors, although the algorithm could be altered to account for that as well.

Additionally we know empirically the best elevators system return the cars to the lobby when not in use, although i have not done that in this situation.

The elevator class could be broken down into further classes to insure better single responsibility but for visualization reasons i have left it slightly monolithic.

The architecture could be extended with further elevators (more occupancy, different speed) and potentially different algorithms altogether.