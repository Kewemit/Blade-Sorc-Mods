using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace AutoLoader
{
    public class LoadArenaOnStart : ThunderScript
    {
        public static string selectedMap = "Arena";
        public bool mapLoaded;
        public static ModOptionString[] Levels =
        {
            new ModOptionString("Arena", "Arena"),
            new ModOptionString("Home", "Home"),
            new ModOptionString("Market", "Market"),
            new ModOptionString("Canyon", "Canyon"),
            new ModOptionString("Sanctuary", "Sanctuary"),
            new ModOptionString("Citadel", "Citadel"),

        };
        [ModOption("Map select","Map options", valueSourceName: nameof(Levels))]
        [ModOptionSave]
        private static void LevelOption(string value) 
        {
            selectedMap = value;
        }



        public override void ScriptEnable()
        {
            base.ScriptEnable();
            mapLoaded = false;
            EventManager.onLevelLoad += EventManager_onLevelLoad;

        }


        public void EventManager_onLevelLoad(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
        {
            if (!mapLoaded)
            {

                if (eventTime == EventTime.OnEnd && levelData.id == "Home")
                {
                    LevelManager.LoadLevel(selectedMap);
                }
                if (levelData.id == selectedMap) 
                {
                    ScriptDisable();
                }
            }

        }

        public override void ScriptDisable()
        {
            EventManager.onLevelLoad -= EventManager_onLevelLoad;
            base.ScriptDisable();
            

        }
    }
}
