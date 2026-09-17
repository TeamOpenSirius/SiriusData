using MessagePack;

namespace Sirius.Protocol.Shared;

[MessagePackObject(false)]
public partial class PartyInfo
{
    [Key(0)] public int LeaderPosition { get; set; }
    [Key(1)] public MultiRoomPartySlot[] MultiRoomPartySlots { get; set; } = Array.Empty<MultiRoomPartySlot>();
}
