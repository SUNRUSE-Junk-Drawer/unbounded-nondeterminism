using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Web;

namespace UnboundedNondeterminism.Web
{
    /// <summary>Implements tasks which need to be performed to match <see cref="ContentType"/>s between <see cref="HttpRequest.AcceptTypes"/> and those we support.</summary>
    public sealed class ContentTypeMatcherTasks : ReceiveActor
    {
        /// <summary>A request to parse a set of <see cref="string"/>s into <see cref="ContentType"/>s, and sort them by <see cref="ContentTypeParser.Parsed.Priority"/> descending.</summary>
        public sealed class ParseAndSort
        {
            /// <summary>The <see cref="ContentType"/>s received in <see cref="HttpRequest.AcceptTypes"/>.</summary>
            public IEnumerable<string> ContentTypes;

            /// <summary>The default <see cref="Encoding"/> to use if one cannot be found in <see cref="ContentType"/>s.</summary>
            public Encoding RequestDefault;
        }

        /// <summary>Returned in response to <see cref="ParseAndSort"/>.</summary>
        public sealed class ParsedAndSorted
        {
            /// <summary>The parsed <see cref="ContentType"/>s, if any, sorted by <see cref="ContentTypeParser.Parsed.Priority"/> descending.</summary>
            public IEnumerable<ContentTypeParser.Parsed> ContentTypes;
        }

        /// <inheritdoc />
        /// <param name="contentTypeParser">The <see cref="ContentTypeParser"/> to defer to.</param>
        public ContentTypeMatcherTasks(IActorRef contentTypeParser)
        {
            Receive<ParseAndSort>(pas => !pas.ContentTypes.Any(), pas => Sender.Tell(new ParsedAndSorted { ContentTypes = Enumerable.Empty<ContentTypeParser.Parsed>() }));
            Receive<ParseAndSort>(pas => Context.ActorOf(Props.Create(() => new ParseAndSortAggregator(contentTypeParser, Self, Sender, pas.ContentTypes, pas.RequestDefault))));
        }

        /// <summary>Aggregates responses from <see cref="ContentTypeParser"/> to generate a completed <see cref="ParsedAndSorted"/>.</summary>
        /// <remarks>One instance is created per <see cref="ParseAndSort"/>.</remarks>
        public sealed class ParseAndSortAggregator : ReceiveActor
        {
            /// <inheritdoc />
            /// <param name="contentTypeParser">The <see cref="ContentTypeParser"/> to defer to.</param>
            /// <param name="sender">The <see cref="ContentTypeMatcherTasks"/>.</param>
            /// <param name="recipient">The <see cref="IActorRef"/> to send the resulting <see cref="ParsedAndSorted"/> to.</param>
            /// <param name="contentTypes">The <see cref="string"/>s to parse to <see cref="ContentType"/>s.</param>
            /// <param name="requestDefault">The default <see cref="Encoding"/> to use if one cannot be found in the parsed <see cref="ContentType"/>.</param>
            public ParseAndSortAggregator(IActorRef contentTypeParser, IActorRef sender, IActorRef recipient, IEnumerable<string> contentTypes, Encoding requestDefault)
            {
                var remaining = contentTypes.Count();
                var parsed = new List<ContentTypeParser.Parsed>();

                foreach (var contentType in contentTypes) contentTypeParser.Tell(new ContentTypeParser.Parse { ContentType = contentType, RequestDefault = requestDefault });

                Action checkDone = () =>
                {
                    remaining--;
                    if (remaining == 0) recipient.Tell(new ParsedAndSorted { ContentTypes = parsed.OrderByDescending(p => p.Priority) }, sender);
                };

                Receive<ContentTypeParser.Parsed>(p =>
                {
                    parsed.Add(p);
                    checkDone();
                });

                Receive<ContentTypeParser.Unparseable>(u => checkDone());
            }
        }
    }
}