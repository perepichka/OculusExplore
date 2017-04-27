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
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Streetview
{

    // Some stuff we need for JSON parsing
    [System.Serializable]
    public class Location
    {
        public string lat;
        public string lng;
    }

    [System.Serializable]
    public class GoogleResponser
    {
        public string copyright;
        public string date;
        public Location location;
        public string pano_id;
        public string status;
        public string error_message;

        public static GoogleResponser CreateFromJson(string json)
        {
            return JsonUtility.FromJson<GoogleResponser>(json);
        }
    }


    public class StreetViewDownloader : MonoBehaviour
    {

        //
        // Constants
        //

        // Tokens for finding the id from a streetview url
        private const string UrlTokenStart = "!1s";
        private const string UrlTokenEnd = "!2e";

        // Base URL Google geo1 server from which to download the images
        private const string UrlDownloadBase =
            @"http://geo1.ggpht.com/cbk?cb_client=maps_sv.tactile&authuser=0&hl=en&panoid=";

        // Other tags that we will need
        private const string UrlDownloadOutput = @"&output=tile";
            
        private const string UrlDownloadValues = "&x=X&y=Y&zoom=Z";

        // Base API path to verify image validity and convert coordinates to panoids
        private const string UrlVerification =
            @"https://maps.googleapis.com/maps/api/streetview/metadata?location=";

        private const string UrlApiKey = @"&key=";

        // Base file path where to store the images (deprecated)
        private const string FilePathBase = @"Assets\Resources\Streetview\Tiles\";

        // Default Zoom level
        private const int DefaultZoomLevel = 4;


        //
        // Members
        //

        public int Size = 512;

        public string BaseUrl;
        public string FileName; 

        // Zoom level controls how zoomed in each individual tile is
        // and subsequently how many tiles are needed in total
        public int ZoomLevel;

        // API stuff
        public bool StreetViewApiEnabled;

        // Use this for initialization
        void Start ()
        {
            ZoomLevel = DefaultZoomLevel;
        }
	
        // Update is called once per frame
        void Update () {
		
        }

        // Downloads the image
        public List<List<Texture2D>> Download(string pano)
        {
            // Checks that we have the pano
            if (string.IsNullOrEmpty(pano))
            {
                return null;
            }

            // If we indeed have the pano, build URL from it and download it
            List<List<Texture2D>> images = DownloadImages(pano, ZoomLevel);

            // Checks that we have the images
            return images;

        }

        //
        // Privates methods methods
        //

        private GoogleResponser GetJsonObject(string jsonStr)
        {
            // Creates object to store JSON
            GoogleResponser g = GoogleResponser.CreateFromJson(jsonStr);

            // If the status isn't OK, the coordinates have no Streetview
            if (g.status == "REQUEST_DENIED")
            {
                Debug.Log("Failed Google API Request: ");
                Debug.Log(g.error_message);
                throw new Exception(g.error_message);
            } else if (g.status == "INVALID_REQUEST")
            {
                Debug.Log("Failed Google API Request: ");
                Debug.Log(g.error_message);
                throw new Exception(g.error_message);
            } else if (g.status == "ZERO_RESULTS")
            {
                // This will happen pretty often so we don't
                // want to throw an exception, just return nulll
                return null;
            } else if (g.status == "OK")
            {
                // This means we got the coordinates, can fully work with them
                return g;
            }
            else
            {
                return null;
            }
        }

        // Gets a panorama ID from two coordinates
        public string CoordinatesToPanorama(string coordinates, string apiKey)
        {
            // Checks if we have API access
            if (!StreetViewApiEnabled)
            {
                return null;
            }

            // Appends coordinates to panorma id
            string newUrl = UrlVerification + coordinates + UrlApiKey + apiKey;

            // Attempts to get the pano id from json found on the site
            string jsonStr = DownloadStreetViewValidationData(newUrl);

            // Gets the JSON responser object
            GoogleResponser g = GetJsonObject(jsonStr);
            
            // If we got null, return null string
            if (g == null)
            {
                return null;
            }

            // Gets our json stuff
            string panoid = g.pano_id;

            // Final verification
            if (!string.IsNullOrEmpty(panoid))
            {
                return panoid;
            }

            return null;

        }

        // Parses URL and extracts the key that we will need to download individual images
        private static string GetKeyFromURL(string url)
        { 
            // Gets starting index
            var index1 = url.IndexOf(UrlTokenStart) + UrlTokenStart.Length;
            var index2 = url.IndexOf(UrlTokenEnd);

            return url.Substring(index1, (index2-index1));
        }

        // Sets up image for download
        private List<List<Texture2D>> DownloadImages(string url, int zoomLevel)
        {
            int XMax = 0;
            int YMax = 0;

            // Sets up our values
            switch (zoomLevel)
            {
                case 3:
                    XMax = 7;
                    YMax = 3;
                    break;
                case 4:
                    XMax = 13;
                    YMax = 6;
                    break;
                default:
                    return null;
            }

            List<List<Texture2D>> images = new List<List<Texture2D>>();

            // Loops through creating the urls we will need
            for (int y = 0; y < YMax; y++)
            {
                List<Texture2D> imageRow = new List<Texture2D>();

                for (int x = 0; x < XMax; x++)
                {
                
                    string tempDownloadValues = UrlDownloadValues.Replace("X", x.ToString()).Replace("Y", y.ToString()).Replace("Z", zoomLevel.ToString());
                    string tempUrl = UrlDownloadBase + url + UrlDownloadOutput + tempDownloadValues;

                    Debug.Log(tempUrl);
                    imageRow.Add(DownloadRemoteImageFile(tempUrl));
                    
                }

                images.Add(imageRow);

            }
            
            return images;
        }

        // Downloads json into string from web
        private string DownloadStreetViewValidationData(string url)
        {
            string jsonData = null;

            var webReader = new WWW(url);

            while (!webReader.isDone)
            {
                Thread.Sleep(500);
            }

            if (webReader.text != null && webReader.error == null)
            {
                jsonData = webReader.text;
            }

            return jsonData;
        }

        // Downloads image from remote server to the memory stream
        private Texture2D DownloadRemoteImageFile(string uri)
        {
            MemoryStream imageStream = new MemoryStream();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if ((response.StatusCode == HttpStatusCode.OK ||
                 response.StatusCode == HttpStatusCode.Moved ||
                 response.StatusCode == HttpStatusCode.Redirect) &&
                response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {

                using (Stream inputStream = response.GetResponseStream())
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = inputStream.Read(buffer, 0, buffer.Length);

                        imageStream.Write(buffer, 0, bytesRead);

                    } while (bytesRead != 0);
                }
            }
            else
            {
                Debug.Log("Failed to load online image");
            }

            Texture2D tex = new Texture2D(Size, Size);
            tex.LoadImage(imageStream.GetBuffer());
            return tex;
        }
    }
}
