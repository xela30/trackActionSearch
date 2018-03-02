using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrackActionSearchConsole
{
    public interface ISearchServices
    {
        Task<IEnumerable<Task<int[]>>> FindAccountsAsync(AccountQuery query);
        Task<IEnumerable<Task<int[]>>> FindContactsAsync(ContactQuery query);
        Task<Action[]> FindActionsByContactsAsync(ActionQuery query, int[] contacts);
        Task<Action[]> FindActionsByAccountsAsync(ActionQuery query, int[] accounts);
        Task<Action[]> FindActionsAsync(ActionQuery query);
    }
}