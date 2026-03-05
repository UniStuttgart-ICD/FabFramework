# FabFramework
[![Identifier](https://img.shields.io/badge/doi-10.18419%2Fdarus--4600-d45815.svg)](https://doi.org/10.18419/darus-4600)

## Overview
The framework is centered around a **Task-Based Fabrication** methodology, where design models are translated into specific, sequenced robotic tasks. Each fabrication step is data-driven and uses predefined parameters to generate, simulate, and execute fabrication operations.

## Key Components

- **Design Model**: The initial digital design that informs the entire fabrication process.
- **Fabrication Data**: This data is oriented to the specific setup and generated in-situ for accuracy, covering material handling and construction details.
- **FabFramework**: The core system managing all elements:
  - **DesignElement**: Organizes geometric data from the design model.
  - **FabElement**: Combines design and fabrication data to prepare the model for production.
  - **FabTask**: Stores and sequences data to guide robotic operations, including specific robot frames, tools, and movements.
  - **FabEnvironment**: Manages static fabrication parameters, such as tools, actors (robots), and peripheral devices.

![Screenshot 2024-10-24 093005](https://media.github.tik.uni-stuttgart.de/user/5072/files/5f935576-62b8-44ec-b652-613b1a5ede6c)

## General notes
Every _DesignElement_, _FabElement_ and _FabTask_ is stored in the _FabCollection_, when created. The _FabCollection_ acts as a 'local database' inside the GH-File. All data inside the _FabCollection_ can be received from any GH-Component on the canvas. Therefore it is important that every _DesignElement_, _FabElement_ and _FabTask_ has to have an individual name to distinguish between all objects and to avoid duplicates!

## Design vs Fab
-	_Design_ holds all the geometrically information, such as Brep, BoundingBox, BaseFrame, Length, Width, Height,…
-	_Fab_ holds all the fabrication necessary information, such as ReferencePlane In-Situ, ReferencePlane MaterialMagazine, ReferencePlane FabricationEnvironment,…
-	_Design_ is mostly used to extract geometric attributes to construct the necessary planes for a FabricationTask, e.g. defining the GlueLines for a beam depending on the length and width of the upper surface
-	_Fab_ is mostly used to orient Planes and Geometry to the correct position, e.g. to orient the GlueLines into the correct angle for the turntable during fabrication

## Beam vs Plate vs Component
-	Every _DesignBeam_, _DesignPlate_, _DesignComponent_ is a _DesignElement_
-	Every _FabBeam_, _FabPlate_, _FabComponent_ is a _FabElement_
-	_Element_ is the BaseClass and holds all the core attributes
-	_Beam_ holds specific attributes only relevant for a Beam
-	_Plate_ holds specific attributes only relevant for a Beam
-	A _Component_ is constructed from a list of _Beams_ and/or _Plates_

![Screenshot 2024-10-24 093023](https://media.github.tik.uni-stuttgart.de/user/5072/files/2084d108-740d-4701-97a8-de55100b7b4f)

 ![LinkedFabDesignElements](https://media.github.tik.uni-stuttgart.de/user/5072/files/230f9cdd-2f07-43c5-8e71-cf05f871d8ce)
 
## How to go about setting up your FabElements
1.	Define all your _DesignBeams_, _DesignPlates_
2.	Construct a _DesignComponent_ from your _DesignBeams_ and/or _DesignPlates_
3.	Define your _StaticEnvironments_:
a.	MagazineEnvironments: e.g. Plate- or BeamTables
b.	FabricationEnvironment: e.g. TurnTable
4.	Link your _StaticEnvironments_ with _DesignComponent_
5.	As a result you end up with a _FabComponent_, which holds your _FabBeams_ and _FabPlates_
a.	Each _FabElement_ is still linked with its respected initial _DesignElement_

![Screenshot 2024-10-24 093103](https://media.github.tik.uni-stuttgart.de/user/5072/files/5b95fb56-355a-40de-85cd-cc6ef30148f8)

![Screenshot 2024-10-24 095934](https://media.github.tik.uni-stuttgart.de/user/5072/files/40d27085-2b0e-4d42-914b-1d75cae1e226)

## FabElements

Every FabElement holds the following informations:
- RefPln_Situ (blue): Main Reference plane of the element.
- RefPln_SituBox (cyan): Reference plane in the corner of the boundingbox of the element.
- RefPln_Mag (red): Reference plane of the element on the magazine.
- RefPln_Fab (orange): Reference plane of the element in the fabrication environment.
- RefPn_FabOut (green): Reference plane of the element in the position it will be manipulated during fabrication.

 ![FabComponents](https://media.github.tik.uni-stuttgart.de/user/5072/files/1a0a3700-4819-40de-b567-b4e5ab9ef92b)

## Finalize FabricationEnvironment
1.	Define _Tools_: e.g. Endeffectors (VacuumGripper) or Cutters (20mm Milling Bit)
2.	Define _Actors_: e.g. KukaRobot
3.	Define _Actions_ linked to _Tools_: e.g. VacuumGripper can “Pick” & “Place”
4.	Define _Actions_ linked to _Actors_: e.g. KukaRobot can “StoreTool” & “TakeTool”
5.	_FabricationEnvironment_ is now finalized and ready to be used to create _FabTasks_ 
 
 ![Screenshot 2023-11-16 121606](https://media.github.tik.uni-stuttgart.de/user/5072/files/747e22da-73d5-48af-b7aa-3992587ddc60)
 
 ## FabTasks
-	FabTasks hold all the necessary information for the actor to execute a task.
-	FabTasks can be split up into FabTask and FabTaskFrame
-	FabTask are all Tasks, which are not frame based: e.g. “StoreTool”, “TakeTool”,…
-	FabTaskFrame are all Tasks, which are frame based: e.g. “GlueBotPlate”, “PickBeam”,…
-	In order to create a FabTask, create a new FabTask object and populate it with the necessary information as seen below. 

**Example of necessary data for a FabTask:**
- Name: _‘Take VacuumGripper with TinTin’_
- Action: _linked Action (TakeTool)_
- Tool: _linked Tool (VacuumGripper)_
- Actors: _linked Actor (TinTin)_

**Example of necessary data for a FabTaskFrame:**
- Name: _Pick_FE_P_Plate_0_
- Action: _linked Action (Pick)_
- Tool: _linked Tool (VacuumGripper)_
- Actors: _linked Actor (TinTin)_
- Main_Frames: _O(2176.85,-533.18,292.00) Z(0.00,0.00,1.00)_
- Main_ExtValues [Dictionary]: _[E1, 469.98], [E2, 120]_
- Offset: _[0, 0, 300]_

![Screenshot 2024-10-24 093122](https://media.github.tik.uni-stuttgart.de/user/5072/files/c517893b-0daa-40cc-9038-a55323fa994b)

![Screenshot 2024-10-24 093145](https://media.github.tik.uni-stuttgart.de/user/5072/files/6783b22e-b67f-452e-9a97-1b219b5b574c)

![Screenshot 2023-11-16 124023](https://media.github.tik.uni-stuttgart.de/user/5072/files/07443779-7bdd-4a3b-84da-6781504d6b02)

- It is also possible to add adtional informations, which are not necessary for the system to work, but can be useful for simulation, e.g. Geometry, FabElementsName, ...
- Further some FabTasksFrames such as Gluing need additional informations, such as Start And End Frames of the GlueLines. Here Main_ & Sub_Frames can be used.
- Using Main_ExtValues and Sub_ExtValues it is possible to add information as a dictionary. For example to define the rotation angle of the turntable for each frame (E2).

![Screenshot 2024-10-24 093202](https://media.github.tik.uni-stuttgart.de/user/5072/files/aaa05c7c-2371-4633-a85b-511e9914fb95)

 ## Deconstruct Everything
 These tools can be used to deconstruct all the individual classes to check the properties. Useful for troubleshooting.
 
 ![Screenshot 2023-11-14 162616](https://media.github.tik.uni-stuttgart.de/user/5072/files/f2361271-d6cc-4240-88c7-5e279f424ea9)
 
## FabCollection
 The tools can be used to get an overview of all DesingElements, FabElements and FabTasks in the current GH-File. It is also possible to reset the entire database. When doing so, make sure you also recompute the canvas, as otherwise the database will be empty and will break certain components!
 
 ![Screenshot 2024-10-24 093230](https://media.github.tik.uni-stuttgart.de/user/5072/files/b90ab548-aad4-4dbf-85a0-c55470f3b841)
![Screenshot 2024-10-24 093249](https://media.github.tik.uni-stuttgart.de/user/5072/files/5d82e0b8-9798-449c-97e4-16b7e47b624f)

## VirtualRobot-Link to simulate all FabricationTasks
These tools deconstruct a list of selected FabTasks into the necessary data for simulation with VirtualRobot. It is possible to extract the data as a list of all FabTasks to plug it into a VirtualRobot Ghost Solver or to receive the data from a single FabTask at a time to visually simulate the Task with VirtualRobot.

![Screenshot 2024-10-24 093301](https://media.github.tik.uni-stuttgart.de/user/5072/files/465601a9-604e-4538-ad14-02a84fd90d6d)

