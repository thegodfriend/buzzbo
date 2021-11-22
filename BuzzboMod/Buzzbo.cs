using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BuzzboMod
{
    public class Buzzbo : Mod //, ITogglableMod
    {

        public Buzzbo() : base("Buzzbo") { }

        public override string GetVersion()
        {
            return "0.4.2.0";
        }



        public override void Initialize()
        {
            Log("Initializing Buzzbo test mod version " + GetVersion());
            ModHooks.AfterSavegameLoadHook += AfterSaveGameLoad;
            ModHooks.NewGameHook += AddFinderComponent;
            ModHooks.LanguageGetHook += LanguageGet;
        }


        //Required for ITogglableMod
        /*public void Unload()
        {
            Log("Unloading Buzzbo stuff");
            ModHooks.Instance.AfterSavegameLoadHook -= AfterSaveGameLoad;
            ModHooks.Instance.NewGameHook -= AddFinderComponent;
        }*/

        private void AfterSaveGameLoad(SaveGameData data)
        {
            AddFinderComponent();
        }

        private void AddFinderComponent() {
            Log("Added Hive Knight Finder");
            GameManager.instance.gameObject.AddComponent<HiveKnightFinder>();

        }

        private string LanguageGet(string key, string sheet, string orig)
        {
            
            string text = orig;
            //Log("Key: " + key);
            //Log("Text: " + text);
            //return text;

            // Teacher's Archives Lore Tablets indicating Hive Knight is hollow?
            switch (key)
            {

                case "NAME_HIVE_KNIGHT":
                    return "Buzzbo";
                case "GG_S_HIVEKNIGHT":
                    return "Unyielding god of self-enhancement";
                case "HIVE_KNIGHT_MAIN":
                    return "Buzzbo";
                case "HIVE_KNIGHT_SUB":
                    return "(Bloodless)";
                case "DESC_HIVE_KNIGHT":
                    return "Greatest warrior of the Hive. Eternal and unyielding.";
                case "NOTE_HIVE_KNIGHT":
                    return "Just saying, this mod is meant to have two updates in the future. Not sure if the first is major enough to even be made public, though.";
                    /*
                case "HIVE_KNIGHT_1":
                    return "Are you watching, Fonsi?";
                case "HIVE_KNIGHT_2":
                    return "Have faith in me, Luis!";
                case "HIVE_KNIGHT_3":
                    return "oof";
                    */
                    /*
                case "ARCHIVE_01":
                    return "Omg hollow knight and hive knight are botb HK maybe :clothflushed: hive kmigjt is hokow night confirme d?";
                case "ARCHIVE_02":
                    return "Me when the hiv nit is hol nit confirm by chery tim :clothflushed:";
                case "ARCHIVE_03":
                    return "when the hiev noghts es hloow :clothflushed:";
                    */
            }

            return Language.Language.GetInternal(key, sheet);
        }

    }
}
