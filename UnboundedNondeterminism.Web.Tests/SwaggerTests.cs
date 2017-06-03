using Akka.Actor;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using UnboundedNondeterminism.Tests;
using Xunit;

namespace UnboundedNondeterminism.Web.Tests
{
    public sealed class SwaggerTests : ConfiguredTestKit
    {
        public sealed class UnexpectedResponse { }

        #region NoParamsWithResponse
        [Fact]
        public async Task NoParamsWithResponseForwardsResponseToClient()
        {
            var rulesetsHandler = CreateTestProbe();
            Global.RulesetsHandler = rulesetsHandler.Ref;
            var task = new Controllers.RulesetsController().Post();
            rulesetsHandler.ExpectMsg<Requests.RulesetsPost>();
            var guid = Guid.NewGuid();
            rulesetsHandler.LastSender.Tell(new Definitions.IdWrapper { Id = guid }, rulesetsHandler.Ref);

            var response = await task;

            Assert.NotNull(response);
            Assert.Equal(guid, response.Id);
        }

        [Fact]
        public async Task NoParamsWithResponseSendsGatewayTimeoutToClientWhenNoResponseFromHandler()
        {
            var rulesetsHandler = CreateTestProbe();
            Global.RulesetsHandler = rulesetsHandler.Ref;

            var exception = await Record.ExceptionAsync((new Controllers.RulesetsController()).Post);

            Assert.NotNull(exception);
            var typedException = Assert.IsType<HttpResponseException>(exception);
            Assert.Equal(HttpStatusCode.GatewayTimeout, typedException.Response.StatusCode);
        }

        [Fact]
        public async Task NoParamsWithResponseSendsInternalServerErrorToClientWhenUnexpectedResponseFromHandler()
        {
            var rulesetsHandler = CreateTestProbe();
            Global.RulesetsHandler = rulesetsHandler.Ref;
            var task = new Controllers.RulesetsController().Post();
            rulesetsHandler.ExpectMsg<Requests.RulesetsPost>();
            rulesetsHandler.LastSender.Tell(new UnexpectedResponse(), rulesetsHandler.Ref);

            var exception = await Record.ExceptionAsync(() => task);

            Assert.NotNull(exception);
            var typedException = Assert.IsType<HttpResponseException>(exception);
            Assert.Equal(HttpStatusCode.InternalServerError, typedException.Response.StatusCode);
        }

        [Fact]
        public async Task NoParamsWithResponseForwardsStatusCodeToClientWhenErrorFromHandler()
        {
            var rulesetsHandler = CreateTestProbe();
            Global.RulesetsHandler = rulesetsHandler.Ref;
            var task = new Controllers.RulesetsController().Post();
            rulesetsHandler.ExpectMsg<Requests.RulesetsPost>();
            rulesetsHandler.LastSender.Tell(new Error { StatusCode = HttpStatusCode.PaymentRequired }, rulesetsHandler.Ref);

            var exception = await Record.ExceptionAsync(() => task);

            Assert.NotNull(exception);
            var typedException = Assert.IsType<HttpResponseException>(exception);
            Assert.Equal(HttpStatusCode.PaymentRequired, typedException.Response.StatusCode);
        }
        #endregion

        #region UrlParamsOnlyWithResponse
        [Fact]
        public async Task UrlParamsOnlyWithResponseForwardsResponseToClient()
        {
            var globalVariablesHandler = CreateTestProbe();
            Global.GlobalVariablesHandler = globalVariablesHandler.Ref;
            var rulesetId = Guid.NewGuid();
            var task = new Controllers.GlobalVariablesController().Post(rulesetId);
            globalVariablesHandler.ExpectMsg<Requests.GlobalVariablesPost>(p => p.RulesetId == rulesetId);
            var globalVariableId = Guid.NewGuid();
            globalVariablesHandler.LastSender.Tell(new Definitions.IdNameWrapper { Id = globalVariableId, Name = "test name" }, globalVariablesHandler.Ref);

            var response = await task;

            Assert.NotNull(response);
            Assert.Equal(globalVariableId, response.Id);
            Assert.Equal("test name", response.Name);
        }

        [Fact]
        public async Task UrlParamsOnlyWithResponseSendsGatewayTimeoutToClientWhenNoResponseFromHandler()
        {
            var globalVariablesHandler = CreateTestProbe();
            Global.GlobalVariablesHandler = globalVariablesHandler.Ref;
            var rulesetId = Guid.NewGuid();

            var exception = await Record.ExceptionAsync(() => new Controllers.GlobalVariablesController().Post(rulesetId));

            Assert.NotNull(exception);
            var typedException = Assert.IsType<HttpResponseException>(exception);
            Assert.Equal(HttpStatusCode.GatewayTimeout, typedException.Response.StatusCode);
        }

