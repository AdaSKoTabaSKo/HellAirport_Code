using System.Collections.Generic;
using System.Linq;
using _GameAssets.Scripts.Managers;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _GameAssets.Scripts.Planes
{
    public class TicketTv : MonoBehaviour
    {
        [Header("Ticket Tv Ui")]
        [SerializeField] private TextMeshProUGUI ticketPoolTmp;
        [SerializeField] private Image ticketPoolbanner;
        
        [Space][Header("Ticket Tv Ui")]
        [SerializeField] private Color canSellColor;
        [SerializeField] private Color cantSellColor;
        [SerializeField] private Sprite greenBannerSprite;
        [SerializeField] private Sprite redBannerSprite;
        
        public async UniTask RefreshTvScreenUi(List<Airplane> airplanes, bool atStart = false)
        {
            await UniTask.DelayFrame(1);
            
            var allSeats = airplanes.Where(a => a.gameObject.activeInHierarchy).Sum(a => a.AllSeatsInPlane());

            ticketPoolTmp.text = $"{AirplanesManager.instance.GiveTicketsPool()}";

            if (AirplanesManager.instance.CanSellTicket())
            {
                ticketPoolTmp.color = canSellColor;
                ticketPoolbanner.sprite = greenBannerSprite;
            }
            else
            {
                ticketPoolTmp.color = cantSellColor;
                ticketPoolbanner.sprite = redBannerSprite;
            }
        }
    }
}