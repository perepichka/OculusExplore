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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Downloader : MonoBehaviour {





	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

//	private static void DownloadRemoteImageFile(string uri, string fileName)
//	{
//		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
//		HttpWebResponse response = (HttpWebResponse)request.GetResponse();
//
//		// Check that the remote file was found. The ContentType
//		// check is performed since a request for a non-existent
//		// image file might be redirected to a 404-page, which would
//		// yield the StatusCode "OK", even though the image was not
//		// found.
//		if ((response.StatusCode == HttpStatusCode.OK || 
//			response.StatusCode == HttpStatusCode.Moved || 
//			response.StatusCode == HttpStatusCode.Redirect) &&
//			response.ContentType.StartsWith("image",StringComparison.OrdinalIgnoreCase))
//		{
//
//			// if the remote file was found, download oit
//			using (Stream inputStream = response.GetResponseStream())
//			using (Stream outputStream = File.OpenWrite(fileName))
//			{
//				byte[] buffer = new byte[4096];
//				int bytesRead;
//				do
//				{
//					bytesRead = inputStream.Read(buffer, 0, buffer.Length);
//					outputStream.Write(buffer, 0, bytesRead);
//				} while (bytesRead != 0);
//			}
//		}
//	}
}
