﻿using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class KinectViewer : MonoBehaviour
{ 
    public GameObject KinectSource;
    
    private KinectSensor _Sensor;
    private CoordinateMapper _Mapper;
    private Mesh _Mesh;
    private Vector3[] _Vertices;
    private Vector2[] _UV;
    private int[] _Triangles;
    
    // Works for powers of 2 past 1 ie 2^1, 2^2 etc.
    private const int _DownsampleSize = 4;

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
        _Triangles = new int[6 * ((width - 1) * (height - 1))];

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

                    _Triangles[triangleIndex++] = topLeft;
                    _Triangles[triangleIndex++] = topRight;
                    _Triangles[triangleIndex++] = bottomLeft;
                    _Triangles[triangleIndex++] = bottomLeft;
                    _Triangles[triangleIndex++] = topRight;
                    _Triangles[triangleIndex++] = bottomRight;
                }
            }
        }

        _Mesh.vertices = _Vertices;
        _Mesh.uv = _UV;
        _Mesh.triangles = _Triangles;
        _Mesh.RecalculateNormals();
    }

    void Update()
    {
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
        
        RefreshData(_MultiManager.GetDepthData(),
                    _MultiManager.ColorWidth,
                    _MultiManager.ColorHeight);

    }
    
    private void RefreshData(ushort[] depthData, int colorWidth, int colorHeight)
    {
        var frameDesc = _Sensor.DepthFrameSource.FrameDescription;
        
        ColorSpacePoint[] colorSpace = new ColorSpacePoint[depthData.Length];
        _Mapper.MapDepthFrameToColorSpace(depthData, colorSpace);
        
        for (int y = 0; y < frameDesc.Height; y += _DownsampleSize)
        {
            for (int x = 0; x < frameDesc.Width; x += _DownsampleSize)
            {
                int indexX = x / _DownsampleSize;
                int indexY = y / _DownsampleSize;
                int smallIndex = (indexY * (frameDesc.Width / _DownsampleSize)) + indexX;
                
                double avg = GetAvg(depthData, x, y, frameDesc.Width, frameDesc.Height);
                
                avg = avg * _DepthScale;
                
                _Vertices[smallIndex].z = (float)avg;
                
                // Update UV mapping with CDRP
                var colorSpacePoint = colorSpace[(y * frameDesc.Width) + x];
                _UV[smallIndex] = new Vector2(colorSpacePoint.X / colorWidth, colorSpacePoint.Y / colorHeight);
            }
        }
        
        _Mesh.vertices = _Vertices;
        _Mesh.uv = _UV;
        _Mesh.triangles = _Triangles;
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
