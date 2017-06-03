using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using System.Net;
using System.Threading.Tasks;
using Akka.Actor;

namespace UnboundedNondeterminism.Web
{
	/// <summary>The entry point for the web API.</summary>
	public partial class Global : HttpApplication
	{
		/// <summary>The global <see cref="ActorSystem" />.</summary>
		public static ActorSystem ActorSystem;

		/// <summary>The <see cref="IActorRef" /> representing <see cref="Handlers.Rulesets" />.</summary>
		public static IActorRef RulesetsHandler;

		/// <summary>The <see cref="IActorRef" /> representing <see cref="Handlers.GlobalVariables" />.</summary>
		public static IActorRef GlobalVariablesHandler;

		/// <summary>The <see cref="IActorRef" /> representing <see cref="Handlers.Rules" />.</summary>
		public static IActorRef RulesHandler;

		private void Application_Start(object sender, EventArgs e) 
		{
			ActorSystem = ActorSystem.Create("UnboundedNondeterminism");
			RulesetsHandler = ActorSystem.ActorOf(Props.Create(() => new Handlers.Rulesets()));
			GlobalVariablesHandler = ActorSystem.ActorOf(Props.Create(() => new Handlers.GlobalVariables()));
			RulesHandler = ActorSystem.ActorOf(Props.Create(() => new Handlers.Rules()));
			GlobalConfiguration.Configure(config => 
			{
				config.Routes.MapHttpRoute(
					name: "RulesetsPost",
					routeTemplate: "rulesets",
					defaults: new { Controller = "Rulesets", Action = "Post" }
				);

				config.Routes.MapHttpRoute(
					name: "RulesetsGet",
					routeTemplate: "rulesets/{rulesetId}",
					defaults: new { Controller = "Rulesets", Action = "Get" }
				);

				config.Routes.MapHttpRoute(
					name: "RulesetsDelete",
					routeTemplate: "rulesets/{rulesetId}",
					defaults: new { Controller = "Rulesets", Action = "Delete" }
				);

				config.Routes.MapHttpRoute(
					name: "GlobalVariablesPost",
					routeTemplate: "rulesets/{rulesetId}/global-variables",
					defaults: new { Controller = "GlobalVariables", Action = "Post" }
				);

				config.Routes.MapHttpRoute(
					name: "GlobalVariablesPut",
					routeTemplate: "rulesets/{rulesetId}/global-variables/{globalVariableId}",
					defaults: new { Controller = "GlobalVariables", Action = "Put" }
				);

				config.Routes.MapHttpRoute(
					name: "GlobalVariablesDelete",
					routeTemplate: "rulesets/{rulesetId}/global-variables/{globalVariableId}",
					defaults: new { Controller = "GlobalVariables", Action = "Delete" }
				);

				config.Routes.MapHttpRoute(
					name: "RulesPost",
					routeTemplate: "rulesets/{rulesetId}/rules",
					defaults: new { Controller = "Rules", Action = "Post" }
				);

				config.Routes.MapHttpRoute(
					name: "RulesGet",
					routeTemplate: "rulesets/{rulesetId}/rules/{ruleId}",
					defaults: new { Controller = "Rules", Action = "Get" }
				);

				config.Routes.MapHttpRoute(
					name: "RulesPut",
					routeTemplate: "rulesets/{rulesetId}/rules/{ruleId}",
					defaults: new { Controller = "Rules", Action = "Put" }
				);

				config.Routes.MapHttpRoute(
					name: "RulesDelete",
					routeTemplate: "rulesets/{rulesetId}/rules/{ruleId}",
					defaults: new { Controller = "Rules", Action = "Delete" }
				);
			});
		}

		private void Application_End(object sender, EventArgs e) 
		{
			if (ActorSystem == null) return;
			ActorSystem.Dispose();
			ActorSystem = null;
		}
	}

	namespace Definitions
	{		
		/// <summary>Wraps a name.</summary>
		public sealed class NameWrapper
		{
			/// <summary>A user-facing (and specified) name up to 50 characters long.</summary>
			public string Name { get; set; }		
		}
		
