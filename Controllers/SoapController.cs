using Microsoft.AspNetCore.Mvc;
using ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace soap_web_client
{
    [ApiController]
    [Route("[controller]")]
    public class SoapController : ControllerBase
    {
        private readonly ACSWSSoapClient _client;

        public SoapController()
        {
            // Initialize the SOAP client
            _client = new ACSWSSoapClient(ACSWSSoapClient.EndpointConfiguration.ACSWSSoap);
        }

        [HttpGet("GetMethods")]
        public IActionResult GetMethods()
        {
            // Use reflection to get method names from the generated client
            var methods = typeof(ACSWSSoapClient).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.DeclaringType == typeof(ACSWSSoapClient))
                .Select(m => m.Name)
                .ToList();

            return Ok(methods);
        }

        [HttpGet("GetRequestTemplate/{methodName}")]
        public IActionResult GetRequestTemplate(string methodName)
        {
            // Use reflection to get method parameters
            var method = typeof(ACSWSSoapClient).GetMethod(methodName);
            if (method == null) return NotFound("Method not found");

            var parameters = method.GetParameters()
                .ToDictionary(p => p.Name, p => p.ParameterType.Name);

            return Ok(parameters);
        }

        [HttpPost("InvokeMethod")]
        public async Task<IActionResult> InvokeMethod(string methodName, [FromBody] Dictionary<string, object> parameters)
        {
            try
            {
                var method = typeof(ACSWSSoapClient).GetMethod(methodName);
                if (method == null) return NotFound("Method not found");

                // Dynamically invoke the method
                var result = await (Task<object>)method.Invoke(_client, parameters.Values.ToArray());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
