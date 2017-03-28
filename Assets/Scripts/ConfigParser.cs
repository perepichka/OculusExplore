using UnityEngine;
using System.Collections;
using System.IO;
using SharpConfig;

public class ConfigParser : MonoBehaviour {

    // Some members
    public string apiKey;
    

    // Use this for initialization
    void Start () {

        // Loads api key from init file. Need to specify one in order to have more than 100 reqs"
        var config = Configuration.LoadFromFile("Config\\init.ini");
        var section = config["api"];
        apiKey = section["API_KEY"].StringValue;

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
