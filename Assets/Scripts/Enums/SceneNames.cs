// Centralized scene name constants.
// Being used isntead of hardcoded strings - preventing typos and making refactoring easier.

public static class SceneNames
{
    // Scene names must match those in Unity's Build Settings exactly.
    // TODO: Check Build Settings to check names are correct.

    public const string MainMenu = "MainMenu";

    // Main gameplay scene in which the player builds
    // fortifications & fights waves of enemies.
    // TODO: Fix tutorial system stuff soon.
    public const string Game = "Game";

    // TODO: Add further as required.
}
