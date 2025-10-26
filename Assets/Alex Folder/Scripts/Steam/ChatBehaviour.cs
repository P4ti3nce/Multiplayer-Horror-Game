using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using TMPro;
using Steamworks;

namespace SteamLobbyNamespace
{
    public class ChatBehaviour : NetworkBehaviour
    {
        #region ChatStuff
        [Header("UI References")]
        [SerializeField] private GameObject chatUI = null;
        [SerializeField] private TMP_Text chatText = null;
        [SerializeField] private TMP_InputField inputField = null;

        [Header("Settings")]
        [SerializeField] private int maxVisibleMessages = 12; // max messages to display

        private readonly SyncList<string> chatMessages = new SyncList<string>();

        private static event Action<string> OnMessage;
        #endregion

        #region UI Setup
        public void SetUIReferences(GameObject ui, TMP_Text text, TMP_InputField input)
        {
            chatUI = ui;
            chatText = text;
            inputField = input;
        }

        public override void OnStartAuthority()
        {
            // Assign UI references from the Lobby UI Manager
            if (LobbyUiManager.Instance != null)
            {
                LobbyUiManager.Instance.AssignChatToPlayer(this);
            }
            else
            {
                // Fallback: find objects by name
                if (chatUI == null) chatUI = GameObject.Find("Chat_UI");
                if (chatText == null)
                {
                    var chatTextGO = GameObject.Find("Chat_Text");
                    if (chatTextGO != null) chatText = chatTextGO.GetComponent<TMP_Text>();
                }
                if (inputField == null)
                {
                    var inputGO = GameObject.Find("Chat_Input");
                    if (inputGO != null) inputField = inputGO.GetComponent<TMP_InputField>();
                }
            }

            if (chatUI != null) chatUI.SetActive(true);

            // Hook up the SyncList callback for networked messages
            chatMessages.Callback += OnChatMessagesChanged;

            // Subscribe local event for backwards compatibility
            OnMessage += HandleNewMessage;
        }

        [ClientCallback]
        private void OnDestroy()
        {
            chatMessages.Callback -= OnChatMessagesChanged;
            OnMessage -= HandleNewMessage;
        }
        #endregion

        #region Message Handling
        private void OnChatMessagesChanged(SyncList<string>.Operation op, int index, string oldItem, string newItem)
        {
            // Rebuild the chat whenever the list changes
            UpdateChatText();
        }

        private void UpdateChatText()
        {
            chatText.text = "";

            // Determine starting index for maxVisibleMessages
            int start = Mathf.Max(0, chatMessages.Count - maxVisibleMessages);

            // Loop through messages from start to end
            for (int i = start; i < chatMessages.Count; i++)
            {
                chatText.text += chatMessages[i] + "\n";
            }
        }

        private void HandleNewMessage(string message)
        {
            UpdateChatText();
        }
        #endregion

        #region Sending Messages
        [Client]
        public void Send(string message)
        {
            // Only send if Enter is pressed while focused
            if (!Input.GetKeyDown(KeyCode.Return)) return;
            if (string.IsNullOrWhiteSpace(message)) return;

            string playerName = SteamFriends.GetPersonaName();

            // Send to server
            CmdSendMessage(playerName, inputField.text);

            // Clear input field and refocus
            inputField.text = string.Empty;
            inputField.ActivateInputField();
        }

        [Command]
        private void CmdSendMessage(string playerName, string message)
        {
            string formatted = $"[{playerName}]: {message}";

            // Add message to networked SyncList
            chatMessages.Add(formatted);

            // Trim if over max messages
            if (chatMessages.Count > maxVisibleMessages)
                chatMessages.RemoveAt(0);

            // Also call RPC for old-style event handling
            RpcHandleMessage(formatted);
        }

        [ClientRpc]
        private void RpcHandleMessage(string message)
        {
            OnMessage?.Invoke(message);
        }
        #endregion
    }
}
