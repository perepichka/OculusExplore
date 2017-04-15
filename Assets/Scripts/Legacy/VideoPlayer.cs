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
using UnityEngine;
using System.Collections;

public class VideoPlayer : MonoBehaviour
{

    public GameObject left;
    public GameObject right;

    // Reference to the movies

    private MovieTexture leftMovieTexture;
    private MovieTexture rightMovieTexture;

    // Old vsynch settings storage
    private int prevVSynch;

    // Use this for initialization
    void Start () {

        // Sets up the references to the textures
        leftMovieTexture = ((MovieTexture)left.GetComponent<Renderer>().material.mainTexture);
	    rightMovieTexture = ((MovieTexture)right.GetComponent<Renderer>().material.mainTexture);

        // Sets quality settings to reduce lag while playing video
        prevVSynch = QualitySettings.vSyncCount;
        QualitySettings.antiAliasing = 0;
        QualitySettings.vSyncCount = 0;

        // Plays the clip
        leftMovieTexture.Play();
        rightMovieTexture.Play();
        
	}
	
	// Update is called once per frame
	void Update () {

    if (!leftMovieTexture.isPlaying && !rightMovieTexture.isPlaying)
    {
        QualitySettings.vSyncCount = prevVSynch;
    }
	
	}
}
