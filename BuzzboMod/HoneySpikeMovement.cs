using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Logger = Modding.Logger;
using System.Collections;


namespace BuzzboMod
{
    class HoneySpikeMovement : MonoBehaviour
    {

        private Rigidbody2D rb;
        public float speedNormal = 30f;
        public float direction;

        private float speed;
        private int phase = 0;

        private void Awake() {
            //Logger.Log("HoneySpikeMovement added to a Honeyspike");
            rb = GetComponent<Rigidbody2D>();

            direction = transform.localRotation.eulerAngles.z;
        }

        private void Start() {
            StartCoroutine(ManageSpeedPhase());


        }

        private void Update() {

            switch (phase) {
                case 1:
                    speed = speedNormal;
                    break;
                case 2:
                    speed = 3f;
                    break;
                case 3:
                    speed = speedNormal;
                    break;
            }

            if (phase == 3) speed = speedNormal;


            //direction = transform.localRotation.eulerAngles.z;

            float x = speed * Mathf.Cos(this.direction*0.0174532924f);
            float y = speed * Mathf.Sin(this.direction*0.0174532924f);
            Vector2 velocity;
            velocity.x = x;
            velocity.y = y;
            this.rb.velocity = velocity;
        }

        IEnumerator ManageSpeedPhase() {
            phase = 1;
            yield return new WaitForSeconds(0.1f);
            phase = 2;
            yield return new WaitForSeconds(0.25f);
            phase = 3;

        }
    }
}
