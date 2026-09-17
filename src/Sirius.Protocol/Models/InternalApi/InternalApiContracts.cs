namespace Sirius.Protocol.InternalApi;

public sealed class RealtimeAuthenticationRequest
{
    public string BearerToken { get; set; } = string.Empty;
}

public sealed class RealtimeAuthenticationReply
{
    public bool IsAuthenticated { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public long AccountId { get; set; }
    public long PublicUserId { get; set; }
    public string UserKey { get; set; } = string.Empty;
    public string HashUserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int PlayerRank { get; set; }
    public long? CircleId { get; set; }
}

public sealed class CircleMembershipRequest
{
    public long AccountId { get; set; }
    public long? ExpectedCircleId { get; set; }
}

public sealed class CircleMembershipReply
{
    public long? CircleId { get; set; }
    public bool IsExpectedCircleMember { get; set; }
}

public sealed class CircleActivityLogRequest
{
    public long AccountId { get; set; }
    public long? ExpectedCircleId { get; set; }
    public int Limit { get; set; }
}

public sealed class CircleActivityLogReply
{
    public CircleActivityLogEntry[] Logs { get; set; } = [];
}

public sealed class CircleActivityLogEntry
{
    public long ActivityLogId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public long MainMCharacterId { get; set; }
    public bool DisplayAwakeningStatus { get; set; }
    public int LogType { get; set; }
    public string LogValue { get; set; } = string.Empty;
    public long LoggedAtUnixTimeMilliseconds { get; set; }
}

public sealed class MultiLiveIdentityRequest
{
    public long AccountId { get; set; }
}

public sealed class MultiLiveDifficultyPermissionRequest
{
    public long AccountId { get; set; }
    public long MusicMasterId { get; set; }
    public int Difficulty { get; set; }
}

public sealed class MultiLiveDifficultyPermissionReply
{
    public bool IsAllowed { get; set; }
}

public sealed class ResolveAcceptedFriendsRequest
{
    public long AccountId { get; set; }
    public string[] UserIds { get; set; } = [];
}

public sealed class MultiLiveIdentityReply
{
    public bool Found { get; set; }
    public long AccountId { get; set; }
    public string HashUserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int PlayerRank { get; set; }
    public long MainCharacterMasterId { get; set; }
    public int MainCharacterTalentStage { get; set; }
    public bool MainCharacterDisplayAwakeningStatus { get; set; }
    public long? NameColorMasterId { get; set; }
    public long? NameplateMasterId { get; set; }
    public long? TrophyMasterId1 { get; set; }
    public long? TrophyMasterId2 { get; set; }
    public long? TrophyMasterId3 { get; set; }
    public long IconFrameMasterId { get; set; }
}

public sealed class MultiLiveIdentityListReply
{
    public MultiLiveIdentityReply[] Users { get; set; } = [];
}

public sealed class UpdateLastSeenRequest
{
    public long AccountId { get; set; }
    public long LastSeenUnixTimeMilliseconds { get; set; }
}

public sealed class FriendRequestRealtimeNotificationRequest
{
    public long RecipientAccountId { get; set; }
    public string RequesterPublicUserId { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
}

public sealed class CircleJoinRealtimeNotificationRequest
{
    public long RecipientAccountId { get; set; }
}

public sealed class RealtimeNotificationDeliveryReply
{
    public int DeliveredConnectionCount { get; set; }
}

public sealed class RealtimePresenceRequest
{
    public long[] AccountIds { get; set; } = [];
}

public sealed class RealtimePresenceReply
{
    public long[] OnlineAccountIds { get; set; } = [];
}

public sealed class RealtimeAccountRevocationRequest
{
    public long AccountId { get; set; }
}

public sealed class RealtimeMultiLiveSessionRequest
{
    public long MultiLiveId { get; set; }
    public long AccountId { get; set; }
}

public sealed class RealtimeMultiLiveSessionReply
{
    public bool Found { get; set; }
    public long MultiLiveId { get; set; }
    public string HallId { get; set; } = string.Empty;
    public int MultiLiveType { get; set; }
    public int HallType { get; set; }
    public long LiveSettingMasterId { get; set; }
    public long SelectedMusicId { get; set; }
    public int Status { get; set; }
    public int ExpectedResultCount { get; set; }
    public int MemberId { get; set; }
    public int? MemberDifficulty { get; set; }
    public int? MemberClearLamp { get; set; }
    public int? TeamChallengeDifficulty { get; set; }
    public long? TeamChallengeCharacterId { get; set; }
    public long? TeamChallengePartyId { get; set; }
    public int[] SubmittedClearLamps { get; set; } = [];
}

public sealed class RealtimeMultiLivePartyBindingRequest
{
    public long MultiLiveId { get; set; }
    public long AccountId { get; set; }
    public long PartyId { get; set; }
}

public sealed class RealtimeMultiLivePartyBindingReply
{
    public bool Succeeded { get; set; }
}

public sealed class EmptyInternalApiReply
{
}
