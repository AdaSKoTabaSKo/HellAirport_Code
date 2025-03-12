using UnityEngine;

namespace _GameAssets.Scripts.Core
{
    public class SingleMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T instance;
        
        protected virtual void Awake()
        {
            if (instance == null)
                instance = GetComponent<T>();
            else
                Destroy(this);
        }
    }
}