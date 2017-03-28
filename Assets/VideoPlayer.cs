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
