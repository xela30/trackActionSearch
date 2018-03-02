using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Options;

namespace TrackActionSearchConsole
{
    internal class Program
    {
        private static readonly int CompanyId = 13;

        private static void Main()
        {
            var sw = Stopwatch.StartNew();

            Console.WriteLine($"[{DateTime.Now:T}] Starting");

            var batchProcessingConfig = new BatchProcessingConfig
            {
                BatchSize = 25000,
                ContactActionsDatabaseSearchMaxDegreeOfParallelism = 3,
                AccountActionsDatabaseSearchMaxDegreeOfParallelism = 5
            };

            TrackActionSearchService trackActionSearch = new TrackActionSearchService(new LoggedSearchService(sw, new SearchServices()), new OptionsWrapper<BatchProcessingConfig>(batchProcessingConfig));
            
            var resultTask = trackActionSearch.FindActionsAsync(new TrackActionQuery
            {
                ActionQuery = new ActionQuery
                {
                    Top = 2000,
                    CompanyId = CompanyId,
                    Min = new DateTime(2017, 7, 1),
                    Max = DateTime.Now
                },
                ContactQuery = new ContactQuery(),
                AccountQuery = new AccountQuery()
            });

            resultTask.Wait();

            Console.WriteLine($"[{DateTime.Now:T}] Found { resultTask.Result.Length}. Elapsed {sw.Elapsed}");
        }

        private static void MainDemo()
        {
            var rand = new Random();

            var contactIds = Enumerable.Range(0, 50000).Select(_ => CompanyId * 100000 + rand.Next(100000)).ToArray();

            using (var connection = new SqlConnection("Data Source=.;Integrated Security=true;Initial Catalog=dbTrackAction"))
            {
                var contactTbl = new DataTable {Columns = {{"ID", typeof(int)}}};
                foreach (var contactId in contactIds)
                {
                    contactTbl.Rows.Add(contactId);
                }

                var sw = Stopwatch.StartNew();

                Execute(sw, connection, contactTbl, "Row");

                Execute(sw, connection, contactTbl, "Column");

                Execute(sw, connection, contactTbl, "Memory");
            }
        }

        private static void Execute(Stopwatch sw, SqlConnection connection, DataTable contactTbl, string mode)
        {
            var option = mode == "Column" ? "option (maxdop 1)" : "";
            sw.Restart();

            var actions = connection.Query<int>(
                $"SELECT TOP 100 action_id FROM [fn_ActionSearchByContacts{mode}Store](@ContactIds, @CompanyId, @DateMin, default) ORDER BY action_dtm DESC {option}",
                new
                {
                    ContactIds = contactTbl.AsTableValuedParameter("IntType"),
                    CompanyId,
                    DateMin = new DateTime(2017, 10, 1)
                }).ToArray();

            Console.Write($"[{sw.Elapsed}] {mode} {actions.Length} actions found out of ");

            sw.Restart();

            var count = connection.Query<int>(
                $"SELECT count(action_id) FROM [fn_ActionSearchByContacts{mode}Store](@ContactIds, @CompanyId, @DateMin, default) {option}",
                new
                {
                    ContactIds = contactTbl.AsTableValuedParameter("IntType"),
                    CompanyId,
                    DateMin = new DateTime(2017, 10, 1)
                }).First();


            Console.WriteLine($"{count} {sw.Elapsed}");
        }
    }
}