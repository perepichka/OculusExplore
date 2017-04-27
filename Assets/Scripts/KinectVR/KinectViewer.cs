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

using Windows.Kinect;
using Boo.Lang;
using UnityEngine;

namespace KinectVR
{
  

    public class KinectViewer : MonoBehaviour
    { 
        public GameObject KinectSource;
    
        private KinectSensor _sensor;
        private CoordinateMapper _mapper;

        // Since Unity has hard limit of 65k vertices per mesh, we will need multiple meshes
        // in order to fit the Mesh within unity without downsampling
        private KinectMesh _kinectMesh;

        private Vector3[] _vertices;
        private Vector2[] _uv;

        // Array of Default triangle indices.
        private int[] _defaultTriangles;

        // Source of Kinect info
        private KinectSource _multiManager;

        // Max distance between two vertices in a triangle after which it stops being rendered in the mesh
        private int TriangleThreshold = 10;
    
        // Works for powers of 2 past 1 ie 2^1, 2^2 etc.
        private const int DownSampleSize = 2;

        private const double DepthScale = 0.1f;


        void Start()
        {

            // Sets up sensor stuff
            _sensor = KinectSensor.GetDefault();
            if (_sensor != null)
            {
                _mapper = _sensor.CoordinateMapper;
                var frameDesc = _sensor.DepthFrameSource.FrameDescription;

                _kinectMesh = new KinectMesh(frameDesc.Width, frameDesc.Height, GameObject.Find("Meshes") );

                if (!_sensor.IsOpen)
                {
                    _sensor.Open();
                }
            }

            // Sets up Oculus Stuff
            GameObject.Find("TrackingSpace").transform.localScale = new Vector3(100, 100, 100);
        }

        void Update()
        {

            // Back to main menu
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }

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
        
            var depthData = _multiManager.GetDepthData();

            ColorSpacePoint[] colorSpacePoints = new ColorSpacePoint[depthData.Length];
            _mapper.MapDepthFrameToColorSpace(depthData, colorSpacePoints);

            _kinectMesh.LoadDepthData(depthData, _multiManager.GetColorTexture(), colorSpacePoints , _multiManager.ColorWidth, _multiManager.ColorHeight);

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
}
