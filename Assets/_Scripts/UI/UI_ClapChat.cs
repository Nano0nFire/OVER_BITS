using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.Services.Vivox;
using CLAPlus.ClapTalk;

namespace CLAPlus.ClapChat
{
    public class UI_ClapChat : NetworkBehaviour
    {
        [SerializeField] GameObject ChatSpace;
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] InputField chatField;
        [SerializeField] Slider slider;
        [SerializeField] TMP_InputField maxMessageField;
        static InputField _chatField;
        static RectTransform _scrollRectTransform;
        static ScrollRect _scrollRect;
        static GameObject _ChatSpace;
        public static bool AlwaysOnDisplay = false;

        void Awake()
        {
            _scrollRectTransform = scrollRect.GetComponent<RectTransform>();
            _scrollRect = scrollRect;
            _chatField = chatField;
            _ChatSpace = ChatSpace;
        }

        public void ShowChatSpace(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (!AlwaysOnDisplay)
                    ChatSpace.SetActive(true);
                chatField.gameObject.SetActive(true);
                ClientGeneralManager.Instance.UseInput = false;
                ClientGeneralManager.Instance.ClearInput();
                _scrollRectTransform.sizeDelta = new Vector2(_scrollRectTransform.sizeDelta.x, 700); // チャットスペースを広げる
                ScrollToBottom();
            }
        }

        public static void CloseChatSpace()
        {
            _chatField.gameObject.SetActive(false);
            _scrollRectTransform.sizeDelta = new Vector2(_scrollRectTransform.sizeDelta.x, 300); // デフォルトの大きさに戻す
            ScrollToBottom();
            if (!AlwaysOnDisplay)
                _ChatSpace.SetActive(false);
        }

        public void SendTextMessage(string text)
        {
            if (ChatCommand.TryRunCommand(text, out string log)) // コマンド実行を試みる
                ClapChat.Instance.AddMessageToChat(log);
            else
                ClapChat.Instance.SendMessageToChannel(text);
            ScrollToBottom();

            CloseChatSpace();
            ClientGeneralManager.Instance.UseInput = true;
        }

        public static void ScrollToBottom()
        {
            Canvas.ForceUpdateCanvases(); // レイアウトを更新して位置を確定
            _scrollRect.verticalNormalizedPosition = 0f; // 一番下に設定
        }

        public void OnChangeShowMode(bool value)
        {
            ChatSpace.SetActive(true);
            AlwaysOnDisplay = value;
        }

        public void Clear()
        {
            chatField.text = "";
        }

        public void MaxMessageAmountSli(bool IsSlider)
        {
            if (IsSlider)
                ClapChat.MaxMessageAmount = (int)slider.value;
            else
                ClapChat.MaxMessageAmount = int.Parse(maxMessageField.text);

            if (ClapChat.MaxMessageAmount > 100)
                ClapChat.MaxMessageAmount = 100;
            if (ClapChat.MaxMessageAmount < 0)
                ClapChat.MaxMessageAmount = 0;

            maxMessageField.text = ClapChat.MaxMessageAmount.ToString();
            slider.value = ClapChat.MaxMessageAmount;
        }
    }
}