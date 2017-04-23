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

using Boo.Lang;
using System;
using System.IO;
using SharpConfig;
using UnityEditor;
using UnityEngine;

namespace Streetview
{
    public class Sticher : MonoBehaviour
    {

        // Stich struct to contain the sitched together texture
        public struct Stich
        {       
            
            // Texture containing the sphere
            public RenderTexture SphereTexture;

            // Texture width, height
            public int Width;

            public int Height;
        }

        // Stiches containing textures
        public List<Stich> Stiches; 

        // Downloader object
        private Downloader _downloader;
       
        // Use this for initialization
        void Start ()
        {
            // Gets a reference to the downloader
            _downloader = transform.GetComponent<Downloader>();

            // Checks that we have the downloader
            if (!_downloader)
            {
                throw new Exception("No Downloader script found");
            }

        }

        // Update is called once per frame
        void Update()
        {

            if (_downloader.ImagesReady)
            {
                AddStich(_downloader.Images, _downloader.Size, _downloader.XMax, _downloader.YMax);
                _downloader.ImagesReady = false;
            }

        }

        // Adds a stich
        void AddStich(List<List<Texture2D>> images, int imageSize, int xCount, int yCount)
        {
            // Loads texture from 
            Texture2D t = new Texture2D(imageSize * xCount, imageSize * yCount);
            t.wrapMode = TextureWrapMode.Clamp;

            for (int y = 0; y < images.Count; y++)
            {
                for (int x = 0; x < images[y].Count; x++)
                {
                    
                    for (int w = 0; w < imageSize; w++)
                    {
                        for (int h = 0; h < imageSize; h++)
                        {
                            t.SetPixel(
                                (x * imageSize) + w,
                                (y * imageSize) + h,
                                images[(images.Count - 1) - y][x].GetPixel(w, h)
                            );
                        }
                    }

                }

            }

            t.Apply();

            GameObject.Find("Sphere").GetComponent<MeshRenderer>().material.mainTexture = t;

        }
        
        // Sitches together textures
        void StichTogether()
        {
            //int sphereHeight = _height * 
            //int sphereWidth = 
            //_sphereTexture.height = 
        }
	
      
    }
}
