using _Scripts.Gameplay.Architecture.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Gameplay.Architecture.Systems{

    public static class Bootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Execute()
        {
            Object gameStateObj;
            Object.DontDestroyOnLoad(gameStateObj = Object.Instantiate(Resources.Load("Prefabs/GameState/GameStateManager")));
            if (gameStateObj != null)
            {
                GameObject gsGO = gameStateObj as GameObject;

                if (gsGO != null)
                {
                    GameStateManager gs = gsGO.GetComponent<GameStateManager>();
                    gs.Initialise();
                }
            }
            Debug.LogWarning("Bootstrapper Execute");
        }
    }

}
