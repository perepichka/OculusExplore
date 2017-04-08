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
    
    private KinectSensor _Sensor;
    private CoordinateMapper _Mapper;

    private Mesh _Mesh;

	// Since Unity has hard limit of 65k vertices per mesh, we will need multiple meshes
	// in order to fit the Mesh within unity without downsampling
	private Mesh[] Meshes;

    private Vector3[] _Vertices;
    private Vector2[] _UV;

	// Array of Default triangle indices.
    private int[] DefaultTriangles;
 

    private double timeCount = 0.0;
    private double timeLimit = 1.0;

    // Max distance between two vertices in a triangle after which it stops being rendered in the mesh
    private int TriangleThreshold = 10;
    
    // Works for powers of 2 past 1 ie 2^1, 2^2 etc.
    private const int _DownsampleSize = 2;

    private const double _DepthScale = 0.1f;
    
    private KinectSource _MultiManager;

    void Start()
    {
        _Sensor = KinectSensor.GetDefault();
        if (_Sensor != null)
        {
            _Mapper = _Sensor.CoordinateMapper;
            var frameDesc = _Sensor.DepthFrameSource.FrameDescription;

            // Downsample to lower resolution
            CreateMesh(frameDesc.Width / _DownsampleSize, frameDesc.Height / _DownsampleSize);

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }

    void CreateMesh(int width, int height)
    {
        _Mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _Mesh;

        _Vertices = new Vector3[width * height];
        _UV = new Vector2[width * height];
        DefaultTriangles = new int[6 * ((width - 1) * (height - 1))];
     

        int triangleIndex = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width) + x;

                _Vertices[index] = new Vector3(x, -y, 0);
                _UV[index] = new Vector2(((float)x / (float)width), ((float)y / (float)height));

                // Skip the last row/col
                if (x != (width - 1) && y != (height - 1))
                {
                    int topLeft = index;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + width;
                    int bottomRight = bottomLeft + 1;

                    DefaultTriangles[triangleIndex++] = topLeft;
                    DefaultTriangles[triangleIndex++] = topRight;
                    DefaultTriangles[triangleIndex++] = bottomLeft;
                    DefaultTriangles[triangleIndex++] = bottomLeft;
                    DefaultTriangles[triangleIndex++] = topRight;
                    DefaultTriangles[triangleIndex++] = bottomRight;
                }
            }
        }

        _Mesh.vertices = _Vertices;
        _Mesh.uv = _UV;
        _Mesh.triangles = DefaultTriangles;
        _Mesh.RecalculateNormals();
    }

    void Update()
    {
        timeCount+=Time.deltaTime;

        if (_Sensor == null)
        {
            return;
        }
			
        if (KinectSource == null)
        {
            return;
        }
        
        _MultiManager = KinectSource.GetComponent<KinectSource>();

        if (_MultiManager == null)
        {
            return;
        }
        
        gameObject.GetComponent<Renderer>().material.mainTexture = _MultiManager.GetColorTexture();

        //if (timeCount > timeLimit)
        //{
            //timeCount = 0;
            RefreshData(_MultiManager.GetDepthData(),
                _MultiManager.ColorWidth,
                _MultiManager.ColorHeight);
//}


    }
    
    private void RefreshData(ushort[] depthData, int colorWidth, int colorHeight)
    {
        var frameDesc = _Sensor.DepthFrameSource.FrameDescription;

        ColorSpacePoint[] colorSpace = new ColorSpacePoint[depthData.Length];
        _Mapper.MapDepthFrameToColorSpace(depthData, colorSpace);
        
		// Populates positions of the vertices
        for (int y = 0; y < frameDesc.Height; y += _DownsampleSize)
        {
            for (int x = 0; x < frameDesc.Width; x += _DownsampleSize)
            {
                int indexX = x / _DownsampleSize;
                int indexY = y / _DownsampleSize;
                int smallIndex = (indexY * (frameDesc.Width / _DownsampleSize)) + indexX;
                
                double avg = GetAvg(depthData, x, y, frameDesc.Width, frameDesc.Height);

                avg = avg * _DepthScale;
                
				_Vertices[smallIndex].z = (float) (-4500.0 * _DepthScale) + (float)avg;
                
                // Update UV mapping with CDRP
                var colorSpacePoint = colorSpace[(y * frameDesc.Width) + x];
                _UV[smallIndex] = new Vector2(colorSpacePoint.X / colorWidth, colorSpacePoint.Y / colorHeight);
            }
        }

        // Loops through Triangles, removing any stretchy ones
        List<int> tempTriangle = new List<int>();

        for (int i = 0; i < DefaultTriangles.Length; i+=3) {
			// Check the distance between the vertices in the triangles
            int triangleV1 = DefaultTriangles[i];
            int triangleV2 = DefaultTriangles[i + 1];
            int triangleV3 = DefaultTriangles[i + 2];

            //float distA = Vector3.Distance (_Vertices [triangleV1], _Vertices [triangleV2]);
			//float distB = Vector3.Distance (_Vertices [triangleV1], _Vertices [triangleV3]);

            float distA = Mathf.Abs(_Vertices[triangleV1].z - _Vertices[triangleV2].z);
            float distB = Mathf.Abs(_Vertices[triangleV1].z - _Vertices[triangleV3].z);

            // If under the threshold, push the vertices. Otherwise, don't
            if (distA < TriangleThreshold && distB < TriangleThreshold)
            {
                tempTriangle.Push(triangleV1);
                tempTriangle.Push(triangleV2);
                tempTriangle.Push(triangleV3);
            }
		}


        
        _Mesh.vertices = _Vertices;
        _Mesh.uv = _UV;
        _Mesh.triangles = tempTriangle.ToArray();
        _Mesh.RecalculateNormals();
    }
    
    private double GetAvg(ushort[] depthData, int x, int y, int width, int height)
    {
        double sum = 0.0;
        
        for (int y1 = y; y1 < y + _DownsampleSize; y1++)
        {
            for (int x1 = x; x1 < x + _DownsampleSize; x1++)
            {
                int fullIndex = (y1 * width) + x1;
                
				if (depthData [fullIndex] == 0)
					sum += 4500;
                else
                    sum += depthData[fullIndex];
            }
        }

		return sum / Mathf.Pow(_DownsampleSize, 2);
    }

    void OnApplicationQuit()
    {
        if (_Mapper != null)
        {
            _Mapper = null;
        }
        
        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }
}
