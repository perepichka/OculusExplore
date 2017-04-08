using UnityEngine;
using System.Collections;

public class VideoLoader : MonoBehaviour
{

    public AudioSource audioSource;

    public GameObject left;
    public GameObject right;

    public int frameCount = 0;
    public int frameRate = 30;

    public Texture2D[] frames;

	// Use this for initialization
	void Start ()
	{
        if (audioSource)
            audioSource.Play();

	    frames = new Texture2D[frameCount];

	    for (int i = 0; i < frameCount; i++)
	    {
	        string frameName = string.Format("Frames\\frame{0:d4}", i + 1);
	        frames[i] = (Texture2D) Resources.Load(frameName);
	    }

	}
	
	// Update is called once per frame
	void Update ()
	{

        int currentFrame = (int) (Time.time * frameRate);

        if (currentFrame >= frames.Length)
			currentFrame = frames.Length - 1;

        left.GetComponent<MeshRenderer>().material.mainTexture = right.GetComponent<MeshRenderer>().material.mainTexture = frames[currentFrame];

        //left.GetComponent<Renderer>().material.SetTexture("_MainTex", frames[currentFrame]);
	    //ight.GetComponent<Renderer>().material.SetTexture("_MainTex", frames[currentFrame]);
	}
}
