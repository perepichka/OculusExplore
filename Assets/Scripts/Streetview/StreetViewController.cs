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
using UnityEngine;

namespace Streetview
{
    public class StreetViewController : MonoBehaviour
    {

        //
        // Constants
        //

        private const double LatitutdeDisplacement = 0.001f;
        private const double LongitudeDisplacement = 0.001f;

        private const float DefaultStartingCoordinateLat = 45.5544263f;//45.5582384f;
        private const float DefaultStartingCoordinateLng = -73.477401f;//-73.5488266f;

        //
        // Public Fields
        //

        public bool CanMove;

        public float StartingLat;
        public float StartingLng;

        public string StreeViewApiKey;

        //
        // Private Fields
        //

        private GameObject _sphere;

        private StreetViewSticher _sticherScript;

        private GameObject _oculusCamera;


        // Use this for initialization
        void Start()
        {
            // Checks that we have the needed info
            if (string.IsNullOrEmpty(StreeViewApiKey))
            {
                Debug.Log("API key empty. Attempting to load from config file.");
                try
                {
                    ConfigParser config = transform.GetComponent<ConfigParser>();
                    
                    StreeViewApiKey = config.ParseConfig();
                    if (string.IsNullOrEmpty(StreeViewApiKey))
                    {
                        throw new Exception();
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Cannot load config. Continuing with disabled movement.");
                    Debug.Log(e);
                    CanMove = false;
                }
            }
            if (StartingLat == 0 || StartingLng == 0)
            {
                Debug.Log("No starting Coordinates. Reverting to defaults.");
                StartingLat = DefaultStartingCoordinateLat;
                StartingLng = DefaultStartingCoordinateLng;
            }

            // Tries to find necessary objects within the scene
            try
            {
                _sticherScript = transform.GetComponent<StreetViewSticher>();
                _sphere = GameObject.Find("Sphere");
                _oculusCamera = GameObject.Find("CenterEyeAnchor");
            }
            catch (Exception e)
            {
                Debug.Log("Faied to find necessary Game Objects and/or Scripts");
                Debug.Log(e);
            }

            // Starts the Sticher script by setting up the starting position
            bool result =_sticherScript.AttemptAddingPoint(
                new Vector2(StartingLat, StartingLng),
                Vector2.up /* random attempt direction */
            );

            // Otherwise, we try to load defauts
            if (!result)
            {
                ResetCoordinate();
                _sticherScript.AttemptAddingPoint(
                    new Vector2(StartingLat, StartingLng),
                    Vector2.up /* random attempt direction */
                );
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown("left") && CanMove)
            {
                MoveToModifiedDirection(Vector2.left);
            }
            else if (Input.GetKeyDown("right") && CanMove)
            {
                MoveToModifiedDirection(Vector2.right);
            }
            else if (Input.GetKeyDown("up") && CanMove)
            {
                MoveToModifiedDirection(Vector2.up);
            }
            else if (Input.GetKeyDown("down") && CanMove)
            {
                MoveToModifiedDirection(Vector2.down);
            }
        }

        //
        // Getters and Setters
        //

        public void SetSphereTexture(Texture2D tex)
        {
            _sphere.GetComponent<MeshRenderer>().material.mainTexture = tex;
        }

        // Moves to the direction specified direction
        public void MoveToModifiedDirection(Vector2 direction)
        {

            // @TODO, make direction depend on camera lookat vec
            Vector2 camDirection = GetCameraDirection();


            _sticherScript.Move(direction);
        }

        public Vector2 GetCameraDirection()
        {
            Vector3 lookAt = _oculusCamera.transform.forward;

            // Discards y from look at and returns it
            return new Vector2(lookAt.x, lookAt.z);
        }

        // 
        // Private methods
        //

        // Resets coordinates to default
        private void ResetCoordinate()
        {
            StartingLat = DefaultStartingCoordinateLat;
            StartingLng = DefaultStartingCoordinateLng;
        }
    }
}