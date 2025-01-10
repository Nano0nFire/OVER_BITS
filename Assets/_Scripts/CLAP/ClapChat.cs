using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Vivox;

namespace CLAPlus.ClapChat
{
    public class ClapChat : NetworkBehaviour
    {
        [SerializeField] Transform Content;
        [SerializeField] GameObject TextPrefab;
        public static int MaxMessageAmount = 20; // 0無限に保存する
        static int messageCount = 0;
        static string TextChannelName = "OpenTextChat";

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

            VivoxService.Instance.ParticipantAddedToChannel += (VivoxParticipant participant) => AddMessageToChat($"{participant.DisplayName} just joined !!", "", Color.black);
            VivoxService.Instance.ParticipantRemovedFromChannel += (VivoxParticipant participant) => AddMessageToChat($"{participant.DisplayName} has left.", "", Color.black);

            VivoxService.Instance.ChannelMessageReceived += OnMessageReceived;
        }

        public static void Setup()
        {
            VivoxService.Instance.JoinGroupChannelAsync(TextChannelName, ChatCapability.TextOnly);
        }

        private void OnDestroy()
        {
            VivoxService.Instance.ParticipantAddedToChannel -= (VivoxParticipant participant) => AddMessageToChat($"{participant.DisplayName} just joined !!", "", Color.black);
            VivoxService.Instance.ParticipantRemovedFromChannel -= (VivoxParticipant participant) => AddMessageToChat($"{participant.DisplayName} has left.", "", Color.black);

            VivoxService.Instance.ChannelMessageReceived -= OnMessageReceived;
        }

        public void OnMessageReceived(VivoxMessage participant)
        {
            AddMessageToChat($"{participant.SenderDisplayName} : {participant.MessageText}");
            UI_ClapChat.ScrollToBottom();
        }

        public void SendMessageToChannel(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            VivoxService.Instance.SendChannelTextMessageAsync(TextChannelName, text);
            // string name = AuthenticationService.Instance.PlayerName;
            // var lastDot = name.LastIndexOf('#');
            // if (lastDot != -1)
            //     name = name[..lastDot]; // #以降を消す
            // AddMessageToChat(text, name);
        }

        public void AddMessageToChat(string text, string Sender = "", Color color = default)
        {
            Transform obj;
            if (messageCount == 0 || messageCount <= MaxMessageAmount)
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
            component.color = color == default ? Color.white : color;
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