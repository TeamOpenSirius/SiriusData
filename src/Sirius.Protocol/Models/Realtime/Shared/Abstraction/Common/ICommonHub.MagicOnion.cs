using MagicOnion;
using Sirius.Protocol.Realtime.Shared;
using Sirius.Protocol.Realtime.Shared.Entity.Common;

namespace Sirius.Protocol.Realtime.Shared.Abstraction.Common;

public partial interface ICommonHub : IStreamingHub<ICommonHub, ICommonHubReceiver>
{
    ValueTask JoinAsync();
    ValueTask<MultiLiveInvitation[]> GetMultiLiveInvitationsFromFriendAsync();
}

public partial interface ICommonHubReceiver
{
    void OnJoin();
    void OnNotifyInviteMultiLiveFromFriend(MultiLiveTypes multiLiveType, string hallId, string name);
}
