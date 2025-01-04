using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Vivox;
using Unity.Services.Authentication;

namespace CLAPlus.ClapChat
{
    public class ClapChat : NetworkBehaviour
    {
        [SerializeField] Transform Content;
        [SerializeField] GameObject TextPrefab;
        public int MaxMessageAmount; // -1で無限に保存する
        static int messageCount = 0;

        // シングルトンインスタンス
        private static ClapChat instance;

        // インスタンスへのプロパティ
        public static ClapChat Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<ClapChat>();

                    if (instance == null)
                    {
                        GameObject singletonObject = new(typeof(ClapChat).Name);
                        instance = singletonObject.AddComponent<ClapChat>();
                    }
                }
                return instance;
            }
        }

        // シングルトンの初期化
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject); // 重複するインスタンスを破棄
            }

            if (!IsServer)
                return;

            VivoxService.Instance.ParticipantAddedToChannel += (VivoxParticipant participant) => AddMessageToChat($"{participant.DisplayName} just joined !!", "", Color.black);
            VivoxService.Instance.ParticipantRemovedFromChannel += (VivoxParticipant participant) => AddMessageToChat($"{participant.DisplayName} has left.", "", Color.black);

            VivoxService.Instance.ChannelMessageReceived += OnMessageReceived;
        }

        private void OnDestroy()
        {
            VivoxService.Instance.ParticipantAddedToChannel -= (VivoxParticipant participant) => AddMessageToChat($"{participant.DisplayName} just joined !!", "", Color.black);
            VivoxService.Instance.ParticipantRemovedFromChannel -= (VivoxParticipant participant) => AddMessageToChat($"{participant.DisplayName} has left.", "", Color.black);

            VivoxService.Instance.ChannelMessageReceived -= OnMessageReceived;
        }

        private void OnMessageReceived(VivoxMessage participant)
        {
            Debug.Log($"Message received from {participant.SenderDisplayName}: {participant.MessageText}");
            AddMessageToChat($"{participant.SenderDisplayName} : {participant.MessageText}");
        }

        public void SendMessageToChannel(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            VivoxService.Instance.SendChannelTextMessageAsync(ClapTalk.ClapTalk.JoinnedChannelName, text);
            AddMessageToChat(text, AuthenticationService.Instance.PlayerName);
        }

        public void AddMessageToChat(string text, string Sender = "", Color color = default)
        {
            Transform obj;
            if (messageCount != -1 && messageCount <= MaxMessageAmount)
            {
                obj = Instantiate(TextPrefab, Content).transform;
                messageCount++;
            }
            else
            {
                obj = Content.GetChild(0); // 親の3番目の子を取得
                obj.SetSiblingIndex(Content.childCount - 1); // 最初の位置に移動
            }

            var component = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (color != default)
                component.color = color;
            component.text = Sender == "" ? $"{text}" : $"{Sender} : {text}";
        }

        // public void ShowChatSpace(InputAction.CallbackContext context)
        // {
        //     if (context.performed)
        //     {
        //         canvas.SetActive(true);
        //         cgManager.UseInput = false;
        //         cgManager.CleatInput();
        //     }
        // }

        // public void SendMessage()
        // {
        //     if (inputField.text == "")
        //     {
        //         canvas.SetActive(false);
        //         cgManager.UseInput = true;
        //         return;
        //     }

        //     SendMessageServerRpc(PlayerName, inputField.text);

        //     if (messageCount <= MaxMessageAmount)
        //     {
        //         var obj = Instantiate(TextPrefab, Content);
        //         obj.GetComponentInChildren<TextMeshProUGUI>().text = $"{PlayerName} : {inputField.text}";
        //         messageCount ++;
        //     }
        //     else
        //     {
        //         Transform obj = Content.GetChild(0); // 親の3番目の子を取得
        //         obj.SetSiblingIndex(Content.childCount - 1); // 最初の位置に移動
        //         obj.GetComponentInChildren<TextMeshProUGUI>().text = $"{PlayerName} : {inputField.text}";
        //     }

        //     ScrollToBottom(); // スクロールビューの最下を表示させる

        //     canvas.SetActive(false);
        //     cgManager.UseInput = true;
        // }
    }
}