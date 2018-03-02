using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleApp2;

namespace ActionGenerator
{
    public class ActionParticipant
    {
        public int action_id { get; set; }
        public int account_id { get; set; } = -1;
        public int contact_id { get; set; } = -1;
        public DateTime action_dtm { get; set; }
        public int company_id { get; set; }
        public bool recent_ind { get; set; }

        public static IEnumerable<ActionParticipant> Generate(Participant[] participants, int actionCount, int actionsPerParticipants, int companyId)
        {
            
            var rand = new Random();

            var minActionId  = actionCount * companyId;

            for (int actionId = minActionId; actionId < minActionId  + actionCount; actionId++)
            {
                var actionDtm = DateTime.Now.AddDays(-rand.Next(1, 365));

                foreach (var participant in Enumerable.Range(0, actionsPerParticipants).Take(rand.Next(1, actionsPerParticipants)).Select(_ => rand.Next(participants.Length - 1)).Distinct().Select(i => participants[i]))
                {
                    yield return new ActionParticipant{ action_id = actionId, action_dtm = actionDtm, account_id = participant.AccountId, contact_id = participant.ContactId, company_id = companyId };
                }
            }
        }
    }
}