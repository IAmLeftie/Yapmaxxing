using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Vintagestory.API.Client;
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
        //private ICoreClientAPI capi;
        private List<string> playerIdsToExclude;
        private List<string> playerIds;

        public ModConfig config;
        //public ModConfigClient configClient;

        private const string CONFIG_NAME = "yapmaxxing.json";
        //private const string CONFIG_NAME_CLIENT = "yapmaxxing_client.json";

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        //public override void Start(ICoreAPI api)
        //{
        //    Mod.Logger.Notification("Hello from Yapmaxxing mod: " + api.Side);
        //}

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;
            playerIds = new List<string>();
            playerIdsToExclude = new List<string>();

            LoadConfig();
            RegisterCommands(api, "yapmaxxing");

            api.Event.PlayerChat += Vocalize;
            api.Event.SaveGameLoaded += OnSaveGameLoading;
            api.Event.GameWorldSave += OnSaveGameSaving;
        }

        //public override void StartClientSide(ICoreClientAPI api)
        //{
        //    capi = api;
        //    LoadConfigClient();
        //    RegisterCommandsClient(api, "yapmaxxing");
        //}

        private void RegisterCommands(ICoreAPI api, string commandName)
        {
            api.ChatCommands.Create(commandName).WithDescription("Manage Yapmaxxing settings").RequiresPrivilege(Privilege.chat)
                .BeginSubCommand("reload")
                .RequiresPrivilege(Privilege.controlserver)
                .WithDescription("Reload the Yapmaxxing config")
                .HandleWith(HandleReloadCommand)
                .EndSubCommand()
                .BeginSubCommand("muteself")
                .WithDescription("Toggle your own vocalizations")
                .HandleWith(HandleToggleVocalizeCommand)
                .EndSubCommand();
        }

        //private void RegisterCommandsClient(ICoreAPI api, string commandName)
        //{
        //    api.ChatCommands.Create(commandName).WithDescription("Manage Yapmaxxing settings").RequiresPrivilege(Privilege.chat)
        //        .BeginSubCommand("muteall")
        //        .WithDescription("Toggle all vocalizations for yourself")
        //        .HandleWith(HandleGlobalToggleCommand)
        //        .EndSubCommand();
        //}

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

        //private int LoadConfigClient()
        //{
        //    try
        //    {
        //        configClient = capi.LoadModConfig<ModConfigClient>(CONFIG_NAME_CLIENT);
        //    }
        //    catch (Exception ex)
        //    {
        //        capi.Logger.Error("Failed to load Yapmaxxing client config: See error below");
        //        capi.Logger.Error(ex.Message);
        //        return 1;
        //    }

        //    if (configClient == null)
        //    {
        //        capi.Logger.Notification("Yapmaxxing config not found, generating default");
        //        configClient = new ModConfigClient();
        //        capi.StoreModConfig(this.configClient, CONFIG_NAME_CLIENT);
        //    }

        //    return 0;
        //}

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

        private TextCommandResult HandleToggleVocalizeCommand(TextCommandCallingArgs args)
        {
            if (!playerIds.Contains(args.Caller.Player.PlayerUID))
            {
                if (!playerIdsToExclude.Contains(args.Caller.Player.PlayerUID))
                {
                    playerIds.Add(args.Caller.Player.PlayerUID);
                    playerIdsToExclude.Add(args.Caller.Player.PlayerUID);
                    return TextCommandResult.Success("[Yapmaxxing] You will no longer vocalize when speaking. Run the command again to re-enable.");
                }
                else
                {
                    playerIds.Add(args.Caller.Player.PlayerUID);
                }
            }
            if (playerIds.Contains(args.Caller.Player.PlayerUID) && playerIdsToExclude.Contains(args.Caller.Player.PlayerUID))
            {
                playerIdsToExclude.Remove(args.Caller.Player.PlayerUID);
                return TextCommandResult.Success("[Yapmaxxing] You will now vocalize when speaking. Run the command again to disable.");
            }
            else if (playerIds.Contains(args.Caller.Player.PlayerUID) && !playerIdsToExclude.Contains(args.Caller.Player.PlayerUID))
            {
                playerIdsToExclude.Add(args.Caller.Player.PlayerUID);
                return TextCommandResult.Success("[Yapmaxxing] You will no longer vocalize when speaking. Run the command again to re-enable.");
            }
            else
            {
                return TextCommandResult.Error("[Yapmaxxing] Encountered an error. Please try again.");
            }
        }

        //private TextCommandResult HandleGlobalToggleCommand(TextCommandCallingArgs args)
        //{
        //    switch (configClient.DisableGlobally)
        //    {
        //        case false:
        //            configClient.DisableGlobally = true;
        //            return TextCommandResult.Success("[Yapmaxxing] Vocalizations disabled globally for yourself. Run the command again to re-enable.");
        //        case true:
        //            configClient.DisableGlobally = false;
        //            return TextCommandResult.Success("[Yapmaxxing] Vocalizations enabled globally for yourself. Run the command again to disable.");
        //    }
        //}

        public void Vocalize(IServerPlayer byPlayer, int channelId, ref string message, ref string data, BoolRef consumed)
        {
            if (byPlayer != null && channelId == 0)
            {
                //If vocalizations are disabled globally, exit early.
                //if (configClient != null && configClient.DisableGlobally) return;
                //If the player is set to not vocalize, exit early.
                if (playerIdsToExclude != null && playerIds != null && playerIds.Contains(byPlayer.PlayerUID) && playerIdsToExclude.Contains(byPlayer.PlayerUID))
                {
                    return;
                }
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

        private void OnSaveGameLoading()
        {
            try
            {
                byte[] data = sapi.WorldManager.SaveGame.GetData("playerIds");
                playerIds = SerializerUtil.Deserialize<List<string>>(data);
                byte[] data2 = sapi.WorldManager.SaveGame.GetData("playerIdsToExclude");
                playerIdsToExclude = SerializerUtil.Deserialize<List<string>>(data);
            }
            catch (Exception ex)
            {
                sapi.Logger.Error("Yapmaxxing: Error loading save data: " + ex.Message);
            }
        }

        private void OnSaveGameSaving()
        {
            try
            {
                sapi.WorldManager.SaveGame.StoreData("playerIds", SerializerUtil.Serialize<List<string>>(playerIds));
                sapi.WorldManager.SaveGame.StoreData("playerIdsToExclude", SerializerUtil.Serialize<List<string>>(playerIdsToExclude));
            }
            catch (Exception ex)
            {
                sapi.Logger.Error("Yapmaxxing: Error saving data: " + ex.Message);
            }
        }
    }
}
