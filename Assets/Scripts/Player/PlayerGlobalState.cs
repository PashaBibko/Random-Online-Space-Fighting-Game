using UnityEngine;

public static class PlayerGlobalState
{
    // The three different class "allignments" the player can have //
    public enum PlayerClass
    {
        GUNNER,
        SCOUT,
        ENGIE
    }

    public static PlayerClass PrimaryClass = PlayerClass.GUNNER;
    public static PlayerClass SecondaryClass = PlayerClass.GUNNER;
}
