using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace Calamity.CalamityCode.Pact;

/// <summary>"I'll take the Pact for this dead player." Broadcast; only the dead player's game acts on it.</summary>
public sealed class PactVolunteerMessage : ICustomMessage
{
    public ulong deadPlayerId;

    public bool ShouldBroadcast => true;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteULong(deadPlayerId);
    }

    public void Deserialize(PacketReader reader)
    {
        deadPlayerId = reader.ReadULong();
    }

    public void HandleMessage(ulong senderId)
    {
        PactRitual.OnVolunteer(deadPlayerId, senderId);
    }
}
