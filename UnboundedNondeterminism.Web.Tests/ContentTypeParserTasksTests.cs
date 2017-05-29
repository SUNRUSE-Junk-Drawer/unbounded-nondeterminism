using Akka.Actor;
using Akka.TestKit.Xunit2;
using System.Net.Mime;
using System.Text;
using Xunit;

namespace UnboundedNondeterminism.Web.Tests
{
    public sealed class ContentTypeParserTasksTests : TestKit
    {
        #region Parse
        [Fact]
        public void ParseNullReturnsUnparseable()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.Parse { ContentType = null });

            ExpectMsgFrom<ContentTypeParserTasks.Unparseable>(tasks);
        }

        [Fact]
        public void ParseEmptyReturnsUnparseable()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.Parse { ContentType = null });

            ExpectMsgFrom<ContentTypeParserTasks.Unparseable>(tasks);
        }

        [Fact]
        public void ParseNonsenseReturnsUnparseable()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.Parse { ContentType = "This is nonsense!" });

            ExpectMsgFrom<ContentTypeParserTasks.Unparseable>(tasks);
        }

        [Fact]
        public void ParseValidReturnsParsed()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.Parse { ContentType = "test/xml+suffix;a53=b62642;c73=d626" });

            var response = ExpectMsgFrom<ContentTypeParserTasks.Parsed>(tasks);
            Assert.NotNull(response.ContentType);
            Assert.Equal("test/xml+suffix", response.ContentType.MediaType);
            Assert.Equal("b62642", response.ContentType.Parameters["a53"]);
            Assert.Equal("d626", response.ContentType.Parameters["c73"]);
        }
        #endregion

        #region GetLeft
        [Fact]
        public void GetLeftNoSuffixesReturnsLeft()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetLeft { ContentType = new ContentType("testleft/testright") });

            ExpectMsgFrom<ContentTypeParserTasks.GotLeft>(tasks, gl => gl.Left == "testleft");
        }

        [Fact]
        public void GetLeftLeftSuffixedReturnsUnsuffixedLeft()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetLeft { ContentType = new ContentType("testleft+abc/testright") });

            ExpectMsgFrom<ContentTypeParserTasks.GotLeft>(tasks, gl => gl.Left == "testleft");
        }

        [Fact]
        public void GetLeftLeftSuffixedMultipleTimesReturnsUnsuffixedLeft()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetLeft { ContentType = new ContentType("testleft+abc+def/testright") });

            ExpectMsgFrom<ContentTypeParserTasks.GotLeft>(tasks, gl => gl.Left == "testleft");
        }

        [Fact]
        public void GetLeftRightSuffixedReturnsLeft()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetLeft { ContentType = new ContentType("testleft/testright+abc") });

            ExpectMsgFrom<ContentTypeParserTasks.GotLeft>(tasks, gl => gl.Left == "testleft");
        }

        [Fact]
        public void GetLeftRightSuffixedMultipleTimesReturnsLeft()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetLeft { ContentType = new ContentType("testleft/testright+abc+def") });

            ExpectMsgFrom<ContentTypeParserTasks.GotLeft>(tasks, gl => gl.Left == "testleft");
        }

        [Fact]
        public void GetLeftLeftSuffixedRightSuffixedReturnsUnsuffixedLeft()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetLeft { ContentType = new ContentType("testleft+abc/testright+def") });

            ExpectMsgFrom<ContentTypeParserTasks.GotLeft>(tasks, gl => gl.Left == "testleft");
        }

        [Fact]
        public void GetLeftLeftSuffixedMultipleTimesRightSuffixedReturnsUnsuffixedLeft()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetLeft { ContentType = new ContentType("testleft+abc+ghi/testright+def") });

            ExpectMsgFrom<ContentTypeParserTasks.GotLeft>(tasks, gl => gl.Left == "testleft");
        }

        [Fact]
        public void GetLeftLeftSuffixedRightSuffixedMultipleTimesReturnsUnsuffixedLeft()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetLeft { ContentType = new ContentType("testleft+abc/testright+def+ghi") });

            ExpectMsgFrom<ContentTypeParserTasks.GotLeft>(tasks, gl => gl.Left == "testleft");
        }

        [Fact]
        public void GetLeftLeftSuffixedMultipleTimesRightSuffixedMultipleTimesReturnsUnsuffixedLeft()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetLeft { ContentType = new ContentType("testleft+abc+ghi/testright+def+jkl+mno") });

            ExpectMsgFrom<ContentTypeParserTasks.GotLeft>(tasks, gl => gl.Left == "testleft");
        }

        [Fact]
        public void GetLeftOnlySuffixesReturnsWildcard()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetLeft { ContentType = new ContentType("+abc+ghi/testright+def+jkl+mno") });

            ExpectMsgFrom<ContentTypeParserTasks.GotLeft>(tasks, gl => gl.Left == "*");
        }
        #endregion

        #region GetRight
        [Fact]
        public void GetRightNoSuffixesReturnsRight()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetRight { ContentType = new ContentType("testleft/testright") });

            ExpectMsgFrom<ContentTypeParserTasks.GotRight>(tasks, gr => gr.Right == "testright");
        }

        [Fact]
        public void GetRightLeftSuffixedReturnsRight()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetRight { ContentType = new ContentType("testleft+abc/testright") });

            ExpectMsgFrom<ContentTypeParserTasks.GotRight>(tasks, gr => gr.Right == "testright");
        }

        [Fact]
        public void GetRightLeftSuffixedMultipleTimesReturnsRight()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetRight { ContentType = new ContentType("testleft+abc+def/testright") });

            ExpectMsgFrom<ContentTypeParserTasks.GotRight>(tasks, gr => gr.Right == "testright");
        }

        [Fact]
        public void GetRightRightSuffixedReturnsUnsuffixedRight()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetRight { ContentType = new ContentType("testleft/testright+abc") });

            ExpectMsgFrom<ContentTypeParserTasks.GotRight>(tasks, gr => gr.Right == "testright");
        }

        [Fact]
        public void GetRightRightSuffixedMultipleTimesReturnsUnsuffixedRight()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetRight { ContentType = new ContentType("testleft/testright+abc+def") });

            ExpectMsgFrom<ContentTypeParserTasks.GotRight>(tasks, gr => gr.Right == "testright");
        }

        [Fact]
        public void GetRightLeftSuffixedRightSuffixedReturnsUnsuffixedRight()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetRight { ContentType = new ContentType("testleft+abc/testright+def") });

            ExpectMsgFrom<ContentTypeParserTasks.GotRight>(tasks, gr => gr.Right == "testright");
        }

        [Fact]
        public void GetRightLeftSuffixedMultipleTimesRightSuffixedReturnsUnsuffixedRight()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetRight { ContentType = new ContentType("testleft+abc+ghi/testright+def") });

            ExpectMsgFrom<ContentTypeParserTasks.GotRight>(tasks, gr => gr.Right == "testright");
        }

        [Fact]
        public void GetRightLeftSuffixedRightSuffixedMultipleTimesReturnsUnsuffixedRight()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetRight { ContentType = new ContentType("testleft+abc/testright+def+ghi") });

            ExpectMsgFrom<ContentTypeParserTasks.GotRight>(tasks, gr => gr.Right == "testright");
        }

        [Fact]
        public void GetRightLeftSuffixedMultipleTimesRightSuffixedMultipleTimesReturnsUnsuffixedRight()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetRight { ContentType = new ContentType("testleft+abc+ghi/testright+def+jkl+mno") });

            ExpectMsgFrom<ContentTypeParserTasks.GotRight>(tasks, gr => gr.Right == "testright");
        }

        [Fact]
        public void GetRightOnlySuffixesReturnsWildcard()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetRight { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno") });

            ExpectMsgFrom<ContentTypeParserTasks.GotRight>(tasks, gr => gr.Right == "*");
        }
        #endregion

        #region Suffix
        [Fact]
        public void GetSuffixNoSuffixesReturnsEmpty()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetSuffix { ContentType = new ContentType("testleft/testright") });

            ExpectMsgFrom<ContentTypeParserTasks.GotSuffix>(tasks, gs => gs.Suffix == "");
        }

        [Fact]
        public void GetSuffixLeftSuffixedReturnsEmpty()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetSuffix { ContentType = new ContentType("testleft+abc/testright") });

            ExpectMsgFrom<ContentTypeParserTasks.GotSuffix>(tasks, gs => gs.Suffix == "");
        }

        [Fact]
        public void GetSuffixLeftSuffixedMultipleTimesReturnsEmpty()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetSuffix { ContentType = new ContentType("testleft+abc+def/testright") });

            ExpectMsgFrom<ContentTypeParserTasks.GotSuffix>(tasks, gs => gs.Suffix == "");
        }

        [Fact]
        public void GetSuffixRightSuffixedReturnsRightSuffix()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetSuffix { ContentType = new ContentType("testleft/testright+abc") });

            ExpectMsgFrom<ContentTypeParserTasks.GotSuffix>(tasks, gs => gs.Suffix == "abc");
        }

        [Fact]
        public void GetSuffixRightSuffixedMultipleTimesReturnsRightSuffixes()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetSuffix { ContentType = new ContentType("testleft/testright+abc+def") });

            ExpectMsgFrom<ContentTypeParserTasks.GotSuffix>(tasks, gs => gs.Suffix == "abc+def");
        }

        [Fact]
        public void GetSuffixLeftSuffixedRightSuffixedReturnsRightSuffix()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetSuffix { ContentType = new ContentType("testleft+abc/testright+def") });

            ExpectMsgFrom<ContentTypeParserTasks.GotSuffix>(tasks, gs => gs.Suffix == "def");
        }

        [Fact]
        public void GetSuffixLeftSuffixedMultipleTimesRightSuffixedReturnsRightSuffix()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetSuffix { ContentType = new ContentType("testleft+abc+ghi/testright+def") });

            ExpectMsgFrom<ContentTypeParserTasks.GotSuffix>(tasks, gs => gs.Suffix == "def");
        }

        [Fact]
        public void GetSuffixLeftSuffixedRightSuffixedMultipleTimesReturnsRightSuffixes()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetSuffix { ContentType = new ContentType("testleft+abc/testright+def+ghi") });

            ExpectMsgFrom<ContentTypeParserTasks.GotSuffix>(tasks, gs => gs.Suffix == "def+ghi");
        }

        [Fact]
        public void GetSuffixLeftSuffixedMultipleTimesRightSuffixedMultipleTimesReturnsRightSuffixes()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetSuffix { ContentType = new ContentType("testleft+abc+ghi/testright+def+jkl+mno") });

            ExpectMsgFrom<ContentTypeParserTasks.GotSuffix>(tasks, gs => gs.Suffix == "def+jkl+mno");
        }

        [Fact]
        public void GetSuffixOnlySuffixesReturnsRightSuffixes()
        {
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetSuffix { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno") });

            ExpectMsgFrom<ContentTypeParserTasks.GotSuffix>(tasks, gs => gs.Suffix == "def+jkl+mno");
        }
        #endregion

        #region GetEncoding
        [Fact]
        public void GetEncodingNoParametersReturnsRequestDefault()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetEncoding { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno"), RequestDefault = requestDefault });

            ExpectMsgFrom<ContentTypeParserTasks.GotEncoding>(tasks, gs => gs.Encoding == requestDefault);
        }

        [Fact]
        public void GetEncodingNoParameterReturnsRequestDefault()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetEncoding { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;paramb=valueb"), RequestDefault = requestDefault });

            ExpectMsgFrom<ContentTypeParserTasks.GotEncoding>(tasks, gs => gs.Encoding == requestDefault);
        }

        [Fact]
        public void GetEncodingEmptyReturnsRequestDefault()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetEncoding { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;charset=\"\";paramb=valueb"), RequestDefault = requestDefault });

            ExpectMsgFrom<ContentTypeParserTasks.GotEncoding>(tasks, gs => gs.Encoding == requestDefault);
        }

        [Fact]
        public void GetEncodingWhitespaceReturnsRequestDefault()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetEncoding { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;charset=\"   \t  \";paramb=valueb"), RequestDefault = requestDefault });

            ExpectMsgFrom<ContentTypeParserTasks.GotEncoding>(tasks, gs => gs.Encoding == requestDefault);
        }

        [Fact]
        public void GetEncodingNotRecognizedReturnsRequestDefault()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetEncoding { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;charset=test-charset-name;paramb=valueb"), RequestDefault = requestDefault });

            ExpectMsgFrom<ContentTypeParserTasks.EncodingNotParseable>(tasks);
        }

        [Fact]
        public void GetEncodingRecognizedReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetEncoding { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;charset=utf-7;paramb=valueb"), RequestDefault = requestDefault });

            ExpectMsgFrom<ContentTypeParserTasks.GotEncoding>(tasks, gs => gs.Encoding == Encoding.UTF7);
        }
        #endregion

        #region GetPriority
        [Fact]
        public void GetPriorityNoParametersReturnsOne()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 1.0m);
        }

        [Fact]
        public void GetPriorityNoParameterReturnsOne()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 1.0m);
        }

        [Fact]
        public void GetPriorityEmptyReturnsOne()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=\"\";paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 1.0m);
        }

        [Fact]
        public void GetPriorityWhitespaceReturnsOne()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=\"  \t  \";paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 1.0m);
        }

        [Fact]
        public void GetPriorityNonNumericReturnsOne()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=awd786bib;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 1.0m);
        }

        [Fact]
        public void GetPriorityUnsignedIntegerZeroReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=0;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 0.0m);
        }

        [Fact]
        public void GetPriorityUnsignedIntegerReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=92374;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 92374.0m);
        }

        [Fact]
        public void GetPriorityPositiveIntegerZeroReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=+0;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 0.0m);
        }

        [Fact]
        public void GetPriorityPositiveIntegerReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=+92374;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 92374.0m);
        }

        [Fact]
        public void GetPriorityNegativeIntegerZeroReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=-0;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 0.0m);
        }

        [Fact]
        public void GetPriorityNegativeIntegerReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=-92374;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == -92374.0m);
        }

        [Fact]
        public void GetPriorityUnsignedDecimalReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=92.374;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 92.374m);
        }

        [Fact]
        public void GetPriorityPositiveDecimalReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=+92.374;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 92.374m);
        }

        [Fact]
        public void GetPriorityNegativeDecimalReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=-92.374;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == -92.374m);
        }

        [Fact]
        public void GetPriorityUnsignedDecimalZeroReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=0.0;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 0.0m);
        }

        [Fact]
        public void GetPriorityPositiveDecimalZeroReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=+0.0;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 0.0m);
        }

        [Fact]
        public void GetPriorityNegativeDecimalZeroReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=-0.0;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 0.0m);
        }

        [Fact]
        public void GetPriorityUnsignedPartDecimalReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=.374;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 0.374m);
        }

        [Fact]
        public void GetPriorityPositivePartDecimalReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=+.374;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 0.374m);
        }

        [Fact]
        public void GetPriorityNegativePartDecimalReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=-.374;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == -0.374m);
        }

        [Fact]
        public void GetPriorityUnsignedPartDecimalZeroReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=.0;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 0.0m);
        }

        [Fact]
        public void GetPriorityPositivePartDecimalZeroReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=+.0;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 0.0m);
        }

        [Fact]
        public void GetPriorityNegativePartDecimalZeroReturnsParsed()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=valuea;q=-.0;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 0.0m);
        }

        [Fact]
        public void GetPriorityIgnoresOtherNumericParameters()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=2.4;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 1.0m);
        }

        [Fact]
        public void GetPriorityIgnoresOtherNumericParametersButCanStillFindPriority()
        {
            var requestDefault = new DummyEncoding();
            var tasks = Sys.ActorOf(Props.Create(() => new ContentTypeParserTasks()));

            tasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = new ContentType("testleft+abc+ghi/+def+jkl+mno;parama=2.4;q=+.374;paramb=valueb") });

            ExpectMsgFrom<ContentTypeParserTasks.GotPriority>(tasks, gs => gs.Priority == 0.374m);
        }
        #endregion
    }
}
