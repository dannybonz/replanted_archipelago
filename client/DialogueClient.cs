using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;

namespace ReplantedArchipelago
{
    class GameMeta
    {
        public List<string> Strings { get; set; }
        public bool MatureGame { get; set; }
    }

    class Dialogue
    {
        public string CrazyDave { get; set; }
        public string AwardScreen { get; set; }
    }

    class DialogueFile
    {
        public string GameName { get; set; }
        public Dictionary<string, Dialogue> Dialogue { get; set; }
    }

    public enum DialogueType
    {
        CrazyDave,
        AwardScreen,
    }

    public class DialogueClient
    {
        private const string DialogueBaseUrl = "https://pvz-ap.vicr123.com";

        private Dictionary<string, DialogueFile> GameDialogue { get; set; } = new();
        private Dictionary<string, GameMeta> GameMeta { get; set; } = new();
        private List<string> RequestedGames { get; set; } = new();
        private HttpClient HttpClient { get; set; } = new();

        public async Task DownloadRepository()
        {
            try
            {
                GameMeta = await HttpClient.GetFromJsonAsync<Dictionary<string, GameMeta>>(
                    DialogueBaseUrl + "/item-text/meta.json", CancellationToken.None);
            }
            catch (Exception e)
            {
                Main.Log("Unable to download dialogue meta");
            }
        }

        public async Task DownloadGameMeta(string gameName)
        {
            if (!GameMeta.TryGetValue(gameName, out var meta))
            {
                return;
            }
            
            if (RequestedGames.Contains(gameName))
            {
                return;
            }

            RequestedGames.Add(gameName);

            var dialogueFile = new DialogueFile
            {
                Dialogue = new(),
                GameName = gameName
            };
            foreach (var location in meta.Strings)
            {
                try
                {
                    var locationDialogueFile = await HttpClient.GetFromJsonAsync<DialogueFile>(DialogueBaseUrl + location, CancellationToken.None);
                    foreach (var dialogue in locationDialogueFile.Dialogue)
                    {
                        dialogueFile.Dialogue.Add(dialogue.Key, dialogue.Value);
                    }
                }
                catch (Exception e)
                {
                    Main.Log($"Unable to download dialogue for game {gameName} from location {location}");
                }
            }
            
            GameDialogue[gameName] = dialogueFile;
        }

        public string GetDialogue(DialogueType dialogueType, ItemInfo itemInfo, ArchipelagoSession apSession)
        {
            if (!GameDialogue.TryGetValue(itemInfo.ItemGame, out var dialogue))
            {
                return null;
            }

            if (!dialogue.Dialogue.TryGetValue(itemInfo.ItemName, out var itemDialogue))
            {
                return null;
            }

            var sourceDialogue = dialogueType switch
            {
                DialogueType.CrazyDave => itemDialogue.CrazyDave,
                DialogueType.AwardScreen => itemDialogue.AwardScreen,
                _ => throw new ArgumentOutOfRangeException(nameof(dialogueType), dialogueType, null)
            };
            if (string.IsNullOrEmpty(sourceDialogue))
            {
                return null;
            }

            sourceDialogue = sourceDialogue.Replace("{{player}}", itemInfo.Player.Alias);
            sourceDialogue = sourceDialogue.Replace("{{player_you}}",
                itemInfo.Player.Slot == apSession.Players.ActivePlayer.Slot ? "you" : itemInfo.Player.Alias);
            sourceDialogue = sourceDialogue.Replace("{{player_your}}",
                itemInfo.Player.Slot == apSession.Players.ActivePlayer.Slot ? "your" : $"{itemInfo.Player.Alias}'s");
            sourceDialogue = sourceDialogue.Replace("{{item}}", itemInfo.ItemDisplayName);
            sourceDialogue = sourceDialogue.Replace("{{game}}", itemInfo.ItemGame);
            
            return sourceDialogue;
        }
        
        public bool TryGetDialogue(DialogueType dialogueType, ItemInfo itemInfo, ArchipelagoSession apSession, out string dialogue)
        {
            dialogue = GetDialogue(dialogueType, itemInfo, apSession);
            return dialogue != null;
        }
    }
}