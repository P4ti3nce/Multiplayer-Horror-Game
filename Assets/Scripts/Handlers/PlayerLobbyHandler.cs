using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/
namespace SteamLobbyNamespace
{
    public class PlayerLobbyHandler : NetworkBehaviour
    {

        [SyncVar(hook = nameof(OnReadyStatusChanged))]
        public bool isReaady = false;
        public Button readyButton;
        public TextMeshProUGUI nameText;

        private void Start()
        {
            readyButton.interactable = isLocalPlayer;
        }
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            readyButton.interactable = true;
            isReaady = false;
        }
        public override void OnStartClient()
        {
            base.OnStartClient();
            LobbyUiManager.Instance.RegisterPlayer(this);
        }
        [Command]
        void CmdSetReady()
        {
            isReaady = !isReaady;
            OnReadyStatusChanged(!isReaady,isReaady);
        }
        public void OnReadyButtonClicked()
        {
            CmdSetReady();
        }
        void SetSelectedButtonColor(Color color)
        {
            ColorBlock cb = readyButton.colors;
            cb.normalColor = color;
            cb.selectedColor = color;
            cb.disabledColor = color;
            readyButton.colors = cb;
        }
        void OnReadyStatusChanged(bool oldValue, bool newValue)
        {
            if (NetworkServer.active)
            {
                LobbyUiManager.Instance.CheckAllPlayersReady();
            }
            if (isReaady)
            {
                SetSelectedButtonColor(Color.green);

            }
            else
            {
                SetSelectedButtonColor(Color.white);
            }
        }

    }
}

