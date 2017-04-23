//  =====================================================================
//  OculusExplore
//  Copyright(C)                                      
//  2017 Maksym Perepichka
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//  GNU General Public License for more details.
//            
//  You should have received a copy of the GNU General Public License 
//  along with this program.If not, see<http://www.gnu.org/licenses/>.
//  =====================================================================

using System.Collections.Generic;
using Windows.Kinect;
using UnityEngine;

namespace KinectVR
{
    public class KinectMesh
    {
        //
        // Constants
        //

        // Max number of vertices per mesh.
        // Note that this number must be a multiple of 3 as we can't have
        // Triangles referring to multiple meshes
        private const int MaxVertices = 64998;

        //
        // Members
        //

        // Object that will have the mesh objects as children
        private GameObject[] _meshHolders;

        // Width and height of Mesh
        private int _width;
        private int _height;

        // Meshes that will be rendered
        private Mesh[] _meshes;

        // We store the individual components of the mesh
        // so that we don't have to access it via the mesh each time
        private Vector3[][] _vertices;
        private Vector2[][] _uv;
        private int[][] _defaultTriangles;
        private int[][] _triangles;

        // Kinect related stuff
        private KinectSource _multiManager;

        // Max distance between two vertices in a triangle after which it stops being rendered in the mesh
        private const int TriangleThreshold = 20;
    
        private const double DepthScale = 0.2f;

        public KinectMesh(int width, int height, GameObject meshHolder)
        {
            // Sets up width and height
            _width = width;
            _height = height;

            // We floor as we don't want half rows as they are a pain to deal with
            // Substract 1 as we need overlap on all but two rows
            int fakeRowsPerMesh = Mathf.FloorToInt( MaxVertices / (float) width ) - 1;

            // Caculates the number of meshes, rounding down to eliminate error caused
            // by the two rows that don't overlap
            int meshCount = Mathf.FloorToInt( (float) height / (float) fakeRowsPerMesh ) + 1;

            // Creates the necessary meshes array
            _meshes = new Mesh[ meshCount ];

            // Creates the necessary mesh holder array
            _meshHolders = new GameObject[ meshCount ];

            // Creates an array to store the default triangles for each mesh
            _vertices = new Vector3[ meshCount ][];
            _uv = new Vector2[ meshCount ][];
            _triangles = new int[ meshCount ][];

            int baseRowsPerMesh = Mathf.FloorToInt(MaxVertices / (float) width);

            for (int i = 0; i < _meshes.Length; i++)
            {
                // Initializes the meshes
                _meshes[i] = new Mesh();

                // Creates an object to store a mesh filter
                var holder = new GameObject();
                holder.name = "MeshHolder";

                // Adds a mesh filter to the object
                var filter = holder.AddComponent<MeshFilter>();
                filter.mesh = _meshes[i];

                // Adds a mesh renderer
                var renderer = holder.AddComponent<MeshRenderer>();

                // Hack to get default material
                renderer.material = new Material(Shader.Find("Diffuse"));

                // Sets the object's parent holder
                holder.transform.parent = meshHolder.transform;

                // Resets holder's own position as it gets messed up
                holder.transform.localPosition = new Vector3(0, 0, 0);

                // Adds the holder to the array
                _meshHolders[i] = (GameObject) holder;

                // Gets the rows per mesh
                int rowsPerMesh = baseRowsPerMesh;

                // Resizes it if this is the last row
                if (i == _meshes.Length - 1)
                {
                    int remainder = height % rowsPerMesh;

                    // If the remainder is 0, we don't change rowsPerMesh
                    if (remainder != 0)
                    {
                        rowsPerMesh = remainder;
                    }
                }

                // Initializes the vertices, uvs, triangles
                _vertices[i] = new Vector3[rowsPerMesh * width];
                _uv[i] = new Vector2[rowsPerMesh * width];
                _triangles[i] = new int[6 * (rowsPerMesh - 1) * (width - 1)];

                int triangleIndex = 0;
                for (int y = 0; y < rowsPerMesh; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = (y * width) + x;

                        _vertices[i][index] = new Vector3(x, -y - (i*(baseRowsPerMesh-1)) , 0);
                        _uv[i][index] = new Vector2(
                            ((float) x / (float) width),
                            ((float) (y + i * (baseRowsPerMesh - 1) )/ (float) height)
                        );

                        // Skip the last row/col
                        if (x != (width - 1) && y != (rowsPerMesh - 1))
                        {
                            int topLeft = index;
                            int topRight = topLeft + 1;
                            int bottomLeft = topLeft + width;
                            int bottomRight = bottomLeft + 1;

                            _triangles[i][triangleIndex++] = topLeft;
                            _triangles[i][triangleIndex++] = topRight;
                            _triangles[i][triangleIndex++] = bottomLeft;
                            _triangles[i][triangleIndex++] = bottomLeft;
                            _triangles[i][triangleIndex++] = topRight;
                            _triangles[i][triangleIndex++] = bottomRight;
                        }
                    }
                }

                _defaultTriangles = (int[][]) _triangles.Clone();

                SetMeshData(i);
            }

        }

