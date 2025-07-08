using API.Common.Interfaces;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace API.Common.Authentification
{
    public class AuthentificationHandler : IAuthentificationHandler
    {
        private readonly ResourceManager _resourceManager;
        private readonly ILogger _logger;

        public AuthentificationHandler(ResourceManager resourceManager, ILogger logger)
        {
            _resourceManager = resourceManager;
            _logger = logger;
        }

        /// <summary>
        /// Validieren des Authentifizierungstokens
        /// </summary>
        /// <param name="authToken">Authentifizierungstoken</param>
        /// <returns>true = ok, false = nicht ok</returns>
        public bool ValidateAuthenticationToken(string authToken)
        {
            string endpoint = _resourceManager.GetString("TokenValidationEndpoint");

            if (string.IsNullOrEmpty(endpoint))
            {
                return false;
            }

            // RestClient erstellen
            RestClient client = new RestClient(endpoint);
            //Erstellen des Requests
            var request = CreateRequest(authToken);
            // Anfrage senden
            var response = client.Execute(request);
            //Response weiterverarbeiten
            return ProcessResponse(response);
        }

        /// <summary>
        /// Verarbeiten des Response
        /// </summary>
        /// <param name="response">RestResponse von API</param>
        /// <returns>true = erfolgreich, false = nicht erfolgreich</returns>
        private bool ProcessResponse(RestResponse response)
        {
            // Antwort verarbeiten
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogHint($"{response.StatusCode}: {response.Content}", Enum.APITypes.Common, "AuthentificationHandler", "ValidateAuthenticationToken");
                return false;
            }
            else
            {
                _logger.LogError($"{response.StatusCode}: {response.ErrorMessage}", Enum.APITypes.Common, "AuthentificationHandler", "ValidateAuthenticationToken");
                return false;
            }
        }

        /// <summary>
        /// Erstelle des RestRequests
        /// </summary>
        /// <param name="authToken">Authentifizierungstoken</param>
        /// <returns></returns>
        private RestRequest CreateRequest(string authToken)
        {

            // Neue Anfrage erstellen
            RestRequest request = new RestRequest("/validateToken/", Method.Post);

            // Anfrage Header setzen
            request.AddHeader("Content-Type", "application/json");

            // JSON Objekt erstellen, um den Anfrage Body zu erstellen
            var requestObject = new { authToken };

            // Anfrage Body serialisieren
            string jsonBody = JsonConvert.SerializeObject(requestObject);

            // Anfrage Body setzen
            request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);

            return request;
        }
    }
}
