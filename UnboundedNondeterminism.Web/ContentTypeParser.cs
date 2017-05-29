using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Web;

namespace UnboundedNondeterminism.Web
{
    /// <summary>Parses <see cref="string"/>s from the client into <see cref="ContentType"/> instances.</summary>
    public sealed class ContentTypeParser : ReceiveActor
    {
        /// <summary>A request to parse a <see cref="string"/> into a <see cref="ContentType"/>.</summary>
        public sealed class Parse
        {
            /// <summary>The <see cref="string"/> to parse into a <see cref="ContentType"/>.</summary>
            public string ContentType;

            /// <summary>The default <see cref="Encoding"/> to use if one cannot be found in the parsed <see cref="ContentType"/>.</summary>
            public Encoding RequestDefault;
        }

        /// <summary>Returned in response to <see cref="Parse"/> when <see cref="Parse.ContentType"/> was parseable to a <see cref="ContentType"/>.</summary>
        public sealed class Parsed
        {
            /// <summary>The part to the left of the forward slash.</summary>
            /// <remarks>Defaults to "*".</remarks>
            public string Left;

            /// <summary>The part to the right of the forward slash, up to the first "+".</summary>
            /// <remarks>Defaults to "*".</remarks>
            public string Right;

            /// <summary>The part following "+" to the right of the forward slash.</summary>
            /// <remarks>Defaults to an empty string.</remarks>
            public string Suffix;

            /// <summary>The <see cref="Encoding"/> to use.</summary>
            /// <remarks>Defaults to <see cref="HttpRequest.ContentEncoding"/>.</remarks>
            public Encoding Encoding;

            /// <summary>The priority of the content type, where 0 is low and 1 is high.</summary>
            /// <remarks>The "q" parameter.  Defaults to 1.</remarks>
            public decimal Priority;
        }

        /// <summary>Returned in response to <see cref="Parse"/> when <see cref="Parse.ContentType"/> was not parseable to a <see cref="ContentType"/>.</summary>
        public sealed class Unparseable { }

        /// <inheritdoc />
        /// <param name="contentTypeParserTasks">The <see cref="ContentTypeParserTasks"/> to defer to.</param>
        public ContentTypeParser(IActorRef contentTypeParserTasks)
        {
            Receive<Parse>(p => Context.ActorOf(Props.Create(() => new Aggregator(contentTypeParserTasks, Self, Sender, p.ContentType, p.RequestDefault))));
        }

        /// <summary>Aggregates responses from <see cref="ContentTypeParserTasks"/> to generate a completed <see cref="Parsed"/> or <see cref="Unparseable"/>.</summary>
        /// <remarks>One instance is created per <see cref="Parse"/>.</remarks>
        public sealed class Aggregator : ReceiveActor
        {
            /// <inheritdoc />
            /// <param name="contentTypeParserTasks">The <see cref="ContentTypeParserTasks"/> to defer to.</param>
            /// <param name="sender">The <see cref="ContentTypeParser"/>.</param>
            /// <param name="recipient">The <see cref="IActorRef"/> to send the resulting <see cref="Parsed"/> or <see cref="Unparseable"/> to.</param>
            /// <param name="contentType">The <see cref="string"/> to parse to a <see cref="ContentType"/>.</param>
            /// <param name="requestDefault">The default <see cref="Encoding"/> to use if one cannot be found in the parsed <see cref="ContentType"/>.</param>
            public Aggregator(IActorRef contentTypeParserTasks, IActorRef sender, IActorRef recipient, string contentType, Encoding requestDefault)
            {
                contentTypeParserTasks.Tell(new ContentTypeParserTasks.Parse { ContentType = contentType } );

                Receive<ContentTypeParserTasks.Unparseable>(u =>
                {
                    recipient.Tell(new Unparseable(), sender);
                    Context.Stop(Self);
                });
                Receive<ContentTypeParserTasks.Parsed>(p =>
                {
                    contentTypeParserTasks.Tell(new ContentTypeParserTasks.GetLeft { ContentType = p.ContentType });
                    contentTypeParserTasks.Tell(new ContentTypeParserTasks.GetRight { ContentType = p.ContentType });
                    contentTypeParserTasks.Tell(new ContentTypeParserTasks.GetSuffix { ContentType = p.ContentType });
                    contentTypeParserTasks.Tell(new ContentTypeParserTasks.GetEncoding { ContentType = p.ContentType, RequestDefault = requestDefault });
                    contentTypeParserTasks.Tell(new ContentTypeParserTasks.GetPriority { ContentType = p.ContentType });
                });

                var haveLeft = false;
                var left = "";
                var haveRight = false;
                var right = "";
                var haveSuffix = false;
                var suffix = "";
                var haveEncoding = false;
                var encoding = Encoding.Default;
                var havePriority = false;
                var priority = 0.0m;

                Action checkDone = () =>
                {
                    if (!haveLeft) return;
                    if (!haveRight) return;
                    if (!haveSuffix) return;
                    if (!haveEncoding) return;
                    if (!havePriority) return;
                    recipient.Tell(new Parsed { Left = left, Right = right, Suffix = suffix, Encoding = encoding, Priority = priority }, sender);
                    Context.Stop(Self);
                };

                Receive<ContentTypeParserTasks.GotLeft>(gl =>
                {
                    left = gl.Left;
                    haveLeft = true;
                    checkDone();
                });

                Receive<ContentTypeParserTasks.GotRight>(gr =>
                {
                    right = gr.Right;
                    haveRight = true;
                    checkDone();
                });

                Receive<ContentTypeParserTasks.GotSuffix>(gs =>
                {
                    suffix = gs.Suffix;
                    haveSuffix = true;
                    checkDone();
                });

                Receive<ContentTypeParserTasks.GotEncoding>(ge =>
                {
                    encoding = ge.Encoding;
                    haveEncoding = true;
                    checkDone();
                });
                Receive<ContentTypeParserTasks.EncodingNotParseable>(enp =>
                {
                    recipient.Tell(new Unparseable(), sender);
                    Context.Stop(Self);
                });

                Receive<ContentTypeParserTasks.GotPriority>(gp =>
                {
                    priority = gp.Priority;
                    havePriority = true;
                    checkDone();
                });
            }
        }
    }
}