SampahCleaner - Walking Robot Animation version
================================================
Saved: 2026-10-24
Unity version: 6000.5.8f1 (Unity 6.5), URP

What is this?
  The version of the project that has the robot animations working
  (Idle / Walk / Run) plus the gameplay scripts.

How to open
  Unity Hub -> Add -> select this folder -> open with 6000.5.8f1.
  The first open takes a few minutes (Library folder is rebuilt automatically).

What is inside (Assets/)
  Models/LandRobot     LandRobot.fbx, LandRobot.controller (Idle/Walk/Run via "Speed"), LandRobot.prefab
  Models/TrashRobot    Level1 / Level2 / Level3 robots + controllers, TrashRobot.prefab
  Models/SwimRobot     swim robot model (static mesh, no animation yet)
  Models/Trash         bottle, box, can (fbx + prefab)
  Scripts              RobotController (WASD move, Shift run), EvolvingRobot,
                       TrashCollector, TrashItem, TrashSpawner
  Scenes               MainMenuScene, SampleScene

How to test the walking robot
  Open Scenes/SampleScene, put a robot prefab in the scene with the
  RobotController script, press Play. WASD = walk, Shift = run.

Note
  Library, Temp and Logs folders are not included (Unity recreates them).
