using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssert;
using Microsoft.Extensions.Options;
using NSubstitute;
using TrackActionSearchConsole;
using Xunit;
using Action = TrackActionSearchConsole.Action;

namespace TrackActionSearchTests
{
    public class UnitTest1
    {
        [Fact]
        public async Task should_return_actions_limited_by_top_modifier()
        {
            var query = new TrackActionQuery
            {
                ActionQuery = new ActionQuery
                {
                    Top = 2
                }
            };

            ISearchServices searchServices = Substitute.For<ISearchServices>();
            searchServices.FindActionsAsync(Arg.Any<ActionQuery>()).Returns(Task.FromResult(new[]
            {
                new Action(1, new DateTime(2001, 10, 2)),
                new Action(1, new DateTime(2001, 1, 2)),
                new Action(1, new DateTime(2001, 9, 2))
            }));

            var sut = new TrackActionSearchService(searchServices, Options.Create(new BatchProcessingConfig()));

            var actions = await sut.FindActionsAsync(query);

            actions.ShouldBeEqualTo(new[]
            {
                new Action(1, new DateTime(2001, 10, 2)),
                new Action(1, new DateTime(2001, 9, 2))
            });
        }

        [Fact]
        public async Task should_return_actions()
        {
            var query = new TrackActionQuery();

            ISearchServices searchServices = Substitute.For<ISearchServices>();
            searchServices.FindActionsAsync(Arg.Any<ActionQuery>()).Returns(Task.FromResult(new[]
            {
                new Action(1, new DateTime(2001, 10, 2)),
                new Action(1, new DateTime(2001, 1, 2)),
                new Action(1, new DateTime(2001, 9, 2))
            }));

            TrackActionSearchService sut = new TrackActionSearchService(searchServices, Options.Create(new BatchProcessingConfig()));

            var actions = await sut.FindActionsAsync(query);

            actions.ShouldBeEqualTo(new[]
            {
                new Action(1, new DateTime(2001, 10, 2)),
                new Action(1, new DateTime(2001, 9, 2)),
                new Action(1, new DateTime(2001, 1, 2))
            });
        }

        [Fact]
        public async Task should_return_actions_by_contact_query()
        {
            var query = new TrackActionQuery
            {
                ContactQuery = new ContactQuery()
            };

            ISearchServices searchServices = Substitute.For<ISearchServices>();
            searchServices.FindContactsAsync(Arg.Any<ContactQuery>()).Returns(Task.FromResult(new[]
            {
                Task.FromResult(Enumerable.Range(0, 20).ToArray()),
                Task.FromResult(Enumerable.Range(20, 7).ToArray())
            }.AsEnumerable()));

            searchServices.FindActionsByContactsAsync(Arg.Any<ActionQuery>(), Arg.Is<int[]>(contacts => contacts.Length == 15)).Returns(Task.FromResult(new[]
            {
                new Action(10, new DateTime(2001, 10, 2)),
                new Action(9, new DateTime(2001, 9, 2))
            }));

            searchServices.FindActionsByContactsAsync(Arg.Any<ActionQuery>(), Arg.Is<int[]>(contacts => contacts.Length == 12)).Returns(Task.FromResult(new[]
            {
                new Action(1, new DateTime(2001, 1, 2)),
                new Action(4, new DateTime(2001, 4, 2)),
                new Action(5, new DateTime(2001, 5, 2))
            }));

            TrackActionSearchService sut = new TrackActionSearchService(searchServices, Options.Create(new BatchProcessingConfig{ BatchSize = 15 }));

            var actions = await sut.FindActionsAsync(query);

            actions.ShouldBeEqualTo(new[]
            {
                new Action(10, new DateTime(2001, 10, 2)),
                new Action(9, new DateTime(2001, 9, 2)),
                new Action(5, new DateTime(2001, 5, 2)),
                new Action(4, new DateTime(2001, 4, 2)),
                new Action(1, new DateTime(2001, 1, 2))
            });
        }

        [Fact]
        public async Task should_return_actions_by_account_query()
        {
            var query = new TrackActionQuery
            {
                AccountQuery = new AccountQuery()
            };

            ISearchServices searchServices = Substitute.For<ISearchServices>();
            searchServices.FindAccountsAsync(Arg.Any<AccountQuery>()).Returns(Task.FromResult(new[]
            {
                Task.FromResult(Enumerable.Range(0, 17).ToArray())
            }.AsEnumerable()));

            searchServices.FindActionsByAccountsAsync(Arg.Any<ActionQuery>(), Arg.Is<int[]>(accounts => accounts.Length == 15)).Returns(Task.FromResult(new[]
            {
                new Action(10, new DateTime(2001, 10, 2)),
                new Action(9, new DateTime(2001, 9, 2))
            }));

            searchServices.FindActionsByAccountsAsync(Arg.Any<ActionQuery>(), Arg.Is<int[]>(contacts => contacts.Length == 2)).Returns(Task.FromResult(new[]
            {
                new Action(1, new DateTime(2001, 1, 2))
            }));

            TrackActionSearchService sut = new TrackActionSearchService(searchServices, Options.Create(new BatchProcessingConfig { BatchSize = 15 }));

            var actions = await sut.FindActionsAsync(query);

            actions.ShouldBeEqualTo(new[]
            {
                new Action(10, new DateTime(2001, 10, 2)),
                new Action(9, new DateTime(2001, 9, 2)),
                new Action(1, new DateTime(2001, 1, 2))
            });
        }

        [Fact]
        public async Task should_return_actions_by_account_and_contact_queries()
        {
            var query = new TrackActionQuery
            {
                AccountQuery = new AccountQuery(),
                ContactQuery = new ContactQuery()
            };

            ISearchServices searchServices = Substitute.For<ISearchServices>();
            searchServices.FindAccountsAsync(Arg.Any<AccountQuery>()).Returns(Task.FromResult(new[]
            {
                Task.FromResult(Enumerable.Range(0, 13).ToArray())
            }.AsEnumerable()));
            searchServices.FindContactsAsync(Arg.Any<ContactQuery>()).Returns(Task.FromResult(new[]
            {
                Task.FromResult(Enumerable.Range(0, 11).ToArray())
            }.AsEnumerable()));

            searchServices.FindActionsByAccountsAsync(Arg.Any<ActionQuery>(), Arg.Is<int[]>(accounts => accounts.Length == 13)).Returns(Task.FromResult(new[]
            {
                new Action(10, new DateTime(2001, 10, 2)),
                new Action(9, new DateTime(2001, 9, 2))
            }));

            searchServices.FindActionsByContactsAsync(Arg.Any<ActionQuery>(), Arg.Is<int[]>(contacts => contacts.Length == 11)).Returns(Task.FromResult(new[]
            {
                new Action(9, new DateTime(2001, 9, 2)),
                new Action(1, new DateTime(2001, 1, 2))
            }));

            TrackActionSearchService sut = new TrackActionSearchService(searchServices, Options.Create(new BatchProcessingConfig { BatchSize = 15 }));

            var actions = await sut.FindActionsAsync(query);

            actions.ShouldBeEqualTo(new[]
            {
                new Action(10, new DateTime(2001, 10, 2)),
                new Action(9, new DateTime(2001, 9, 2)),
                new Action(1, new DateTime(2001, 1, 2))
            });
        }
    }
}
