using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using MLAgents;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class TheAIConfSelfDrivingCar : Agent
    {
        private float CONFIRM_DETECTION = 1f;
        private float CONFIRM_NONDETECTION = 0f;

        // private properties required for use in object functions
        private CarController carController; 
        private Rigidbody rigidBody;

        // editable properties exposed to the Unity editor
        public Transform resetPoint;
        public float visibleDistance = 25.0f;
        public float raycastY = 1.3f;

        // Called when the game object enters a Unity scene 
        // (basically a secondary initialiser)
        private void Awake()
        {
            // get the car controller
            carController = GetComponent<CarController>();
    
            // get a handle on the rigid body
            rigidBody = GetComponent<Rigidbody>();
        }
        
        //==================================================================================
        // FixedUpdate() replaced by AgentAction(), called when the Agent requests an action
        //==================================================================================

        // Called every 'tick' to check what the car should do next
        // (passes user input OR BRAIN instructions to object)
        // - parameter vectorAction: A len(2) Vector representing [horizontal action, vertical action] 
        //      with elements from {-1, 0, 1}
        // - parameter textAction: Not used
        public override void AgentAction(float[] vectorAction, string textAction)
        {
            float h = vectorAction[0]; // horizontal movement (-1 = left, 0 = no change, 1 = right)
            float v = vectorAction[1]; // horizontal movement (-1 = back, 0 = no change, 1 = forward)

            // move a the car accordingly (steering, acceleration, footbrake, handbrake)
            carController.Move(h, v, v, 0);
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

        //==================================================================================
        // Reset() function now called AgentReset() so the Agent can call it on its own
        //==================================================================================

        // Called when the car has met a loss condition and needs to respawn at checkpoint
        public void AgentReset()
        {
            // reset the car's current velocity to stationary
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;

            // move and rotate the car back to the reset position
            transform.position = resetPoint.position;
            transform.rotation = resetPoint.rotation;
        }

        //==================================================================================
        // New function allows Agent to 'see' by collecting key depth observations
        //==================================================================================

        // Called every 'tick' to collect information needed for the Agent to decide what
        // to do next (or to record for comparison when recording imitation demonstration)
        public override void CollectObservations()
        {
            RaycastHit hit;

            // get position, direction and velocity details of car in this moment
            Vector3 currentVelocity = transform.InverseTransformVector(rigidBody.velocity);
            Vector3 currentAngularVelocity = transform.InverseTransformVector(rigidBody.angularVelocity);
            Vector3 rayOrigin = new Vector3(transform.position.x, raycastY, transform.position.z);
            Vector3[] rayDirections = { 
                this.transform.forward, // forward
                this.transform.right,   // right
                -this.transform.right,  // left
                Quaternion.AngleAxis(45, Vector3.down) * this.transform.right,  // right 45
                Quaternion.AngleAxis(45, Vector3.up) * -this.transform.right }; // left 45
            }

            // cast rays outwards from car to detect nearby objects and report if found
            for (var direction in rayDirections)
            {
                Color colour = Color.green;
                Vector3 rayDirection = new Vector3(direction.x, 0f, direction.z);

                if (Physics.Raycast(rayOrigin, rayDirection, out hit, visibleDistance))
                {
                    float normalisedDistance = hit.distance / visibleDistance;
                    colour = (normalisedDistance < 0.15) ? Color.red : Color.yellow;

                    AddVectorObs(normalisedDistance);   // transmit (normalised) hit distance
                    AddVectorObs(CONFIRM_DETECTION);    // transmit 'true' signal to confirm this was a detection
                }
                else
                {
                    AddVectorObs(1f);                   // transmit max (normalised) hit distance
                    AddVectorObs(CONFIRM_NONDETECTION); // transmit 'false' signal to affirm this was not a detection
                }

                // draw ray (green if hitting nothing, red if hitting very close, yellow otherwise)
                Debug.DrawRay(rayOrigin, rayDirection * visibleDistance, colour);
            }

            // send velocity observations
            AddVectorObs(currentVelocity.x);
            AddVectorObs(currentVelocity.y);
            AddVectorObs(currentVelocity.z);
            AddVectorObs(currentAngularVelocity.y);
        }
    }
}
