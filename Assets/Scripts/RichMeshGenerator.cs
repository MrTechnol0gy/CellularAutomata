using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RichMeshGenerator : MonoBehaviour
{
    // Mesh Generation
    public SquareGrid squareGrid;
    // Edits to fix broken tutorial
    // Distinguish between walls and floors
    public Mesh wallMesh;
    public Mesh floorMesh;
    List<Vector3> vertices;
    List<int> triangles;
    // List of outlines
    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    // List of outlines
    List<List<int>> outlines = new List<List<int>>();
    // List of checked vertices
    HashSet<int> checkedVertices = new HashSet<int>();  // What is a hash set? https://www.geeksforgeeks.org/c-sharp-hashset-with-examples/
    public void GenerateMesh(int[,] map, float squareSize)
    {
        // Clear the lists
        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        squareGrid = new SquareGrid(map, squareSize);

        // Create a new mesh for walls
        wallMesh = CreateWallMesh();

        // Create a new mesh for floors
        floorMesh = GenerateFloors();
    }

    Mesh GenerateFloors()
    {
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
        floorMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = floorMesh;

        // Set the vertices and triangles of the mesh
        floorMesh.vertices = vertices.ToArray();
        floorMesh.triangles = triangles.ToArray();
        // Recalculate the normals of the mesh
        floorMesh.RecalculateNormals();

        return floorMesh;
    }

    Mesh CreateWallMesh()
    {
        // Calculate the mesh outlines
        CalculateMeshOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        wallMesh = new Mesh();
        float wallHeight = 5;

        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]); // left vertex
                wallVertices.Add(vertices[outline[i + 1]]); // right vertex
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight); // bottom left vertex
                wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight); // bottom right vertex

                // Add the triangles
                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }
        // Set the vertices and triangles of the wall mesh
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();

        // Recalculate the normals of the wall mesh
        wallMesh.RecalculateNormals();

        return wallMesh;
    }
    void DebugDrawNormals(Mesh mesh)
    {
        // Debug-draw normals in the scene view for debugging purposes
        for (int i = 0; i < mesh.normals.Length; i++)
        {
            Vector3 vertex = mesh.vertices[i];
            Vector3 normal = mesh.normals[i];
            float debugNormalLength = 1f;
            Debug.DrawRay(vertex, normal * debugNormalLength, Color.green);
        }
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
                // Because all surrounding sqaures are walls, we can add these directly to the index
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
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

        // Create a new triangle
        Triangle triangle = new Triangle (a, b, c);
		AddTriangleToDictionary (triangle.vertexIndexA, triangle);
		AddTriangleToDictionary (triangle.vertexIndexB, triangle);
		AddTriangleToDictionary (triangle.vertexIndexC, triangle);
    }

    // Triangle Dictionary
    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        // If the triangle dictionary already contains the vertex index key
        if (triangleDictionary.ContainsKey(vertexIndexKey))
        {
            // Add the triangle to the list of triangles
            triangleDictionary[vertexIndexKey].Add(triangle);
        }
        // Else, if the triangle dictionary does NOT contain the vertex index key
        else
        {
            // Create a new list of triangles
            List<Triangle> triangleList = new List<Triangle>();
            // Add the triangle to the list of triangles
            triangleList.Add(triangle);
            // Add the vertex index key and the list of triangles to the triangle dictionary
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    // Calculate Mesh Outlines
    void CalculateMeshOutlines()
    {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex ++)
        {
            if(!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);

                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    // Follow Outline
    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);

        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if(nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    // Get Connected Outline Vertices
    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        // Loop through every triangle that contains the vertex
        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            // Get the triangle
            Triangle triangle = trianglesContainingVertex[i];

            // Loop through every vertex in the triangle
            for (int j = 0; j < 3; j++)
            {
                // Get the vertex index
                int vertexB = triangle[j];

                // If the vertex index is not the same as the original vertex index and is not in the outline vertices list
                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    // If the vertex is an outline edge
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        // Return the vertex index
                        return vertexB;
                    }
                }
            }
        }
        return -1;
    }

    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        // Get the list of triangles that contain vertex A
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        // Create a counter for the number of triangles that contain vertex A and vertex B
        int sharedTriangleCount = 0;

        // Loop through every triangle that contains vertex A
        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            // If the triangle contains vertex B
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                // Increment the counter
                sharedTriangleCount++;
                // If the counter is greater than 1
                if (sharedTriangleCount > 1)
                {
                    // Break out of the loop
                    break;
                }
            }
        }

        // Return whether or not the counter is greater than 1
        return sharedTriangleCount > 1;
    }

    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        // Constructor
        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            // Create a new array of vertex indices
            vertices = new int[3];
            // Set the vertex indices
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        public int this[int i]
        {
            get
            {
                // Return the vertex index at the given index
                return vertices[i];
            }
        }

        public bool Contains(int vertexIndex)
        {
            // Return whether or not the triangle contains the given vertex index
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
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
