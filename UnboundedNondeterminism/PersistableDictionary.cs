using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnboundedNondeterminism
{
    /// <summary>Wraps a <see cref="Dictionary{TKey, TValue}"/>, persisting it.</summary>
    /// <typeparam name="TKey">The <see cref="Type"/> to use as a key.</typeparam>
    /// <typeparam name="TValue">The <see cref="Type"/> to use as a value.</typeparam>
    public sealed class PersistableDictionary<TKey, TValue> : PersistableBase
    {
        /// <summary>Tell to set a <typeparamref name="TKey" /> to a <typeparamref name="TValue" />.</summary>
        /// <remarks>The property will be created if it does not exist, or replaced if it does.</remarks>
        public sealed class Specify
        {
            /// <summary>The <typeparamref name="TKey" /> of the property to create or replace.</summary>
            public TKey Key;

            /// <summary>The <typeparamref name="TValue" /> to create/replace with.</summary>
            public TValue Value;
        }

        /// <summary>Returned in response to <see cref="Specify"/>.</summary>
        public sealed class Specified { }


        /// <summary>Tell to remove a property if it exists.</summary>
        public sealed class Delete
        {
            /// <summary>The <typeparamref name="TKey" /> of the property to remove.</summary>
            public TKey Key;
        }

        /// <summary>Returned in response to <see cref="Delete"/>.</summary>
        /// <remarks>Returned regardless of whether the property existed to delete.</remarks>
        public sealed class Deleted { }


        /// <summary>Retrieves the <typeparamref name="TValue" /> of a property by its <typeparamref name="TKey" />.</summary>
        public sealed class Get
        {
            /// <summary>The <typeparamref name="TKey" /> of the property to retrieve.</summary>
            public TKey Key;
        }

        /// <summary>Returned in response to <see cref="Get"/> when the property existed.</summary>
        public sealed class Got
        {
            /// <summary>The <typeparamref name="TValue" /> of the property.</summary>
            public TValue Value;
        }

        /// <summary>Returned in response to <see cref="Get"/> when the property did not exist.</summary>
        public sealed class NotFound { }

        /// <inheritdoc />
        public PersistableDictionary(Guid persistenceGuid) : base(persistenceGuid)
        {
            var properties = new Dictionary<TKey, TValue>();

            Command<Specify>(sp => Persist(sp, psp =>
            {
                properties[psp.Key] = psp.Value;
                Sender.Tell(new Specified());
            }));
            Recover<Specify>(sp => properties[sp.Key] = sp.Value);

            Command<Delete>(dp => Persist(dp, pdp => 
            {
                properties.Remove(pdp.Key);
                Sender.Tell(new Deleted());
            }));
            Recover<Delete>(dp => properties.Remove(dp.Key));

            Command<Get>(gp => !properties.ContainsKey(gp.Key), gp => Sender.Tell(new NotFound()));
            Command<Get>(gp => Sender.Tell(new Got { Value = properties[gp.Key] }));
        }
    }
}
