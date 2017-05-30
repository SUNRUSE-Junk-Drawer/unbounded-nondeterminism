using Akka.Actor;
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
    }
}
