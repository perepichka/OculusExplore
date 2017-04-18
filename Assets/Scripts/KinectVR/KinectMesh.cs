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

        // Meshes that will be rendered
        private Mesh[] _meshes;
        private int _vertexCount;

        // Original triangles
        private int[] defaultTriangles;

        //KinectMesh(int height, int width)
        //{
        //    // Creates the necessary meshes array
        //    _meshes = new Mesh[ Mathf.CeilToInt((height * width) / (float) MaxVertices) ];

            
        //    _vertices = new Vector3[width * height];
        //    _uv = new Vector2[width * height];
        //    _defaultTriangles = new int[6 * ((width - 1) * (height - 1))];

        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            int index = (y * width) + x;

        //            _vertices[index] = new Vector3(x, -y, 0);
        //            _uv[index] = new Vector2(((float)x / (float)width), ((float)y / (float)height));

        //            // Skip the last row/col
        //            if (x != (width - 1) && y != (height - 1))
        //            {
        //                int topLeft = index;
        //                int topRight = topLeft + 1;
        //                int bottomLeft = topLeft + width;
        //                int bottomRight = bottomLeft + 1;

        //                _defaultTriangles[triangleIndex++] = topLeft;
        //                _defaultTriangles[triangleIndex++] = topRight;
        //                _defaultTriangles[triangleIndex++] = bottomLeft;
        //                _defaultTriangles[triangleIndex++] = bottomLeft;
        //                _defaultTriangles[triangleIndex++] = topRight;
        //                _defaultTriangles[triangleIndex++] = bottomRight;
        //            }
        //        }
        //    }


        //}

        KinectMesh(Vector3[] vertices, Vector2[] uv, int[] triangles, GameObject meshHolder)
        {
            // Sets up the meshholder
            _meshHolder = meshHolder;

            // Number of meshes needed
            int numberOfMeshes = Mathf.CeilToInt(vertices.Length / (float) MaxVertices);
            
            // Initializes the mesh array
            _meshes = new Mesh[numberOfMeshes];

            //int vertexCounter = vertices.Length;

            // Initializes the meshes
            for (int i = 0; i < _meshes.Length; i++)
            {
                _meshes[i] = new Mesh();

                // Creates an object to store a mesh filter
                var holder = new GameObject();
                holder.name = "MeshHolder";

                // Adds a mesh filter to the object
                var filter = holder.AddComponent<MeshFilter>();
                filter.mesh = _meshes[i];
                
                // Sets the object's parent holder
                holder.transform.parent = meshHolder.transform;
                
                //// Initializes the vertices, uvs, triangles
                //int verticesLengths;
                //if (vertexCounter >= MaxVertices)
                //{
                //    verticesLengths = MaxVertices;
                //    vertexCounter -= MaxVertices;
                //}
                //else
                //{
                //    verticesLengths = vertexCounter;
                //    vertexCounter = 0;
                //}

                //_meshes[i].vertices = new Vector3[verticesLengths];
                //_meshes[i].uv = new Vector2[verticesLengths];

                //_meshes[i].triangles = new Vector2[width * height];
            }

            // Sets up vertex count
            _vertexCount = vertices.Length;

            // Initializes everything
            foreach (Mesh m in _meshes)
            {
                int size = (i != (vertices.Length - 1) || (vertices.Length % MaxVertices == 0) )
                    ? MaxVertices : (vertices.Length % MaxVertices) + 1 ;

                // Since both of these are the same length, we can set them up at the same time
                _meshes[GetMeshIndex(i)].vertices = new Vector3[size];
                _meshes[GetMeshIndex(i)].uv = new Vector2[size];

                // We need more voodo math for the triangles
                int triangleSize = 
                
            }

            // Sets up vertices, UVs
            for (int i = 0; i < vertices.Length; i++)
            {
                if (_meshes[GetMeshIndex(i)].vertices == null)
                {
                    // Bunch of voodoo math to determine the size of the array 
                    // (it is either MaxVertices size or the size of the remainder, with a special 
                    // case for 0 remainder which is the same as being MaxVertices)

                } 
            }

            // Sets up triangles
            int triangleCount = triangles.Length;
            for (int i = 0; i < triangles.Length; i++)
            {
                _meshes[].triangles
            }

            // Sets up Triangles
            for (int i = 0; i < Mathf.CeilToInt(triangles.Length / MaxVertices * 3); i += MaxVertices * 3)
            {
                _meshes[i].t
            }

        }

        // 
        // Getters
        //

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
