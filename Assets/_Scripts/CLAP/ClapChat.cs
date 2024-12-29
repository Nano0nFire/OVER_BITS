using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CLAPlus
{
    public class ClapChat : NetworkBehaviour
    {
        [HideInInspector] public string PlayerName;
        [HideInInspector] public ClientGeneralManager cgManager;
        public GameObject canvas;
        [SerializeField] Transform Content;
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] InputField inputField;
        [SerializeField] GameObject TextPrefab;
        public int MaxMessageAmount; // -1で無限に保存する
        int messageCount = 0;

        public void ShowChatSpace(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                canvas.SetActive(true);
                cgManager.UseInput = false;
                cgManager.CleatInput();
            }
        }

        public void SendMessage()
        {
            if (inputField.text == "")
            {
                canvas.SetActive(false);
                cgManager.UseInput = true;
                return;
            }

            SendMessageServerRpc(PlayerName, inputField.text);

            if (messageCount <= MaxMessageAmount)
            {
                var obj = Instantiate(TextPrefab, Content);
                obj.GetComponentInChildren<TextMeshProUGUI>().text = $"{PlayerName} : {inputField.text}";
                messageCount ++;
            }
            else
            {
                Transform obj = Content.GetChild(0); // 親の3番目の子を取得
                obj.SetSiblingIndex(Content.childCount - 1); // 最初の位置に移動
                obj.GetComponentInChildren<TextMeshProUGUI>().text = $"{PlayerName} : {inputField.text}";
            }

            ScrollToBottom(); // スクロールビューの最下を表示させる
        }

        public void SendMessage(string text, string playerName)
        {
            if (messageCount <= MaxMessageAmount)
            {
                var obj = Instantiate(TextPrefab, Content);
                obj.GetComponentInChildren<TextMeshProUGUI>().text = $"{playerName} : {text}";
            }
            else
            {
                Transform obj = Content.GetChild(0); // 親の3番目の子を取得
                obj.SetSiblingIndex(Content.childCount - 1); // 最初の位置に移動
                obj.GetComponentInChildren<TextMeshProUGUI>().text = $"{playerName} : {text}";
            }

            ScrollToBottom(); // スクロールビューの最下を表示させる
            inputField.text = "";
        }

        public void ScrollToBottom()
        {
            Canvas.ForceUpdateCanvases(); // レイアウトを更新して位置を確定
            scrollRect.verticalNormalizedPosition = 0f; // 一番下に設定
        }

        public void Clear()
        {
            inputField.text = "";
        }

        [ServerRpc(RequireOwnership = false)]
        void SendMessageServerRpc(string playerName, string text)
        {
            if (IsServer)
                SendMessageClientRpc(playerName, text);

            Debug.Log("server rpc");
        }

        [ClientRpc]
        void SendMessageClientRpc(string playerName, string text)
        {
            if (playerName == PlayerName) // 自分のメッセージは処理しない
                return;

            SendMessage(text, playerName);
            Debug.Log("cluent rpc");
        }
    }
}