		/// <summary>Wraps an Id.</summary>
		public sealed class IdWrapper
		{
			/// <summary>Identifies an object or entity.</summary>
			public Guid Id { get; set; }		
		}
		
		/// <summary>Wraps an associated Id/Name pair.</summary>
		public sealed class IdNameWrapper
		{
			/// <summary>Identifies an object or entity.</summary>
			public Guid Id { get; set; }		

			/// <summary>A user-facing (and specified) name up to 50 characters long.</summary>
			public string Name { get; set; }		
		}
		
		/// <summary>Represents a ruleset from a high-level view; an index of its contents.</summary>
		public sealed class Ruleset
		{
			/// <summary>The global variables of the ruleset.</summary>
			public IEnumerable<IdNameWrapper> GlobalVariables { get; set; }		

			/// <summary>The names and ids of the rules in the ruleset.</summary>
			public IEnumerable<IdNameWrapper> Rules { get; set; }		
		}
	}

	namespace Requests
	{
		/// <summary>A message passed by <see cref="Controllers.RulesetsController.Post" /> to <see cref="Handlers.Rulesets" /> for every request.</summary>
		public sealed class RulesetsPost
		{		}

		/// <summary>A message passed by <see cref="Controllers.RulesetsController.Get" /> to <see cref="Handlers.Rulesets" /> for every request.</summary>
		public sealed class RulesetsGet
		{
			/// <summary>The "rulesetId" URL fragment.</summary>
			public Guid RulesetId;
		}

		/// <summary>A message passed by <see cref="Controllers.RulesetsController.Delete" /> to <see cref="Handlers.Rulesets" /> for every request.</summary>
		public sealed class RulesetsDelete
		{
			/// <summary>The "rulesetId" URL fragment.</summary>
			public Guid RulesetId;
		}

		/// <summary>A message passed by <see cref="Controllers.GlobalVariablesController.Post" /> to <see cref="Handlers.GlobalVariables" /> for every request.</summary>
		public sealed class GlobalVariablesPost
		{
			/// <summary>The "rulesetId" URL fragment.</summary>
			public Guid RulesetId;
		}

		/// <summary>A message passed by <see cref="Controllers.GlobalVariablesController.Put" /> to <see cref="Handlers.GlobalVariables" /> for every request.</summary>
		public sealed class GlobalVariablesPut
		{
			/// <summary>The request body.</summary>
			public Definitions.NameWrapper Body;

			/// <summary>The "rulesetId" URL fragment.</summary>
			public Guid RulesetId;

			/// <summary>The "globalVariableId" URL fragment.</summary>
			public Guid GlobalVariableId;
		}

		/// <summary>A message passed by <see cref="Controllers.GlobalVariablesController.Delete" /> to <see cref="Handlers.GlobalVariables" /> for every request.</summary>
		public sealed class GlobalVariablesDelete
		{
			/// <summary>The "rulesetId" URL fragment.</summary>
			public Guid RulesetId;

			/// <summary>The "globalVariableId" URL fragment.</summary>
			public Guid GlobalVariableId;
		}

		/// <summary>A message passed by <see cref="Controllers.RulesController.Post" /> to <see cref="Handlers.Rules" /> for every request.</summary>
		public sealed class RulesPost
		{
			/// <summary>The "rulesetId" URL fragment.</summary>
			public Guid RulesetId;
		}

		/// <summary>A message passed by <see cref="Controllers.RulesController.Get" /> to <see cref="Handlers.Rules" /> for every request.</summary>
		public sealed class RulesGet
		{
			/// <summary>The "rulesetId" URL fragment.</summary>
			public Guid RulesetId;

			/// <summary>The "ruleId" URL fragment.</summary>
			public Guid RuleId;
		}

		/// <summary>A message passed by <see cref="Controllers.RulesController.Put" /> to <see cref="Handlers.Rules" /> for every request.</summary>
		public sealed class RulesPut
		{
			/// <summary>The "rulesetId" URL fragment.</summary>
			public Guid RulesetId;

