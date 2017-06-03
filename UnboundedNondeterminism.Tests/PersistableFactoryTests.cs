using System;
using Akka.TestKit.Xunit2;
using Xunit;
using Akka.Actor;
using System.Collections.Generic;
using System.Linq;

namespace UnboundedNondeterminism.Tests
{
    public sealed class PersistableFactoryTests : ConfiguredTestKit
    {
        public static SynchronizedCollection<Guid> Created = new SynchronizedCollection<Guid>();
        public static SynchronizedCollection<Guid> Deleted = new SynchronizedCollection<Guid>();

        protected override void AfterAll()
        {
            // Stop the actors before clearing the lists they write to.
            base.AfterAll();
            Created.Clear();
            Deleted.Clear();
        }

        public sealed class TestRequest { public Guid Reference; }
        public sealed class TestResponse
        {
            public Guid Reference;
            public Guid PersistenceGuid;
            public IActorRef CorrectSender;
            public IActorRef RespondingTo;
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
                Command<TestRequest>(cnt => Sender.Tell(new TestResponse
                {
                    Reference = cnt.Reference,
                    PersistenceGuid = persistenceGuid,
                    CorrectSender = Self,
                    RespondingTo = Sender
                }));
                Command(m => Assert.True(false));
            }
        }

        [Fact]
        public void DoesNothingWithoutBeingSentMessages()
        {
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(Guid.NewGuid())));

            ExpectNoMsg();
            Assert.Empty(Created);
            Assert.Empty(Deleted);
        }

        [Fact]
        public void ForwardToEmptyDoesNothing()
        {
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(Guid.NewGuid())));

            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = Guid.NewGuid() }, PersistenceGuid = Guid.NewGuid() });

            ExpectNoMsg();
            Assert.Empty(Created);
            Assert.Empty(Deleted);
        }

        [Fact]
        public void DeleteEmptyReturnsDeleted()
        {
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(Guid.NewGuid())));

            factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = Guid.NewGuid() });

            ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);
            Assert.Empty(Created);
            Assert.Empty(Deleted);
        }

        [Fact]
        public void CreateReturnsCreated()
        {
            var factoryGuid = Guid.NewGuid();
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(factoryGuid)));

            factory.Tell(new PersistableFactory<TestType>.Create());

            var created = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            Assert.NotEqual(factoryGuid, created.PersistenceGuid);
            // Nothing was actually created as no messages were forwarded.
            Assert.Empty(Created);
            Assert.Empty(Deleted);
        }

        [Fact]
        public void ForwardToNonexistentDoesNothing()
        {
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(Guid.NewGuid())));

            factory.Tell(new PersistableFactory<TestType>.Create());
            var createdA = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = Guid.NewGuid() }, PersistenceGuid = Guid.NewGuid() });

            ExpectNoMsg();
            // Nothing was actually created as no messages were forwarded.
            Assert.Empty(Created);
            Assert.Empty(Deleted);
        }

        [Fact]
        public void ForwardToCreatedForwards()
        {
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(Guid.NewGuid())));

            factory.Tell(new PersistableFactory<TestType>.Create());
            var createdA = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var referenceA = Guid.NewGuid();
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceA }, PersistenceGuid = createdA.PersistenceGuid });

            ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceA && response.PersistenceGuid == createdA.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);
            Assert.Equal(new[] { createdA.PersistenceGuid }, Created);
            Assert.Empty(Deleted);
        }

        [Fact]
        public void ForwardTwiceToSameCreatedForwardsToSameInstance()
        {
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(Guid.NewGuid())));

            factory.Tell(new PersistableFactory<TestType>.Create());
            var createdA = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var referenceA = Guid.NewGuid();
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceA }, PersistenceGuid = createdA.PersistenceGuid });
            var responseA = ExpectMsg<TestResponse>();
            var referenceB = Guid.NewGuid();
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceB }, PersistenceGuid = createdA.PersistenceGuid });

            var responseB = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceB && response.PersistenceGuid == createdA.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);
            Assert.Equal(responseA.CorrectSender, responseB.CorrectSender);
            Assert.Equal(new[] { createdA.PersistenceGuid }, Created);
            Assert.Empty(Deleted);
        }

        [Fact]
        public void CreateMultipleTimesReturnsOnceForEachCall()
        {
            var factoryGuid = Guid.NewGuid();
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(factoryGuid)));

            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());

            var createdA = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdB = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdC = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            Assert.Equal(4, (new[] { factoryGuid, createdA.PersistenceGuid, createdB.PersistenceGuid, createdC.PersistenceGuid }).Distinct().Count());
            // Nothing was actually created as no messages were forwarded.
            Assert.Empty(Created);
            Assert.Empty(Deleted);
        }

        [Fact]
        public void ForwardToOneOfManySendsMessageOnlyToThatSpecified()
        {
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(Guid.NewGuid())));
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            var createdA = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdB = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdC = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);

            var referenceA = Guid.NewGuid();
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceA }, PersistenceGuid = createdB.PersistenceGuid });

            var responseB = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceA && response.PersistenceGuid == createdB.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);
            Assert.Equal(new[] { createdB.PersistenceGuid }, Created);
            Assert.Empty(Deleted);
        }

        [Fact]
        public void ForwardToDeletedDoesNothing()
        {
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(Guid.NewGuid())));
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            var createdA = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdB = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdC = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = createdB.PersistenceGuid });
            ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = createdB.PersistenceGuid });

            ExpectNoMsg();
            // Nothing was actually created as no messages were forwarded.
            Assert.Empty(Created);
            Assert.Empty(Deleted);
        }

        [Fact]
        public void ForwardToDeletedPreviouslyForwardedToDoesNothing()
        {
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(Guid.NewGuid())));
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            var createdA = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdB = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdC = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = createdB.PersistenceGuid });
            ExpectMsg<TestResponse>();
            factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = createdB.PersistenceGuid });
            ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = createdB.PersistenceGuid });

            ExpectNoMsg();
            Assert.Equal(new[] { createdB.PersistenceGuid }, Created);
            Assert.Equal(new[] { createdB.PersistenceGuid }, Deleted);
        }

        [Fact]
        public void ForwardToUndeletedAmongstDeletedSendsMessageOnlyToThatSpecified()
        {
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(Guid.NewGuid())));
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            var createdA = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdB = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdC = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = createdC.PersistenceGuid });
            ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

            var referenceA = Guid.NewGuid();
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceA }, PersistenceGuid = createdB.PersistenceGuid });

            ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceA && response.PersistenceGuid == createdB.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);
            Assert.Equal(new[] { createdB.PersistenceGuid }, Created);
            Assert.Empty(Deleted);
        }

        [Fact]
        public void DeleteDeletedReturnsDeleted()
        {
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(Guid.NewGuid())));
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            var createdA = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdB = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdC = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = createdB.PersistenceGuid });
            ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

            factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = createdB.PersistenceGuid });

            ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);
            // Nothing was actually created as no messages were forwarded.
            Assert.Empty(Created);
            Assert.Empty(Deleted);
        }

        [Fact]
        public void PersistsEmpty()
        {
            var factoryGuid = Guid.NewGuid();
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(factoryGuid)));
            Assert.Empty(Created);
            Assert.Empty(Deleted);
            factory.Tell(new PersistableBase.Stop());
            ExpectNoMsg();
            Deleted.Clear();
            factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(factoryGuid)));

            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = Guid.NewGuid() });
            ExpectNoMsg();

            factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = Guid.NewGuid() });
            ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);
            Assert.Empty(Created);
            Assert.Empty(Deleted);
        }

        [Fact]
        public void PersistsNonEmpty()
        {
            var factoryGuid = Guid.NewGuid();
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(factoryGuid)));
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            var createdA = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdB = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdDeleted = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdForwardedRecovered = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdForwardedDeletedRecovered = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = createdDeleted.PersistenceGuid });
            ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);
            var preReferenceA = Guid.NewGuid();
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = preReferenceA }, PersistenceGuid = createdForwardedRecovered.PersistenceGuid });
            ExpectMsg<TestResponse>((response, sender) => response.Reference == preReferenceA && response.PersistenceGuid == createdForwardedRecovered.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

            var preReferenceB = Guid.NewGuid();
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = preReferenceB }, PersistenceGuid = createdForwardedDeletedRecovered.PersistenceGuid });
            ExpectMsg<TestResponse>((response, sender) => response.Reference == preReferenceB && response.PersistenceGuid == createdForwardedDeletedRecovered.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

            factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = createdForwardedDeletedRecovered.PersistenceGuid });
            ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

            Assert.Equal(new[] { createdForwardedRecovered.PersistenceGuid, createdForwardedDeletedRecovered.PersistenceGuid }, Created);
            Assert.Equal(new[] { createdForwardedDeletedRecovered.PersistenceGuid }, Deleted);
            factory.Tell(new PersistableBase.Stop());
            ExpectNoMsg();
            Assert.Equal(new[] { createdForwardedRecovered.PersistenceGuid, createdForwardedDeletedRecovered.PersistenceGuid }, Created);
            Assert.Equal(new[] { createdForwardedDeletedRecovered.PersistenceGuid, createdForwardedRecovered.PersistenceGuid }, Deleted);
            Created.Clear();
            Deleted.Clear();
            factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(factoryGuid)));

            var referenceA = Guid.NewGuid();
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceA }, PersistenceGuid = createdB.PersistenceGuid });
            var responseBA = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceA && response.PersistenceGuid == createdB.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

            var referenceB = Guid.NewGuid();
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceB }, PersistenceGuid = createdA.PersistenceGuid });
            var responseA = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceB && response.PersistenceGuid == createdA.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

            var referenceC = Guid.NewGuid();
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceC }, PersistenceGuid = createdB.PersistenceGuid });
            var responseBB = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceC && response.PersistenceGuid == createdB.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

            Assert.Equal(responseBA.CorrectSender, responseBB.CorrectSender);

            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = createdDeleted.PersistenceGuid });
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = Guid.NewGuid() });
            ExpectNoMsg();

            factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = createdDeleted.PersistenceGuid });
            ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

            factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = Guid.NewGuid() });
            ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

            Assert.Equal(new[] { createdB.PersistenceGuid, createdA.PersistenceGuid }, Created);
            Assert.Empty(Deleted);
        }

        [Fact]
        public void AllowsChangesFollowingRestoreOfEmpty()
        {
            var factoryGuid = Guid.NewGuid();
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(factoryGuid)));
            Assert.Empty(Created);
            Assert.Empty(Deleted);
            factory.Tell(new PersistableBase.Stop());
            ExpectNoMsg();
            Created.Clear();
            Deleted.Clear();
            factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(factoryGuid)));

            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            var recoveredCreatedA = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var recoveredCreatedB = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var recoveredCreatedDeleted = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var recoveredCreatedForwardedDeleted = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);

            var referenceA = Guid.NewGuid();
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceA }, PersistenceGuid = recoveredCreatedA.PersistenceGuid });
            var responseAA = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceA && response.PersistenceGuid == recoveredCreatedA.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

            var referenceB = Guid.NewGuid();
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceB }, PersistenceGuid = recoveredCreatedB.PersistenceGuid });
            var responseB = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceB && response.PersistenceGuid == recoveredCreatedB.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

            var referenceC = Guid.NewGuid();
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceC }, PersistenceGuid = recoveredCreatedA.PersistenceGuid });
            var responseAB = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceC && response.PersistenceGuid == recoveredCreatedA.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

            Assert.Equal(responseAA.CorrectSender, responseAB.CorrectSender);

            var referenceD = Guid.NewGuid();
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceD }, PersistenceGuid = recoveredCreatedForwardedDeleted.PersistenceGuid });
            var responseC = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceD && response.PersistenceGuid == recoveredCreatedForwardedDeleted.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

            factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = recoveredCreatedDeleted.PersistenceGuid });
            ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

            factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = recoveredCreatedForwardedDeleted.PersistenceGuid });
            ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = recoveredCreatedDeleted.PersistenceGuid });
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = recoveredCreatedForwardedDeleted.PersistenceGuid });
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = Guid.NewGuid() });
            ExpectNoMsg();

            factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = Guid.NewGuid() });
            ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

            Assert.Equal(new[] { recoveredCreatedA.PersistenceGuid, recoveredCreatedB.PersistenceGuid, recoveredCreatedForwardedDeleted.PersistenceGuid }, Created);
            Assert.Equal(new[] { recoveredCreatedForwardedDeleted.PersistenceGuid }, Deleted);
        }

        [Fact]
        public void AllowsChangesFollowingRestoreOfNonEmpty()
        {
            var factoryGuid = Guid.NewGuid();
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(factoryGuid)));
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            var createdA = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdB = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdDeleted = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdRecoveredDeleted = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdRecoveredForwardedDeleted = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdForwardedRecovered = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdForwardedDeletedRecovered = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);

            var preReferenceA = Guid.NewGuid();
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = preReferenceA }, PersistenceGuid = createdForwardedRecovered.PersistenceGuid });
            ExpectMsg<TestResponse>((response, sender) => response.Reference == preReferenceA && response.PersistenceGuid == createdForwardedRecovered.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

            var preReferenceB = Guid.NewGuid();
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = preReferenceB }, PersistenceGuid = createdForwardedDeletedRecovered.PersistenceGuid });
            ExpectMsg<TestResponse>((response, sender) => response.Reference == preReferenceB && response.PersistenceGuid == createdForwardedDeletedRecovered.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

            factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = createdForwardedDeletedRecovered.PersistenceGuid });
            ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

            Assert.Equal(new[] { createdForwardedRecovered.PersistenceGuid, createdForwardedDeletedRecovered.PersistenceGuid }, Created);
            Assert.Equal(new[] { createdForwardedDeletedRecovered.PersistenceGuid }, Deleted);

            factory.Tell(new PersistableBase.Stop());
            ExpectNoMsg();
            Assert.Equal(new[] { createdForwardedRecovered.PersistenceGuid, createdForwardedDeletedRecovered.PersistenceGuid }, Created);
            Assert.Equal(new[] { createdForwardedDeletedRecovered.PersistenceGuid, createdForwardedRecovered.PersistenceGuid }, Deleted);
            var allGuids = new List<Guid> { factoryGuid, createdA.PersistenceGuid, createdB.PersistenceGuid, createdDeleted.PersistenceGuid, createdRecoveredDeleted.PersistenceGuid, createdRecoveredForwardedDeleted.PersistenceGuid, createdForwardedRecovered.PersistenceGuid, createdForwardedDeletedRecovered.PersistenceGuid };
            Created.Clear();
            Deleted.Clear();
            factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(factoryGuid)));

            var expectedCreated = new List<Guid>();
            var expectedDeleted = new List<Guid>();

            {
                factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = createdRecoveredDeleted.PersistenceGuid });
                ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

                var referenceA = Guid.NewGuid();
                factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceA }, PersistenceGuid = createdRecoveredForwardedDeleted.PersistenceGuid });
                expectedCreated.Add(createdRecoveredForwardedDeleted.PersistenceGuid);
                var responseA = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceA && response.PersistenceGuid == createdRecoveredForwardedDeleted.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

                factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = createdRecoveredForwardedDeleted.PersistenceGuid });
                ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

                factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = createdRecoveredDeleted.PersistenceGuid });
                factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = createdRecoveredForwardedDeleted.PersistenceGuid });
                ExpectNoMsg();

                expectedDeleted.Add(createdRecoveredForwardedDeleted.PersistenceGuid);
            }

            {
                var referenceA = Guid.NewGuid();
                factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceA }, PersistenceGuid = createdB.PersistenceGuid });
                expectedCreated.Add(createdB.PersistenceGuid);
                var responseBA = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceA && response.PersistenceGuid == createdB.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

                var referenceB = Guid.NewGuid();
                factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceB }, PersistenceGuid = createdA.PersistenceGuid });
                expectedCreated.Add(createdA.PersistenceGuid);
                var responseA = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceB && response.PersistenceGuid == createdA.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

                var referenceC = Guid.NewGuid();
                factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceC }, PersistenceGuid = createdB.PersistenceGuid });
                var responseBB = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceC && response.PersistenceGuid == createdB.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

                Assert.Equal(responseBA.CorrectSender, responseBB.CorrectSender);

                factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = createdDeleted.PersistenceGuid });
                ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

                factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = Guid.NewGuid() });
                ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

                var referenceD = Guid.NewGuid();
                factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceD }, PersistenceGuid = createdB.PersistenceGuid });
                var responseCA = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceD && response.PersistenceGuid == createdB.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

                factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = createdForwardedRecovered.PersistenceGuid });
                ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

                factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = createdForwardedDeletedRecovered.PersistenceGuid });
                factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = Guid.NewGuid() });
                factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = createdDeleted.PersistenceGuid });
                ExpectNoMsg();
            }

            {
                factory.Tell(new PersistableFactory<TestType>.Create());
                factory.Tell(new PersistableFactory<TestType>.Create());
                factory.Tell(new PersistableFactory<TestType>.Create());
                factory.Tell(new PersistableFactory<TestType>.Create());
                var recoveredCreatedA = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
                var recoveredCreatedB = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
                var recoveredCreatedDeleted = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
                var recoveredCreatedForwardedDeleted = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);

                allGuids.Add(recoveredCreatedA.PersistenceGuid);
                allGuids.Add(recoveredCreatedB.PersistenceGuid);
                allGuids.Add(recoveredCreatedDeleted.PersistenceGuid);
                allGuids.Add(recoveredCreatedForwardedDeleted.PersistenceGuid);

                var referenceA = Guid.NewGuid();
                factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceA }, PersistenceGuid = recoveredCreatedA.PersistenceGuid });
                expectedCreated.Add(recoveredCreatedA.PersistenceGuid);
                var responseAA = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceA && response.PersistenceGuid == recoveredCreatedA.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

                var referenceB = Guid.NewGuid();
                factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceB }, PersistenceGuid = recoveredCreatedB.PersistenceGuid });
                expectedCreated.Add(recoveredCreatedB.PersistenceGuid);
                var responseB = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceB && response.PersistenceGuid == recoveredCreatedB.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

                var referenceC = Guid.NewGuid();
                factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceC }, PersistenceGuid = recoveredCreatedA.PersistenceGuid });
                var responseAB = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceC && response.PersistenceGuid == recoveredCreatedA.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

                Assert.Equal(responseAA.CorrectSender, responseAB.CorrectSender);

                var referenceD = Guid.NewGuid();
                factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceD }, PersistenceGuid = recoveredCreatedForwardedDeleted.PersistenceGuid });
                expectedCreated.Add(recoveredCreatedForwardedDeleted.PersistenceGuid);
                var responseC = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceD && response.PersistenceGuid == recoveredCreatedForwardedDeleted.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

                factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = recoveredCreatedDeleted.PersistenceGuid });
                ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

                factory.Tell(new PersistableFactory<TestType>.Delete { PersistenceGuid = recoveredCreatedForwardedDeleted.PersistenceGuid });
                ExpectMsgFrom<PersistableFactory<TestType>.Deleted>(factory);

                factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = recoveredCreatedDeleted.PersistenceGuid });
                factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = recoveredCreatedForwardedDeleted.PersistenceGuid });
                ExpectNoMsg();

                expectedDeleted.Add(recoveredCreatedForwardedDeleted.PersistenceGuid);
            }

            Assert.Equal(allGuids.Count, allGuids.Distinct().Count());
            Assert.Equal(expectedCreated, Created);
            Assert.Equal(expectedDeleted, Deleted);
        }

        [Fact]
        public void KeepsSeparateRecordsOfDifferentGuids()
        {
            var guidA = Guid.NewGuid();
            var guidB = Guid.NewGuid();
            var factoryA = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(guidA)));
            var factoryB = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(guidB)));
            factoryA.Tell(new PersistableFactory<TestType>.Create());
            var createdA = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factoryA);
            factoryB.Tell(new PersistableFactory<TestType>.Create());
            var createdB = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factoryB);

            Assert.NotEqual(createdA.PersistenceGuid, createdB.PersistenceGuid);

            var referenceA = Guid.NewGuid();
            factoryA.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceA }, PersistenceGuid = createdA.PersistenceGuid });
            var responseA = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceA && response.PersistenceGuid == createdA.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

            var referenceB = Guid.NewGuid();
            factoryB.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceB }, PersistenceGuid = createdB.PersistenceGuid });
            var responseB = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceB && response.PersistenceGuid == createdB.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

            Assert.NotEqual(responseA.CorrectSender, responseB.CorrectSender);

            factoryA.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = createdB.PersistenceGuid });
            factoryB.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = createdA.PersistenceGuid });
            ExpectNoMsg();

            factoryA.Tell(new PersistableBase.Stop());
            factoryB.Tell(new PersistableBase.Stop());
            ExpectNoMsg();
            factoryB = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(guidB)));
            factoryA = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(guidA)));

            var referenceC = Guid.NewGuid();
            factoryA.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceC }, PersistenceGuid = createdA.PersistenceGuid });
            var responseC = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceC && response.PersistenceGuid == createdA.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

            var referenceD = Guid.NewGuid();
            factoryB.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceD }, PersistenceGuid = createdB.PersistenceGuid });
            var responseD = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceD && response.PersistenceGuid == createdB.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

            Assert.NotEqual(responseA.CorrectSender, responseB.CorrectSender);

            factoryA.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = createdB.PersistenceGuid });
            factoryB.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest(), PersistenceGuid = createdA.PersistenceGuid });
            ExpectNoMsg();
        }

        [Fact]
        public void StoppingFactoryStopsCreatedInstances()
        {
            var factory = Sys.ActorOf(Props.Create(() => new PersistableFactory<TestType>(Guid.NewGuid())));
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            factory.Tell(new PersistableFactory<TestType>.Create());
            var createdA = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdB = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var createdC = ExpectMsgFrom<PersistableFactory<TestType>.Created>(factory);
            var referenceA = Guid.NewGuid();
            factory.Tell(new PersistableFactory<TestType>.Forward { Message = new TestRequest { Reference = referenceA }, PersistenceGuid = createdB.PersistenceGuid });
            var responseB = ExpectMsg<TestResponse>((response, sender) => response.Reference == referenceA && response.PersistenceGuid == createdB.PersistenceGuid && sender == response.CorrectSender && response.RespondingTo == TestActor);

            factory.Tell(new PersistableBase.Stop());
            ExpectNoMsg();

            Assert.Equal(new[] { createdB.PersistenceGuid }, Deleted);
        }
    }
}