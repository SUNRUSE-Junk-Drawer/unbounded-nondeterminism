using System.Reflection;
using System.Web.Http;
using UnboundedNondeterminism.Tests;
using Xunit;

namespace UnboundedNondeterminism.Web.Tests
{
    public sealed class GlobalTests : ConfiguredTestKit
    {
        protected override void AfterAll()
        {
            base.AfterAll();
            Global.ActorSystem = null;
            Global.GlobalVariablesHandler = null;
            Global.RulesetsHandler = null;
            Global.RulesHandler = null;
            GlobalConfiguration.Configuration.Routes.Clear();
        }

        [Fact]
        public void StartCreatesANewActorSystem()
        {
            var global = new Global();   

            typeof(Global).GetMethod("Application_Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(global, new object[] { null, null });

            Assert.NotNull(Global.ActorSystem);
        }

        [Fact]
        public void StartNamesTheActorSystemUnboundedNondeterminism()
        {
            var global = new Global();

            typeof(Global).GetMethod("Application_Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(global, new object[] { null, null });

            Assert.Equal("UnboundedNondeterminism", Global.ActorSystem.Name);
        }

        [Fact]
        public void StartDoesNotDisposeOfTheActorSystem()
        {
            var global = new Global();

            typeof(Global).GetMethod("Application_Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(global, new object[] { null, null });

            Assert.False(Global.ActorSystem.WhenTerminated.IsCompleted);
        }

        [Fact]
        public void StartCreatesInstancesOfHandlerActors()
        {
            var global = new Global();

            typeof(Global).GetMethod("Application_Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(global, new object[] { null, null });

            Assert.NotNull(Global.RulesetsHandler);
            Assert.NotNull(Global.RulesHandler);
            Assert.NotNull(Global.GlobalVariablesHandler);
        }

        [Fact]
        public void StartConfiguresRoutes()
        {
            var global = new Global();

            typeof(Global).GetMethod("Application_Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(global, new object[] { null, null });

            var globalVariablesPut = GlobalConfiguration.Configuration.Routes["GlobalVariablesPut"];
            Assert.Empty(globalVariablesPut.Constraints);
            Assert.Null(globalVariablesPut.DataTokens);
            Assert.Equal("GlobalVariables", globalVariablesPut.Defaults["Controller"]);
            Assert.Equal("Put", globalVariablesPut.Defaults["Action"]);
            Assert.Null(globalVariablesPut.Handler);
            Assert.Equal("rulesets/{rulesetId}/global-variables/{globalVariableId}", globalVariablesPut.RouteTemplate);
        }

        [Fact]
        public void EndRemovesTheReferenceToTheActorSystem()
        {
            var global = new Global();
            typeof(Global).GetMethod("Application_Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(global, new object[] { null, null });

            typeof(Global).GetMethod("Application_End", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(global, new object[] { null, null });

            Assert.Null(Global.ActorSystem);
        }

        [Fact]
        public void EndDisposesOfTheActorSystem()
        {
            var global = new Global();
            typeof(Global).GetMethod("Application_Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(global, new object[] { null, null });
            var actorSystem = Global.ActorSystem;

            typeof(Global).GetMethod("Application_End", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(global, new object[] { null, null });

            AwaitCondition(() => actorSystem.WhenTerminated.IsCompleted);
        }
    }
}