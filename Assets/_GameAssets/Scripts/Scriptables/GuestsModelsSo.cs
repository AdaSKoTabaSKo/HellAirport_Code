using UnityEngine;

namespace _GameAssets.Scripts.Scriptables
{
    [CreateAssetMenu(fileName = "GuestsPrefabs", menuName = "Guests Prefabs")]
    public class GuestsModelsSo : ScriptableObject
    {
        public GameObject[] guestsModelsPrefabs;
        public GameObject[] luggageModelsPrefabs;

        public GameObject GiveRandomGuestModel()
        {
            var ranNumber = Random.Range(0, guestsModelsPrefabs.Length);
            return guestsModelsPrefabs[ranNumber];
        }

        public GameObject GiveRandomLuggageModel()
        {
            var ranNumber = Random.Range(0, luggageModelsPrefabs.Length);
            return luggageModelsPrefabs[ranNumber];
        }
    }
}