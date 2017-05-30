using Akka.Actor;
using Akka.TestKit.TestActors;
using Akka.TestKit.Xunit2;
using System;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Xunit;

namespace UnboundedNondeterminism.Web.Tests
{
    public sealed class ContentTypeMatcherTasksTests : TestKit
    {
        #region ParseAndSort
        public static Encoding RequestDefault = new DummyEncoding();
        public static ContentTypeParser.Parsed ParsedA = new ContentTypeParser.Parsed { Priority = 6.0m };
        public static ContentTypeParser.Parsed ParsedB = new ContentTypeParser.Parsed { Priority = 6.4m };
        public static ContentTypeParser.Parsed ParsedC = new ContentTypeParser.Parsed { Priority = 4.0m };
        public static ContentTypeParser.Parsed ParsedD = new ContentTypeParser.Parsed { Priority = 3.0m };
        public static ContentTypeParser.Parsed ParsedE = new ContentTypeParser.Parsed { Priority = 2.7m };
        public static ContentTypeParser.Parsed ParsedF = new ContentTypeParser.Parsed { Priority = 9.4m };
        public static ContentTypeParser.Parsed ParsedG = new ContentTypeParser.Parsed { Priority = 4.4m };
        public static ContentTypeParser.Parsed ParsedH = new ContentTypeParser.Parsed { Priority = 6.5m };
        public static ContentTypeParser.Parsed ParsedI = new ContentTypeParser.Parsed { Priority = 4.3m };
        public static ContentTypeParser.Parsed ParsedJ = new ContentTypeParser.Parsed { Priority = 6.5m };
        public static ContentTypeParser.Parsed ParsedK = new ContentTypeParser.Parsed { Priority = 4.3m };
        public static ContentTypeParser.Parsed ParsedL = new ContentTypeParser.Parsed { Priority = 2.1m };

        public sealed class DummyContentTypeParser : ReceiveActor
        {
            public DummyContentTypeParser()
            {
                Receive<ContentTypeParser.Parse>(p => p.ContentType == "test unparseable a" && p.RequestDefault == RequestDefault, p => Sender.Tell(new ContentTypeParser.Unparseable()));
                Receive<ContentTypeParser.Parse>(p => p.ContentType == "test unparseable b" && p.RequestDefault == RequestDefault, p => Sender.Tell(new ContentTypeParser.Unparseable()));
                Receive<ContentTypeParser.Parse>(p => p.ContentType == "test unparseable c" && p.RequestDefault == RequestDefault, p => Sender.Tell(new ContentTypeParser.Unparseable()));
                Receive<ContentTypeParser.Parse>(p => p.ContentType == "test unparseable d" && p.RequestDefault == RequestDefault, p => Sender.Tell(new ContentTypeParser.Unparseable()));
                Receive<ContentTypeParser.Parse>(p => p.ContentType == "test parseable a" && p.RequestDefault == RequestDefault, p => Sender.Tell(ParsedA));
                Receive<ContentTypeParser.Parse>(p => p.ContentType == "test parseable b" && p.RequestDefault == RequestDefault, p => Sender.Tell(ParsedB));
                Receive<ContentTypeParser.Parse>(p => p.ContentType == "test parseable c" && p.RequestDefault == RequestDefault, p => Sender.Tell(ParsedC));
                Receive<ContentTypeParser.Parse>(p => p.ContentType == "test parseable d" && p.RequestDefault == RequestDefault, p => Sender.Tell(ParsedD));
                Receive<ContentTypeParser.Parse>(p => p.ContentType == "alternative test parseable a" && p.RequestDefault == RequestDefault, p => Sender.Tell(ParsedE));
                Receive<ContentTypeParser.Parse>(p => p.ContentType == "alternative test parseable b" && p.RequestDefault == RequestDefault, p => Sender.Tell(ParsedF));
                Receive<ContentTypeParser.Parse>(p => p.ContentType == "alternative test parseable c" && p.RequestDefault == RequestDefault, p => Sender.Tell(ParsedG));
                Receive<ContentTypeParser.Parse>(p => p.ContentType == "alternative test parseable d" && p.RequestDefault == RequestDefault, p => Sender.Tell(ParsedH));
                Receive<ContentTypeParser.Parse>(p => p.ContentType == "stable sorted test parseable a" && p.RequestDefault == RequestDefault, p => Sender.Tell(ParsedI));
                Receive<ContentTypeParser.Parse>(p => p.ContentType == "stable sorted test parseable b" && p.RequestDefault == RequestDefault, p => Sender.Tell(ParsedJ));
                Receive<ContentTypeParser.Parse>(p => p.ContentType == "stable sorted test parseable c" && p.RequestDefault == RequestDefault, p => Sender.Tell(ParsedK));
                Receive<ContentTypeParser.Parse>(p => p.ContentType == "stable sorted test parseable d" && p.RequestDefault == RequestDefault, p => Sender.Tell(ParsedL));
                ReceiveAny(m => Assert.True(false));
            }
        }

