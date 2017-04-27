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

        private static readonly Vector2[] Directions = {Vector2.up, Vector2.down, Vector2.right, Vector2.left};

        private const double LatitutdeDisplacement = 0.001f;
        private const double LongitudeDisplacement = 0.001f;

        private const float DefaultStartingCoordinateLat = 45.5589642f;//45.5618409f;//45.5551925f;// 45.5582384f;
        private const float DefaultStartingCoordinateLng = -73.4546582f;//-73.45338f;//-73.4666552f; //-73.5488266f;

        //
        // Public Fields
        //

        public bool CanMove;

        public float StartingLat;
        public float StartingLng;

        public string StreeViewApiKey;

        public bool snapping;

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

            // Otherwise, we try to load defaults
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
            else if (Input.GetKeyDown("space") && CanMove)
            {
                MoveToCameraDirection();
            }
        }

        //
        // Getters and Setters
        //

        public void SetSphereTexture(Texture2D tex)
        {
            _sphere.GetComponent<MeshRenderer>().material.mainTexture = tex;
        }

        public Vector2 GetCameraDirection()
        {
            Vector3 lookAt = _oculusCamera.transform.forward;

            // Discards y from look at and returns it
            Vector2 vec = new Vector2(lookAt.x, lookAt.z);

            // We need a normalized vec
            vec.Normalize();

            Debug.Log(vec.x + " " + vec.y);

            return vec;
        }

        //
        // Movement related
        //

        // Moves to the direction specified direction
        public void MoveToModifiedDirection(Vector2 direction)
        {
            // Movement should be relative to the LookAt vector
            Vector2 camDirection = GetCameraDirection();

            // Gets the modified camera direction
            if (direction == Vector2.left)
            {
                direction = new Vector2(camDirection.y, -camDirection.x);
            }
            else if (direction == Vector2.right)
            {
                direction = new Vector2(-camDirection.y, camDirection.x);
            }
            else if (direction == Vector2.down)
            {
                direction = new Vector2(-camDirection.x, -camDirection.y);
            }
            else if (direction == Vector2.up)
            {
                direction = camDirection;
            }

            if (snapping)
            {
                // Now snap to the closest (ie the one that maximizes the dot product)
                Vector2 direct = Vector2.up;
                float maxDot = 0;
                foreach (Vector2 dir in Directions)
                {
                    float dot = Vector2.Dot(direction, dir);
                    if (dot > maxDot)
                    {
                        maxDot = dot;
                        direct = dir; 
                    }
                }
                direction = direct;
            }

            _sticherScript.Move(direction);
        }

        // Moves to a direction free of snapping to predefined ones
        public void MoveToCameraDirection()
        {
            // Movement should be relative to the LookAt vector
            Vector2 camDirection = GetCameraDirection();

            _sticherScript.Move(camDirection);
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