using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UnboundedNondeterminism
{
    /// <summary>A persistable store of <see cref="PersistableBase"/>s which can be created and deleted.</summary>
    /// <typeparam name="T">The <see cref="Type"/> to create instances of </typeparam>
    public sealed class PersistableFactory<T> : PersistableBase where T : PersistableBase
    {
        /// <summary>Create a new <typeparamref name="T" />.</summary>
        public sealed class Create { }

        /// <summary>Returned in response to <see cref="Create"/>.</summary>
        public sealed class Created
        {
            /// <summary>The <see cref="PersistableBase.PersistenceGuid"/> of the created <typeparamref name="T" />.</summary>
            public Guid PersistenceGuid;
        }

        /// <summary>Passes a message onto a previously created <typeparamref name="T" />.</summary>
        public sealed class Forward
        {
            /// <summary>The message to pass to the <typeparamref name="T" /> specified by <see cref="PersistenceGuid"/></summary>
            public object Message;

            /// <summary>The <see cref="PersistableBase.PersistenceGuid"/> of the previously created <typeparamref name="T" />.</summary>
            public Guid PersistenceGuid;
        }

        /// <summary>Tell to <see cref="ActorSystem.Stop"/> and delete our reference to a previously created <typeparamref name="T" /> if it exists.</summary>
        public sealed class Delete
        {
            /// <summary>The <see cref="PersistableBase.PersistenceGuid"/> of the <typeparamref name="T" /> to remove.</summary>
            public Guid PersistenceGuid;
        }

        /// <summary>Returned in response to <see cref="Delete"/>, or <see cref="Forward"/> when the <typeparamref name="T" /> associated with <see cref="Forward.PersistenceGuid"/> was previously deleted.</summary>
        public sealed class Deleted { }

        /// <summary>Returned in response to <see cref="Delete"/> or <see cref="Forward"/> when no <typeparamref name="T" /> associated with <see cref="Delete.PersistenceGuid"/>/<see cref="Forward.PersistenceGuid"/> was ever created.</summary>
        public sealed class NeverCreated { }

        private readonly Dictionary<Guid, IActorRef> Instances = new Dictionary<Guid, IActorRef>();
        private readonly HashSet<Guid> DeletedInstances = new HashSet<Guid>();

        /// <inheritdoc />
        protected override void PostStop()
        {
            foreach (var instance in Instances.Values) if (instance != null) instance.Tell(new Stop());
            Instances.Clear();
            base.PostStop();
        }

        /// <inheritdoc />
        public PersistableFactory(Guid persistenceGuid) : base(persistenceGuid)
        {
            Command<Create>(c => Persist(new Created { PersistenceGuid = Guid.NewGuid() }, pc =>
            {
                Instances[pc.PersistenceGuid] = null;
                Sender.Tell(pc);
            }));
            Recover<Created>(c => Instances[c.PersistenceGuid] = null);

            Command<Delete>(d => DeletedInstances.Contains(d.PersistenceGuid), d => Sender.Tell(new Deleted()));
            Command<Delete>(d => !Instances.ContainsKey(d.PersistenceGuid), d => Sender.Tell(new NeverCreated()));
            Command<Delete>(d => Persist(d, pd =>
            {
                if (Instances[pd.PersistenceGuid] != null) Instances[pd.PersistenceGuid].Tell(new Stop());
                Instances.Remove(pd.PersistenceGuid);
                DeletedInstances.Add(pd.PersistenceGuid);
                Sender.Tell(new Deleted());
            }));
            Recover<Delete>(d =>
            {
                Instances.Remove(d.PersistenceGuid);
                DeletedInstances.Add(d.PersistenceGuid);
            });

            Command<Forward>(d => DeletedInstances.Contains(d.PersistenceGuid), d => Sender.Tell(new Deleted()));
            Command<Forward>(d => !Instances.ContainsKey(d.PersistenceGuid), d => Sender.Tell(new NeverCreated()));
            Command<Forward>(d => Instances[d.PersistenceGuid] == null, d =>
            {
                var actor = Context.ActorOf(Props.Create<T>(Expression.Lambda<Func<T>>(Expression.New(typeof(T).GetConstructor(new[] { typeof(Guid) }), new[] { Expression.Constant(d.PersistenceGuid) }), Enumerable.Empty<ParameterExpression>())));
                Instances[d.PersistenceGuid] = actor;
                actor.Tell(d.Message, Sender);
            });
            Command<Forward>(d => Instances[d.PersistenceGuid].Tell(d.Message, Sender));
        }
    }
}
