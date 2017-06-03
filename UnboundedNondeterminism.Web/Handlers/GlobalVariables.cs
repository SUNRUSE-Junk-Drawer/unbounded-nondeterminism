using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace UnboundedNondeterminism.Web.Handlers
{
    /// <summary>An actor to handle requests from <see cref="Controllers.GlobalVariablesController"/>.</summary>
    public sealed class GlobalVariables : ReceiveActor
    {
        /// <inheritdoc />
        public GlobalVariables()
        {
            Receive<Requests.GlobalVariablesPost>(p => Sender.Tell(new Error { StatusCode = HttpStatusCode.NotImplemented }));
            Receive<Requests.GlobalVariablesPut>(p => Sender.Tell(new Error { StatusCode = HttpStatusCode.NotImplemented }));
            Receive<Requests.GlobalVariablesDelete>(d => Sender.Tell(new Error { StatusCode = HttpStatusCode.NotImplemented }));
        }
    }
}