        [Fact]
        public async Task UrlParamsOnlyWithResponseSendsInternalServerErrorToClientWhenUnexpectedResponseFromHandler()
        {
            var globalVariablesHandler = CreateTestProbe();
            Global.GlobalVariablesHandler = globalVariablesHandler.Ref;
            var rulesetId = Guid.NewGuid();
            var task = new Controllers.GlobalVariablesController().Post(rulesetId);
            globalVariablesHandler.ExpectMsg<Requests.GlobalVariablesPost>(p => p.RulesetId == rulesetId);
            globalVariablesHandler.LastSender.Tell(new UnexpectedResponse(), globalVariablesHandler.Ref);

            var exception = await Record.ExceptionAsync(() => task);

            Assert.NotNull(exception);
            var typedException = Assert.IsType<HttpResponseException>(exception);
            Assert.Equal(HttpStatusCode.InternalServerError, typedException.Response.StatusCode);
        }

        [Fact]
        public async Task UrlParamsOnlyWithResponseForwardsStatusCodeToClientWhenErrorFromHandler()
        {
            var globalVariablesHandler = CreateTestProbe();
            Global.GlobalVariablesHandler = globalVariablesHandler.Ref;
            var rulesetId = Guid.NewGuid();
            var task = new Controllers.GlobalVariablesController().Post(rulesetId);
            globalVariablesHandler.ExpectMsg<Requests.GlobalVariablesPost>(p => p.RulesetId == rulesetId);
            globalVariablesHandler.LastSender.Tell(new Error { StatusCode = HttpStatusCode.PaymentRequired }, globalVariablesHandler.Ref);

            var exception = await Record.ExceptionAsync(() => task);

            Assert.NotNull(exception);
            var typedException = Assert.IsType<HttpResponseException>(exception);
            Assert.Equal(HttpStatusCode.PaymentRequired, typedException.Response.StatusCode);
        }
        #endregion

        #region UrlParamsOnlyWithoutResponse
        [Fact]
        public async Task UrlParamsOnlyWithoutResponseForwardsResponseToClient()
        {
            var globalVariablesHandler = CreateTestProbe();
            Global.GlobalVariablesHandler = globalVariablesHandler.Ref;
            var rulesetId = Guid.NewGuid();
            var globalVariableId = Guid.NewGuid();
            var task = new Controllers.GlobalVariablesController().Delete(rulesetId, globalVariableId);
            globalVariablesHandler.ExpectMsg<Requests.GlobalVariablesDelete>(d => d.RulesetId == rulesetId && d.GlobalVariableId == globalVariableId);
            var guid = Guid.NewGuid();
            globalVariablesHandler.LastSender.Tell(new Success(), globalVariablesHandler.Ref);

            await task;
        }

        [Fact]
        public async Task UrlParamsOnlyWithoutResponseSendsGatewayTimeoutToClientWhenNoResponseFromHandler()
        {
            var globalVariablesHandler = CreateTestProbe();
            Global.GlobalVariablesHandler = globalVariablesHandler.Ref;
            var rulesetId = Guid.NewGuid();
            var globalVariableId = Guid.NewGuid();

            var exception = await Record.ExceptionAsync(() => new Controllers.GlobalVariablesController().Delete(rulesetId, globalVariableId));

            Assert.NotNull(exception);
            var typedException = Assert.IsType<HttpResponseException>(exception);
            Assert.Equal(HttpStatusCode.GatewayTimeout, typedException.Response.StatusCode);
        }

        [Fact]
        public async Task UrlParamsOnlyWithoutResponseSendsInternalServerErrorToClientWhenUnexpectedResponseFromHandler()
        {
            var globalVariablesHandler = CreateTestProbe();
            Global.GlobalVariablesHandler = globalVariablesHandler.Ref;
            var rulesetId = Guid.NewGuid();
            var globalVariableId = Guid.NewGuid();
            var task = new Controllers.GlobalVariablesController().Delete(rulesetId, globalVariableId);
            globalVariablesHandler.ExpectMsg<Requests.GlobalVariablesDelete>(d => d.RulesetId == rulesetId && d.GlobalVariableId == globalVariableId);
            globalVariablesHandler.LastSender.Tell(new UnexpectedResponse(), globalVariablesHandler.Ref);

            var exception = await Record.ExceptionAsync(() => task);

            Assert.NotNull(exception);
            var typedException = Assert.IsType<HttpResponseException>(exception);
            Assert.Equal(HttpStatusCode.InternalServerError, typedException.Response.StatusCode);
        }

