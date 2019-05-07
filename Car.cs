using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof (CarController))]
    public class TheAIConfCar : MonoBehaviour
    {
        // private properties required for use in object functions
        private CarController carController; 
        private Rigidbody rigidBody;

        // editable properties exposed to the Unity editor
        public Transform resetPoint;
        public float visibleDistance = 25.0f;

        // Called when the game object enters a Unity scene 
        // (basically a secondary initialiser)
        private void Awake()
        {
            // get the car controller
            carController = GetComponent<CarController>();
    
            // get a handle on the rigid body
            rigidBody = GetComponent<Rigidbody>();
        }

        // Called every 'tick' to check what the car should do next
        // (passes user input instructions to object)
        private void FixedUpdate()
        {
            // pass the input to the car
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");

            // move a the car accordingly (steering, acceleration, footbrake, handbrake)
            carController.Move(h, v, v, 0f);
        }

        // Called when the Rigidbody's collider collides with another object's collider
        // - parameter collision: The Collision object containing details of where the collision
        //      occurred and what was collided with
        void OnCollisionEnter(Collision collision)
        {
            // if the thing we hit was not the road (which we're always hitting)
            if (collision.collider.tag != "road") 
            {
                // print what we hit and reset the car
                Debug.Log($"Car collided with {collision.gameObject.name}.");
                Reset();
            }
        }

        // Called when the car has met a loss condition and needs to respawn at checkpoint
        public void Reset()
        {
            // reset the car's current velocity to stationary
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;

            // move and rotate the car back to the reset position
            transform.position = resetPoint.position;
            transform.rotation = resetPoint.rotation;
        }
    }
}
