using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeVolt_BLL.IServices
{
    public interface ILoggerService
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message, Exception exception);
    }
}
