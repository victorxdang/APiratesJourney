/*****************************************************************************************************************
 - IUpdatableObject.cs -
-----------------------------------------------------------------------------------------------------------------
 Author:             Victor Dang
 Game/Program Name:  A Pirate's Journey
 Engine Version:     Unity 2018.3.3f1
-----------------------------------------------------------------------------------------------------------------
 Description: 
     Contains UpdateMe and Active methods. Implement this interface in game objects that requires 
     frame-by-frame updates. UpdateMe() will replace Unity's built-in Update() function. Active() will return
     whether or not the game object is active or whatever other condition is required in order for it to
     update (see each class for its implementation of Active).

 Classes that implement this interface:
    - Ship
    - Player (inherited from Ship)
    - Enemy (inherited from Ship)
    - Map
    - Cannonball
*****************************************************************************************************************/

public interface IUpdatableObject
{
    void UpdateMe();
    bool Active();
}
