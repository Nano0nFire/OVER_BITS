using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.Services.Vivox;

namespace CLAPlus.ClapChat
{
    public class UI_ClapChat : NetworkBehaviour
    {
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] InputField inputField;
        static InputField _inputField;
        static RectTransform _scrollRect;

        void Awake()
        {
            _scrollRect = scrollRect.GetComponent<RectTransform>();
            _inputField = inputField;
        }
        public void ShowChatSpace(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                inputField.gameObject.SetActive(true);
                ClientGeneralManager.Instance.UseInput = false;
                ClientGeneralManager.Instance.ClearInput();
                _scrollRect.sizeDelta = new Vector2(_scrollRect.sizeDelta.x, 700); // チャットスペースを広げる
            }
        }

        public static void CloseChatSpace()
        {
            _inputField.gameObject.SetActive(false);
            _scrollRect.sizeDelta = new Vector2(_scrollRect.sizeDelta.x, 300); // デフォルトの大きさに戻す
        }

        public void SendTextMessage(string text)
        {
            ClapChat.Instance.SendMessageToChannel(text);
            ScrollToBottom();

            CloseChatSpace();
            ClientGeneralManager.Instance.UseInput = true;
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
    }
}