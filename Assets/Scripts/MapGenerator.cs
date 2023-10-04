using System.Collections;
using System.Collections.Generic;
using static System.Random;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    // Cellular Automata
    public int width;
    public int height;
    public string seed;
    public bool useRandomSeed;
    [Range(0, 100)]
    public int randomFillPercent;
    int[,] map;

    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
    }

    void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();

        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }

        // Create a border
        int borderSize = 5;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        // Loop through every coordinate in our map
        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            // For each column, loop through all the rows
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                // If the coordinate is within the map
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    // Set the coordinate to be the map coordinate
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                // Else, if the coordinate is NOT within the map
                else
                {
                    // Set the coordinate to be a wall
                    borderedMap[x, y] = 1;
                }
            }
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        // Passes the map to the mesh generator, along with the size of each square
        meshGen.GenerateMesh(borderedMap, 1);
    }

    void RandomFillMap()
    {
        // If we want to use a random seed
        if (useRandomSeed)
        {
            // Set the seed to be the current time
            seed = Time.time.ToString();
        }

        // Create a new random object with the seed
        System.Random seedInt = new System.Random(seed.GetHashCode());
        
        // Loop through every coordinate in our map
        for (int x = 0; x < width; x++)
        {
            // For each column, loop through all the rows
            for (int y = 0; y < height; y++)
            {
                // If the coordinate's on the very edge of the map (creates a border)
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    // Set the coordinate to be a wall
                    map[x, y] = 1;
                }
                // Else, if the coordinate is NOT on the border
                else
                {
                    // Set the coordinate to be either a wall or a floor
                    map[x, y] = (seedInt.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    // Smooths out the map
    void SmoothMap()
    {
        // Loop through every coordinate in our map
        for (int x = 0; x < width; x++)
        {
            // For each column, loop through all the rows
            for (int y = 0; y < height; y++)
            {
                // Get the number of walls surrounding the coordinate
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                // If the coordinate is a wall
                if (map[x, y] == 1)
                {
                    // If the number of walls surrounding the coordinate is less than 4
                    if (neighbourWallTiles < 4)
                    {
                        // Set the coordinate to be a floor
                        map[x, y] = 0;
                    }
                }
                // Else, if the coordinate is a floor
                else if (map[x, y] == 0)
                {
                    // If the number of walls surrounding the coordinate is greater than or equal to 5
                    if (neighbourWallTiles >= 5)
                    {
                        // Set the coordinate to be a wall
                        map[x, y] = 1;
                    }
                }
            }
        }
    }

    // Returns the number of walls surrounding a coordinate
    int GetSurroundingWallCount(int gridX, int gridY)
    {
        // Set the number of walls to be 0
        int wallCount = 0;

        // Loop through every coordinate surrounding the given coordinate
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                // If the coordinate is within the map
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                {
                    // If the coordinate is NOT the given coordinate
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                       wallCount += map[neighbourX, neighbourY];
                    }
                }
                // Else, if the coordinate is NOT within the map
                else
                {
                    // Increment the number of walls
                    wallCount++;
                }
            }
        }

        // Return the number of walls
        return wallCount;
    }

    // private void OnDrawGizmos() 
    // {
    //     if (map != null)
    //     {
    //         // Loop through every coordinate in our map
    //         for (int x = 0; x < width; x++)
    //         {
    //             // For each column, loop through all the rows
    //             for (int y = 0; y < height; y++)
    //             {
    //                 // If the coordinate is a wall
    //                 if (map[x, y] == 1)
    //                 {
    //                     // Set the color to be black
    //                     Gizmos.color = Color.black;
    //                 }
    //                 // Else, if the coordinate is a floor
    //                 else
    //                 {
    //                     // Set the color to be white
    //                     Gizmos.color = Color.white;
    //                 }

    //                 // Set a Vector 3 Position for the coordinate
    //                 Vector3 pos = new Vector3(-width / 2 + x + 0.5f, -height / 2 + y + 0.5f, 0);
    //                 // Draw a cube at the coordinate
    //                 Gizmos.DrawCube(pos, Vector3.one);
    //             }
    //         }
    //     }
    // }
}
