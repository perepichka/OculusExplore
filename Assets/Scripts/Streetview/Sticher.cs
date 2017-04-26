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
using System.Collections.Generic;
using System.IO;
using NUnit.Framework.Constraints;
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

        // Image size for Google Streetview Images
        private const int ImageSize = 500;

        // Min. Degree to which the coordinates will be incremented during movement
        private const float CoordinateIncrement = 0.0001f;

        // Max. Range of coordinates to look for during movement before giving up
        // Ideally this would be infinite, simply just going to the next available image,
        // But I somehow doubt it is a good idea to spam Google's servers with millions of requests
        // Considering we pass our API key to do this... 
        private const float CoordinateMaxRange = 0.01f;

        // Stich struct to contain the sitched together texture
        public struct Stich
        {       
            // Texture containing the sphere
            public Texture2D SphereTexture;

            // Texture width, height
            public int Width, Height;

            // Longitude and latitude
            public double Lat, Lng;

            // Panorama id for this stich
            public string Panorama;

        }

        // Stiches containing textures
        public Dictionary<string, Stich> Stiches; 

        // The stich that is loaded onto the sphere
        public string CurrentStichCoordinates;

        // Downloader object
        private Downloader _downloader;
        private Controller _controller;
       
        // Use this for initialization
        void Start ()
        {
            // Sets up stiches
            Stiches = new Dictionary<string, Stich>();

            CurrentStichCoordinates = null;

            // Gets a reference to the other scripts
            _downloader = transform.GetComponent<Downloader>();
            _controller = transform.GetComponent<Controller>();

            // Checks that we have the downloader
            if (!_downloader)
            {
                throw new Exception("No Downloader script found");
            }

        }

        // Update is called once per frame
        void Update()
        {
 

        }

        // 
        // Methods for accessing stichs
        //

        // Moves in the direction to the next stich
        public void Move(Vector2 direction)
        {

            Stich currentStich;

            Stiches.TryGetValue(CurrentStichCoordinates, out currentStich);

            // Gets the Lat and long
            Vector2 point = new Vector2( (float) currentStich.Lat, (float) currentStich.Lng);

            for(float d = 1.0f; d<CoordinateMaxRange; d+=CoordinateIncrement)
            {
                // Adds the vector times the offset and check its validity
                point += (direction * d);

                // Converts point to string coords
                string combinedCoords = GetCombinedCoordinate(point[0], point[1]);

                // Cheaper to check if we have it than to send web request
                
                // Secondly checks if it is in the dictionary
                if (HasStich(combinedCoords))
                {
                    SetCurrentStich(combinedCoords);
                    break;
                }

                // Gets the panorama for these coordinates
                string pano = _downloader.CoordinatesToPanorama(combinedCoords);

                if (pano == null)
                {
                    // Go to the next possibility
                    continue;
                }

                // Downloads the stiches and parses them
                List<List<Texture2D>> texs = _downloader.Download(pano);
                Texture2D tex = CombinedTextureList(texs, ImageSize);

                // Loads the stich
                AddStich(
                    tex,
                    combinedCoords,
                    pano,
                    point[0],
                    point[1]
                );

                // Sets the current switch
                SetCurrentStich(combinedCoords);
                break;
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

        public string GetCombinedCoordinate(float lat, float lng)
        {
            return (lat.ToString() + "," + lng.ToString());
        }

        //
        // Methods for creating stiches
        //

        // Adds a stich at some coordinates by downloading it and stiching it
        private void AddStich(Texture2D tex, string coordinates, string panoid, float lat, float lng)
        {
            // Creates a stich struct
            Stich s = new Stich();

            // Keep these for later
            s.Panorama = panoid;

            // Sets the Stich texture
            s.SphereTexture = tex;

            // Set coords
            s.Lat = lat;
            s.Lng = lng;

            // Adds the stich to the stich array
            Stiches.Add(coordinates, s);

        }

        // Combines texture into another texture
        public Texture2D CombinedTextureList(List<List<Texture2D>> images, int imageSize)
        {
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

            return t;
        }

        public bool SetCurrentStich(string coordinates)
        {
            Stich s;

            if (Stiches.TryGetValue(coordinates, out s))
            {
                _controller.SetSphereTexture(s.SphereTexture);
                CurrentStichCoordinates = coordinates;
            }
            
            // The try to get the value failed
            return false;
        }
 
    }
}
