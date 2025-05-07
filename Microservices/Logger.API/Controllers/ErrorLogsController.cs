using Microservices.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;

namespace Logger.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorLogsController : ControllerBase
    {
        /// <summary>
        /// This method is used to log exceptions that arise in server side.
        /// </summary>
        /// <param name="errorLogs"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult LogErrors(ErrorLogs errorLogs)
        {
            try
            {
                if (errorLogs.Error == null)
                {
                    return StatusCode(400, "Error cannot be null");
                }
                else
                {
                    return LogInDatabaseUsingSerilog
                        (
                        errorLogs.EmployeeCode,
                        errorLogs.ApplicationName,
                        errorLogs.Error.StackTrace,
                        errorLogs.Error.Message
                        );
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// This method is used to log exceptions that arise in Client Side.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost("logclienterrors")]
        public IActionResult LogErrors(dynamic payload)
        {
            try
            {
                if (payload == null || string.IsNullOrEmpty(payload.ToString()))
                {
                    return StatusCode(400, "payload cannot be null or empty");
                }
                else
                {
                    HttpContext httpContext = HttpContext;
                    var errorMessage = payload["errorMessage"].ToString();
                    //This is done because server error sent by angular applications does not have stack trace.
                    var stackTrace = payload["stackTrace"].ToString() == "" ? errorMessage : payload["stackTrace"].ToString();
                    var employeeCode = httpContext.Request.Headers["EmployeeCode"].ToString().ToUpper().Contains("BAIN\\") ?
                        httpContext.Request.Headers["EmployeeCode"].ToString() :
                        "BAIN\\" + httpContext.Request.Headers["EmployeeCode"].ToString();
                    return LogInDatabaseUsingSerilog
                        (
                        employeeCode,
                        "Staffing Client",
                        stackTrace,
                        errorMessage
                        );
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }
        /// <summary>
        /// This method is used to insert excetions in Database using Serilog.
        /// These exceptions include server side and Client Side exceptions.
        /// </summary>
        /// <param name="employeeCode">Which specfic employee faced the problem.</param>
        /// <param name="applicationName">To check whether the exception was client side or server side. If server side then which Microservice.</param>
        /// <param name="stackTrace">Exception in details.</param>
        /// <param name="errorMessage">Basic details of the application.</param>
        /// <returns></returns>
        protected IActionResult LogInDatabaseUsingSerilog(string employeeCode, string applicationName, string stackTrace, string errorMessage)
        {
            if (string.IsNullOrEmpty(stackTrace) && string.IsNullOrEmpty(errorMessage))
            {
                return StatusCode(400, "Both stackTrace and errorMessage cannot be null");
            }
            else
            {
                Log.Logger.ForContext("EmployeeCode", employeeCode)
                    .ForContext("ApplicationName", applicationName)
                    .ForContext("Exception", stackTrace)
                    .Error(errorMessage);
                return Ok();
            }
        }
    }
}