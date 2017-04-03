/************************************************************************************
Filename    :   MoviePlayerSample.cs
Content     :   An example of how to use the Moonlight video player
Created     :   July 12, 2014
Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.
Use of this software is subject to the terms of the Oculus LLC license
agreement provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.
************************************************************************************/

using UnityEngine;
using System.Collections;					// required for Coroutines
using System.Runtime.InteropServices;		// required for DllImport
using System;								// requred for IntPtr
using System.IO;							// required for File

/************************************************************************************
Usage:
	Place a simple textured quad surface with the correct aspect ratio in your scene.
	Add the MoviePlayerSample.cs script to the surface object.
	Supply the name of the media file to play:
	This sample assumes the media file is placed in "Assets/StreamingAssets", ie
	"ProjectName/Assets/StreamingAssets/MovieName.mp4".
	On Desktop, Unity MovieTexture functionality is used. Note: the media file
	is loaded at runtime, and therefore expected to be converted to Ogg Theora
	beforehand.
Implementation:
	In the MoviePlayerSample Awake() call, GetNativeTexturePtr() is called on 
	renderer.material.mainTexture.
	
	When the MediaSurface plugin gets the initialization event on the render thread, 
	it creates a new Android SurfaceTexture and Surface object in preparation 
	for receiving media. 
	When the game wants to start the video playing, it calls the StartVideoPlayerOnTextureId()
	script call, which creates an Android MediaPlayer java object, issues a 
	native plugin call to tell the native code which texture id to put the video
	on and return the Android Surface object to pass to MediaPlayer, then sets
	up the media stream and starts it.
	
	Every frame, the SurfaceTexture object is checked for updates.  If there 
	is one, the target texId is re-created at the correct dimensions and format
	if it is the first frame, then the video image is rendered to it and mipmapped.  
	The following frame, instead of Unity drawing the image that was placed 
	on the surface in the Unity editor, it will draw the current video frame.
	It is important to note that the texture is actually replaced -- the original
	version is gone, and the video will now show up everywhere that texture was
	used, not just on the GameObject that ran the script.
************************************************************************************/

public class MoviePlayerSample : MonoBehaviour
{
    public string movieName = string.Empty;

    private string mediaFullPath = string.Empty;
    private bool startedVideo = false;
    private bool videoPaused = false;

    private MovieTexture movieTexture = null;
    private AudioSource audioEmitter = null;
    private Renderer mediaRenderer = null;

    private enum MediaSurfaceEventType
    {
        Initialize = 0,
        Shutdown = 1,
        Update = 2,
        Max_EventType
    };

    /// <summary>
    /// The start of the numeric range used by event IDs.
    /// </summary>
    /// <description>
    /// If multiple native rundering plugins are in use, the Oculus Media Surface plugin's event IDs
    /// can be re-mapped to avoid conflicts.
    /// 
    /// Set this value so that it is higher than the highest event ID number used by your plugin.
    /// Oculus Media Surface plugin event IDs start at eventBase and end at eventBase plus the highest
    /// value in MediaSurfaceEventType.
    /// </description>
    public static int eventBase
    {
        get { return _eventBase; }
        set { _eventBase = value; }
    }

    private static int _eventBase = 0;

    private static void IssuePluginEvent(MediaSurfaceEventType eventType)
    {
        GL.IssuePluginEvent((int) eventType + eventBase);
    }

    /// <summary>
    /// Initialization of the movie surface
    /// </summary>
    void Awake()
    {
        Debug.Log("MovieSample Awake");

        mediaRenderer = GetComponent<Renderer>();
        audioEmitter = GetComponent<AudioSource>();

        if (mediaRenderer.material == null || mediaRenderer.material.mainTexture == null)
        {
            Debug.LogError("Can't GetNativeTexturePtr() for movie surface");
        }

        if (movieName != string.Empty)
        {
            StartCoroutine(RetrieveStreamingAsset(movieName));
        }
        else
        {
            Debug.LogError("No media file name provided");
        }
    }

    /// <summary>
    /// Construct the streaming asset path.
    /// Note: For Android, we need to retrieve the data from the apk.
    /// </summary>
    IEnumerator RetrieveStreamingAsset(string mediaFileName)
    {
        //string mediaFileNameOgv = Path.GetFileNameWithoutExtension(mediaFileName) + ".ogv";
        string streamingMediaPath = "file:///" + mediaFileName;

        //+ Application.streamingAssetsPath + "/" + mediaFileNameOgv;
        WWW wwwReader = new WWW(streamingMediaPath);
        yield return wwwReader;
        Debug.Log("HAPPENS!!!");

        if (wwwReader.error != null)
        {
            Debug.LogError("wwwReader error: " + wwwReader.error);
        }

        movieTexture = wwwReader.GetMovieTexture();
        mediaRenderer.material.mainTexture = movieTexture;
        audioEmitter.clip = movieTexture.audioClip;
        mediaFullPath = streamingMediaPath;
        Debug.Log("Movie FullPath: " + mediaFullPath);
    }

    /// <summary>
    /// Auto-starts video playback
    /// </summary>
    IEnumerator DelayedStartVideo()
    {
        yield return null; // delay 1 frame to allow MediaSurfaceInit from the render thread.

        if (!startedVideo)
        {
            Debug.Log("Mediasurface DelayedStartVideo");

            startedVideo = true;
            if (movieTexture != null && movieTexture.isReadyToPlay)
            {
                movieTexture.Play();
                if (audioEmitter != null)
                {
                    audioEmitter.Play();
                }
            }
        }
    }

    void Start()
    {
        Debug.Log("MovieSample Start");
        StartCoroutine(DelayedStartVideo());
    }

    void Update()
    {
        if (movieTexture != null)
        {
            if (movieTexture.isReadyToPlay != movieTexture.isPlaying)
            {
                movieTexture.Play();
                if (audioEmitter != null)
                {
                    audioEmitter.Play();
                }
            }
        }
    }

    /// <summary>
    /// Pauses video playback when the app loses or gains focus
    /// </summary>
    void OnApplicationPause(bool wasPaused)
    {
        if (movieTexture != null)
        {
            videoPaused = wasPaused;
            if (videoPaused)
            {
                movieTexture.Pause();
                if (audioEmitter != null)
                {
                    audioEmitter.Pause();
                }
            }
            else
            {
                movieTexture.Play();
                if (audioEmitter != null)
                {
                    audioEmitter.Play();
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");

        // This will trigger the shutdown on the render thread
        IssuePluginEvent(MediaSurfaceEventType.Shutdown);
    }
}