        [Fact]
        public async Task UrlParamsOnlyWithoutResponseForwardsStatusCodeToClientWhenErrorFromHandler()
        {
            var globalVariablesHandler = CreateTestProbe();
            Global.GlobalVariablesHandler = globalVariablesHandler.Ref;
            var rulesetId = Guid.NewGuid();
            var globalVariableId = Guid.NewGuid();
            var task = new Controllers.GlobalVariablesController().Delete(rulesetId, globalVariableId);
            globalVariablesHandler.ExpectMsg<Requests.GlobalVariablesDelete>(d => d.RulesetId == rulesetId && d.GlobalVariableId == globalVariableId);
            globalVariablesHandler.LastSender.Tell(new Error { StatusCode = HttpStatusCode.PaymentRequired }, globalVariablesHandler.Ref);

            var exception = await Record.ExceptionAsync(() => task);

            Assert.NotNull(exception);
            var typedException = Assert.IsType<HttpResponseException>(exception);
            Assert.Equal(HttpStatusCode.PaymentRequired, typedException.Response.StatusCode);
        }
        #endregion

        #region UrlParamsAndBodyWithoutResponse
        [Fact]
        public async Task UrlParamsAndBodyWithoutResponseForwardsResponseToClient()
        {
            var globalVariablesHandler = CreateTestProbe();
            Global.GlobalVariablesHandler = globalVariablesHandler.Ref;
            var rulesetId = Guid.NewGuid();
            var globalVariableId = Guid.NewGuid();
            var task = new Controllers.GlobalVariablesController().Put(new Definitions.NameWrapper { Name = "test name" }, rulesetId, globalVariableId);
            globalVariablesHandler.ExpectMsg<Requests.GlobalVariablesPut>(p => p.Body.Name == "test name" && p.RulesetId == rulesetId && p.GlobalVariableId == globalVariableId);
            var guid = Guid.NewGuid();
            globalVariablesHandler.LastSender.Tell(new Success(), globalVariablesHandler.Ref);

            await task;
        }

        [Fact]
        public async Task UrlParamsAndBodyWithoutResponseSendsGatewayTimeoutToClientWhenNoResponseFromHandler()
        {
            var globalVariablesHandler = CreateTestProbe();
            Global.GlobalVariablesHandler = globalVariablesHandler.Ref;
            var rulesetId = Guid.NewGuid();
            var globalVariableId = Guid.NewGuid();

            var exception = await Record.ExceptionAsync(() => new Controllers.GlobalVariablesController().Put(new Definitions.NameWrapper { Name = "test name" }, rulesetId, globalVariableId));

            Assert.NotNull(exception);
            var typedException = Assert.IsType<HttpResponseException>(exception);
            Assert.Equal(HttpStatusCode.GatewayTimeout, typedException.Response.StatusCode);
        }

        [Fact]
        public async Task UrlParamsAndBodyWithoutResponseSendsInternalServerErrorToClientWhenUnexpectedResponseFromHandler()
        {
            var globalVariablesHandler = CreateTestProbe();
            Global.GlobalVariablesHandler = globalVariablesHandler.Ref;
            var rulesetId = Guid.NewGuid();
            var globalVariableId = Guid.NewGuid();
            var task = new Controllers.GlobalVariablesController().Put(new Definitions.NameWrapper { Name = "test name" }, rulesetId, globalVariableId);
            globalVariablesHandler.ExpectMsg<Requests.GlobalVariablesPut>(p => p.Body.Name == "test name" && p.RulesetId == rulesetId && p.GlobalVariableId == globalVariableId);
            globalVariablesHandler.LastSender.Tell(new UnexpectedResponse(), globalVariablesHandler.Ref);

            var exception = await Record.ExceptionAsync(() => task);

            Assert.NotNull(exception);
            var typedException = Assert.IsType<HttpResponseException>(exception);
            Assert.Equal(HttpStatusCode.InternalServerError, typedException.Response.StatusCode);
        }

        [Fact]
        public async Task UrlParamsAndBodyWithoutResponseForwardsStatusCodeToClientWhenErrorFromHandler()
        {
            var globalVariablesHandler = CreateTestProbe();
            Global.GlobalVariablesHandler = globalVariablesHandler.Ref;
            var rulesetId = Guid.NewGuid();
            var globalVariableId = Guid.NewGuid();
            var task = new Controllers.GlobalVariablesController().Put(new Definitions.NameWrapper { Name = "test name" }, rulesetId, globalVariableId);
            globalVariablesHandler.ExpectMsg<Requests.GlobalVariablesPut>(p => p.Body.Name == "test name" && p.RulesetId == rulesetId && p.GlobalVariableId == globalVariableId);
            globalVariablesHandler.LastSender.Tell(new Error { StatusCode = HttpStatusCode.PaymentRequired }, globalVariablesHandler.Ref);

            var exception = await Record.ExceptionAsync(() => task);

            Assert.NotNull(exception);
            var typedException = Assert.IsType<HttpResponseException>(exception);
            Assert.Equal(HttpStatusCode.PaymentRequired, typedException.Response.StatusCode);
        }
        #endregion
    }
}