        // Gets the index of the Mesh from a vertex index
        public int GetMeshIndex(int vertexIndex)
        {
            return Mathf.FloorToInt(vertexIndex / (float) MaxVertices);
        }

        // Loads depth data onto the models
        public void LoadDepthData(ushort[] depthData, Texture colorTexture, ColorSpacePoint[] colorSpacePoints,  int colorWidth, int colorHeight)
        { 
            int baseRowsPerMesh = Mathf.FloorToInt(MaxVertices / (float) _width);

            for (int i = 0; i < _vertices.Length; i++)
            {
                _meshHolders[i].GetComponent<Renderer>().material.mainTexture = colorTexture;

                int offset = (i * (baseRowsPerMesh - 1) * _width); 

                for (int j = 0; j < _vertices[i].Length; j++)
                {
                    int depthIndex = offset + j;

                    double sum = depthData[depthIndex] ;//== 0 ? 4500 : depthData[depthIndex];

                    _vertices[i][j].z = (float) (-4500.0 * DepthScale) + (float) (sum * DepthScale);

                    var colorSpacePoint = colorSpacePoints[depthIndex];

                    _uv[i][j] = new Vector2(
                        (colorSpacePoint.X  / (float) colorWidth),
                        (colorSpacePoint.Y / (float) colorHeight)
                    );

                }
                SetMeshData(i);
            }
            PruneTriangles();
        }

        // Loops thru triangles, pruning the ones that 
        // have depths over the defined threshold
        private void PruneTriangles()
        {
            for (int m = 0; m < _meshes.Length; m++)
            {
                // Loops through Triangles, removing any stretchy ones
                List<int> tempTriangle = new List<int>();

                for (int i = 0; i < _defaultTriangles[m].Length; i+=3) {

                    // Check the distance between the vertices in the triangles
                    int triangleV1 = _defaultTriangles[m][i];
                    int triangleV2 = _defaultTriangles[m][i + 1];
                    int triangleV3 = _defaultTriangles[m][i + 2];

                    float distA = Mathf.Abs(_vertices[m][triangleV1].z - _vertices[m][triangleV2].z);
                    float distB = Mathf.Abs(_vertices[m][triangleV1].z - _vertices[m][triangleV3].z);

                    // If under the threshold, push the vertices. Otherwise, don't
                    if (distA < TriangleThreshold && distB < TriangleThreshold)
                    {
                        tempTriangle.Add(triangleV1);
                        tempTriangle.Add(triangleV2);
                        tempTriangle.Add(triangleV3);
                    }
                }

                _triangles[m] = tempTriangle.ToArray();

                SetMeshData(m);

            }
        }
    
        // Sets the mesh data for all meshes
        private void SetMeshData(int meshIndex)
        {
            _meshes[meshIndex].vertices = _vertices[meshIndex];
            _meshes[meshIndex].uv = _uv[meshIndex];
            _meshes[meshIndex].triangles = _triangles[meshIndex];
            _meshes[meshIndex].RecalculateNormals();
        }

    }
}