        [Fact]
        public void ParseAndSortNothingReturnsEmptyParsedAndSorted()
        {
            var contentTypeParser = Sys.ActorOf(Props.Create(() => new DummyContentTypeParser()));
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeMatcherTasks(contentTypeParser)));

            tasks.Tell(new ContentTypeMatcherTasks.ParseAndSort { ContentTypes = Enumerable.Empty<string>(), RequestDefault = RequestDefault });

            Assert.Empty(ExpectMsgFrom<ContentTypeMatcherTasks.ParsedAndSorted>(tasks).ContentTypes);
        }

        [Fact]
        public void ParseAndSortNothingParsesReturnsEmptyParsedAndSorted()
        {
            var contentTypeParser = Sys.ActorOf(Props.Create(() => new DummyContentTypeParser()));
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeMatcherTasks(contentTypeParser)));

            tasks.Tell(new ContentTypeMatcherTasks.ParseAndSort { ContentTypes = new[] { "test unparseable a", "test unparseable b", "test unparseable c", "test unparseable d" }, RequestDefault = RequestDefault });

            Assert.Empty(ExpectMsgFrom<ContentTypeMatcherTasks.ParsedAndSorted>(tasks).ContentTypes);
        }

        [Fact]
        public void ParseAndSortSomeParseReturnsOnlyParsedSorted()
        {
            var contentTypeParser = Sys.ActorOf(Props.Create(() => new DummyContentTypeParser()));
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeMatcherTasks(contentTypeParser)));

            tasks.Tell(new ContentTypeMatcherTasks.ParseAndSort { ContentTypes = new[] { "test parseable a", "test parseable b", "test unparseable c", "test parseable d" }, RequestDefault = RequestDefault });

            Assert.Equal(new[] { ParsedB, ParsedA, ParsedD }, ExpectMsgFrom<ContentTypeMatcherTasks.ParsedAndSorted>(tasks).ContentTypes);
        }

        [Fact]
        public void ParseAndSortAllParseReturnsParsedSorted()
        {
            var contentTypeParser = Sys.ActorOf(Props.Create(() => new DummyContentTypeParser()));
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeMatcherTasks(contentTypeParser)));

            tasks.Tell(new ContentTypeMatcherTasks.ParseAndSort { ContentTypes = new[] { "test parseable a", "test parseable b", "test parseable c", "test parseable d" }, RequestDefault = RequestDefault });

            Assert.Equal(new[] { ParsedB, ParsedA, ParsedC, ParsedD }, ExpectMsgFrom<ContentTypeMatcherTasks.ParsedAndSorted>(tasks).ContentTypes);
        }

        [Fact]
        public void ParseAndSortIsStateless()
        {
            var contentTypeParser = Sys.ActorOf(Props.Create(() => new DummyContentTypeParser()));
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeMatcherTasks(contentTypeParser)));
            var probeA = CreateTestProbe();
            var probeB = CreateTestProbe();

            tasks.Tell(new ContentTypeMatcherTasks.ParseAndSort { ContentTypes = new[] { "test parseable a", "test parseable b", "test parseable c", "test parseable d" }, RequestDefault = RequestDefault }, probeA);
            tasks.Tell(new ContentTypeMatcherTasks.ParseAndSort { ContentTypes = new[] { "alternative test parseable a", "alternative test parseable b", "alternative test parseable c", "alternative test parseable d" }, RequestDefault = RequestDefault }, probeB);

            Assert.Equal(new[] { ParsedB, ParsedA, ParsedC, ParsedD }, probeA.ExpectMsgFrom<ContentTypeMatcherTasks.ParsedAndSorted>(tasks).ContentTypes);
            Assert.Equal(new[] { ParsedF, ParsedH, ParsedG, ParsedE }, probeB.ExpectMsgFrom<ContentTypeMatcherTasks.ParsedAndSorted>(tasks).ContentTypes);
        }

        [Fact]
        public void ParseAndSortAllParseReturnsParsedSortedStably()
        {
            var contentTypeParser = Sys.ActorOf(Props.Create(() => new DummyContentTypeParser()));
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeMatcherTasks(contentTypeParser)));

            tasks.Tell(new ContentTypeMatcherTasks.ParseAndSort { ContentTypes = new[] { "stable sorted test parseable a", "stable sorted test parseable b", "stable sorted test parseable c", "stable sorted test parseable d" }, RequestDefault = RequestDefault });

            Assert.Equal(new[] { ParsedJ, ParsedI, ParsedK, ParsedL }, ExpectMsgFrom<ContentTypeMatcherTasks.ParsedAndSorted>(tasks).ContentTypes);
        }
        #endregion

        #region Match
        [Theory]
        [InlineData("*", "*", "*", "*", "*", "*", true)]
        [InlineData("A Left", "*", "*", "*", "*", "*", true)]
        [InlineData("*", "A Right", "*", "*", "*", "*", true)]
        [InlineData("A Left", "A Right", "*", "*", "*", "*", true)]
        [InlineData("*", "*", "A Suffix", "*", "*", "*", true)]
        [InlineData("A Left", "*", "A Suffix", "*", "*", "*", true)]
        [InlineData("*", "A Right", "A Suffix", "*", "*", "*", true)]
        [InlineData("A Left", "A Right", "A Suffix", "*", "*", "*", true)]
        [InlineData("*", "*", "*", "B Left", "*", "*", true)]
        [InlineData("A Left", "*", "*", "B Left", "*", "*", false)]
        [InlineData("Same Left", "*", "*", "Same Left", "*", "*", true)]
        [InlineData("*", "A Right", "*", "B Left", "*", "*", true)]
        [InlineData("A Left", "A Right", "*", "B Left", "*", "*", false)]
        [InlineData("Same Left", "A Right", "*", "Same Left", "*", "*", true)]
        [InlineData("*", "*", "A Suffix", "B Left", "*", "*", true)]
        [InlineData("A Left", "*", "A Suffix", "B Left", "*", "*", false)]
        [InlineData("Same Left", "*", "A Suffix", "Same Left", "*", "*", true)]
        [InlineData("*", "A Right", "A Suffix", "B Left", "*", "*", true)]
        [InlineData("A Left", "A Right", "A Suffix", "B Left", "*", "*", false)]
        [InlineData("Same Left", "A Right", "A Suffix", "Same Left", "*", "*", true)]
        [InlineData("*", "*", "*", "*", "B Right", "*", true)]
        [InlineData("A Left", "*", "*", "*", "B Right", "*", true)]
        [InlineData("*", "A Right", "*", "*", "B Right", "*", false)]
        [InlineData("*", "Same Right", "*", "*", "Same Right", "*", true)]
        [InlineData("A Left", "A Right", "*", "*", "B Right", "*", false)]
        [InlineData("A Left", "Same Right", "*", "*", "Same Right", "*", true)]
        [InlineData("*", "*", "A Suffix", "*", "B Right", "*", true)]
        [InlineData("A Left", "*", "A Suffix", "*", "B Right", "*", true)]
        [InlineData("*", "A Right", "A Suffix", "*", "B Right", "*", false)]
        [InlineData("*", "Same Right", "A Suffix", "*", "Same Right", "*", true)]
        [InlineData("A Left", "A Right", "A Suffix", "*", "B Right", "*", false)]
        [InlineData("A Left", "Same Right", "A Suffix", "*", "Same Right", "*", true)]
        [InlineData("*", "*", "*", "B Left", "B Right", "*", true)]
        [InlineData("A Left", "*", "*", "B Left", "B Right", "*", false)]
        [InlineData("Same Left", "*", "*", "Same Left", "B Right", "*", true)]
        [InlineData("*", "A Right", "*", "B Left", "B Right", "*", false)]
        [InlineData("*", "Same Right", "*", "B Left", "Same Right", "*", true)]
        [InlineData("A Left", "A Right", "*", "B Left", "B Right", "*", false)]
        [InlineData("A Left", "Same Right", "*", "B Left", "Same Right", "*", false)]
        [InlineData("Same Left", "A Right", "*", "Same Left", "B Right", "*", false)]
        [InlineData("Same Left", "Same Right", "*", "Same Left", "Same Right", "*", true)]
        [InlineData("*", "*", "A Suffix", "B Left", "B Right", "*", true)]
        [InlineData("A Left", "*", "A Suffix", "B Left", "B Right", "*", false)]
        [InlineData("Same Left", "*", "A Suffix", "Same Left", "B Right", "*", true)]
        [InlineData("*", "A Right", "A Suffix", "B Left", "B Right", "*", false)]
        [InlineData("*", "Same Right", "A Suffix", "B Left", "Same Right", "*", true)]
        [InlineData("A Left", "A Right", "A Suffix", "B Left", "B Right", "*", false)]
        [InlineData("A Left", "Same Right", "A Suffix", "B Left", "Same Right", "*", false)]
        [InlineData("Same Left", "A Right", "A Suffix", "Same Left", "B Right", "*", false)]
        [InlineData("Sample Left", "Same Right", "A Suffix", "Sample Left", "Same Right", "*", true)]
        [InlineData("*", "*", "*", "*", "*", "B Suffix", true)]
        [InlineData("A Left", "*", "*", "*", "*", "B Suffix", true)]
        [InlineData("*", "A Right", "*", "*", "*", "B Suffix", true)]
        [InlineData("A Left", "A Right", "*", "*", "*", "B Suffix", true)]
        [InlineData("*", "*", "A Suffix", "*", "*", "B Suffix", false)]
        [InlineData("*", "*", "Same Suffix", "*", "*", "Same Suffix", true)]
        [InlineData("A Left", "*", "A Suffix", "*", "*", "B Suffix", false)]
        [InlineData("A Left", "*", "Same Suffix", "*", "*", "Same Suffix", true)]
        [InlineData("*", "A Right", "A Suffix", "*", "*", "B Suffix", false)]
        [InlineData("*", "A Right", "Same Suffix", "*", "*", "Same Suffix", true)]
        [InlineData("A Left", "A Right", "A Suffix", "*", "*", "B Suffix", false)]
        [InlineData("A Left", "A Right", "Same Suffix", "*", "*", "Same Suffix", true)]
        [InlineData("*", "*", "*", "B Left", "*", "B Suffix", true)]
        [InlineData("A Left", "*", "*", "B Left", "*", "B Suffix", false)]
        [InlineData("Same Left", "*", "*", "Same Left", "*", "B Suffix", true)]
        [InlineData("*", "A Right", "*", "B Left", "*", "B Suffix", true)]
        [InlineData("A Left", "A Right", "*", "B Left", "*", "B Suffix", false)]
        [InlineData("Same Left", "A Right", "*", "Same Left", "*", "B Suffix", true)]
        [InlineData("*", "*", "A Suffix", "B Left", "*", "B Suffix", false)]
        [InlineData("*", "*", "Same Suffix", "B Left", "*", "Same Suffix", true)]
        [InlineData("A Left", "*", "A Suffix", "B Left", "*", "B Suffix", false)]
        [InlineData("Same Left", "*", "A Suffix", "Same Left", "*", "B Suffix", false)]
        [InlineData("A Left", "*", "Same Suffix", "B Left", "*", "Same Suffix", false)]
        [InlineData("Same Left", "*", "Same Suffix", "Same Left", "*", "Same Suffix", true)]
        [InlineData("*", "A Right", "A Suffix", "B Left", "*", "B Suffix", false)]
        [InlineData("*", "A Right", "Same Suffix", "B Left", "*", "Same Suffix", true)]
        [InlineData("A Left", "A Right", "A Suffix", "B Left", "*", "B Suffix", false)]
        [InlineData("Same Left", "A Right", "A Suffix", "Same Left", "*", "B Suffix", false)]
        [InlineData("A Left", "A Right", "Same Suffix", "B Left", "*", "Same Suffix", false)]
        [InlineData("Same Left", "A Right", "Same Suffix", "Same Left", "*", "Same Suffix", true)]
        [InlineData("*", "*", "*", "*", "B Right", "B Suffix", true)]
        [InlineData("A Left", "*", "*", "*", "B Right", "B Suffix", true)]
        [InlineData("*", "A Right", "*", "*", "B Right", "B Suffix", false)]
        [InlineData("*", "Same Right", "*", "*", "Same Right", "B Suffix", true)]
        [InlineData("A Left", "A Right", "*", "*", "B Right", "B Suffix", false)]
        [InlineData("A Left", "Same Right", "*", "*", "Same Right", "B Suffix", true)]
        [InlineData("*", "*", "A Suffix", "*", "B Right", "B Suffix", false)]
        [InlineData("*", "*", "Same Suffix", "*", "B Right", "Same Suffix", true)]
        [InlineData("A Left", "*", "A Suffix", "*", "B Right", "B Suffix", false)]
        [InlineData("A Left", "*", "Same Suffix", "*", "B Right", "Same Suffix", true)]
        [InlineData("*", "A Right", "A Suffix", "*", "B Right", "B Suffix", false)]
        [InlineData("*", "Same Right", "A Suffix", "*", "Same Right", "B Suffix", false)]
        [InlineData("*", "A Right", "Same Suffix", "*", "B Right", "Same Suffix", false)]
        [InlineData("*", "Same Right", "Same Suffix", "*", "Same Right", "Same Suffix", true)]
        [InlineData("A Left", "A Right", "A Suffix", "*", "B Right", "B Suffix", false)]
        [InlineData("A Left", "Same Right", "A Suffix", "*", "Same Right", "B Suffix", false)]
        [InlineData("A Left", "A Right", "Same Suffix", "*", "B Right", "Same Suffix", false)]
        [InlineData("A Left", "Same Right", "Same Suffix", "*", "Same Right", "Same Suffix", true)]
        [InlineData("*", "*", "*", "B Left", "B Right", "B Suffix", true)]
        [InlineData("A Left", "*", "*", "B Left", "B Right", "B Suffix", false)]
        [InlineData("Same Left", "*", "*", "Same Left", "B Right", "B Suffix", true)]
        [InlineData("*", "A Right", "*", "B Left", "B Right", "B Suffix", false)]
        [InlineData("*", "Same Right", "*", "B Left", "Same Right", "B Suffix", true)]
        [InlineData("A Left", "A Right", "*", "B Left", "B Right", "B Suffix", false)]
        [InlineData("A Left", "Same Right", "*", "B Left", "Same Right", "B Suffix", false)]
        [InlineData("Same Left", "A Right", "*", "Same Left", "B Right", "B Suffix", false)]
        [InlineData("Same Left", "Same Right", "*", "Same Left", "Same Right", "B Suffix", true)]
        [InlineData("*", "*", "A Suffix", "B Left", "B Right", "B Suffix", false)]
        [InlineData("*", "*", "Same Suffix", "B Left", "B Right", "Same Suffix", true)]
        [InlineData("A Left", "*", "A Suffix", "B Left", "B Right", "B Suffix", false)]
        [InlineData("Same Left", "*", "A Suffix", "Same Left", "B Right", "B Suffix", false)]
        [InlineData("A Left", "*", "Same Suffix", "B Left", "B Right", "Same Suffix", false)]
        [InlineData("Same Left", "*", "Same Suffix", "Same Left", "B Right", "Same Suffix", true)]
        [InlineData("*", "A Right", "A Suffix", "B Left", "B Right", "B Suffix", false)]
        [InlineData("*", "Same Right", "A Suffix", "B Left", "Same Right", "B Suffix", false)]
        [InlineData("*", "A Right", "Same Suffix", "B Left", "B Right", "Same Suffix", false)]
        [InlineData("*", "Same Right", "Same Suffix", "B Left", "Same Right", "Same Suffix", true)]
        [InlineData("A Left", "A Right", "A Suffix", "B Left", "B Right", "B Suffix", false)]
        [InlineData("A Left", "Same Right", "A Suffix", "B Left", "Same Right", "B Suffix", false)]
        [InlineData("Same Left", "A Right", "A Suffix", "Same Left", "B Right", "B Suffix", false)]
        [InlineData("Sample Left", "Same Right", "A Suffix", "Sample Left", "Same Right", "B Suffix", false)]
        [InlineData("A Left", "A Right", "Same Suffix", "B Left", "B Right", "Same Suffix", false)]
        [InlineData("A Left", "Same Right", "Same Suffix", "B Left", "Same Right", "Same Suffix", false)]
        [InlineData("Same Left", "A Right", "Same Suffix", "Same Left", "B Right", "Same Suffix", false)]
        [InlineData("Sample Left", "Same Right", "Same Suffix", "Sample Left", "Same Right", "Same Suffix", true)]
        public void Match(string leftA, string rightA, string suffixA, string leftB, string rightB, string suffixB, bool isMatch)
        {
            var blackHole = Sys.ActorOf(Props.Create<BlackHoleActor>());
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeMatcherTasks(blackHole)));

            tasks.Tell(new ContentTypeMatcherTasks.Match
            {
                A = new ContentTypeParser.Parsed { Left = leftA, Right = rightA, Suffix = suffixA },
                B = new ContentTypeParser.Parsed { Left = leftB, Right = rightB, Suffix = suffixB }
            });

            if (isMatch)
                ExpectMsgFrom<ContentTypeMatcherTasks.Matched>(tasks);
            else
                ExpectMsgFrom<ContentTypeMatcherTasks.NotMatched>(tasks);
        }
        #endregion
    }
}
