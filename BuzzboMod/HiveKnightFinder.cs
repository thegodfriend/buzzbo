using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;


namespace BuzzboMod
{
    internal class HiveKnightFinder : MonoBehaviour
    {
        
        private void Start()
        {
            USceneManager.activeSceneChanged += SceneChanged;
        }

        private void SceneChanged(Scene arg0, Scene arg1)
        {
            //Modding.Logger.Log("Secne change to " + arg1.name);
            if (arg1.name != "GG_Hive_Knight") return;
            Modding.Logger.Log("Entered Godhome Hive Knight scene.");

            StartCoroutine(AddAlteringComponent());
        }
        
        
        private static IEnumerator AddAlteringComponent()
        {
            Modding.Logger.Log("Adding Hive Knight Altering Component");
            yield return null;

            GameObject.Find("Hive Knight").AddComponent<HiveKnightAlter>();
        }

        private void OnDestroy()
        {
            USceneManager.activeSceneChanged -= SceneChanged;
        }

    }
}
