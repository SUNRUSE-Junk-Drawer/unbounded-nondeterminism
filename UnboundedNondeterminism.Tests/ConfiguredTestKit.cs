using Akka.TestKit.Xunit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnboundedNondeterminism.Tests
{
    public abstract class ConfiguredTestKit : TestKit
    {
        public ConfiguredTestKit() : base(@"
            akka {
                actor {
                    serialize-messages = on
                    serialize-creators = on
                }
            }
")
        {}
    }
}
