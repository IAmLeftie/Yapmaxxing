using ProtoBuf;

namespace Yapmaxxing.Configs
{
    [ProtoContract]
    public class ModConfig
    {
        [ProtoMember(1)]
        public bool IgnoreParentheses { get; set; } = false;

        [ProtoMember(2)]
        public string[] GreetingKeywords { get; set; }

        [ProtoMember(3)]
        public string[] GoodbyeKeywords { get; set; }

        [ProtoMember(4)]
        public string[] LaughKeywords { get; set; }

        [ProtoMember(5)]
        public string[] HurtKeywords { get; set; }

        [ProtoAfterDeserialization]
        private void OnDeserialized()
        {
            // If arrays are null after deserialization, initialize them with defaults
            InitializeDefaultsIfNeeded();
        }

        // Helper method to initialize default values only if not already set
        public void InitializeDefaultsIfNeeded()
        {
            GreetingKeywords ??= ["hi", "hello", "greetings", "salutations", "hewwo", "hallo", "aloha"];
            GoodbyeKeywords ??= ["goodbye", "bye", "byebye", "adios", "sayonara", "adieu"];
            LaughKeywords ??= ["lol", "lmao", "lmfao", "haha", "hehe"];
            HurtKeywords ??= ["oof", "ow", "oww", "ouch", "owwie", "ouchie", "ouchies"];
        }
    }
}
