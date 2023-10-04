using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    // Mesh Generation
    public SquareGrid squareGrid;
    List<Vector3> vertices;
    List<int> triangles;
    public void GenerateMesh(int[,] map, float squareSize)
    {
        squareGrid = new SquareGrid(map, squareSize);

        // Create lists of vertices and triangles
        vertices = new List<Vector3>();
        triangles = new List<int>();

        // Loop through every coordinate in our map
        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                // Triangulate the square
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        // Create a new mesh
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Set the vertices and triangles of the mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        // Recalculate the normals of the mesh
        mesh.RecalculateNormals();
    }

    // Triangulation
    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;
            // 1 points:
            case 1: // 0001
                MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                break;
            case 2: // 0010
                MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                break;
            case 4: // 0100
                MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                break;
            case 8: // 1000
                MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;
            // 2 points:
            case 3: // 0011
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 6: // 0110
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 9: // 1001
                MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 12: // 1100
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            case 5: // 0101
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 10: // 1010
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;
            // 3 points:
            case 7: // 0111
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 11: // 1011
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 13: // 1101
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 14: // 1110
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;
            // 4 points:
            case 15: // 1111
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                break;
        }

    }

    // Mesh From Points
    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
        {
            CreateTriangle(points[0].vertexIndex, points[1].vertexIndex, points[2].vertexIndex);
        }
        if (points.Length >= 4)
        {
            CreateTriangle(points[0].vertexIndex, points[2].vertexIndex, points[3].vertexIndex);
        }
        if (points.Length >= 5)
        {
            CreateTriangle(points[0].vertexIndex, points[3].vertexIndex, points[4].vertexIndex);
        }
        if (points.Length >= 6)
        {
            CreateTriangle(points[0].vertexIndex, points[4].vertexIndex, points[5].vertexIndex);
        }
    }

    // Assign Vertices
    void AssignVertices(Node[] points)
    {
        // Loop through every point
        for (int i = 0; i < points.Length; i++)
        {
            // If the point has not been assigned a vertex index
            if (points[i].vertexIndex == -1)
            {
                // Assign the vertex index to be the number of vertices
                points[i].vertexIndex = vertices.Count;
                // Add the point to the list of vertices
                vertices.Add(points[i].position);
            }
        }
    }

    // Create Triangle
    void CreateTriangle(int a, int b, int c)
    {
        // Add the vertices to the list of triangles
        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(c);
    }

    // Square Grid
    public class SquareGrid
    {
        public Square[,] squares;
        public SquareGrid(int[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            // Create 2d array of control nodes
            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            // Loop through every coordinate in our map
            for (int x = 0; x < nodeCountX; x++)
            {
                // For each column, loop through all the rows
                for (int y = 0; y < nodeCountY; y++)
                {
                    // Calculate the position of the node
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                    // Create a new control node at that position
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
                }
            }

            // Create the squares
            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            // Loop through every coordinate in our map
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                // For each column, loop through all the rows
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    // Create a new square at that position
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }
    public class Square
    {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;
        
        // Constructor
        public Square (ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;
            
            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;

            // If the top left node is active
            if (topLeft.active)
            {
                // Add 8 to the configuration
                configuration += 8;
            }
            // If the top right node is active
            if (topRight.active)
            {
                // Add 4 to the configuration
                configuration += 4;
            }
            // If the bottom right node is active
            if (bottomRight.active)
            {
                // Add 2 to the configuration
                configuration += 2;
            }
            // If the bottom left node is active
            if (bottomLeft.active)
            {
                // Add 1 to the configuration
                configuration += 1;
            }
        }
    }
    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos)
        {
            position = _pos;
        }
    }

    // Control nodes are the nodes that are used to create the mesh
    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;

        // Constructor
        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
        {
            active = _active;
            // Create the nodes above and to the right of this node
            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
        }
    }

    // Gizmos
    // void OnDrawGizmos()
    // {
    //     if(squareGrid != null)
    //     {
    //         for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
    //         {
    //             for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
    //             {
    //                 // Draw the top left to bottom right diagonal
    //                 Gizmos.color = (squareGrid.squares[x, y].topLeft.active) ? Color.black : Color.white;
    //                 Gizmos.DrawCube(squareGrid.squares[x, y].topLeft.position, Vector3.one * 0.4f);

    //                 // Draw the top right to bottom left diagonal
    //                 Gizmos.color = (squareGrid.squares[x, y].topRight.active) ? Color.black : Color.white;
    //                 Gizmos.DrawCube(squareGrid.squares[x, y].topRight.position, Vector3.one * 0.4f);

    //                 // Draw the centre top to centre bottom line
    //                 Gizmos.color = (squareGrid.squares[x, y].bottomRight.active) ? Color.black : Color.white;
    //                 Gizmos.DrawCube(squareGrid.squares[x, y].bottomRight.position, Vector3.one * 0.4f);

    //                 // Draw the centre left to centre right line
    //                 Gizmos.color = (squareGrid.squares[x, y].bottomLeft.active) ? Color.black : Color.white;
    //                 Gizmos.DrawCube(squareGrid.squares[x, y].bottomLeft.position, Vector3.one * 0.4f);

    //                 // Draws the centre of each node
    //                 Gizmos.color = Color.grey;
    //                 // Draw the centre top node
    //                 Gizmos.DrawCube(squareGrid.squares[x, y].centreTop.position, Vector3.one * 0.15f); 

    //                 // Draw the centre right node
    //                 Gizmos.DrawCube(squareGrid.squares[x, y].centreRight.position, Vector3.one * 0.15f);

    //                 // Draw the centre bottom node
    //                 Gizmos.DrawCube(squareGrid.squares[x, y].centreBottom.position, Vector3.one * 0.15f);

    //                 // Draw the centre left node
    //                 Gizmos.DrawCube(squareGrid.squares[x, y].centreLeft.position, Vector3.one * 0.15f);
    //             }
    //         }
    //     }
    // }
}
