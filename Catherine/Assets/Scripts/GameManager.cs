using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int mapArrY { get; private set; }
    public int mapArrZ { get; private set; }
    public int mapArrX { get; private set; }

    public int[] blockArray;

    public enum BlockType
    {
        EMPT,
        NORMAL
    }

    // Start is called before the first frame update
    void Start()
    {
        mapArrY = 2;
        mapArrZ = 2;
        mapArrX = 5;

        blockArray = new int[mapArrY * mapArrZ * mapArrX];


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
