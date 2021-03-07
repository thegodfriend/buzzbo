using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using Modding;
using ModCommon.Util;
using UnityEngine;
using Logger = Modding.Logger;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Collections;

namespace BuzzboMod
{
    internal class HiveKnightAlter : MonoBehaviour
    {
        
        private HealthManager _hm;

        private PlayMakerFSM _stunControl;
        private PlayMakerFSM _control;

        private GameObject honeyspike;
        private GameObject glob;
        private GameObject _globs;

        private float direction; // 1 = facing left, -1 = facing right

        private bool shootingDown = false;
        private bool dashing = false;
        private bool shootingSpiral = false;

        private bool isStunned = false;

        private void FullLog(string message)
        {
            Logger.Log($"[{DateTime.Now.ToLocalTime()}] [{GetHashCode()}] [{System.Threading.Thread.CurrentThread.ManagedThreadId}] {message}"); 
        }

        private void Awake()
        {
            ModHooks.Instance.LanguageGetHook += LanguageGet;

            Modding.Logger.Log("Added Hive Knight Altering MonoBehaviour");
            
            _hm = gameObject.GetComponent<HealthManager>();

            _stunControl = gameObject.LocateMyFSM("Stun Control");
            _control = gameObject.LocateMyFSM("Control");

            honeyspike = GameObject.Find("Battle Scene").transform.Find("Globs").Find("Hive Knight Glob").Find("Stingers").Find("Stinger (1)").gameObject;
            glob = GameObject.Find("Battle Scene").transform.Find("Globs").Find("Hive Knight Glob").gameObject;
            _globs = GameObject.Find("Battle Scene").transform.Find("Globs").gameObject;

        }

        private string LanguageGet(string key, string sheet)
        {

            string text = Language.Language.GetInternal(key, sheet);
            //return text;

            switch (key)
            {
                case "HIVE_KNIGHT_1":
                    return (isStunned) ? "This is so sad... Alexa play Vespacito" : "Are you watching, Fonsi?";
                case "HIVE_KNIGHT_2":
                    return (isStunned) ? "This is so sad... Alexa play Vespacito" : "Have faith in me, Luis!";
                case "HIVE_KNIGHT_3":
                    return (isStunned) ? "This is so sad... Alexa play Vespacito" : "oof";
                    
            }

            return Language.Language.GetInternal(key, sheet);
        }

        private void Start()
        {
            
            // Track Stunning
            _stunControl.InsertMethod("Stun", 0, () => {
                isStunned = true;
                dashing = false;
                shootingDown = false;
                shootingSpiral = false;
            });
            _control.InsertMethod("Stun Recover", 0, () => { isStunned = false;});
            


            // Altering HP values
            _hm.hp = (_hm.hp / 2) * 3 + 200; // 850 -> 1475, 1300 -> 2150
            _control.FsmVariables.GetFsmInt("P2 HP").Value = (_hm.hp / 10) * 6; // 780 -> 885, (Irrelevant: 780 -> 1290)
            _control.FsmVariables.GetFsmInt("P3 HP").Value = (_hm.hp / 10) * 4; // 550 -> 590, (Irrelevant: 550 -> 860)



            // Generally harder Stunning
            _stunControl.FsmVariables.FindFsmFloat("Combo Time").Value = 0.75f;
            _stunControl.FsmVariables.FindFsmInt("Stun Combo").Value = 11;
            _stunControl.FsmVariables.FindFsmInt("Stun Hit Max").Value = 20;
            

            
            // Less idle time between attacks
            _control.FsmVariables.GetFsmFloat("Idle Time").Value = 0.05f;


            
            // Increases Surprise Slash speed. No special deceleration for now.
            //_control.GetAction<DecelerateXY>("Slash Recover", 2).decelerationX = 0.5f;
            _control.FsmVariables.GetFsmFloat("Slash Speed").Value *= 1.58f;

            // Adds spikes to the Surprise Slash
            _control.InsertMethod("TeleIn 2", 0, SpawnTeleInHoneyspikes);



            // Increases Lunge speed, and allows for stopping almost immediately at the end
            _control.FsmVariables.GetFsmFloat("Dash Speed").Value *= 1.4f;
            _control.GetAction<DecelerateXY>("Dash Recover", 1).decelerationX = 0.01f;

            // Adds spikes to Lunge
            StartCoroutine(ShootDashSpikes());
            _control.InsertMethod("Dash Antic", 0, () => { dashing = true; });
            _control.InsertMethod("Dash Recover", 0, () => { dashing = false; });



            // Adds spikes to Leap
            StartCoroutine(ShootDownSpikes());
            _control.InsertMethod("Jump Antic", 0, () => { shootingDown = true; });
            _control.InsertMethod("Land", 0, () => { shootingDown = false; });
            
            

            // Adds Spike Spiral to Globs and Roar
            StartCoroutine(ShootSpiralSpikes());
            _control.InsertMethod("Glob Antic 2", 0, () => { shootingSpiral = true; });
            _control.InsertMethod("Glob Recover", 0, () => { shootingSpiral = false; });
            _control.InsertMethod("Roar", 0, () => { shootingSpiral = true; });
            _control.InsertMethod("Roar Cooldown", 0, () => { shootingSpiral = false; });




            //_control.InsertMethod("Glob Strike", 0, SpawnExtraGlobSpikes);
            {// +++++++++++++++++++++++++++++++++
             /*
             GameObject.Instantiate(glob).transform.SetParent(_globs.transform);


             foreach (Transform child in _globs.transform)
             {
                 Logger.Log("Child of " + _globs.transform.name + ": " + child.name);
             }
             */
             // +++++++++++++++++++++++++++++++++
            } // A failed something for the future
        }

