using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace UnboundedNondeterminism.Web.Handlers
{
    /// <summary>An actor to handle requests from <see cref="Controllers.RulesetsController"/>.</summary>
    public sealed class Rulesets : ReceiveActor
    {
        /// <inheritdoc />
        public Rulesets()
        {
            Receive<Requests.RulesetsPost>(p => Sender.Tell(new Error { StatusCode = HttpStatusCode.NotImplemented }));
            Receive<Requests.RulesetsGet>(g => Sender.Tell(new Error { StatusCode = HttpStatusCode.NotImplemented }));
            Receive<Requests.RulesetsDelete>(d => Sender.Tell(new Error { StatusCode = HttpStatusCode.NotImplemented }));
        }
    }
}