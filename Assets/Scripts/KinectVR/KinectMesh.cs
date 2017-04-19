using System;
using System.Collections.Generic;
using System.Linq;
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
        private int[][] _defaultTriangles;

        KinectMesh(int height, int width, GameObject meshHolder)
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

            // Creates the necessary meshes array
            _meshes = new Mesh[ meshCount ];

            // Creates an array to store the default triangles for each mesh
            _defaultTriangles = new int[ meshCount ][];


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

                // Sets the object's parent holder
                holder.transform.parent = meshHolder.transform;

                // Initializes the vertices, uvs, triangles
                int rowsPerMesh = Mathf.FloorToInt( MaxVertices / (float) width );

                _meshes[i].vertices = new Vector3[rowsPerMesh * width];
                _meshes[i].uv = new Vector2[rowsPerMesh * width];
                _meshes[i].triangles = new int[6 * (rowsPerMesh - 1) * (width - 1)];

                int triangleIndex = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = (y * width) + x;

                        _meshes[i].vertices[index] = new Vector3(x, -y, 0);
                        _meshes[i].uv[index] = new Vector2(((float) x / (float) width), ((float) y / (float) rowsPerMesh));

                        // Skip the last row/col
                        if (x != (width - 1) && y != (rowsPerMesh - 1))
                        {
                            int topLeft = index;
                            int topRight = topLeft + 1;
                            int bottomLeft = topLeft + width;
                            int bottomRight = bottomLeft + 1;

                            _meshes[i].triangles[triangleIndex++] = topLeft;
                            _meshes[i].triangles[triangleIndex++] = topRight;
                            _meshes[i].triangles[triangleIndex++] = bottomLeft;
                            _meshes[i].triangles[triangleIndex++] = bottomLeft;
                            _meshes[i].triangles[triangleIndex++] = topRight;
                            _meshes[i].triangles[triangleIndex++] = bottomRight;
                        }
                    }
                }

                // Before we finish, store a copy of the default triangles
                _defaultTriangles[i] = (int[]) _meshes[i].triangles.Clone();

            }

        }

        // Gets the index of the Mesh from a vertex index
        int GetMeshIndex(int vertexIndex)
        {
            return Mathf.FloorToInt(vertexIndex / (float) MaxVertices);
        }

        Vector3[] GetVertices()
        {
            // Gets the total vertex length
            Vector3[] vertices = new Vector3[_vertexCount];
            
            int index = 0;
            foreach (Mesh m in _meshes)
            {
                m.vertices.CopyTo(vertices, index);
                index += MaxVertices;
            }

            return vertices;

        }

        Vector2[] GetUV()
        {
            // Gets the total vertex length
            Vector2[] uvs = new Vector2[_vertexCount];
            
            int index = 0;
            foreach (Mesh m in _meshes)
            {
                m.uv.CopyTo(uvs, index);
                index += MaxVertices;
            }

            return uvs;
            
        }

        List<int> GetTriangles()
        {
            // Triangle counts vary so we must recalculate each time
            // Additionally, all vertices in a triangle must belong
            // to the same mesh.

            List<int> triangles = new List<int>();

            foreach (Mesh m in _meshes)
            {
                foreach (int t in m.triangles)
                {
                    triangles.Add(t);
                }
            }

            // We return a List object directly since in any case, we will need
            // to work with one later, and there is no sense in converting back
            // and forwards
            return triangles;

        }

        //
        // Setters
        //

        // Sets up Vertices for all meshes
        void SetVertices(Vector3[] vertices)
        {
            for (int i = 0; i<vertices.Length; i++)
            {
                _meshes[Mathf.FloorToInt( (i + 1) / (float) MaxVertices)].vertices[i] = vertices[i];
            }
        }

        // Sets up UV for all meshes
        void SetUV(Vector2[] uv)
        {
            for (int i = 0; i<uv.Length; i++)
            {
                _meshes[Mathf.FloorToInt( (i + 1) / (float) MaxVertices)].uv[i] = uv[i];
            }
        }

        // Sets up Triangles for all meshes
        void SetTriangles(List<int> triangles)
        {
            List<int> tempTriangles = new List<int>();

            int prevMesh = 0;

            for (int i = 0; i < triangles.Count; i++)
            {
                // Since triangles vary, we need to do some checks on the vertices
                // to find out what mesh they belong to
                int meshIndex = Mathf.FloorToInt((triangles[i] + 1) / (float) MaxVertices);

                if (meshIndex != prevMesh)
                {
                    _meshes[prevMesh].triangles = tempTriangles.ToArray();
                    prevMesh++;
                    tempTriangles = new List<int>();
                }

                tempTriangles.Add(triangles[i]);
            }

            // Final case for the last vertices
            if (prevMesh != _meshes.Length - 1)
            {
                _meshes[prevMesh].triangles = tempTriangles.ToArray();
            }
        }

        // Calls recalculate normals on all meshes
        void RecalculateNormals()
        {
            foreach (Mesh m in _meshes)
            {
                m.RecalculateNormals();
            }
        }

        void PruneTriangles()
        {
            
        }

    }
}
