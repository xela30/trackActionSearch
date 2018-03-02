using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TrackActionSearchConsole
{
    public class LoggedSearchService: ISearchServices
    {
        private volatile int _counter = 0;
        private readonly Stopwatch _sw;
        private readonly ISearchServices _searchServices;

        public LoggedSearchService(Stopwatch sw, ISearchServices searchServices)
        {
            _sw = sw;
            _searchServices = searchServices;
        }

        public async Task<IEnumerable<Task<int[]>>> FindAccountsAsync(AccountQuery query)
        {
            var i = _counter++;
            Console.WriteLine($"[{_sw.Elapsed}] [{i}] FindAccountsAsync invoking");

            try
            {
                return await _searchServices.FindAccountsAsync(query);
            }
            finally 
            {
                Console.WriteLine($"[{_sw.Elapsed}] [{i}] FindAccountsAsync invoked");
            }
        }

        public async Task<IEnumerable<Task<int[]>>> FindContactsAsync(ContactQuery query)
        {
            var i = _counter++;
            Console.WriteLine($"[{_sw.Elapsed}] [{i}] FindContactsAsync invoking");

            try
            {
                return await _searchServices.FindContactsAsync(query);
            }
            finally
            {
                Console.WriteLine($"[{_sw.Elapsed}] [{i}] FindContactsAsync invoked");
            }
        }

        public async Task<Action[]> FindActionsByContactsAsync(ActionQuery query, int[] contacts)
        {
            var i = _counter++;
            Console.WriteLine($"[{_sw.Elapsed}] [{i}] FindActionsByContactsAsync invoking");

            try
            {
                return await _searchServices.FindActionsByContactsAsync(query, contacts);
            }
            finally
            {
                Console.WriteLine($"[{_sw.Elapsed}] [{i}] FindActionsByContactsAsync invoked");
            }
        }

        public async Task<Action[]> FindActionsByAccountsAsync(ActionQuery query, int[] accounts)
        {
            var i = _counter++;
            Console.WriteLine($"[{_sw.Elapsed}] [{i}] FindActionsByAccountsAsync invoking");

            try
            {
                return await _searchServices.FindActionsByAccountsAsync(query, accounts);
            }
            finally
            {
                Console.WriteLine($"[{_sw.Elapsed}] [{i}] FindActionsByAccountsAsync invoked");
            }
        }

        public async Task<Action[]> FindActionsAsync(ActionQuery query)
        {
            var i = _counter++;
            Console.WriteLine($"[{_sw.Elapsed}] [{i}] FindActionsAsync invoking");

            try
            {
                return await _searchServices.FindActionsAsync(query);
            }
            finally
            {
                Console.WriteLine($"[{_sw.Elapsed}] [{i}] FindActionsAsync invoked");
            }
        }
    }
}
