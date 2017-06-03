using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace UnboundedNondeterminism.Web.Handlers
{
    /// <summary>An actor to handle requests from <see cref="Controllers.RulesController"/>.</summary>
    public sealed class Rules : ReceiveActor
    {
        /// <inheritdoc />
        public Rules()
        {
            Receive<Requests.RulesPost>(p => Sender.Tell(new Error { StatusCode = HttpStatusCode.NotImplemented }));
            Receive<Requests.RulesGet>(g => Sender.Tell(new Error { StatusCode = HttpStatusCode.NotImplemented }));
            Receive<Requests.RulesPut>(p => Sender.Tell(new Error { StatusCode = HttpStatusCode.NotImplemented }));
            Receive<Requests.RulesDelete>(d => Sender.Tell(new Error { StatusCode = HttpStatusCode.NotImplemented }));
        }
    }
}