using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    //
    // Fields
    //
    public GameObject Loading;

    private GameObject _cameraGameObject;
    private GameObject _videoGameObject;
    private GameObject _streetViewGameObject;
    private GameObject _kinectGameObject;



    private GameObject _setMenuItem;

	// Use this for initialization
	void Start ()
	{
	    _videoGameObject = GameObject.Find("Video");
	    _streetViewGameObject = GameObject.Find("StreetView");
	    _kinectGameObject = GameObject.Find("Kinect");
	    _cameraGameObject = GameObject.Find("CenterEyeAnchor");
	    _setMenuItem = null;
	}
	
	// Update is called once per frame
	void Update () {

        // We raycast and then check buttons
		RaycastHit hit;
	    if (Physics.Raycast(_cameraGameObject.transform.position, _cameraGameObject.transform.forward, out hit, 1000.0F))
	    {
	        var parent = hit.transform.parent;
	        if (parent.transform == _videoGameObject.transform)
	        {
	            _setMenuItem = _videoGameObject;
	            _setMenuItem.GetComponent<Image>().fillCenter = false;
	            _setMenuItem.GetComponent<Image>().color = Color.green;
	            _streetViewGameObject.GetComponent<Image>().color = new Color32(144, 99, 99, 255);
	            _kinectGameObject.GetComponent<Image>().color =  new Color32(144, 99, 99, 255);
	            _streetViewGameObject.GetComponent<Image>().fillCenter = true;
	            _kinectGameObject.GetComponent<Image>().fillCenter = true;
	        }
	        else if (parent.transform == _streetViewGameObject.transform)
	        {
	            _setMenuItem = _streetViewGameObject;
	            _setMenuItem.GetComponent<Image>().color = Color.green;
	            _setMenuItem.GetComponent<Image>().fillCenter = false;
	            _videoGameObject.GetComponent<Image>().color =  new Color32(144, 99, 99, 255);
	            _kinectGameObject.GetComponent<Image>().color =  new Color32(144, 99, 99, 255);
	            _videoGameObject.GetComponent<Image>().fillCenter = true;
	            _kinectGameObject.GetComponent<Image>().fillCenter = true;
	        }
	        else if (parent.transform == _kinectGameObject.transform)
	        {
	            _setMenuItem = _kinectGameObject;
	            _setMenuItem.GetComponent<Image>().color = Color.green;
	            _setMenuItem.GetComponent<Image>().fillCenter = false;
	            _videoGameObject.GetComponent<Image>().color =  new Color32(144, 99, 99, 255);
	            _streetViewGameObject.GetComponent<Image>().color = new Color32(144, 99, 99, 255);
	            _videoGameObject.GetComponent<Image>().fillCenter = true;
	            _streetViewGameObject.GetComponent<Image>().fillCenter = true;
	        }
	        else
	        {
                _setMenuItem = null;
	            _videoGameObject.GetComponent<Image>().color =  new Color32(144, 99, 99, 255);
	            _kinectGameObject.GetComponent<Image>().color =  new Color32(144, 99, 99, 255);
	            _streetViewGameObject.GetComponent<Image>().color = new Color32(144, 99, 99, 255);
	            _videoGameObject.GetComponent<Image>().fillCenter = true;
	            _streetViewGameObject.GetComponent<Image>().fillCenter = true;
	            _kinectGameObject.GetComponent<Image>().fillCenter = true;
	        }
	    }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("HAPPENS!!");
            if (_setMenuItem == _videoGameObject)
            {
                SceneManager.LoadScene("Video");
            } else if (_setMenuItem == _kinectGameObject)
            {
                SceneManager.LoadSceneAsync("Kinect");
            } else if (_setMenuItem == _streetViewGameObject)
            {
                Loading.SetActive(true);
                SceneManager.LoadSceneAsync("StreetView");
                Loading.SetActive(false);
            }
        }
	}
}
