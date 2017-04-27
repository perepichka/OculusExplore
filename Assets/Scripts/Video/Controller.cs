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
using UnityEngine;
using UnityEngine.Video;

namespace Video
{
    public class Controller : MonoBehaviour
    {

        public bool IsMono;

        public VideoClip VideoClip;
        public Texture Image;

        //
        // Members
        //

        // Scene stuff
        private GameObject _monoSphere;
        private GameObject _leftSphere;
        private GameObject _rightSphere;

        private AudioSource _audioSource;
        private UnityEngine.Video.VideoPlayer _videoPlayer;

        private RenderTexture _outputTexture;

        // Use this for initialization
        void Start ()
        {
            // Gets references to the objects in the scene
            _audioSource = GetComponent<AudioSource>();
            _videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();

            // Sets up the Display spheres
            _rightSphere = GameObject.Find("Right Sphere");
            _leftSphere = GameObject.Find("Left Sphere");
            _monoSphere = GameObject.Find("Mono Sphere");

            // Determines which spheres are to be used
            if (!IsMono)
            {
                _rightSphere.SetActive(true);
                _leftSphere.SetActive(true);
                _monoSphere.SetActive(false);
            }
            else
            {
                _rightSphere.SetActive(false);
                _leftSphere.SetActive(false);
                _monoSphere.SetActive(true);
            }

            // Calls coroutine that set up the video
            if (VideoClip)
            {
                StartCoroutine(SetupVideo());
            }
            else if (Image)
            {
                StartCoroutine(SetupImage());
            }
            else
            {
                throw new Exception("Both Image and Video fields are unspecified");
            }

        }
	
        // Update is called once per frame
        void Update () {
            // Back to main menu
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
            // Video controls
            if (VideoClip)
            {
                if (Input.GetKeyDown("space") && _videoPlayer.isPrepared)
                {
                    if (_videoPlayer.isPlaying)
                    {
                        _videoPlayer.Pause();
                    }
                    else
                    {
                        _videoPlayer.Play();
                    }

                }
            }

        }

        IEnumerator SetupImage()
        {
            if (!IsMono)
            {
                _leftSphere.GetComponent<MeshRenderer>().material.mainTexture = Image;
                _rightSphere.GetComponent<MeshRenderer>().material.mainTexture = Image;
            }
            else
            {
                _monoSphere.GetComponent<MeshRenderer>().material.mainTexture = Image;
            }

            yield return null;
        }

        IEnumerator SetupVideo()
        {
            // Sends the VideoClip to the VideoPlayer
            _videoPlayer.clip = VideoClip;

            // Sets up Texture to be used with Spheres
            _outputTexture = new RenderTexture((int) _videoPlayer.clip.width, (int) _videoPlayer.clip.height, 24);

            // Sets the sphere textures
            if (!IsMono)
            {
                _leftSphere.GetComponent<MeshRenderer>().material.mainTexture = _outputTexture;
                _rightSphere.GetComponent<MeshRenderer>().material.mainTexture = _outputTexture;
            }
            else
            {
                _monoSphere.GetComponent<MeshRenderer>().material.mainTexture = _outputTexture;
            }

            // Sets the VideoPlayer to render to the created texture
            _videoPlayer.targetTexture = _outputTexture;

            // Sets the AudioSource
            _videoPlayer.SetTargetAudioSource(0, _audioSource);

            // Plays the video
            _videoPlayer.Play();

            yield return null;
        }
    }
}
