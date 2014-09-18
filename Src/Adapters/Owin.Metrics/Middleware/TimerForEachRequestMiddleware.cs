﻿using Metrics;
using Metrics.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class TimerForEachRequestMiddleware : MetricMiddleware
    {
        private const string RequestStartTimeKey = "__Metrics.RequestStartTime__";

        private readonly MetricsContext context;
        private readonly string metricPrefix;

        private AppFunc next;

        public TimerForEachRequestMiddleware(MetricsContext context, string metricPrefix, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            this.context = context;
            this.metricPrefix = metricPrefix;
        }

        public void Initialize(AppFunc next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (PerformMetric(environment))
            {
                environment[RequestStartTimeKey] = Clock.Default.Nanoseconds;

                await next(environment);

                var httpResponseStatusCode = int.Parse(environment["owin.ResponseStatusCode"].ToString());
                var httpMethod = environment["owin.RequestMethod"].ToString().ToUpper();

                if (httpResponseStatusCode != (int)HttpStatusCode.NotFound)
                {
                    var httpRequestPath = environment["owin.RequestPath"].ToString().ToUpper();
                    var name = string.Format("{0}.{1} [{2}]", metricPrefix, httpMethod, httpRequestPath);
                    var startTime = (long)environment[RequestStartTimeKey];
                    var elapsed = Clock.Default.Nanoseconds - startTime;
                    this.context.Timer(name, Unit.Requests, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Milliseconds).Record(elapsed, TimeUnit.Nanoseconds);
                }
            }
            else
            {
                await next(environment);
            }
        }
    }
}