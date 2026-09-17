using MagicOnion;
using Sirius.Protocol.Realtime.Shared;
using Sirius.Protocol.Realtime.Shared.Entity.Circle;
using Sirius.Protocol.Realtime.Shared.Entity.Results;

namespace Sirius.Protocol.Realtime.Shared.Circle;

public partial interface ICircleHub : IStreamingHub<ICircleHub, ICircleHubReceiver>
{
    ValueTask JoinAsync();
    ValueTask<CircleChatSendResult> TrySendChatAsync(CircleChatPayload payload);
    ValueTask<CircleChatResult[]> GetChatsAsync(long? lastCircleChatsId);
    ValueTask<CircleReadChat> GetReadChatAsync();
    ValueTask SaveReadChatAsync(long lastReadChatId);
    ValueTask DeleteChatAsync(long chatId);
    ValueTask<ActivityLogResult[]> GetActivityLogsAsync();
}

public partial interface ICircleHubReceiver
{
    void OnJoin();
    void OnJoinStatus(CircleStatus circleStatus);
    void OnReceiveChat(CircleChatResult chatResult);
    void OnNotifyFriendRequest(string userId, string name);
    void OnNotifyMultiLiveRequest(MultiLiveTypes multiLiveType, string hallId, string name);
    void OnDeleteChat(long chatId);
    void OnReceiveReadChat(CircleReadChat circleReadChat);
    void OnReceiveActivityLog(ActivityLogResult activityLogResult);
}
