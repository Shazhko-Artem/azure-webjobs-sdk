﻿using System;
using System.IO;

namespace Microsoft.WindowsAzure.Jobs
{
    // Provide services for executing a function on a Worker Role.
    // FunctionExecutionContext is the common execution operations that aren't Worker-role specific.
    // Everything else is worker role specific. 
    internal class WebExecutionLogger : IExecutionLogger
    {
        private readonly FunctionExecutionContext _ctx;

        public WebExecutionLogger(Guid hostInstanceId, Services services, Action<TextWriter> addHeaderInfo)
        {
            _ctx = new FunctionExecutionContext
            {
                HostInstanceId = hostInstanceId,
                OutputLogDispenser = new FunctionOutputLogDispenser(
                    services.AccountInfo,
                    addHeaderInfo,
                    EndpointNames.ConsoleOuputLogContainerName
                ),
                Logger = services.GetFunctionUpdatedLogger(),
                FunctionTable = services.GetFunctionTable(),
            };
        }

        public FunctionExecutionContext GetExecutionContext()
        {
            return _ctx;
        }
    }
}