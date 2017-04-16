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
using UnityEngine;

namespace KinectVR
{
    public class KinectSource : MonoBehaviour {
        public int ColorWidth { get; private set; }
        public int ColorHeight { get; private set; }
    
        private KinectSensor _Sensor;
        private MultiSourceFrameReader _Reader;
        private Texture2D _ColorTexture;

        private ushort[] _DepthData;
        private byte[] _ColorData;

        public Texture2D GetColorTexture()
        {
            return _ColorTexture;
        }
    
        public ushort[] GetDepthData()
        {
            return _DepthData;
        }

        void Start () 
        {
            _Sensor = KinectSensor.GetDefault();
        
            if (_Sensor != null) 
            {
                _Reader = _Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);
            
                var colorFrameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
                ColorWidth = colorFrameDesc.Width;
                ColorHeight = colorFrameDesc.Height;
            
                _ColorTexture = new Texture2D(colorFrameDesc.Width, colorFrameDesc.Height, TextureFormat.RGBA32, false);
                _ColorData = new byte[colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];
            
                var depthFrameDesc = _Sensor.DepthFrameSource.FrameDescription;
                _DepthData = new ushort[depthFrameDesc.LengthInPixels];
            
                if (!_Sensor.IsOpen)
                {
                    _Sensor.Open();
                }
            }
        }
    
        void Update () 
        {
            if (_Reader != null) 
            {
                var frame = _Reader.AcquireLatestFrame();
                if (frame != null)
                {
                    var colorFrame = frame.ColorFrameReference.AcquireFrame();
                    if (colorFrame != null)
                    {
                        var depthFrame = frame.DepthFrameReference.AcquireFrame();
                        if (depthFrame != null)
                        {
                            colorFrame.CopyConvertedFrameDataToArray(_ColorData, ColorImageFormat.Rgba);
                            _ColorTexture.LoadRawTextureData(_ColorData);
                            _ColorTexture.Apply();
                        
                            depthFrame.CopyFrameDataToArray(_DepthData);
                        
                            depthFrame.Dispose();
                            depthFrame = null;
                        }
                
                        colorFrame.Dispose();
                        colorFrame = null;
                    }
                
                    frame = null;
                }
            }
        }
    
        void OnApplicationQuit()
        {
            if (_Reader != null)
            {
                _Reader.Dispose();
                _Reader = null;
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
}