			/// <summary>The "ruleId" URL fragment.</summary>
			public Guid RuleId;
		}

		/// <summary>A message passed by <see cref="Controllers.RulesController.Delete" /> to <see cref="Handlers.Rules" /> for every request.</summary>
		public sealed class RulesDelete
		{
			/// <summary>The "rulesetId" URL fragment.</summary>
			public Guid RulesetId;

			/// <summary>The "ruleId" URL fragment.</summary>
			public Guid RuleId;
		}
	}

	/// <summary>A message to pass from the Akka handler back to the Web API controller when the response will be <see cref="HttpStatusCode.NoContent" />.</summary>
	public sealed class Success {}

	/// <summary>A message to pass from the Akka handler back to the Web API controller when the response represents an error.</summary>
	public sealed class Error 
	{
		/// <summary>The <see cref="HttpStatusCode" /> to send in response.</summary>
		public HttpStatusCode StatusCode;
	}

	namespace Controllers
	{
		/// <remarks>Provides access to rulesets</remarks>
		public sealed class RulesetsController : ApiController 
		{
			/// <summary>Create a new, empty ruleset</summary>
			[HttpPost]
			public async Task<Definitions.IdWrapper> Post() 
			{
				object response;
				try
				{
					response = await Global.RulesetsHandler.Ask(new Requests.RulesetsPost 
					{		
					}, TimeSpan.FromSeconds(3));
				}
				catch (TaskCanceledException)
				{
					throw new HttpResponseException(HttpStatusCode.GatewayTimeout);
				}

				if (response is Definitions.IdWrapper) return (Definitions.IdWrapper) response;
				if (response is Error) throw new HttpResponseException(((Error)response).StatusCode);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}

			/// <summary>Get a previously created ruleset</summary>
			[HttpGet]
			public async Task<Definitions.Ruleset> Get([FromUri] Guid rulesetId) 
			{
				object response;
				try
				{
					response = await Global.RulesetsHandler.Ask(new Requests.RulesetsGet 
					{
						RulesetId = rulesetId		
					}, TimeSpan.FromSeconds(3));
				}
				catch (TaskCanceledException)
				{
					throw new HttpResponseException(HttpStatusCode.GatewayTimeout);
				}

				if (response is Definitions.Ruleset) return (Definitions.Ruleset) response;
				if (response is Error) throw new HttpResponseException(((Error)response).StatusCode);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}

			/// <summary>Delete a previously created ruleset</summary>
			[HttpDelete]
			public async Task Delete([FromUri] Guid rulesetId) 
			{
				object response;
				try
				{
					response = await Global.RulesetsHandler.Ask(new Requests.RulesetsDelete 
					{
						RulesetId = rulesetId		
					}, TimeSpan.FromSeconds(3));
				}
				catch (TaskCanceledException)
				{
					throw new HttpResponseException(HttpStatusCode.GatewayTimeout);
				}

				if (response is Success) return;
				if (response is Error) throw new HttpResponseException(((Error)response).StatusCode);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}
		}

		/// <remarks>Provides access to rulesets' global variables</remarks>
		public sealed class GlobalVariablesController : ApiController 
		{
			/// <summary>Create a new global variable</summary>
			[HttpPost]
			public async Task<Definitions.IdNameWrapper> Post([FromUri] Guid rulesetId) 
			{
				object response;
				try
				{
					response = await Global.GlobalVariablesHandler.Ask(new Requests.GlobalVariablesPost 
					{
						RulesetId = rulesetId		
					}, TimeSpan.FromSeconds(3));
				}
				catch (TaskCanceledException)
				{
					throw new HttpResponseException(HttpStatusCode.GatewayTimeout);
				}

				if (response is Definitions.IdNameWrapper) return (Definitions.IdNameWrapper) response;
				if (response is Error) throw new HttpResponseException(((Error)response).StatusCode);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}

