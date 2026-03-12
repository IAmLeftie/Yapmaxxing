using ProtoBuf;

namespace Yapmaxxing.Configs
{
    [ProtoContract]
    public class ModConfigClient
    {
        [ProtoMember(1)]
        public bool DisableGlobally { get; set; } = false;
    }
}
