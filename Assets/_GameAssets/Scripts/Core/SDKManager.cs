using System.Collections.Generic;
using _GameAssets.Scripts.Managers;
//using Tabtale.TTPlugins;
using UnityEngine;

namespace _GameAssets.Scripts.Core
{
    public class SDKManager : MonoBehaviour
    {
        //When player levels up
        public static void LevelUp()
        {
            //TTPGameProgression.FirebaseEvents.LevelUp(LevelManager.instance.CurrentPlayerLevel + 1,
               // new Dictionary<string, object>());
            Debug.Log($"[EVENTS] Level up: {LevelManager.instance.CurrentPlayerLevel + 1}");
        }

        //When starting unlocking step
        public static void BuyAreasStepStarted(int stepIndex)
        {
            var parameters = new Dictionary<string, object>
            {
                { "stepIndex", stepIndex },
                { "balance", CurrencyManager.Cash }
            };

            SendFirebaseEvent("UnlockingStepStarted", parameters);
            Debug.Log($"[EVENTS] Unlocking step started: {stepIndex}");
        }
        
        //When finishing unlocking step
        public static void BuyAreasStepComplete(int stepIndex)
        {
            var parameters = new Dictionary<string, object>
            {
                { "stepIndex", stepIndex },
                { "balance", CurrencyManager.Cash }
            };

            SendFirebaseEvent("UnlockingStepCompleted", parameters);
            Debug.Log($"[EVENTS] Unlocking step completed: {stepIndex}");
        }
        
        //When starting an in game event
        public static void InGameEventStarted(string eventName)
        {
            var parameters = new Dictionary<string, object>
            {
                { "eventName", eventName },
                { "balance", CurrencyManager.Cash }
            };

            SendFirebaseEvent("InGameEventStarted", parameters);
            Debug.Log($"[EVENTS] In game event started: {eventName}");
        }
        
        //When completing an in game event
        public static void InGameEventCompleted(string eventName)
        {
            var parameters = new Dictionary<string, object>
            {
                { "eventName", eventName },
                { "balance", CurrencyManager.Cash }
            };

            SendFirebaseEvent("InGameEventCompleted", parameters);
            Debug.Log($"[EVENTS] In game event completed: {eventName}");
        }
        
        //When Starting Tutorial Step
        public static void TutorialStepStarted(int tutorialStepIndex)
        {
            var parameters = new Dictionary<string, object>
            {
                { "tutorialStepIndex", tutorialStepIndex },
            };

            SendFirebaseEvent("TutorialStepStarted", parameters);
            Debug.Log($"[EVENTS] Tutorial step started: {tutorialStepIndex}");
        }
        
        //When Completing Tutorial Step
        public static void TutorialStepCompleted(int tutorialStepIndex)
        {
            var parameters = new Dictionary<string, object>
            {
                { "tutorialStepIndex", tutorialStepIndex },
            };

            SendFirebaseEvent("TutorialStepCompleted", parameters);
            Debug.Log($"[EVENTS] Tutorial step completed: {tutorialStepIndex}");
        }

        //When Starting new airportArea
        public static void MissionStarted(int currentLevel, string missionName)
        {
            var parameters = new Dictionary<string, object>
            {
                { "missionName", missionName },
                { "balance", CurrencyManager.Cash }
            };

            //TTPGameProgression.FirebaseEvents.MissionStarted(currentLevel, parameters);
            Debug.Log($"[EVENTS] Mission started: {currentLevel}");
        }

        //When buying new airportArea
        public static void MissionComplete(int currentLevel, string missionName)
        {
            var parameters = new Dictionary<string, object>
            {
                { "missionName", missionName },
                { "balance", CurrencyManager.Cash },
                { "lifeTimeCashStat", CurrencyManager.CashLifetimeStat }
            };

            //TTPGameProgression.FirebaseEvents.MissionComplete(parameters);
            Debug.Log($"[EVENTS] Mission complete: {currentLevel}");
        }

        public static void MiniLevelStarted()
        {
            var parameters = new Dictionary<string, object>
            {
                {"miniMissionName", "playerBoughtStuff"},
            };

            //TTPGameProgression.MiniLevelStarted(GameManager.BoughtStuff, parameters);
            Debug.Log($"[EVENTS] Mini level started: {GameManager.BoughtStuff}\nminiMissionName: playerBoughtStuff");
            
        }

        public static void MiniLevelCompleted()
        {
            //TTPGameProgression.MiniLevelCompleted();
            Debug.Log("minilevelCompleted");
            GameManager.BoughtStuff++;
        }

        private static void SendFirebaseEvent(string name, IDictionary<string, object> parameters)
        {
            //TTPAnalytics.LogEvent(AnalyticsTargets.ANALYTICS_TARGET_FIREBASE, name, parameters, false);
        }


        /*We dont us this for now but maybe later

        public static void MissionFailed(string missionName)
        {
            var parameters = new Dictionary<string, object>
            {
                {"missionName", missionName},
                {"balance", CurrencyManager.instance.Cash}
            };

            TTPGameProgression.FirebaseEvents.MissionFailed(parameters);
            Debug.Log($"[EVENTS] Mission failed: {GameplayManager.CurrentLevelStats}");
        }

        */
    }
}