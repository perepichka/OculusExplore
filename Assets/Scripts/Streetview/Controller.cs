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

namespace Streetview
{
    public class Controller : MonoBehaviour {

        //
        // Constants
        //

        // Coordinate
        private const double LatitutdeDisplacement = 0.001f;
        private const double LongitudeDisplacement = 0.001f;

        //
        // Members
        //

        private GameObject _sphere;s

        public bool CanMove;

        private Sticher _sticherScript;


        // Use this for initialization
        void Start ()
        {
            _sticherScript = transform.GetComponent<Sticher>();
            _sphere = GameObject.Find("Sphere");
        }
	
        // Update is called once per frame
        void Update () {

            if (Input.GetKeyDown("left") && CanMove)
            {
                

            } else if (Input.GetKeyDown("right") && CanMove)
            {


            }
            else if (Input.GetKeyDown("up") && CanMove)
            {


            }
            else if (Input.GetKeyDown("down") && CanMove)
            {


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
            return new Vector2(0,0);
        }
    
}
