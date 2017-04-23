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
using System.Collections;
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
            public Texture2D SphereTexture;

            // Texture width, height
            public int Width, Height;

            // X, Y Positions
            public double Longitude, Latitude;
        }

        // Stiches containing textures
        public List<Stich> Stiches; 

        // Downloader object
        private Downloader _downloader;

        // Load a texture
        public bool texEmpty;
       
        // Use this for initialization
        void Start ()
        {
            // Sets up stiches
            Stiches = new List<Stich>();

            // Gets a reference to the downloader
            _downloader = transform.GetComponent<Downloader>();

            // Checks that we have the downloader
            if (!_downloader)
            {
                throw new Exception("No Downloader script found");
            }

            // Sets tex empty
            texEmpty = true;
        }

        // Update is called once per frame
        void Update()
        {

            if (_downloader.ImagesReady)
            {
                StartCoroutine(AddStich(
                    _downloader.Images,
                    _downloader.Size,
                    _downloader.XMax,
                    _downloader.YMax)
                );
                
            }

            if (Stiches.Count != 0 && texEmpty)
            {
                texEmpty = false;
                GameObject.Find("Sphere").GetComponent<MeshRenderer>().material.mainTexture = Stiches[0].SphereTexture;

                Resources.
            }

        }

        // Adds a stich
        IEnumerator AddStich(List<List<Texture2D>> images, int imageSize, int xCount, int yCount)
        {
            // Creates a stich struct
            Stich s = new Stich();

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

            // Sets the Stich texture
            s.SphereTexture = t;

            // Adds the stich to the stich array
            Stiches.Add(s);

            // Set the Downloader back to being ready
            _downloader.ImagesReady = false;

            yield return null;
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
