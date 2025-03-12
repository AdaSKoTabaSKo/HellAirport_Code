using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _GameAssets.Scripts.InteractionObjects
{
    public class InteractionObject : MonoBehaviour
    {
        public virtual async UniTask ManageGuest()
        {
            
        }

        public virtual bool AddNewGuest(Guest.GuestSystem g)
        {
            return true;
        }
        
        public virtual bool CanAddGuest()
        {
            return false;
        }
    }
}