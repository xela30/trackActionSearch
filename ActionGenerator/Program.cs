using System;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using ActionGenerator;

namespace ConsoleApp2
{
    public class Program
    {
        private static volatile int Inserted = 0;

        private static readonly int ActionCount = 500000;
        private static readonly int CompanyId = 57;

        public static void Main(string[] args)
        {
            var participants = Participant.Generate(100000, 100000, CompanyId).ToArray();

            var actionBatchBlock = new BatchBlock<ActionParticipant>(7500);

            var insertActionParticipants = new ActionBlock<ActionParticipant[]>(items =>
            {
                var bulkInsertAsync = DataImporter.BulkInsertAsync(items);

                Inserted += items.Length;
                Console.WriteLine($"Inserted {Inserted} out of { ActionCount}");

                return bulkInsertAsync;
            });

            actionBatchBlock.LinkTo(insertActionParticipants);

            actionBatchBlock.Completion.ContinueWith(delegate { insertActionParticipants.Complete(); });

            foreach (var actionParticipant in ActionParticipant.Generate(participants, ActionCount, 30000, CompanyId))
            {
                actionBatchBlock.Post(actionParticipant);
            }
            actionBatchBlock.Complete();

            insertActionParticipants.Completion.Wait();
        }
    }
}