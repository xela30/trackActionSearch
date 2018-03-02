
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp2
{
    public class Participant
    {
        public int AccountId { get; set; }

        public int ContactId { get; set; }

        public static IEnumerable<Participant> Generate(int maxContacts, int maxAccounts, int companyId)
        {
            int accountMin = companyId * maxAccounts;
            int contactMin = companyId * maxContacts;

            var rand = new Random();

            var accounts = Enumerable.Range(accountMin, maxAccounts).OrderBy(_ => rand.Next()).ToArray();
            var contacts = Enumerable.Range(contactMin, maxContacts).OrderBy(_ => rand.Next()).ToArray();

            foreach (var account in accounts)
            {
                yield return new Participant {AccountId = account };
            }

            foreach (var contact in contacts)
            {
                int account = accounts[rand.Next(maxAccounts - 1)];

                yield return new Participant { ContactId = contact, AccountId = account < 3000 + accountMin ? -1 : account };
            }
        }
    }
}