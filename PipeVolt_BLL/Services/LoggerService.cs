using Microsoft.Extensions.Logging;
using PipeVolt_BLL.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_BLL.Services
{
    public class LoggerService:ILoggerService
    {
        private readonly ILogger<LoggerService> _logger;

        public LoggerService(ILogger<LoggerService> logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation($"{DateTime.Now}: {message}");
        }

        public void LogWarning(string message)
        {
            _logger.LogInformation($"{DateTime.Now}: {message}");
        }

        public void LogError(string message, Exception exception)
        {
            _logger.LogError("_______________________________________");
            _logger.LogError($"{DateTime.Now}: {exception.Message}", exception);
            _logger.LogError($"{DateTime.Now}: {message}");
            _logger.LogError("_______________________________________");
        }


        public void WriteLogDebug(string title, string debug)
        {
            _logger.LogDebug("--------------------------------");
            _logger.LogDebug($"{DateTime.Now}: {title}: {debug}");
            _logger.LogDebug("--------------------------------");
        }

    }
}
