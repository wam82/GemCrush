using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "World", menuName = "Level")]
public class Level : ScriptableObject
{
    public int levelID;

    public int[] scoreGoals;

    public AudioClip backgroundMusic;

    public Sprite backgroundImage;
}
