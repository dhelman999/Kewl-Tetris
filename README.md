# Kewl-Tetris
Unity-based Tetris clone

My interpretation of a themed Tetris with Unity!

Game Description:

Movement - W, A, S, D and arrow keys
           W and UpArrow performs rotation
           SpaceBar drops the piece

A level up occurs after every 10 rows are cleared
Level up triggers custom effects and a Color palette change
Level up also speeds up the dropping of the pieces


Features include:

* The 7 original tetris pieces with new bright colors.
* Hand-drawn UI and inspired from the original game.
* A 'next' piece area that shows what piece will come next.
* A statistics area that keeps track of the number of each piece.
* Layered border to hide initial pieces.
* A toggleable shadow that shows where a dropped piece will go.
* Smooth movement while holding down movement keys.
* Custom effects for entering the game, destroying rows, leveling up, game end.
* Color palette change on level up.
* Current/High score tracking.
* Movement sounds
* Soundtrack

Build/Development description:

Unity Version: 2021.3.33f1
Microsoft Visual Studio Community 2022 (64-bit) - Version 17.5.5

Developed at a base resolution of 1920x1080

Large files pushed with:

https://git-lfs.com/

ArtifactDB and some *.dylib files were too large and required the following:

git lfs track ArtifactDB

git lfs track ".dylib"

git add .gitattributes

git lfs migrate import --include="*.dylib"

git lfs migrate import --include="ArtifactDB"

