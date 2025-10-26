using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Steamworks;
using Mirror.Examples.SyncDir;
using System;


namespace SteamLobbyNamespace
{
    public class LobbyUiManager : NetworkBehaviour
    {
        public static LobbyUiManager Instance;
        public Transform playerListParent;
        public List<TextMeshProUGUI> playerNameTexts = new List<TextMeshProUGUI>();
        public List<PlayerLobbyHandler> playerLobbyHandlers = new List<PlayerLobbyHandler>();
        public Button playGameButton;

        [Header("Chat UI (scene)")]
        public GameObject chatUI;                 
        public TMP_Text chatText;                 
        public TMP_InputField chatInputField;     


        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;  
            }
            else if(Instance != null)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            playGameButton.interactable = false;
            if (chatUI != null) chatUI.SetActive(false);
        }
        #region Lobby Function
        public void UpdatePlayerLobbyUI()
        {
            playerNameTexts.Clear();
            playerLobbyHandlers.Clear();

            var lobby = new CSteamID(SteamLobby.instance.lobbyID);
            int memberCount = SteamMatchmaking.GetNumLobbyMembers(lobby);

            CSteamID hostID = new CSteamID(ulong.Parse(SteamMatchmaking.GetLobbyData(lobby, "HostAddress")));
            List<CSteamID> orderedMembers = new List<CSteamID>();

            if(memberCount == 0)
            {
                Debug.LogWarning("Lobby has no members.. retyring...");
                StartCoroutine(RetryUpdate());
                return;
            }

            orderedMembers.Add(hostID);

            for(int i = 0; i<memberCount; i++)
            {

                CSteamID memberID = SteamMatchmaking.GetLobbyMemberByIndex(lobby, i);   
                if(memberID != hostID)
                {
                    orderedMembers.Add(memberID);
                }
            }
            int j = 0;
            foreach(var member in orderedMembers)
            {
                TextMeshProUGUI txtMesh = playerListParent.GetChild(j).GetChild(0).GetComponent<TextMeshProUGUI>();
                PlayerLobbyHandler playerLobbyHander = playerListParent.GetChild(j).GetComponent<PlayerLobbyHandler>();

                playerLobbyHandlers.Add(playerLobbyHander);
                playerNameTexts.Add(txtMesh);

                string playerName = SteamFriends.GetFriendPersonaName(member);
                Debug.Log(playerName);
                playerNameTexts[j].text = playerName;
                j++;
            }
        }

        public void OnPlayButtonClicked()
        {
            if (NetworkServer.active)
            {
                CustomNetworkManager.singleton.ServerChangeScene("HorrorForest");
            }
        }
        public void AssignChatToPlayer(ChatBehaviour playerChat)
        {
            if (playerChat == null) return;

            playerChat.SetUIReferences(chatUI, chatText, chatInputField);

            // Clear previous listeners so you don’t double-register
            chatInputField.onEndEdit.RemoveAllListeners();

            // Add runtime event binding
            chatInputField.onEndEdit.AddListener((string text) =>
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    playerChat.Send(text);
                }
            });

            // Ensure input is single-line for "Enter to submit"
            chatInputField.lineType = TMPro.TMP_InputField.LineType.SingleLine;
        }

        public void RegisterPlayer(PlayerLobbyHandler player)
        {
            player.transform.SetParent(playerListParent, false);
            UpdatePlayerLobbyUI();
        }

        [Server]
        public void CheckAllPlayersReady()
        {
            foreach(var player in playerLobbyHandlers)
            {
                if (!player.isReaady)
                {
                    RpcSetPlayButtonInteractable(false);
                    return;

                }
            }
            RpcSetPlayButtonInteractable(true);
        }

        [ClientRpc]
        void RpcSetPlayButtonInteractable(bool truthStatus)
        {
            playGameButton.interactable = truthStatus;
        }

        private IEnumerator RetryUpdate()
        {
            yield return new WaitForSeconds(1f);
            UpdatePlayerLobbyUI();
        }
        public string MySteamName(string name)
        {
            return name;
        }
        #endregion
        
    }
}

