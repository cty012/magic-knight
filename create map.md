1. Maps are now named `Map1`, `Map2`, ... instead of `Scene1`, `Scene2`
2. Currently `Map1` is reserved for testing. Use `Map2` to create the actual map.
3. All objects in the map are created from a template in the `Assets/Resources/Prefabs` folder. To use the template:
   - Drag and drop the template you want to use to the desired hierarchial location in the scene to instantiate the prefab
   - Change the name of the instance you create (make sure it is unique and easy to recognize)
   - Change the `Pos X/Y` in the `Rect Transform` component. This locates the **center** of the object
   - Change the `Width` and `Height` properties in the `Rect Transform` component. Make sure that the `Size` property in `Sprite Renderer` and `Box Collider 2D` are changed accordingly.
4. `Soldiers` and `Slimes` have fixed sizes. Do not change their size because their `Sprite Renderer` will not automatically resize.
5. Remember to replace the `Main Camera` with the `Camera` in the `Assets/Resources/Prefabs` folder.
