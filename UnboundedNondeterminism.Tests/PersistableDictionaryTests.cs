using Akka.Actor;
using Akka.TestKit.Xunit2;
using System;
using Xunit;

namespace UnboundedNondeterminism.Tests
{
    public sealed class PersistableDictionaryTests : ConfiguredTestKit
    {
        [Fact]
        public void DoesNothingWithoutBeingSentMessages()
        {
            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(Guid.NewGuid())));
            ExpectNoMsg();
        }

        [Fact]
        public void ReturnsDeletedWhenEmpty()
        {
            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(Guid.NewGuid())));

            dictionary.Tell(new PersistableDictionary<string, int>.Delete { Key = "KeyA" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Deleted>(dictionary);
        }

        [Fact]
        public void ReturnsDeletedWhenKeyDoesNotExist()
        {
            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(Guid.NewGuid())));

            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyA", Value = 7647 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 65463 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyC", Value = 535 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);

            dictionary.Tell(new PersistableDictionary<string, int>.Delete { Key = "KeyD" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Deleted>(dictionary);
        }

        [Fact]
        public void ReturnsDeletedWhenKeyExists()
        {
            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(Guid.NewGuid())));

            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyA", Value = 7647 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 65463 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyC", Value = 535 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);

            dictionary.Tell(new PersistableDictionary<string, int>.Delete { Key = "KeyB" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Deleted>(dictionary);
        }

        [Fact]
        public void ReturnsNotFoundWhenEmpty()
        {
            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(Guid.NewGuid())));

            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "KeyA" });
            ExpectMsgFrom<PersistableDictionary<string, int>.NotFound>(dictionary);
        }

        [Fact]
        public void ReturnsSpecifiedAfterCreatingAProperty()
        {
            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(Guid.NewGuid())));

            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyA", Value = 7647 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
        }

        [Fact]
        public void ReturnsSpecifiedAfterCreatingASubsequentProperty()
        {
            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(Guid.NewGuid())));
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyA", Value = 7647 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);

            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 65463 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
        }

        [Fact]
        public void ReturnsSpecifiedAfterReplacingAProperty()
        {
            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(Guid.NewGuid())));
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyA", Value = 7647 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 65463 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyC", Value = 535 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);

            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 653653 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
        }

        [Fact]
        public void ReturnsSpecifiedAfterReplacingAPropertyWithTheSameValue()
        {
            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(Guid.NewGuid())));
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyA", Value = 7647 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 65463 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyC", Value = 535 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);

            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 65463 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
        }

        [Fact]
        public void ReturnsSpecifiedAfterReplacingAPreviouslyReplacedProperty()
        {
            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(Guid.NewGuid())));
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyA", Value = 7647 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 65463 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyC", Value = 535 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 47474 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);

            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 653653 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
        }

        [Fact]
        public void ReturnsSpecifiedAfterRecreatingAPreviouslyDeletedProperty()
        {
            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(Guid.NewGuid())));
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyA", Value = 7647 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 65463 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyC", Value = 535 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Delete { Key = "KeyB" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Deleted>(dictionary);

            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 653653 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
        }

        [Fact]
        public void ReturnsNotFoundWhenDeleted()
        {
            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(Guid.NewGuid())));
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyA", Value = 7647 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 65463 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyC", Value = 535 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Delete { Key = "KeyB" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Deleted>(dictionary);

            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "KeyB" });
            ExpectMsgFrom<PersistableDictionary<string, int>.NotFound>(dictionary);
        }

        [Fact]
        public void ReturnsGotWhenExists()
        {
            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(Guid.NewGuid())));
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyA", Value = 7647 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 65463 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyC", Value = 535 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);

            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "KeyB" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 65463);
        }

        [Fact]
        public void ReturnsGotWhenReplaced()
        {
            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(Guid.NewGuid())));
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyA", Value = 7647 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 65463 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyC", Value = 535 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 47474 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);

            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "KeyB" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 47474);
        }

        [Fact]
        public void ReturnsGotWhenOthersReplaced()
        {
            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(Guid.NewGuid())));
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyA", Value = 7647 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 65463 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyC", Value = 535 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 47474 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);

            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "KeyC" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 535);
        }

        [Fact]
        public void ReturnsGotWhenOthersDeleted()
        {
            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(Guid.NewGuid())));
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyA", Value = 7647 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 65463 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyC", Value = 535 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Delete { Key = "KeyB" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Deleted>(dictionary);

            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "KeyC" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 535);
        }

        [Fact]
        public void ReturnsGotWhenDeletedAndRecreated()
        {
            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(Guid.NewGuid())));
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyA", Value = 7647 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 65463 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyC", Value = 535 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Delete { Key = "KeyB" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Deleted>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "KeyB", Value = 47474 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);

            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "KeyB" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 47474);
        }

        [Fact]
        public void PersistsEmpty()
        {
            var guid = Guid.NewGuid();

            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(guid)));
            Sys.Stop(dictionary);
            dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(guid)));

            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "KeyA" });
            ExpectMsgFrom<PersistableDictionary<string, int>.NotFound>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "KeyB" });
            ExpectMsgFrom<PersistableDictionary<string, int>.NotFound>(dictionary);
        }

        [Fact]
        public void PersistsNonEmpty()
        {
            var guid = Guid.NewGuid();

            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(guid)));
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "CreatedKey", Value = 47474 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "DeletedKey", Value = 6336363 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Delete { Key = "DeletedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Deleted>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "ReplacedKey", Value = 9979 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "ReplacedKey", Value = 7647 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "DeletedThenCreatedKey", Value = 55525 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Delete { Key = "DeletedThenCreatedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Deleted>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "DeletedThenCreatedKey", Value = 53838 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            Sys.Stop(dictionary);
            dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(guid)));

            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "NonexistentKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.NotFound>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "CreatedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 47474);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "DeletedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.NotFound>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "ReplacedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 7647);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "DeletedThenCreatedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 53838);
        }

        [Fact]
        public void AllowsChangesFollowingRestoreOfEmpty()
        {
            var guid = Guid.NewGuid();

            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(guid)));
            Sys.Stop(dictionary);
            dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(guid)));
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "CreatedKey", Value = 47474 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "DeletedKey", Value = 6336363 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Delete { Key = "DeletedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Deleted>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "ReplacedKey", Value = 9979 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "ReplacedKey", Value = 7647 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "DeletedThenCreatedKey", Value = 55525 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Delete { Key = "DeletedThenCreatedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Deleted>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "DeletedThenCreatedKey", Value = 53838 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);

            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "NonexistentKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.NotFound>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "CreatedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 47474);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "DeletedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.NotFound>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "ReplacedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 7647);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "DeletedThenCreatedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 53838);
        }

        [Fact]
        public void AllowsChangesFollowingRestoreOfNonEmpty()
        {
            var guid = Guid.NewGuid();

            var dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(guid)));
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "CreatedKey", Value = 47474 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "DeletedKey", Value = 6336363 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Delete { Key = "DeletedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Deleted>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "ReplacedKey", Value = 9979 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "ReplacedKey", Value = 7647 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "DeletedThenCreatedKey", Value = 55525 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Delete { Key = "DeletedThenCreatedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Deleted>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "DeletedThenCreatedKey", Value = 53838 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "CreatedSubsequentlyReplacedKey", Value = 74674 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "CreatedSubsequentlyDeletedKey", Value = 4764764 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "DeletedSubsequentlyRecreatedKey", Value = 653653 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            Sys.Stop(dictionary);
            dictionary = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(guid)));
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "SubsequentlyCreatedKey", Value = 737437 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "SubsequentlyDeletedKey", Value = 8456468 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Delete { Key = "SubsequentlyDeletedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Deleted>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "SubsequentlyReplacedKey", Value = 2472742 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "SubsequentlyReplacedKey", Value = 8353853 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "SubsequentlyDeletedThenCreatedKey", Value = 27242 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Delete { Key = "SubsequentlyDeletedThenCreatedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Deleted>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "SubsequentlyDeletedThenCreatedKey", Value = 728338 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "CreatedSubsequentlyReplacedKey", Value = 363383 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Delete { Key = "CreatedSubsequentlyDeletedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Deleted>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Specify { Key = "DeletedSubsequentlyRecreatedKey", Value = 653653 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionary);

            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "NonexistentKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.NotFound>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "CreatedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 47474);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "DeletedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.NotFound>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "ReplacedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 7647);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "DeletedThenCreatedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 53838);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "SubsequentlyCreatedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 737437);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "SubsequentlyDeletedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.NotFound>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "SubsequentlyReplacedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 8353853);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "SubsequentlyDeletedThenCreatedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 728338);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "CreatedSubsequentlyReplacedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 363383);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "CreatedSubsequentlyDeletedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.NotFound>(dictionary);
            dictionary.Tell(new PersistableDictionary<string, int>.Get { Key = "DeletedSubsequentlyRecreatedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionary, gp => gp.Value == 653653);
        }

        [Fact]
        public void KeepsSeparateRecordsOfDifferentGuids()
        {
            var guidA = Guid.NewGuid();
            var dictionaryA = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(guidA)));
            dictionaryA.Tell(new PersistableDictionary<string, int>.Specify { Key = "CreatedKey", Value = 47474 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionaryA);
            var guidB = Guid.NewGuid();
            var dictionaryB = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(guidB)));
            dictionaryB.Tell(new PersistableDictionary<string, int>.Specify { Key = "CreatedKey", Value = 2424 });
            ExpectMsgFrom<PersistableDictionary<string, int>.Specified>(dictionaryB);
            Sys.Stop(dictionaryA);
            Sys.Stop(dictionaryB);
            dictionaryB = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(guidB)));
            dictionaryA = Sys.ActorOf(Props.Create(() => new PersistableDictionary<string, int>(guidA)));

            dictionaryB.Tell(new PersistableDictionary<string, int>.Get { Key = "CreatedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionaryB, gp => gp.Value == 2424);
            dictionaryA.Tell(new PersistableDictionary<string, int>.Get { Key = "CreatedKey" });
            ExpectMsgFrom<PersistableDictionary<string, int>.Got>(dictionaryA, gp => gp.Value == 47474);
        }
    }
}
