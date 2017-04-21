using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Kinect;
using UnityEditorInternal;
using UnityEngine;

namespace KinectVR
{
    public class KinectMesh
    {

        //
        // Public properties
        //


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
        private GameObject _meshHolder;

        // Width and height of Mesh
        private int _width;
        private int _height;

        // Meshes that will be rendered
        private Mesh[] _meshes;

        // We store the individual components of the mesh
        // so that we don't have to access it via the mesh each time
        private Vector3[][] _vertices;
        private Vector2[][] _uv;
        private int[][] _triangles;

        // Kinect related stuff
        private KinectSource _multiManager;

        // Max distance between two vertices in a triangle after which it stops being rendered in the mesh
        private const int TriangleThreshold = 10;
    
        // Works for powers of 2 past 1 ie 2^1, 2^2 etc.
        private const int DownSampleSize = 2;

        private const double DepthScale = 0.1f;

        public KinectMesh(int height, int width, GameObject meshHolder)
        {
            // Sets up width and height
            _width = width;
            _height = height;

            // We floor as we don't want half rows as they are a pain to deal with
            // Substract 1 as we need overlap on all but two rows
            int fakeRowsPerMesh = Mathf.FloorToInt( MaxVertices / (float) width ) - 1;

            // Caculates the number of meshes, rounding down to eliminate error caused
            // by the two rows that don't overlap
            int meshCount = Mathf.FloorToInt( (float) height / (float) fakeRowsPerMesh );

            // We need at least one mesh
            if (meshCount == 0)
                meshCount = 1;

            // Creates the necessary meshes array
            _meshes = new Mesh[ meshCount ];

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
                        _uv[i][index] = new Vector2(((float) x / (float) width), ((float) y / (float) rowsPerMesh));

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

                SetMeshData(i);
            }

        }

        // Gets the index of the Mesh from a vertex index
        public int GetMeshIndex(int vertexIndex)
        {
            return Mathf.FloorToInt(vertexIndex / (float) MaxVertices);
        }

        // Loads depth data from a kinect source
        public void LoadDepthData(ushort[] depthData, 
            Windows.Kinect.FrameDescription frameDesc,
            ColorSpacePoint[] colorSpace, int colorWidth, int colorHeight)
        {
            for (int m = 0; m < _meshes.Length; m++)
            {

                // Populates positions of the vertices
                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        int index = (y * (_width)) + x;

                        double sum = 0.0;

                        if (depthData[index] == 0)
                            sum += 4500;
                        else
                            sum += depthData[index];

                        sum = sum * DepthScale;

                        _vertices[m][index].z = (float) (-4500.0 * DepthScale) + (float) sum;

                        // Update UV mapping with CDRP
                        var colorSpacePoint = colorSpace[(y * _width) + x];
                        _uv[m][index] = new Vector2(
                            colorSpacePoint.X / (float) colorWidth,
                            colorSpacePoint.Y / (float) colorHeight);
                    }
                }
            }
        }

        // Loops thru triangles, pruning the ones that 
        // have depths over the defined threshold
        private void PruneTriangles()
        {
            for (int m = 0; m < _meshes.Length; m++)
            {
                // Loops through Triangles, removing any stretchy ones
                List<int> tempTriangle = new List<int>();

                for (int i = 0; i < _triangles[i].Length; i+=3) {

                    // Check the distance between the vertices in the triangles
                    int triangleV1 = _triangles[m][i];
                    int triangleV2 = _triangles[m][i + 1];
                    int triangleV3 = _triangles[m][i + 2];

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
