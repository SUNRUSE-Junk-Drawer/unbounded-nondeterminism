using Akka.Actor;
using Akka.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnboundedNondeterminism
{
    /// <summary>
    /// A base class which extends <see cref="ReceivePersistentActor"/> to:
    /// • Use a <see cref="Eventsourced.PersistenceId"/> generated from the <see cref="MemberInfo.Name"/> and a <see cref="Guid"/>.
    /// • Handle a <see cref="PersistableBase.Stop"/> message to safely stop the actor.
    /// </summary>
    public abstract class PersistableBase : ReceivePersistentActor
    {
        /// <summary>Instructs an <see cref="ActorBase"/> to stop when possible.</summary>
        /// <remarks>http://getakka.net/docs/persistence/persistent-actors#safely-shutting-down-persistent-actors</remarks>
        public sealed class Stop { }

        /// <summary>Returned in response to <see cref="Stop"/> when the <see cref="ActorBase"/> has stopped.</summary>
        public sealed class Stopped { }

        /// <summary>A <see cref="Guid"/> for <see langword="this"/>.</summary>
        public readonly Guid PersistenceGuid;

        /// <inheritdoc />
        public override string PersistenceId => $"{GetType().Name}-{PersistenceGuid}";

        /// <inheritdoc />
        /// <param name="persistenceGuid"><see cref="PersistenceGuid"/>.</param>
        public PersistableBase(Guid persistenceGuid)
        {
            PersistenceGuid = persistenceGuid;

            Command<Stop>(s => Context.Stop(Self));
        }
    }
}
