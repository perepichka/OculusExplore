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
using UnityEngine;

namespace Streetview
{
    public class Downloader : MonoBehaviour
    {

        //
        // Constants
        //

        // Tokens for finding the id from a streetview url
        private const string UrlTokenStart = "!1s";
        private const string UrlTokenEnd = "!2e";

        // Base URL from which to download the images
        private const string UrlDownloadBase =
            @"https://geo1.ggpht.com/cbk?cb_client=maps_sv.tactile&authuser=0&hl=en&panoid=";

        // Other tags that we will need
        private const string UrlDownloadOutput = @"&output=tile";
            
        private const string UrlDownloadValues = "&x=X&y=Y&zoom=Z";

        // Base file path where to store the images
        private const string FilePathBase = @"Assets\Resources\Streetview\Tiles\";

        //
        // Members
        //
        public string BaseUrl;
        public string FileName; 

        // Zoom level controls how zoomed in each individual tile is
        // and subsequently how many tiles are needed in total
        public int ZoomLevel;

        // Use this for initialization
        void Start ()
        {
            //string apiKey = GameObject.Find("StreetViewManager").GetComponent<ConfigParser>().apiKey;
            //DownloadRemoteImageFile(BaseUrl + apiKey, FileName);

            string tempUrl = ParseUrl(BaseUrl);

            DownloadImages( tempUrl , ZoomLevel);

        }
	
        // Update is called once per frame
        void Update () {
		
        }

        //
        // Privates static methods
        //

        // Parses URL and extracts the key that we will need to download individual images
        private static string ParseUrl(string url)
        {
            // Gets starting index
            var index1 = url.IndexOf(UrlTokenStart) + UrlTokenStart.Length;
            var index2 = url.IndexOf(UrlTokenEnd);

            return url.Substring(index1, (index2-index1));
        }

        // Sets up image for download
        private static bool DownloadImages(string url, int zoomLevel)
        {
            int xMax=0;
            int yMax=0;

            // Sets up our values
            switch (zoomLevel)
            {
                case 3:
                    xMax = 6;
                    yMax = 3;
                    break;
                case 4:
                    xMax = 12;
                    yMax = 6;
                    break;
                default:
                    return false;
            }
            
            // Loops through creating the urls we will need
            for (int x = 0; x < xMax; x++)
            {
                for (int y = 0; y < yMax; y++)
                {
                    string tempDownloadValues = UrlDownloadValues.Replace("X", x.ToString()).Replace("Y", y.ToString()).Replace("Z", zoomLevel.ToString());
                    string tempUrl = UrlDownloadBase + url + UrlDownloadOutput + tempDownloadValues;
                    string tempFileName = FilePathBase + "tile-x" + x + "-y" + y;
                    DownloadRemoteImageFile(tempUrl, tempFileName);
                }
            }

            return true;
        }

        // Downloads image from remote server
        private static void DownloadRemoteImageFile(string uri, string fileName)
        {
            print(uri);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // Check that the remote file was found. The ContentType
            // check is performed since a request for a non-existent
            // image file might be redirected to a 404-page, which would
            // yield the StatusCode "OK", even though the image was not
            // found.
            if ((response.StatusCode == HttpStatusCode.OK || 
                 response.StatusCode == HttpStatusCode.Moved || 
                 response.StatusCode == HttpStatusCode.Redirect) &&
                response.ContentType.StartsWith("image",StringComparison.OrdinalIgnoreCase))
            {

                // if the remote file was found, download oit
                using (Stream inputStream = response.GetResponseStream())
                using (Stream outputStream = System.IO.File.OpenWrite(fileName))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                        outputStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                }
            }
        }
    }
}
