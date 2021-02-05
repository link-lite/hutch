using System.Collections.Generic;

using LinkLite.Services.QueryServices;

using Xunit;

namespace LinkLite.Test
{
    public class ListCombine
    {
        [Theory]
        [MemberData(nameof(AndPrimitiveTestData))]
        public void AndIntersectsPrimitiveLists(List<List<int>> integrants, HashSet<int> expected)
        {
            var actual = RquestOmopQueryService.Combine(
                QueryCombinators.And,
                integrants);

            Assert.StrictEqual(expected.Count, actual.Count);
            Assert.All(actual, item => expected.Contains(item));
        }

        public static List<object[]> AndPrimitiveTestData =>
        new()
        {
            new object[]
            {
                new List<List<int>>
                {
                    new() {1,1,1,1},
                    new() {1,2,3,4}
                },
                new HashSet<int> { 1 }
            },
            new object[]
            {
                new List<List<int>>
                {
                    new() {1,1,1,1},
                    new(),
                    new() {1,2,3,4}
                },
                new HashSet<int>()
            },
            new object[]
            {
                new List<List<int>>
                {
                    new() {2,4,6,8},
                    new() {1,2,3,4},
                    new() {1,2,4,5,7,8}
                },
                new HashSet<int> {2,4}
            },
        };

        [Theory]
        [MemberData(nameof(AndObjectTestData))]
        public void AndIntersectsObjectLists(List<List<Stub>> integrants, HashSet<int> expected)
        {
            var actual = RquestOmopQueryService.Combine(
                QueryCombinators.And,
                integrants,
                x => x.Id);

            Assert.StrictEqual(expected.Count, actual.Count);
            Assert.All(actual, item => expected.Contains(item.Id));
        }

        public static List<object[]> AndObjectTestData =>
        new()
        {
            new object[]
            {
                new List<List<Stub>>
                {
                    new() {new(),new(),new(),new()},
                    new() {new(),new(2), new(3), new(4)}
                },
                new HashSet<int> { 1 }
            },
            new object[]
            {
                new List<List<Stub>>
                {
                    new() {new(), new(), new(), new() },
                    new(),
                    new() {new(),new(2), new(3), new(4)}
                },
                new HashSet<int>()
            },
            new object[]
            {
                new List<List<Stub>>
                {
                    new() {new(2),new(4), new(6), new(8)},
                    new() {new(),new(2), new(3), new(4)},
                    new() {new(),new(2), new(4), new(5), new(7), new(8)}
                },
                new HashSet<int> {2,4}
            },
        };

        [Theory]
        [MemberData(nameof(OrPrimitiveTestData))]
        public void OrUnionsPrimitiveLists(List<List<int>> integrants, HashSet<int> expected)
        {
            var actual = RquestOmopQueryService.Combine(
                QueryCombinators.Or,
                integrants);

            Assert.StrictEqual(expected.Count, actual.Count);
            Assert.All(actual, item => expected.Contains(item));
        }

        public static List<object[]> OrPrimitiveTestData =>
        new()
        {
            new object[]
            {
                new List<List<int>>
                {
                    new() {1,1,1,1},
                    new() {1,2,3,4}
                },
                new HashSet<int> { 1,2,3,4 }
            },
            new object[]
            {
                new List<List<int>>
                {
                    new() {1,1,1,1},
                    new(),
                    new() {1,2,3,4}
                },
                new HashSet<int>() { 1, 2, 3, 4 }
            },
            new object[]
            {
                new List<List<int>>
                {
                    new() {2,4,6,8},
                    new() {1,2,3,4},
                    new() {1,2,4,5,7,8}
                },
                new HashSet<int> {1,2,3,4,5,6,7,8}
            },
        };

        [Theory]
        [MemberData(nameof(OrObjectTestData))]
        public void OrUnionsObjectLists(List<List<Stub>> integrants, HashSet<int> expected)
        {
            var actual = RquestOmopQueryService.Combine(
                QueryCombinators.Or,
                integrants,
                x => x.Id);

            Assert.StrictEqual(expected.Count, actual.Count);
            Assert.All(actual, item => expected.Contains(item.Id));
        }

        public static List<object[]> OrObjectTestData =>
        new()
        {
            new object[]
            {
                new List<List<Stub>>
                {
                    new() {new(),new(),new(),new()},
                    new() {new(),new(2), new(3), new(4)}
                },
                new HashSet<int> { 1,2,3,4 }
            },
            new object[]
            {
                new List<List<Stub>>
                {
                    new() {new(), new(), new(), new() },
                    new(),
                    new() {new(),new(2), new(3), new(4)}
                },
                new HashSet<int>() { 1, 2, 3, 4 }
            },
            new object[]
            {
                new List<List<Stub>>
                {
                    new() {new(2),new(4), new(6), new(8)},
                    new() {new(),new(2), new(3), new(4)},
                    new() {new(),new(2), new(4), new(5), new(7), new(8)}
                },
                new HashSet<int> {1,2,3,4,5,6,7,8}
            },
        };
    }

    public class Stub
    {
        public Stub(int id = 1) => Id = id;
        public int Id { get; set; }
    }
}
