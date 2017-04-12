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

using System;
using UnityEngine;
using System.Collections;
using Windows.Kinect;
using Boo.Lang;

public class KinectViewer : MonoBehaviour
{ 
    public GameObject KinectSource;
    
    private KinectSensor _sensor;
    private CoordinateMapper _mapper;

    private Mesh _mesh;

	// Since Unity has hard limit of 65k vertices per mesh, we will need multiple meshes
	// in order to fit the Mesh within unity without downsampling
	private Mesh[] _Meshes;

    private Vector3[] _vertices;
    private Vector2[] _uv;

	// Array of Default triangle indices.
    private int[] _defaultTriangles;

    // Source of Kinect info
    private KinectSource _multiManager;

    private double _timeCount = 0.0;
    private double _timeLimit = 1.0;

    // Max distance between two vertices in a triangle after which it stops being rendered in the mesh
    private int TriangleThreshold = 10;
    
    // Works for powers of 2 past 1 ie 2^1, 2^2 etc.
    private const int DownSampleSize = 2;

    private const double DepthScale = 0.1f;


    void Start()
    {
        _sensor = KinectSensor.GetDefault();
        if (_sensor != null)
        {
            _mapper = _sensor.CoordinateMapper;
            var frameDesc = _sensor.DepthFrameSource.FrameDescription;

            // Downsample to lower resolution
            CreateMesh(frameDesc.Width / DownSampleSize, frameDesc.Height / DownSampleSize);

            if (!_sensor.IsOpen)
            {
                _sensor.Open();
            }
        }
    }

    void CreateMesh(int width, int height)
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;

        _vertices = new Vector3[width * height];
        _uv = new Vector2[width * height];
        _defaultTriangles = new int[6 * ((width - 1) * (height - 1))];
     

        int triangleIndex = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width) + x;

                _vertices[index] = new Vector3(x, -y, 0);
                _uv[index] = new Vector2(((float)x / (float)width), ((float)y / (float)height));

                // Skip the last row/col
                if (x != (width - 1) && y != (height - 1))
                {
                    int topLeft = index;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + width;
                    int bottomRight = bottomLeft + 1;

                    _defaultTriangles[triangleIndex++] = topLeft;
                    _defaultTriangles[triangleIndex++] = topRight;
                    _defaultTriangles[triangleIndex++] = bottomLeft;
                    _defaultTriangles[triangleIndex++] = bottomLeft;
                    _defaultTriangles[triangleIndex++] = topRight;
                    _defaultTriangles[triangleIndex++] = bottomRight;
                }
            }
        }

        _mesh.vertices = _vertices;
        _mesh.uv = _uv;
        _mesh.triangles = _defaultTriangles;
        _mesh.RecalculateNormals();
    }

    void Update()
    {
        _timeCount+=Time.deltaTime;

        if (_sensor == null)
        {
            return;
        }
			
        if (KinectSource == null)
        {
            return;
        }
        
        _multiManager = KinectSource.GetComponent<KinectSource>();

        if (_multiManager == null)
        {
            return;
        }
        
        gameObject.GetComponent<Renderer>().material.mainTexture = _multiManager.GetColorTexture();

        RefreshData(_multiManager.GetDepthData(),
            _multiManager.ColorWidth,
            _multiManager.ColorHeight);


    }
    
    private void RefreshData(ushort[] depthData, int colorWidth, int colorHeight)
    {
        var frameDesc = _sensor.DepthFrameSource.FrameDescription;

        ColorSpacePoint[] colorSpace = new ColorSpacePoint[depthData.Length];
        _mapper.MapDepthFrameToColorSpace(depthData, colorSpace);
        
		// Populates positions of the vertices
        for (int y = 0; y < frameDesc.Height; y += DownSampleSize)
        {
            for (int x = 0; x < frameDesc.Width; x += DownSampleSize)
            {
                int indexX = x / DownSampleSize;
                int indexY = y / DownSampleSize;
                int smallIndex = (indexY * (frameDesc.Width / DownSampleSize)) + indexX;

                // Averages the values
                double sum = 0.0;

                for (int y1 = y; y1 < y + DownSampleSize; y1++)
                {
                    for (int x1 = x; x1 < x + DownSampleSize; x1++)
                    {
                        int fullIndex = (y1 * frameDesc.Width) + x1;

                        if (depthData[fullIndex] == 0)
                            sum += 4500;
                        else
                            sum += depthData[fullIndex];
                    }
                }

                double avg = sum / Mathf.Pow(DownSampleSize, 2);

                avg = avg * DepthScale;
                
				_vertices[smallIndex].z = (float) (-4500.0 * DepthScale) + (float)avg;
                
                // Update UV mapping with CDRP
                var colorSpacePoint = colorSpace[(y * frameDesc.Width) + x];
                _uv[smallIndex] = new Vector2(colorSpacePoint.X / colorWidth, colorSpacePoint.Y / colorHeight);
            }
        }

        // Loops through Triangles, removing any stretchy ones
        List<int> tempTriangle = new List<int>();

        for (int i = 0; i < _defaultTriangles.Length; i+=3) {
			// Check the distance between the vertices in the triangles
            int triangleV1 = _defaultTriangles[i];
            int triangleV2 = _defaultTriangles[i + 1];
            int triangleV3 = _defaultTriangles[i + 2];

            //float distA = Vector3.Distance (_vertices [triangleV1], _vertices [triangleV2]);
			//float distB = Vector3.Distance (_vertices [triangleV1], _vertices [triangleV3]);

            float distA = Mathf.Abs(_vertices[triangleV1].z - _vertices[triangleV2].z);
            float distB = Mathf.Abs(_vertices[triangleV1].z - _vertices[triangleV3].z);

            // If under the threshold, push the vertices. Otherwise, don't
            if (distA < TriangleThreshold && distB < TriangleThreshold)
            {
                tempTriangle.Push(triangleV1);
                tempTriangle.Push(triangleV2);
                tempTriangle.Push(triangleV3);
            }
		}

        _mesh.vertices = _vertices;
        _mesh.uv = _uv;
        _mesh.triangles = tempTriangle.ToArray();
        _mesh.RecalculateNormals();
    }
  
    void OnApplicationQuit()
    {
        if (_mapper != null)
        {
            _mapper = null;
        }
        
        if (_sensor != null)
        {
            if (_sensor.IsOpen)
            {
                _sensor.Close();
            }

            _sensor = null;
        }
    }
}
