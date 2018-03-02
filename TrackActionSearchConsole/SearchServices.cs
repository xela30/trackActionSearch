using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace TrackActionSearchConsole
{
    public class SearchServices : ISearchServices
    {
        private static readonly DataTable ItemsTable = new DataTable {Columns = {{"ID", typeof(int)}}};

        public Task<IEnumerable<Task<int[]>>> FindAccountsAsync(AccountQuery query)
        {
            return GenerateRandomResponse();
        }
        
        public Task<IEnumerable<Task<int[]>>> FindContactsAsync(ContactQuery query)
        {
            return GenerateRandomResponse();
        }

        private static async Task<IEnumerable<Task<int[]>>> GenerateRandomResponse()
        {
            var initialDelay = 1500; //Initial delay to find out how many pages are available
            var batchDelay = 500; //Simulate delay per each page call

            //Lets assume total # of contact/accounts is about 100k within company
            //So 10 calls per 1k response should be sufficient
            var count = 10;
            var batchSize = 10000;

            await Task.Delay(initialDelay);
            
            return Enumerable.Range(5, count)
                .Select(ind => Task.Delay(batchDelay) 
                    .ContinueWith(_ => Enumerable.Range(1300000 + 100 * ind, batchSize).ToArray()))
                .ToArray();
        }
        
        public async Task<Action[]> FindActionsByContactsAsync(ActionQuery query, int[] contacts)
        {
            if (contacts.Length == 0)
            {
                return new Action[0];
            }

            using (var connection = new SqlConnection("Data Source=.;Integrated Security=true;Initial Catalog=dbTrackAction"))
            {
                var actions = await connection.QueryAsync<Action>(
                    " SELECT " +
                    (query.Top.HasValue ? $" TOP {query.Top.Value} ": "") +
                    " action_id as ActionId, action_dtm as ActionDateTime" +
                    " FROM [fn_ActionSearchByContactsColumnStore](@ContactIds, @CompanyId, @DateMin, @DateMax)" +
                    " ORDER BY action_dtm DESC",
                    new
                    {
                        ContactIds = AsTableValuedParameter(contacts),
                        query.CompanyId,
                        DateMin = query.Min,
                        DateMax = query.Max
                    });

                return actions.ToArray();
            }
        }

        public async Task<Action[]> FindActionsByAccountsAsync(ActionQuery query, int[] accounts)
        {
            if (accounts.Length == 0)
                return new Action[0];

            using (var connection = new SqlConnection("Data Source=.;Integrated Security=true;Initial Catalog=dbTrackAction"))
            {
                var actions = await connection.QueryAsync<Action>(
                    " SELECT " +
                    (query.Top.HasValue ? $" TOP {query.Top.Value} " : "") +
                    " action_id as ActionId, action_dtm as ActionDateTime" +
                    " FROM [fn_ActionSearchByAccountsColumnStore](@Accounts, @CompanyId, @DateMin, @DateMax)" +
                    " ORDER BY action_dtm DESC",
                    new
                    {
                        Accounts = AsTableValuedParameter(accounts),
                        query.CompanyId,
                        DateMin = query.Min,
                        DateMax = query.Max
                    });

                return actions.ToArray();
            }
        }

        public async Task<Action[]> FindActionsAsync(ActionQuery query)
        {
            using (var connection = new SqlConnection("Data Source=.;Integrated Security=true;Initial Catalog=dbTrackAction"))
            {
                var actions = await connection.QueryAsync<Action>(
                    " SELECT " +
                    (query.Top.HasValue ? $" TOP {query.Top.Value} " : "") +
                    " action_id as ActionId, action_dtm as ActionDateTime" +
                    " FROM [fn_ActionSearch](@CompanyId, @DateMin, @DateMax)" +
                    " ORDER BY action_dtm DESC",
                    new
                    {
                        query.CompanyId,
                        DateMin = query.Min,
                        DateMax = query.Max
                    });

                return actions.ToArray();
            }
        }

        private static SqlMapper.ICustomQueryParameter AsTableValuedParameter(int[] items)
        {
            //http://codingcramp.blogspot.com/2009/02/datatable-performance.html
            //https://stackoverflow.com/questions/28244558/extreme-performance-difference-when-using-datatable-add
            //http://www.cshandler.com/2011/10/fastest-way-to-populate-datatable-using.html#.Wpkvk-huZaQ

            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                var itemsTable = ItemsTable.Clone();
                itemsTable.BeginLoadData();

                foreach (var item in items)
                {
                    var row = itemsTable.NewRow();

                    row[0] = item;

                    itemsTable.Rows.Add(row);
                }

                itemsTable.EndLoadData();

                return itemsTable.AsTableValuedParameter("IntType");
            }
            finally
            {
                Console.WriteLine($"{sw.Elapsed} AsTableValuedParameter. Count {items.Length}");
            }
        }
    }
}