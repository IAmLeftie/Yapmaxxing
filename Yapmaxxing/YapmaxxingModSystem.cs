using System;
using System.Linq;
using System.Text.RegularExpressions;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace Yapmaxxing
{
    public class YapmaxxingModSystem : ModSystem
    {

        private ICoreClientAPI capi;
        private ICoreServerAPI sapi;

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            Mod.Logger.Notification("Hello from Yapmaxxing mod: " + api.Side);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;
            api.Event.PlayerChat += Vocalize;
        }

        public void Vocalize(IServerPlayer byPlayer, int channelId, ref string message, ref string data, BoolRef consumed)
        {
            if (byPlayer != null && channelId == 0)
            {
                //First we want to check for special context clues, such as greetings, laughter, etc.
                //Some words are cut short because we can use them as wildcards for variations
                //("chuckl" can match to chuckle, chuckling, AND chuckled)
                string[] greetingKeywords = ["hi", "hello", "greetings", "salutations", "hewwo", "hallo", "aloha"];
                string[] goodbyeKeywords = ["bye", "adios", "sayonara", "adieu"];
                string[] laughKeywords = ["lol", "lmao", "lmfao", "ha"];
                string[] hurtKeywords = ["oof", "ow", "oww", "ouch", "owwie", "ouchie", "ouchies"];

                //Split the message into its individual words
                string _message = Regex.Replace(message.Split(">").Last().Trim(), @"[^\w\s]+", "");
                string[] sentence = _message.Split(" ");
                int wordCount = sentence.Length;

                //Do our context checks and if a match is found then use a contextual variation
                foreach (string _word in sentence)
                {
                    string word = _word.ToLower();
                    foreach (string keyword in greetingKeywords)
                    {
                        if (word.Contains(keyword))
                        {
                            byPlayer.Entity.talkUtil.Talk(EnumTalkType.Meet);
                            return;
                        }
                    }
                    foreach (string keyword in goodbyeKeywords)
                    {
                        if (word.Contains(keyword))
                        {
                            byPlayer.Entity.talkUtil.Talk(EnumTalkType.Goodbye);
                            return;
                        }
                    }
                    foreach (string keyword in laughKeywords)
                    {
                        if (word.Contains(keyword))
                        {
                            byPlayer.Entity.talkUtil.Talk(EnumTalkType.Laugh);
                            return;
                        }
                    }
                    foreach (string keyword in hurtKeywords)
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
