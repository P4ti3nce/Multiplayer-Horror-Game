using UnityEngine;
using System.Collections.Generic;
namespace SteamLobbyNamespace
{
    public class PanelSwapper : MonoBehaviour
    {
        public List<Panel> panels = new List<Panel>(); 

        public void SwapPanel(string panelName)
        {
            foreach(Panel panel in panels)
            {
                if(panel.Panelname == panelName)
                {
                    panel.gameObject.SetActive(true);
                }
                else
                {
                    panel.gameObject.SetActive(false);
                }
            }
                
        }


    }
}

