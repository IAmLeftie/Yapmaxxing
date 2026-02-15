using System;
using System.Linq;
using System.Text.RegularExpressions;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Yapmaxxing.Configs;

namespace Yapmaxxing
{
    public class YapmaxxingModSystem : ModSystem
    {
        private ICoreServerAPI sapi;

        public ModConfig config;
        private const string CONFIG_NAME = "yapmaxxing.json";

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            Mod.Logger.Notification("Hello from Yapmaxxing mod: " + api.Side);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;

            LoadConfig();
            RegisterCommands(api, "yapmaxxingreload");

            api.Event.PlayerChat += Vocalize;
        }

        private void RegisterCommands(ICoreAPI api, string commandName)
        {
            api.ChatCommands.Create(commandName)
                .RequiresPrivilege(Privilege.controlserver)
                .WithDescription("Reload the Yapmaxxing config")
                .HandleWith(HandleReloadCommand);
        }

        private int LoadConfig()
        {
            try
            {
                config = sapi.LoadModConfig<ModConfig>(CONFIG_NAME);
            }
            catch (Exception ex)
            {
                sapi.Server.LogError("Failed to load Yapmaxxing config: See error below");
                sapi.Server.LogError(ex.Message);
                return 1;
            }

            if (config == null)
            {
                sapi.Server.LogNotification("Yapmaxxing config not found, generating default");
                config = new ModConfig();
                config.InitializeDefaultsIfNeeded();
                sapi.StoreModConfig(this.config, CONFIG_NAME);
            }

            return 0;
        }

        private TextCommandResult HandleReloadCommand(TextCommandCallingArgs args)
        {
            int code = LoadConfig();
            if (code == 0)
            {
                return TextCommandResult.Success("[Yapmaxxing] Config reloaded successfully!");
            }
            else
            {
                return TextCommandResult.Error("[Yapmaxxing] An error occurred while trying to reload the config.");
            }
        }

        public void Vocalize(IServerPlayer byPlayer, int channelId, ref string message, ref string data, BoolRef consumed)
        {
            if (byPlayer != null && channelId == 0)
            {
                //First, we want to check if the config is set to ignore messages entirely surrounded in at least
                //one set of parentheses -- for cases like OOC when using The Basics RP Proximity Chat
                if (config.IgnoreParentheses)
                {
                    if (message.Split(">").Last().Trim().StartsWith('(') && message.Split(">").Last().Trim().EndsWith(')')) return;
                }
                //Next we want to check for special context clues, such as greetings, laughter, etc.
                //as defined by the server in the mod config

                //Split the message into its individual words
                string _message = Regex.Replace(message.Split(">").Last().Trim(), @"[^\w\s]+", "");
                string[] sentence = _message.Split(" ");
                int wordCount = sentence.Length;

                //Do our context checks and if a match is found then use a contextual variation
                foreach (string _word in sentence)
                {
                    string word = _word.ToLower();
                    foreach (string keyword in config.GreetingKeywords)
                    {
                        if (word == keyword)
                        {
                            byPlayer.Entity.talkUtil.Talk(EnumTalkType.Meet);
                            return;
                        }
                    }
                    foreach (string keyword in config.GoodbyeKeywords)
                    {
                        if (word == keyword)
                        {
                            byPlayer.Entity.talkUtil.Talk(EnumTalkType.Goodbye);
                            return;
                        }
                    }
                    foreach (string keyword in config.LaughKeywords)
                    {
                        if (word == keyword)
                        {
                            byPlayer.Entity.talkUtil.Talk(EnumTalkType.Laugh);
                            return;
                        }
                    }
                    foreach (string keyword in config.HurtKeywords)
                    {
                        if (word == keyword)
                        {
                            byPlayer.Entity.talkUtil.Talk(EnumTalkType.Hurt);
                            return;
                        }
                    }
                }

                //If we didn't hit anything, just do a normal vocalization based on word count
                if (wordCount <= 5)
                {
                    byPlayer.Entity.talkUtil.Talk(EnumTalkType.IdleShort);
                    return;
                }
                else
                {
                    byPlayer.Entity.talkUtil.Talk(EnumTalkType.Idle);
                    return;
                }
            }
        }
    }
}
