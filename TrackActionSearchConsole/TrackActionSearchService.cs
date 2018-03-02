using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TrackActionSearchConsole
{
    public class TrackActionSearchService
    {
        private readonly IOptions<BatchProcessingConfig> _batchOptions;
        private readonly ISearchServices _searchServices;
        private static readonly DataflowLinkOptions PropagateCompletion = new DataflowLinkOptions{PropagateCompletion = true };

        public TrackActionSearchService(ISearchServices searchServices, IOptions<BatchProcessingConfig> batchOptions)
        {
            _batchOptions = batchOptions;
            _searchServices = searchServices;
        }

        public async Task<Action[]> FindActionsAsync(TrackActionQuery query)
        {
            var actionQueue = new BufferBlock<Action[]>();

            var produceTask = ProduceActionsAsync(query, actionQueue).Unwrap();

            var mergeActionsTask = MergeActionsAsync(actionQueue, query.ActionQuery.Top);

            await Task.WhenAll(mergeActionsTask, produceTask.ContinueWith(_ => actionQueue.Complete()));

            return mergeActionsTask.Result;
        }

        private async Task<Action[]> MergeActionsAsync(ISourceBlock<Action[]> actionSource, int? take)
        {
            var result = new Action[0];

            while (await actionSource.OutputAvailableAsync())
            {
                Console.WriteLine($"[{DateTime.Now:T}] OutputAvailableAsync");

                var next = await actionSource.ReceiveAsync();

                Console.WriteLine($"[{DateTime.Now:T}] Merging next result {next.Length}. Total {result.Length}");

                if (next.Length == 0)
                {
                    continue;
                }

                IEnumerable<Action> current = new HashSet<Action>(result.Concat(next)).OrderByDescending(action => action.ActionDateTime);

                if (take.HasValue)
                {
                    current = current.Take(take.Value);
                }

                result = current.ToArray();

                Console.WriteLine($"[{DateTime.Now:T}] Merged with next result {next.Length}. Total {result.Length}");
            }

            return result;
        }

        private async Task<Task> ProduceActionsAsync(TrackActionQuery query, ITargetBlock<Action[]> actionQueue)
        {
            if (query.AccountQuery == null && query.ContactQuery == null)
            {
                bool sendStatus = await actionQueue.SendAsync(await _searchServices.FindActionsAsync(query.ActionQuery));

                if (!sendStatus)
                {
                    throw new InvalidOperationException("Revisit bottleneck");
                }

                return Task.CompletedTask;
            }

            var produceAccountActionTask = query.AccountQuery != null ? ProduceAccountActionsAsync(query, actionQueue) : Task.CompletedTask;

            var produceContactActionTask = query.ContactQuery != null ? ProduceContactActionsAsync(query, actionQueue) : Task.CompletedTask;

            return Task.WhenAll(produceAccountActionTask, produceContactActionTask);
        }

        private Task ProduceContactActionsAsync(TrackActionQuery query, ITargetBlock<Action[]> actionQueue)
        {
            return ProduceParticipantActionsAsync(
                () => _searchServices.FindContactsAsync(query.ContactQuery),
                contacts => _searchServices.FindActionsByContactsAsync(query.ActionQuery, contacts),
                actionQueue,
                _batchOptions.Value.ContactActionsDatabaseSearchMaxDegreeOfParallelism
            ).Unwrap();
        }

        private Task ProduceAccountActionsAsync(TrackActionQuery query, ITargetBlock<Action[]> actionQueue)
        {
            return ProduceParticipantActionsAsync(
                () => _searchServices.FindAccountsAsync(query.AccountQuery),
                accounts => _searchServices.FindActionsByAccountsAsync(query.ActionQuery, accounts),
                actionQueue,
                _batchOptions.Value.AccountActionsDatabaseSearchMaxDegreeOfParallelism
            ).Unwrap();
        }

        private async Task<Task> ProduceParticipantActionsAsync(
            Func<Task<IEnumerable<Task<int[]>>>> getParticipants,
            Func<int[], Task<Action[]>> getParticipantActions,
            ITargetBlock<Action[]> actionQueue, int maxDegreeOfParallelism
        )
        {
            var participantsQueryBlock = new TransformManyBlock<Task<int[]>, int>(async task => await task, new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = -1});

            var participantsBuffer = new BatchBlock<int>(_batchOptions.Value.BatchSize);

            var getParticipantActionsBlock = new ActionBlock<int[]>(async participantIds =>
            {
                var actions = await getParticipantActions(participantIds);
                bool sendStatus = await actionQueue.SendAsync(actions);

                if (!sendStatus)
                {
                    throw new InvalidOperationException("Revisit bottleneck");
                }
            }, new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = maxDegreeOfParallelism});

            participantsQueryBlock.LinkTo(participantsBuffer, PropagateCompletion);
            participantsBuffer.LinkTo(getParticipantActionsBlock, PropagateCompletion);

            foreach (var participantTask in await getParticipants())
            {
                bool sendStatus = await participantsQueryBlock.SendAsync(participantTask);

                if (!sendStatus)
                {
                    throw new InvalidOperationException("Revisit bottleneck");
                }
            }

            participantsQueryBlock.Complete();

            return getParticipantActionsBlock.Completion;
        }
    }
}