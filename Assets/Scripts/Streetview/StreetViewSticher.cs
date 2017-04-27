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
    public class StreetViewSticher : MonoBehaviour
    {

        // 
        // Constants
        //

        // Image size for Google Streetview Images
        private const int ImageSize = 512;

        // Min. Degree to which the coordinates will be incremented during movement
        private const float CoordinateIncrement = 0.0002f;

        // Max. tries of coordinates to look for during movement before giving up
        // Ideally this would be infinite, simply just going to the next available image,
        // But I somehow doubt it is a good idea to spam Google's servers with millions of requests
        // Considering we pass our API key to do this... 
        private const int CoordinateMaxTries = 100;

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
        private StreetViewDownloader _downloader;
        private StreetViewController _controller;

        // Use this for initialization
        void Start ()
        {
            // Sets up stiches
            Stiches = new Dictionary<string, Stich>();

            CurrentStichCoordinates = null;

            // Gets a reference to the other scripts
            _downloader = transform.GetComponent<StreetViewDownloader>();
            _controller = transform.GetComponent<StreetViewController>();

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

            // We start looking at the next point already
            AttemptAddingPoint(
                point + direction * CoordinateIncrement,
                direction
            );
        }

        // Attempts to add the point, moving in the direction each time it isn't succesful,
        // until it either succeeds or runs out of attempts.
        public bool AttemptAddingPoint(Vector2 point, Vector2 direction)
        {
            Vector2 offset = direction * CoordinateIncrement;
            int tries = 0;

            while(tries < CoordinateMaxTries)
            {
                bool result = AddPoint(point);
                if (result)
                {
                    return true;
                }

                // Adds the vector times the offset and check its validity
                point += offset;

                tries++;
            }
            return false;
        }

        // Adds the coordinate point. True if successful, false otherwise
        public bool AddPoint(Vector2 point)
        {
            // Converts point to string coords
            string combinedCoords = GetCombinedCoordinate(point[0], point[1]);

            // Cheaper to check if we have it than to send web request
            
            // Secondly checks if it is in the dictionary
            if (HasStich(combinedCoords))
            {
                SetCurrentStich(combinedCoords);

                // If set succeeds then we are done
                return true;
            }

            // Gets the panorama for these coordinates
            string pano = _downloader.CoordinatesToPanorama(combinedCoords, _controller.StreeViewApiKey);

            if (string.IsNullOrEmpty(pano))
            {
                // Go to the next possibility
                return false;
            }

            // Makes sure we don't already have this pano loaded under slightly differnt coords
            string possibleCoords = GetPanoramaKey(pano);

            if (!string.IsNullOrEmpty(possibleCoords))
            {
                if (possibleCoords == CurrentStichCoordinates)
                {
                    // If it is the current one, we need to keep looking
                    return false;
                }

                // Otherwise we load the one it is
                SetCurrentStich(possibleCoords);
                return true;
            }

            // Downloads the stiches and parses them
            List<List<Texture2D>> texs = _downloader.Download(pano);
            Texture2D tex = CombineTextureList(texs, ImageSize);

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
            return true;
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

        // Checks if the panorama is currently loaded
        public bool IsPanoramaLoaded(string panoid)
        {
            Stich s;

            if (Stiches.TryGetValue(CurrentStichCoordinates, out s))
            {
                if (s.Panorama == panoid)
                {
                    return true;
                }
            }
            return false;
        }

        // Gets the coordinate key associated to the loaded panorama. Null if not in dict
        public string GetPanoramaKey(string panoid)
        { 
            // Checks the dict for the key
            foreach (KeyValuePair<string, Stich> entry in Stiches)
            {
                if (entry.Value.Panorama == panoid)
                {
                    return entry.Key;
                }
            }
            return null;
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
        public Texture2D CombineTextureList(List<List<Texture2D>> images, int imageSize)
        {
            // Loads texture from 
            Texture2D t = new Texture2D(imageSize * images[0].Count, imageSize * images.Count);
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