        private void Update() {
            direction = gameObject.transform.localScale.x;
        }

        private GameObject SpawnHoneySpike(Vector3 position, float rotation)
        {
            //Modding.Logger.Log("Spawning Honeyspike");

            GameObject Spike = GameObject.Instantiate(honeyspike);
            Spike.SetActive(true);

            Spike.transform.localPosition = position;
            Spike.transform.localRotation = Quaternion.Euler(0, 0, rotation);

            var hks = Spike.GetComponent<HiveKnightStinger>();
            Destroy(hks);
            Spike.AddComponent<HoneySpikeMovement>();

            return Spike;
        }


        private void SpawnTeleInHoneyspikes() {
            GameObject[] Spikes = new GameObject[7];

            for (int i = 0; i < 7; i++) {
                Vector3 spikePos;
                float spikeRot=0;

                spikePos = this.transform.position;

                
                switch (direction) {
                    case -1: // Facing right
                        spikeRot = 45 - i * 22.5f;
                        break;
                    case 1: // Facing left
                        spikeRot = 135 + i * 22.5f;
                        break;
                }

                Spikes[i] = SpawnHoneySpike(spikePos, spikeRot);

            }
        }

        private void SpawnDashHoneyspikes_even() {
            GameObject[] Spikes = new GameObject[4];

            for (int i = 0; i < 4; i++)
            {
                Vector3 spikePos;
                float spikeRot = 0;

                spikePos = this.transform.position;

                switch (direction)
                {
                    case 1: // Facing left
                        spikeRot = 45 - i * 30f;
                        break;
                    case -1: // Facing right
                        spikeRot = 135 + i * 30f;
                        break;
                }

                Spikes[i] = SpawnHoneySpike(spikePos, spikeRot);

            }
        }
        private void SpawnDashHoneyspikes_odd()
        {
            GameObject[] Spikes = new GameObject[3];

            for (int i = 0; i < 3; i++)
            {
                Vector3 spikePos;
                float spikeRot = 0;

                spikePos = this.transform.position;

                switch (direction)
                {
                    case 1: // Facing left
                        spikeRot = 30 - i * 30f;
                        break;
                    case -1: // Facing right
                        spikeRot = 150 + i * 30f;
                        break;
                }

                Spikes[i] = SpawnHoneySpike(spikePos, spikeRot);

            }
        }

        /*
        private void SpawnExtraGlobSpikes() {
            foreach (Transform child in _globs.transform) {
                Vector3 centerPos;
                GameObject[] Spikes = new GameObject[8];

                centerPos = new Vector3(69.4f, 25.0f, 0.0f);// child.transform.position;

                for (int i = 0; i < 8; i++) {   
                    float spikeRot = 22.5f + 45 * i;

                    Spikes[i] = SpawnHoneySpike(centerPos, spikeRot);
                }


            }
        }*/


        IEnumerator ShootDownSpikes() {
            float interval = 0.1f; // 0.08f;
            float nextPotentialSpike = Time.time + interval;

            while (true)
            {
                if (Time.time >= nextPotentialSpike)
                {
                    nextPotentialSpike = Time.time + interval;

                    if (shootingDown) SpawnHoneySpike(this.transform.position, -90);
                }

                yield return null;
            }
        }

        IEnumerator ShootDashSpikes() {
            float interval = 0.08f; //0.002f;
            float nextPotentialSpike = Time.time + interval;
            bool even = true;

            while (true)
            {

                if (Time.time >= nextPotentialSpike)
                {
                    nextPotentialSpike = Time.time + interval;

                    if (dashing)
                    {
                        if (even) SpawnDashHoneyspikes_even();
                        else SpawnDashHoneyspikes_odd();

                        even = !even;
                    }
                }

                yield return null;
            }
        }

        IEnumerator ShootSpiralSpikes() {
            float interval = 0.06f;
            float nextPotentialSpike = Time.time + interval;

            float angle = 0;

            while (true) {

                if (Time.time >= nextPotentialSpike)
                {
                    nextPotentialSpike = Time.time + interval;

                    if (shootingSpiral) SpawnHoneySpike(this.transform.position, angle);

                    angle += 23;
                    angle %= 360;

                }

                yield return null;
            }
        }


        private void OnDestroy() {
            ModHooks.Instance.LanguageGetHook -= LanguageGet;
        }

    }
}
