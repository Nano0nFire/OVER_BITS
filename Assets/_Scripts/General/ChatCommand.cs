using System;
using NUnit.Framework;
using UnityEngine;
namespace CLAPlus.ClapTalk
{
    public static class ChatCommand
    {
        static string[] itemDataParameters = {"Amount","Mods","Enchants","PriAddon","SecAddon","Attributes"};
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns>実行ができたかどうかをboolで返す</returns>
        public static bool TryRunCommand(string input, out string log)
        {
            if (string.IsNullOrWhiteSpace(input) || !input.StartsWith("/"))
            {
                log = "error";
                return false;
            }

            // スペースで分割
            var parts = input.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

            switch (parts[0][1..])
            {
                case "give":
                    GiveCommand(parts[1..]);
                    log = "Give Command worked fine";
                    break;

                case "test":
                    log = "Test Command worked fine";
                    break;

                case "cid":
                    log = $"Your ClientID is {ClientGeneralManager.clientID}";
                    break;

                case "action":
                    ActionSwitchCommand(parts[1..]);
                    log = "Action Command worked fine";
                    break;

                default: // コマンドとして認識できない
                    log = "error";
                    return false;
            }


            return true;
        }

        static void GiveCommand(string[] options)
        {
            ulong clientID = options[0] == "@" ? ClientGeneralManager.clientID : ulong.Parse(options[0]);
            //{\"FirstIndex\":0,\"SecondIndex\":1,\"Amount\":0,\"Mods\":[],\"Enchants\":[],\"PriAddon\":0,\"SecAddon\":0,\"Attributes\":[]}
            Debug.Log(options);
            Debug.Log(int.Parse(options[1]) + " : " + int.Parse(options[2]));

            ItemData itemData = new()
            {
                FirstIndex = int.Parse(options[1]),
                SecondIndex = int.Parse(options[2])
            };
            int i;
            foreach (var option in options)
            {
                i = 0;
                int lastColone;
                foreach (var parameter in itemDataParameters)
                {
                    if (option.Contains(parameter))
                    {
                        lastColone = option.LastIndexOf(':');
                        if (lastColone == -1)
                            continue;

                        switch(i)
                        {
                            case 0: // Ammount
                                itemData.Amount = int.Parse(option[lastColone..]);
                                break;

                            case 1: // Mods
                                break;

                            case 2: // Enchants
                                break;

                            case 3: // PriAddon
                                itemData.PriAddon = int.Parse(option[lastColone..]);
                                break;

                            case 4: // SecAddon
                                itemData.SecAddon = int.Parse(option[lastColone..]);
                                break;

                            case 5: // Attribute
                                break;
                        }
                    }
                    else
                        i++;

                }
            }
            PlayerDataManager.Instance.AddItem(itemData, clientID, Force : true);
            Debug.Log("Command End");
        }

        static void ActionSwitchCommand(string[] options)
        {
            ClientGeneralManager.Instance.SelectedActionSkill = options[0] == "dodge" ? States.dodge : options[0] == "rush" ? States.rush : States.walk;
            Debug.Log("Command End");
        }
    }
}
