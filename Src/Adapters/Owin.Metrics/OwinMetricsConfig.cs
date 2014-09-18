﻿using Metrics;
using Metrics.Core;
using Owin.Metrics.Middleware;
using System;
using System.Text.RegularExpressions;
namespace Owin.Metrics
{
    public class OwinMetricsConfig
    {
        private readonly Action<object> middlewareRegistration;
        private readonly MetricsContext context;
        private readonly Func<HealthStatus> healthStatus;

        public OwinMetricsConfig(Action<object> middlewareRegistration, MetricsContext context, Func<HealthStatus> healthStatus)
        {
            this.middlewareRegistration = middlewareRegistration;
            this.context = context;
            this.healthStatus = healthStatus;
        }

        public OwinMetricsConfig WithRequestMetricsConfig(Action<OwinRequestMetricsConfig> config, Regex[] ignoreRequestPathPatterns = null)
        {
            OwinRequestMetricsConfig requestConfig = new OwinRequestMetricsConfig(this.middlewareRegistration, this.context,
                ignoreRequestPathPatterns);
            config(requestConfig);
            return this;
        }

        public OwinMetricsConfig WithMetricsEndpoint()
        {
            WithMetricsEndpoint(_ => { });
            return this;
        }

        public OwinMetricsConfig WithMetricsEndpoint(Action<OwinMetricsEndpointConfig> config)
        {
            OwinMetricsEndpointConfig endpointConfig = new OwinMetricsEndpointConfig();
            config(endpointConfig);
            var metricsEndpointMiddleware = new MetricsEndpointMiddleware(endpointConfig, this.context.DataProvider.CurrentMetricsData, this.healthStatus);
            this.middlewareRegistration(metricsEndpointMiddleware);
            return this;
        }
    }
}