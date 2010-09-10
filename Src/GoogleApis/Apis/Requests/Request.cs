/*
Copyright 2010 Google Inc

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Google.Apis.Discovery;
using Google.Apis;
using Google.Apis.Authentication;

namespace Google.Apis.Requests
{
	/// <summary>
	/// 
	/// </summary>
	public class Request {
		
		public enum ReturnTypeEnum {
			Json,
			Atom
		}
		
		private Authenticator Authenticator {get; set;}
		private Service Service {get; set;}
		private Method Method {get;set;}
		private Uri BaseURI {get; set;}
		private string PathUrl {get;set;}
		private string RPCName {get;set;}
		private string Body {get;set;}
		private Dictionary<string, string> Parameters {get;set;}
		private Uri RequestUrl;
		private ReturnTypeEnum ReturnType {get; set; }
		
		
		/// <summary>
		/// Given an API method, create the appropriate Request for it.
		/// </summary>
		/// <param name="method">
		/// A <see cref="Method"/>
		/// </param>
		/// <returns>
		/// A <see cref="Request"/>
		/// </returns>
		public static Request CreateRequest(Service service, Method method) {
			
			switch(method.HttpMethod) {
			case "GET":
				return new GETRequest { Service = service, Method = method, BaseURI = service.BaseUri };
			case "PUT":
				return new PUTRequest { Service = service, Method = method, BaseURI = service.BaseUri };
			case "POST":
				return new POSTRequest { Service = service, Method = method, BaseURI = service.BaseUri };
			case "DELETE":
				return new DELETERequest { Service = service, Method = method, BaseURI = service.BaseUri };
			}
			
			return null;// Should throw an exception.
		}
		
		/// <summary>
		/// The method to call
		/// </summary>
		/// <returns>
		/// A <see cref="Request"/>
		/// </returns>
		public Request On(string rpcName) {
			RPCName = rpcName;
			
			return this;
		}
		
		/// <summary>
		/// Sets the type of data that is expected to be returned from the request.
		/// 
		/// Defaults to Json
		/// </summary>
		/// <param name="returnType">
		/// A <see cref="ReturnType"/>
		/// </param>
		/// <returns>
		/// A <see cref="Request"/>
		/// </returns>
		public Request Returning(ReturnTypeEnum returnType) {
			this.ReturnType = returnType;
			return this;
		}
		
		
		/// <summary>
		/// Adds the parameters to the request.
		/// </summary>
		/// <returns>
		/// A <see cref="Request"/>
		/// </returns>
		public Request WithParameters(Dictionary<string, string> parameters) {
			// Convert the parameters
			
			Parameters = parameters;
			return this;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameters">
		/// A <see cref="Dictionary<System.String, System.String>"/>
		/// </param>
		/// <returns>
		/// A <see cref="Request"/>
		/// </returns>
		public Request WithParameters(string parameters) {
			// Check to ensure that the 
			Parameters = Utilities.QueryStringToDictionary(parameters);
			return this;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameters">
		/// A <see cref="Dictionary<System.String, Object>"/>
		/// </param>
		/// <returns>
		/// A <see cref="Request"/>
		/// </returns>
		public Request WithBody(Dictionary<string, string> parameters) {
			// Check to ensure that the 
			Body = parameters.ToString();
			return this;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="body">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="Request"/>
		/// </returns>
		public Request WithBody(string body) {
			// Check to ensure that the 
			Body = body;
			return this;
		}
		
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="authenticator">
		/// A <see cref="Authenticator"/>
		/// </param>
		/// <returns>
		/// A <see cref="Request"/>
		/// </returns>
		public Request WithAuthentication(Authenticator authenticator) {
			this.Authenticator = authenticator;
			// Check to ensure that the 
			return this;
		}
		
		
		/// <summary>
		/// Checks that the supplied parameters are valid given the discovery document
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		private bool ValidateParameters() {
			// Itterate accross all the parameters in the discovery document, and check them against supplied arguments.
			foreach(var parameter in Method.Parameters) {
				var parameterInfo = parameter.Value;
				string currentParam;
				bool parameterPresent = Parameters.TryGetValue(parameter.Key, out currentParam);
				
				// If a required parameter is not present. bail
				if(parameterInfo.Required && String.IsNullOrEmpty(currentParam)) {
					return false;
				}
				
				if(parameterPresent == false) {
					// The parameter is not present in the input and is not required, skip validation.
					continue;	
				}
				else {
					// The parameter is present, validte the regex.
					bool isValidData = ValidateRegex(parameterInfo.Pattern, currentParam.ToString());
					if(isValidData == false) {
						return false;
					}
				}
			}
			
			return true;
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pattern">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="stringValue">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		private bool ValidateRegex(string pattern, string stringValue) {
			Regex r = new Regex(pattern);
			
			return r.IsMatch(stringValue);;	
		}
		
		/// <summary>
		/// 
		/// </summary>
		private void BuildRequestUrl() {
			var restPath = Method.RestPath;
			var queryParams = new List<string>();
			var restQuery = "";
			
			if(this.ReturnType == Request.ReturnTypeEnum.Json) {
				queryParams.Add("alt=json");
			}
			else {
				queryParams.Add("alt=atom");	
			}
			
			
			// Replace the substitution parameters
			foreach(var parameter in this.Parameters) {
				var parameterDefinition = Method.Parameters[parameter.Key];
				if(parameterDefinition.ParameterType == "path") {
					restPath = restPath.Replace(String.Format("{{{0}}}", parameter.Key), parameter.Value.ToString());
				}
				
				if(parameterDefinition.ParameterType == "query") {
					queryParams.Add(parameterDefinition.Name + "=" + parameter.Value);
				}
			}
			
			var path = restPath;
			
			if(queryParams.Count > 0) {
				path += "?" + String.Join("&", queryParams.ToArray());
			}
			
			
			RequestUrl = new Uri(BaseURI,path);
		}
		
	
		/// <summary>
		/// Executes a request given the configuration options supplied.
		/// </summary>
		/// <returns>
		/// A <see cref="Stream"/>
		/// </returns>
		public Stream ExecuteRequest() {
			
			if(ValidateParameters() == false)
				return Stream.Null;
			
			// Formulate the RequestUrl
			BuildRequestUrl();
			
			//
			HttpWebRequest request = this.Authenticator.CreateHttpWebRequest(this.Method.HttpMethod, RequestUrl);
	
			if(this.ReturnType == Request.ReturnTypeEnum.Json) {
				//All requests are JSON.
				request.ContentType =  "application/json";
			}
			else {
				request.ContentType =  "application/atom+xml";
			}
			
			// Attach a body if a POST and there is something to attach.
			if(String.IsNullOrEmpty(Body) == false && (this.Method.HttpMethod == "POST" || this.Method.HttpMethod == "PUT")) {
				using(var bodyStream = request.GetRequestStream()) {
					byte[] postBody = System.Text.Encoding.ASCII.GetBytes(Body);
					bodyStream.Write(postBody, 0, postBody.Length);
				}
			}
			
			try {
				HttpWebResponse response = (HttpWebResponse) request.GetResponse();
				return response.GetResponseStream();	
			}
			catch(WebException ex) {
				if(ex.Response != null) {
					return ex.Response.GetResponseStream();	
				}
				else {
					// The exception is not something the client can handle via a stream.
					throw;
				}
			}
		}
	}
}