			/// <summary>Rename a previously created global variable</summary>
			[HttpPut]
			public async Task Put([FromBody] Definitions.NameWrapper body, [FromUri] Guid rulesetId, [FromUri] Guid globalVariableId) 
			{
				object response;
				try
				{
					response = await Global.GlobalVariablesHandler.Ask(new Requests.GlobalVariablesPut 
					{
						Body = body, 
						RulesetId = rulesetId, 
						GlobalVariableId = globalVariableId		
					}, TimeSpan.FromSeconds(3));
				}
				catch (TaskCanceledException)
				{
					throw new HttpResponseException(HttpStatusCode.GatewayTimeout);
				}

				if (response is Success) return;
				if (response is Error) throw new HttpResponseException(((Error)response).StatusCode);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}

			/// <summary>Delete a previously created global variable</summary>
			[HttpDelete]
			public async Task Delete([FromUri] Guid rulesetId, [FromUri] Guid globalVariableId) 
			{
				object response;
				try
				{
					response = await Global.GlobalVariablesHandler.Ask(new Requests.GlobalVariablesDelete 
					{
						RulesetId = rulesetId, 
						GlobalVariableId = globalVariableId		
					}, TimeSpan.FromSeconds(3));
				}
				catch (TaskCanceledException)
				{
					throw new HttpResponseException(HttpStatusCode.GatewayTimeout);
				}

				if (response is Success) return;
				if (response is Error) throw new HttpResponseException(((Error)response).StatusCode);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}
		}

		/// <remarks>Provides access to rulesets' rules</remarks>
		public sealed class RulesController : ApiController 
		{
			/// <summary>Create a new, empty rule</summary>
			[HttpPost]
			public async Task<Definitions.IdNameWrapper> Post([FromUri] Guid rulesetId) 
			{
				object response;
				try
				{
					response = await Global.RulesHandler.Ask(new Requests.RulesPost 
					{
						RulesetId = rulesetId		
					}, TimeSpan.FromSeconds(3));
				}
				catch (TaskCanceledException)
				{
					throw new HttpResponseException(HttpStatusCode.GatewayTimeout);
				}

				if (response is Definitions.IdNameWrapper) return (Definitions.IdNameWrapper) response;
				if (response is Error) throw new HttpResponseException(((Error)response).StatusCode);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}

			/// <summary>Retrieve a previously created rule</summary>
			[HttpGet]
			public async Task Get([FromUri] Guid rulesetId, [FromUri] Guid ruleId) 
			{
				object response;
				try
				{
					response = await Global.RulesHandler.Ask(new Requests.RulesGet 
					{
						RulesetId = rulesetId, 
						RuleId = ruleId		
					}, TimeSpan.FromSeconds(3));
				}
				catch (TaskCanceledException)
				{
					throw new HttpResponseException(HttpStatusCode.GatewayTimeout);
				}

				if (response is Success) return;
				if (response is Error) throw new HttpResponseException(((Error)response).StatusCode);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}

			/// <summary>Update a previously created rule</summary>
			[HttpPut]
			public async Task Put([FromUri] Guid rulesetId, [FromUri] Guid ruleId) 
			{
				object response;
				try
				{
					response = await Global.RulesHandler.Ask(new Requests.RulesPut 
					{
						RulesetId = rulesetId, 
						RuleId = ruleId		
					}, TimeSpan.FromSeconds(3));
				}
				catch (TaskCanceledException)
				{
					throw new HttpResponseException(HttpStatusCode.GatewayTimeout);
				}

				if (response is Success) return;
				if (response is Error) throw new HttpResponseException(((Error)response).StatusCode);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}

			/// <summary>delete a previously created rule</summary>
			[HttpDelete]
			public async Task Delete([FromUri] Guid rulesetId, [FromUri] Guid ruleId) 
			{
				object response;
				try
				{
					response = await Global.RulesHandler.Ask(new Requests.RulesDelete 
					{
						RulesetId = rulesetId, 
						RuleId = ruleId		
					}, TimeSpan.FromSeconds(3));
				}
				catch (TaskCanceledException)
				{
					throw new HttpResponseException(HttpStatusCode.GatewayTimeout);
				}

				if (response is Success) return;
				if (response is Error) throw new HttpResponseException(((Error)response).StatusCode);
				throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}
		}
	}
}