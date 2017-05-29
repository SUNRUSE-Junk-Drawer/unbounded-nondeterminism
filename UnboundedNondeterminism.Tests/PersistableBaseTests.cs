using System;
using Akka.TestKit.Xunit2;
using Xunit;
using Akka.Actor;
using System.Collections.Generic;
using System.Linq;

namespace UnboundedNondeterminism.Tests
{
    public sealed class PersistableBaseTests : TestKit
    {
        public static SynchronizedCollection<Guid> Created = new SynchronizedCollection<Guid>();
        public static SynchronizedCollection<Guid> Deleted = new SynchronizedCollection<Guid>();
        public static SynchronizedCollection<object> Sent = new SynchronizedCollection<object>();

        protected override void AfterAll()
        {
            // Stop the actors before clearing the lists they write to.
            base.AfterAll();
            Created.Clear();
            Deleted.Clear();
            Sent.Clear();
        }

        public sealed class TestType : PersistableBase
        {
            protected override void PostStop()
            {
                Deleted.Add(PersistenceGuid);
                base.PostStop();
            }

            public TestType(Guid persistenceGuid) : base(persistenceGuid)
            {
                Created.Add(persistenceGuid);
                Command(Sent.Add);
            }
        }

        [Fact]
        public void DoesNothingWithoutBeingSentMessages()
        {
            var guid = Guid.NewGuid();
            var instance = Sys.ActorOf(Props.Create(() => new TestType(guid)));

            ExpectNoMsg();
            Assert.Equal(new[] { guid }, Created);
            Assert.Empty(Deleted);
            Assert.Empty(Sent);
        }

        [Fact]
        public void StopsWhenSentStop()
        {
            var guid = Guid.NewGuid();
            var instance = Sys.ActorOf(Props.Create(() => new TestType(guid)));

            instance.Tell(new PersistableBase.Stop());

            ExpectNoMsg();
            Assert.Equal(new[] { guid }, Created);
            Assert.Equal(new[] { guid }, Deleted);
            Assert.Empty(Sent);
        }

        [Fact]
        public void DoesNotInterceptOtherMessages()
        {
            var guid = Guid.NewGuid();
            var instance = Sys.ActorOf(Props.Create(() => new TestType(guid)));

            instance.Tell("Test message one");
            ExpectNoMsg();
            instance.Tell("Test message two");
            ExpectNoMsg();

            Assert.Equal(new[] { guid }, Created);
            Assert.Empty(Deleted);
            Assert.Equal(new[] { "Test message one", "Test message two" }, Sent);
        }
    }
}