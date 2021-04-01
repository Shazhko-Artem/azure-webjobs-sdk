﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SampleHost.Filters;
using SampleHost.Models;

namespace SampleHost
{
    [ErrorHandler]
    public class Functions
    {
        private readonly ISampleServiceA _sampleServiceA;
        private readonly ISampleServiceB _sampleServiceB;

        private static readonly Random _rand = new Random();

        public Functions(ISampleServiceA sampleServiceA, ISampleServiceB sampleServiceB)
        {
            _sampleServiceA = sampleServiceA;
            _sampleServiceB = sampleServiceB;
        }

        [Disable]
        public static async Task ConcurrencyTest_HighCpu([QueueTrigger("concurrency-work-items-1")] string message, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {message}");

            await GenerateLoadAllCoresAsync();
        }

        //[Disable]
        public static async Task ConcurrencyTest_Lightweight([QueueTrigger("concurrency-work-items-2")] string message, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {message}");

            await Task.Delay(250);
        }

        [Disable]
        public static async Task ConcurrencyTest_HighMemory([QueueTrigger("concurrency-work-items-3")] string message, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {message}");

            // allocate a large chunk of memory
            int numMBs = _rand.Next(100, 500);
            int numBytes = numMBs * 1024 * 1024;
            byte[] bytes = new byte[numBytes];

            // write to that memory
            _rand.NextBytes(bytes);

            await Task.Delay(_rand.Next(50, 250));
        }

        public static async Task GenerateLoadAllCoresAsync()
        {
            int cores = GetEffectiveCoresCount();
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < cores; i++)
            {
                var task = Task.Run(() => GenerateLoad());
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        public static void GenerateLoad()
        {
            int start = 2000;
            int numPrimes = 200;

            for (int i = start; i < start + numPrimes; i++)
            {
                FindPrimeNumber(i);
            }
        }

        public static long FindPrimeNumber(int n)
        {
            int count = 0;
            long a = 2;
            while (count < n)
            {
                long b = 2;
                int prime = 1; // to check if found a prime
                while (b * b <= a)
                {
                    if (a % b == 0)
                    {
                        prime = 0;
                        break;
                    }
                    b++;
                }
                if (prime > 0)
                {
                    count++;
                }
                a++;
            }
            return (--a);
        }

        public static int GetEffectiveCoresCount()
        {
            // When not running on VMSS, the dynamic plan has some limits that mean that a given instance is using effectively a single core,
            // so we should not use Environment.Processor count in this case.
            var effectiveCores = (IsWindowsConsumption() && !IsVMSS()) ? 1 : Environment.ProcessorCount;
            return effectiveCores;
        }

        public static bool IsWindowsConsumption()
        {
            string value = Environment.GetEnvironmentVariable("WEBSITE_SKU");
            return string.Equals(value, "Dynamic", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsVMSS()
        {
            string value = Environment.GetEnvironmentVariable("RoleInstanceId");
            return value != null && value.IndexOf("HostRole", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
