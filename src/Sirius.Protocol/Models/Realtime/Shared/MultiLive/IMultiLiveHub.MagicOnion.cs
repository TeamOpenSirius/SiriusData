using MagicOnion;
using Sirius.Protocol.Realtime.Shared;
using Sirius.Protocol.Realtime.Shared.Entity.MultiLive;
using Sirius.Protocol.Realtime.Shared.Entity.Results;
using Sirius.Protocol.Shared;

namespace Sirius.Protocol.Realtime.Shared.MultiLive;

public partial interface IMultiLiveHub : IStreamingHub<IMultiLiveHub, IMultiLiveHubReceiver>
{
    ValueTask<MultiLiveCreatePrivateHallResult> CreatePrivateHallAsync(
        MultiLiveHallType multiLiveHallType,
        long liveSettingMasterId,
        string userName,
        long userNamePlateColorId,
        MultiLiveCharacter leaderCharacter);

    ValueTask<MultiLiveJoinResult> JoinPrivateHallFromInviteAsync(
        string hallId,
        string userName,
        long userNamePlateColorId,
        MultiLiveCharacter leaderCharacter);

    ValueTask<MultiLiveJoinResult> JoinPrivateHallWithKeyCodeAsync(
        int keyCode,
        string userName,
        long userNamePlateColorId,
        MultiLiveCharacter leaderCharacter);

    ValueTask<MultiLiveJoinResult> JoinPublicHallAsync(
        long liveSettingMasterId,
        string userName,
        long userNamePlateColorId,
        MultiLiveCharacter leaderCharacter);

    ValueTask OpenPrivateRoomAsync();
    ValueTask NotifyCircleMemberAsync(long mainCharacterId, bool displayAwakeningStatus, long mIconFrameMasterId);

    ValueTask NotifyFriendsAsync(
        string[] friendUserIds,
        long mainCharacterId,
        bool displayAwakeningStatus,
        int playerRank,
        long? trophyMasterId1,
        long? trophyMasterId2,
        long? trophyMasterId3,
        long iconFrameMasterId);

    ValueTask ReadyForDecideMember(bool isReady);
    ValueTask DecideMemberAsync();
    ValueTask SelectMusicAsync(long musicId, bool isRandom, bool isAFK);
    ValueTask SelectStampAsync(long stampId);
    ValueTask SelectDifficultyAsync(MusicDifficulties difficulty, bool isAFK);
    ValueTask<MultiLiveFetchUsersResult> FetchUsersAsync();
    ValueTask ReadyGameAsync();
    ValueTask BeforeGameCalculateAsync();
    ValueTask StartGameAsync();
    ValueTask ExitGameAsync(
        long score,
        ClearLamps clearLamp,
        IReadOnlyDictionary<TimingTypes, int> timingCounts,
        int maxCombo);
    ValueTask EntryFinalResultAsync();
    ValueTask<MultiLiveJoinResult> ContinuePlayAsync();
    ValueTask SelectGoalDifficultyForTeamChallengeAsync(TeamChallengeDifficultyTypes difficulty);
    ValueTask SelectMusicDifficultyAndPartyForTeamChallengeAsync(TeamChallengeSelectDifficulyAndParty payload);
    ValueTask SyncInGameStatusAsync(int comboCount, ComboTypes comboType, int life);
}

public partial interface IMultiLiveHubReceiver
{
    void OnJoin(MultiLiveUser multiLiveUser);
    void OnLeaveAnyOne(int memberId);
    void OnCountDownDecideMember(int? currentCount);
    void OnReadyDecideMember(int memberId, bool isReady);
    void OnReadyGroup(bool isPrivate);
    void OnSelectMusic(int memberId, long musicId, bool isRandom);
    void OnSelectStamp(int memberId, long stampId);
    void OnSelectDifficulty(int memberId, MusicDifficulties difficulty);
    void OnLotMusic(long musicId, long multiLiveId);
    void OnGoGame();
    void OnBeforeGameCalculate();
    void OnPlayGame();
    void OnExitGameAnyOne(int memberCount, int exitGameMemberCount);
    void OnExitAllGames(int[] mvpMemberIds, MultiLiveFetchUsersResult multiLiveUsers);
    void OnSyncGameResult(int memberId, IReadOnlyDictionary<TimingTypes, int> timingCounts, int maxCombo);
    void OnDestroy();
    void OnJoinedRoom(MultiLiveJoinResult multiLiveJoinResult);
    void OnLotMusicFaultStatus();
    void OnForceDisconnected(string hashUserId, ForceDisconnectReason reason);
    void OnSelectDifficultyAndPartyForTeamChallenge(
        int memberId,
        TeamChallengeSelectDifficulyAndParty result);
    void OnStartSessionForTeamChallenge(StartTeamChallengeSessionResult result);
    void OnExitAllGamesForTeamChallenge(
        int[] mvpMemberIds,
        MultiLiveFetchUsersResult multiLiveUsers,
        TeamChallengeResult result);
    void OnSyncInGameStatus(int memberId, int comboCount, ComboTypes comboType, int life);
}
