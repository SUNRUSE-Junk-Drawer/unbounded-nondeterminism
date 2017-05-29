using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Web;

namespace UnboundedNondeterminism.Web
{
    /// <summary>Implements tasks which need to be performed to parse a <see cref="string"/> as a <see cref="ContentType"/>.</summary>
    public sealed class ContentTypeParserTasks : ReceiveActor
    {
        /// <summary>A request to parse a <see cref="string"/> to a <see cref="ContentType"/>.</summary>
        public sealed class Parse
        {
            /// <summary>The <see cref="string"/> to parse to a <see cref="ContentType"/>.</summary>
            public string ContentType;
        }

        /// <summary>Returned in response to <see cref="Parse"/> when <see cref="Parse.ContentType"/> was parseable.</summary>
        public sealed class Parsed
        {
            /// <summary>The <see cref="ContentType"/> parsed from <see cref="Parse.ContentType"/>.</summary>
            public ContentType ContentType;
        }

        /// <summary>Returned in response to <see cref="Parse"/> when <see cref="Parse.ContentType"/> was not parseable.</summary>
        public sealed class Unparseable { }

        /// <summary>A request to extract the part before the slash in <see cref="ContentType.MediaType"/>.</summary>
        public sealed class GetLeft
        {
            /// <summary>The <see cref="ContentType"/> to parse.</summary>
            public ContentType ContentType;
        }

        /// <summary>Returned in response to <see cref="GetLeft"/>.</summary>
        public sealed class GotLeft
        {
            /// <summary>The part before the slash in <see cref="ContentType.MediaType"/>.  If not specified, "*".</summary>
            public string Left;
        }

        /// <summary>A request to extract the part following the slash in <see cref="ContentType.MediaType"/>.</summary>
        public sealed class GetRight
        {
            /// <summary>The <see cref="ContentType"/> to parse.</summary>
            public ContentType ContentType;
        }

        /// <summary>Returned in response to <see cref="GetRight"/>.</summary>
        public sealed class GotRight
        {
            /// <summary>The part following the slash in <see cref="ContentType.MediaType"/>.  If not specified, "*".</summary>
            public string Right;
        }

        /// <summary>A request to extract the part following the first plus following the first slash in <see cref="ContentType.MediaType"/>.</summary>
        public sealed class GetSuffix
        {
            /// <summary>The <see cref="ContentType"/> to parse.</summary>
            public ContentType ContentType;
        }

        /// <summary>Returned in response to <see cref="GetSuffix"/>.</summary>
        public sealed class GotSuffix
        {
            /// <summary>The part following the first plus following the first slash in <see cref="ContentType.MediaType"/>.</summary>
            public string Suffix;
        }

        /// <summary>A request to determine which <see cref="Encoding"/> to use.</summary>
        public sealed class GetEncoding
        {
            /// <summary>The <see cref="ContentType"/> to parse.</summary>
            public ContentType ContentType;

            /// <summary>The default <see cref="Encoding"/> to use if one cannot be found in <see cref="ContentType"/>.</summary>
            public Encoding RequestDefault;
        }

        /// <summary>Returned in response to <see cref="GetEncoding"/> when the <see cref="Encoding"/> is missing or parseable.</summary>
        public sealed class GotEncoding
        {
            /// <summary>The <see cref="Encoding"/> to use in the response body.</summary>
            public Encoding Encoding;
        }

        /// <summary>Returned in response to <see cref="GetEncoding"/> when the <see cref="Encoding"/> is present but unparseable.</summary>
        public sealed class EncodingNotParseable { }

        /// <summary>A request to determine the <see cref="GotPriority.Priority"/> of a <see cref="ContentType"/>.</summary>
        public sealed class GetPriority
        {
            /// <summary>The <see cref="ContentType"/> to parse.</summary>
            public ContentType ContentType;
        }

        /// <summary>Returned in response to <see cref="GetPriority"/>.</summary>
        public sealed class GotPriority
        {
            /// <summary>The priority of the parsed <see cref="ContentType"/>, where greater values should take priority.  If unparsable or missing, 1.0.</summary>
            public decimal Priority;
        }

        /// <inheritdoc />
        public ContentTypeParserTasks()
        {
            Receive<Parse>(p =>
            {
                object response = new Unparseable();
                try
                {
                    response = new Parsed { ContentType = new ContentType(p.ContentType) };
                }
                catch { }
                Sender.Tell(response);
            });

            Receive<GetLeft>(gl =>
            {
                var left = gl.ContentType.MediaType.Substring(0, gl.ContentType.MediaType.IndexOf('/'));
                var indexOfPlus = left.IndexOf('+');
                switch (indexOfPlus)
                {
                    case -1: Sender.Tell(new GotLeft { Left = left }); break;
                    case 0: Sender.Tell(new GotLeft { Left = "*" }); break;
                    default: Sender.Tell(new GotLeft { Left = left.Substring(0, indexOfPlus) }); break;
                }
            });

            Receive<GetRight>(gr =>
            {
                var right = gr.ContentType.MediaType.Substring(gr.ContentType.MediaType.IndexOf('/') + 1);
                var indexOfPlus = right.IndexOf('+');
                switch (indexOfPlus)
                {
                    case -1: Sender.Tell(new GotRight { Right = right }); break;
                    case 0: Sender.Tell(new GotRight { Right = "*" }); break;
                    default: Sender.Tell(new GotRight { Right = right.Substring(0, indexOfPlus) }); break;
                }
            });

            Receive<GetSuffix>(gs =>
            {
                var right = gs.ContentType.MediaType.Substring(gs.ContentType.MediaType.IndexOf('/') + 1);
                var indexOfPlus = right.IndexOf('+');
                Sender.Tell(new GotSuffix { Suffix = indexOfPlus == -1 ? "" : right.Substring(indexOfPlus + 1) });
            });

            Receive((GetEncoding ge) =>
            {
                var encoding = ge.RequestDefault;
                var charset = ge.ContentType.Parameters["charset"];
                if (!string.IsNullOrWhiteSpace(charset)) try
                    {
                        encoding = Encoding.GetEncoding(charset);
                    }
                    catch
                    {
                        Sender.Tell(new EncodingNotParseable());
                        return;
                    }
                Sender.Tell(new GotEncoding { Encoding = encoding });
            });

            Receive<GetPriority>(gp =>
            {
                decimal priority;
                if (!decimal.TryParse(gp.ContentType.Parameters["q"], out priority)) priority = 1.0m;
                Sender.Tell(new GotPriority { Priority = priority });
            });
        }
    }
}