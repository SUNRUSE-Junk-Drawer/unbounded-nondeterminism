using Akka.Actor;
using Akka.TestKit.Xunit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnboundedNondeterminism.Web.Tests
{
    public sealed class ContentTypeParserTests : TestKit
    {
        public static ContentType WithParseableEncoding = new ContentType();
        public static ContentType WithUnparseableEncoding = new ContentType();
        public static ContentType AlternativeWithParseableEncoding = new ContentType();
        public static IActorRef ExpectedSender;
        public static Encoding RequestDefault = new DummyEncoding();
        public static Encoding ResultEncoding = new DummyEncoding();
        public static Encoding AlternativeResultEncoding = new DummyEncoding();

        public sealed class DummyContentTypeParserTasks : ReceiveActor
        {
            public DummyContentTypeParserTasks()
            {
                Receive<ContentTypeParserTasks.Parse>(p => p.ContentType == "test unparseable", p => Sender.Tell(new ContentTypeParserTasks.Unparseable())); ;
                Receive<ContentTypeParserTasks.Parse>(p => p.ContentType == "test parseable with unparseable encoding", p => Sender.Tell(new ContentTypeParserTasks.Parsed {  ContentType = WithUnparseableEncoding })); ;
                Receive<ContentTypeParserTasks.Parse>(p => p.ContentType == "test parseable with parseable encoding", p => Sender.Tell(new ContentTypeParserTasks.Parsed { ContentType = WithParseableEncoding }));
                Receive<ContentTypeParserTasks.GetLeft>(p => p.ContentType == WithUnparseableEncoding || p.ContentType == WithParseableEncoding, p => Sender.Tell(new ContentTypeParserTasks.GotLeft { Left = "test left" }));
                Receive<ContentTypeParserTasks.GetRight>(p => p.ContentType == WithUnparseableEncoding || p.ContentType == WithParseableEncoding, p => Sender.Tell(new ContentTypeParserTasks.GotRight { Right = "test right" }));
                Receive<ContentTypeParserTasks.GetSuffix>(p => p.ContentType == WithUnparseableEncoding || p.ContentType == WithParseableEncoding, p => Sender.Tell(new ContentTypeParserTasks.GotSuffix { Suffix = "test suffix" }));
                Receive<ContentTypeParserTasks.GetPriority>(p => p.ContentType == WithUnparseableEncoding || p.ContentType == WithParseableEncoding, p => Sender.Tell(new ContentTypeParserTasks.GotPriority { Priority = 224.56m }));
                Receive<ContentTypeParserTasks.GetEncoding>(p => p.ContentType == WithUnparseableEncoding && p.RequestDefault == RequestDefault, p => Sender.Tell(new ContentTypeParserTasks.EncodingNotParseable()));
                Receive<ContentTypeParserTasks.GetEncoding>(p => p.ContentType == WithParseableEncoding && p.RequestDefault == RequestDefault, p => Sender.Tell(new ContentTypeParserTasks.GotEncoding { Encoding = ResultEncoding }));

                Receive<ContentTypeParserTasks.Parse>(p => p.ContentType == "alternative test parseable with parseable encoding", p => Sender.Tell(new ContentTypeParserTasks.Parsed { ContentType = AlternativeWithParseableEncoding }));
                Receive<ContentTypeParserTasks.GetLeft>(p => p.ContentType == AlternativeWithParseableEncoding, p => Sender.Tell(new ContentTypeParserTasks.GotLeft { Left = "alternative test left" }));
                Receive<ContentTypeParserTasks.GetRight>(p => p.ContentType == AlternativeWithParseableEncoding, p => Sender.Tell(new ContentTypeParserTasks.GotRight { Right = "alternative test right" }));
                Receive<ContentTypeParserTasks.GetSuffix>(p => p.ContentType == AlternativeWithParseableEncoding, p => Sender.Tell(new ContentTypeParserTasks.GotSuffix { Suffix = "alternative test suffix" }));
                Receive<ContentTypeParserTasks.GetPriority>(p => p.ContentType == AlternativeWithParseableEncoding, p => Sender.Tell(new ContentTypeParserTasks.GotPriority { Priority = 65.3m }));
                Receive<ContentTypeParserTasks.GetEncoding>(p => p.ContentType == AlternativeWithParseableEncoding && p.RequestDefault == RequestDefault, p => Sender.Tell(new ContentTypeParserTasks.GotEncoding { Encoding = AlternativeResultEncoding }));

                ReceiveAny(m => Assert.True(false));
            }
        }

        [Fact]
        public void ParseReturnsUnparseableWhenNotParseableToNetFrameworkContentType()
        {
            var contentTypeParserTasks = Sys.ActorOf(Props.Create(() => new DummyContentTypeParserTasks()));
            ExpectedSender = Sys.ActorOf(Props.Create(() => new ContentTypeParser(contentTypeParserTasks)));

            ExpectedSender.Tell(new ContentTypeParser.Parse { ContentType = "test unparseable", RequestDefault = RequestDefault });

            ExpectMsgFrom<ContentTypeParser.Unparseable>(ExpectedSender);
        }

        [Fact]
        public void ParseReturnsUnparseableWhenParseableToNetFrameworkContentTypeButWithAnUnparseableEncoding()
        {
            var contentTypeParserTasks = Sys.ActorOf(Props.Create(() => new DummyContentTypeParserTasks()));
            ExpectedSender = Sys.ActorOf(Props.Create(() => new ContentTypeParser(contentTypeParserTasks)));

            ExpectedSender.Tell(new ContentTypeParser.Parse { ContentType = "test parseable with unparseable encoding", RequestDefault = RequestDefault });

            ExpectMsgFrom<ContentTypeParser.Unparseable>(ExpectedSender);
        }

        [Fact]
        public void ParseReturnsParsedWhenParseableToNetFrameworkContentTypeWithAParseableEncoding()
        {
            var contentTypeParserTasks = Sys.ActorOf(Props.Create(() => new DummyContentTypeParserTasks()));
            ExpectedSender = Sys.ActorOf(Props.Create(() => new ContentTypeParser(contentTypeParserTasks)));

            ExpectedSender.Tell(new ContentTypeParser.Parse { ContentType = "test parseable with parseable encoding", RequestDefault = RequestDefault });

            ExpectMsgFrom<ContentTypeParser.Parsed>(ExpectedSender, p => p.Left == "test left" && p.Right == "test right" && p.Suffix == "test suffix" && p.Encoding == ResultEncoding && p.Priority == 224.56m);
        }

        [Fact]
        public void ActorContainsNoState()
        {
            var contentTypeParserTasks = Sys.ActorOf(Props.Create(() => new DummyContentTypeParserTasks()));
            ExpectedSender = Sys.ActorOf(Props.Create(() => new ContentTypeParser(contentTypeParserTasks)));
            var probeA = CreateTestProbe();
            var probeB = CreateTestProbe();

            ExpectedSender.Tell(new ContentTypeParser.Parse { ContentType = "test parseable with parseable encoding", RequestDefault = RequestDefault }, probeA.Ref);
            ExpectedSender.Tell(new ContentTypeParser.Parse { ContentType = "alternative test parseable with parseable encoding", RequestDefault = RequestDefault }, probeB.Ref);

            probeA.ExpectMsgFrom<ContentTypeParser.Parsed>(ExpectedSender, p => p.Left == "test left" && p.Right == "test right" && p.Suffix == "test suffix" && p.Encoding == ResultEncoding && p.Priority == 224.56m);
            probeB.ExpectMsgFrom<ContentTypeParser.Parsed>(ExpectedSender, p => p.Left == "alternative test left" && p.Right == "alternative test right" && p.Suffix == "alternative test suffix" && p.Encoding == AlternativeResultEncoding && p.Priority == 65.3m);
        }
    }
}