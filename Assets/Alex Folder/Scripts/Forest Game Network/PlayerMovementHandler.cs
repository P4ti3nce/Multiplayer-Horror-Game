using System.Collections.Generic;
using UnityEngine;
using Mirror;


namespace SteamLobbyNamespace
{
    public class PlayerMovementHandler : NetworkBehaviour
    {
        private void Update()
        {
            if (isLocalPlayer)
            {
                float h = Input.GetAxis("Horizontal");
                float v = Input.GetAxis("Vertical");
                Vector3 playerMovement = new Vector3 (h * 0.25f, v * 0.25f);
                transform.position = transform.position + playerMovement;
            }
        }
    }
}


