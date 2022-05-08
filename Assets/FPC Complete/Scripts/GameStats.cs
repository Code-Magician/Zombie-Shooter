using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// JUST A CLASS WITH STATIC VARIABLES WHICH CAN BE ACCESSED FROM ANY SCRIPT DIRECTLY...
public class GameStats : MonoBehaviour
{
    public static bool gameOver = false;
    public static bool canShoot = true;
    public static bool reachedHome = false;
    public static bool allLivesFinished = false;
}
