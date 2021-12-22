# Some Tips for Transferring to Unity
## Major changes to the project
1. Screen size is reset to *1920x1080* to fit a larger portion of the map (easier to navigate in game).
2. Scripts are categorized into four categories: `Managers`, `Controllers`, `Events`, and `Others`. They can be found in the `Scripts` folder. They are all renamed with certain patterns to avoid confusion (E.g. *XxxManager*, *XxxController*, *XxxEvent*). ***PLEASE REMEMBER TO CHECK THE NAMING CONVENTION BEFORE CREATING A NEW FILE/VARIABLE!!!*** That may require you to read though other people's code. Having consistent names in the project can greatly reduce the work.
3. Just a reminder for how to name stuff:
   - Folders and files: **PascalCase**
   - Name of scripts: **PascalCase**. Depend on category. See #2
   - Name of functions: **PascalCase**. This is different from Java.
   - Name of variables: **camelCase**
   - Name of constants and enums values: **UPPER_SNAKE_CASE**
   - Name of String tags (such as the one used in the event system): **lowercase-separated-with-hyphen**
4. Scenes are also renamed. *SceneXXX* is replaced with *MapXXX*.
## Scene and map creation
1. For game objects, they need to use `Rect Transform` component instead of normal `Transform` (Just directly add the `Rect Transform` component).
2. For Non-sprite objects (Map, Terrain, Enemy, etc.), set `Pivot` to be *(0, 0)* so that objects can use them as references. For sprite objects (Player, Slime1, Ground1, etc.), set `pivot` to be *(0.5, 0.5)* so that the sprite can be rendered correctly.
3. However, if the position of the sprite doesn't matter (*XxxManager*) then you can use the normal `Transform` and set everything to *0*.
4. When resizing an object, change the `Scale` property instead of changing the `Width` and `Height` properties. Otherwise the `Sprite` will not be resized. (The `Width` and `Height` properties should both be kept at *1*).
5. Map and world coordinates are now aligned.
## Scripts
1. Keep track of the console messages you add during debugging. You do not want to leave behind debugging messages that are too verbose. (btw `Debug.Log()` is how to log messages in Unity)
2. Unity supports checking the state and hierarchy of the scene and nodes during runtime. Make good use of that when debugging your code.
3. Currently I assume everyone uses either *visual studio* or *visual studio code*. If anyone uses another IDE/code editor remeber to add the configuration files in the `.gitignore` file in the root folder.
4. Make all variables private or provide getters and setters if necessary. If the variable is public it will show up in the Unity inspector as a property, so unless you mean to do that, make it private/provide getters and setters. (See PlayerController.cs for details)
