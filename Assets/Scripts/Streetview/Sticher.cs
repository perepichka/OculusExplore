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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SharpConfig;
using UnityEditor;
using UnityEngine;

namespace Streetview
{
    public class Sticher : MonoBehaviour
    {

        // 
        // Constants
        //

        // Min. Degree to which the coordinates will be incremented during movement
        private const double CoordinateIncrement = 0.0001f;

        // Max. Range of coordinates to look for during movement before giving up
        // Ideally this would be infinite, simply just going to the next available image,
        // But I somehow doubt it is a good idea to spam Google's servers with millions of requests
        // Considering we pass our API key to do this... 
        private const double CoordinateMaxRange = 0.01f;

        // Stich struct to contain the sitched together texture
        public struct Stich
        {       
            // Texture containing the sphere
            public Texture2D SphereTexture;

            // Texture width, height
            public int Width, Height;

        }

        // Stiches containing textures
        public Dictionary<string, Stich> Stiches; 

        // Downloader object
        private Downloader _downloader;

        // Load a texture
        public bool texEmpty;
       
        // Use this for initialization
        void Start ()
        {
            // Sets up stiches
            Stiches = new Dictionary<string, Stich>();

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

            

            if (Stiches.Count != 0 && texEmpty)
            {
                texEmpty = false;
                GameObject.Find("Sphere").GetComponent<MeshRenderer>().material.mainTexture = Stiches[0].SphereTexture;

                //Resources.
            }

        }

        // 
        // Methods for accessing stiche
        //

        // Loads a stich at a coordinate
        public void LoadStich(double lat, double lng)
        {
            string combinedCoord = GetCombinedCoordinate(lat, lng);

            // Secondly checks if it is in the dictionary
            bool inDict = HasStich(combinedCoord);

            // If we don't have the stich, we are sad :( since our prediction system failed
            // However, there is still a job to be done, so we have to download it
            if (!inDict)
            {
                _downloader.Download();
            }

            if (_downloader.ImagesReady)
            {
                StartCoroutine(AddStich(
                    _downloader.Images,
                    _downloader.Size)
                );

            }

        }

        // Check if we have the stich for the coordinate or if will need to be downloaded
        public bool HasStich(string combinedCoordinate)
        { 
            // Checks the dict for the key
            if (Stiches.ContainsKey(combinedCoordinate))
            {
                return true;
            }
            return false;
        }

        //
        // Helper method for working with coordinates
        //

        public string GetCombinedCoordinate(double lat, double lng)
        {
            return (lat.ToString() + "," + lng.ToString());
        }       

        // Gets the closest Panorama to the coordinates given
        private void GetClosestCoordinatesTo(double lat, double lng)
        {
            // We first start off 
            for ()

            //int sphereHeight = _height * 
        }

        //
        // Methods for creating stiches
        //

        // Adds a stich at some coordinates by downloading it and stiching it
        private IEnumerator AddStich(double lat, double lng, int imageSize)
        {
            // Converts coordinates to panorama
            string pano = _downloader.CoordinatesToPanorama(coordinates);

            // Now

            // Creates a stich struct
            Stich s = new Stich();

            // Loads texture from 
            Texture2D t = new Texture2D(imageSize * images.Count, imageSize * images[0].Count);
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
 
    }
}
