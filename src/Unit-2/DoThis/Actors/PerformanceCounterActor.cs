﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;

namespace ChartApp.Actors
{
    public class PerformanceCounterActor: UntypedActor
    {
        private readonly string _seriesName;
        private readonly Func<PerformanceCounter> _performanceCounterGenerator;
        private PerformanceCounter _counter;

        private readonly HashSet<IActorRef> _subscriptions;
        private readonly ICancelable _cancelPublishing;

        /// <inheritdoc />
        public PerformanceCounterActor(string seriesName, Func<PerformanceCounter> performanceCounterGenerator)
        {
            _seriesName = seriesName;
            _performanceCounterGenerator = performanceCounterGenerator;
            _subscriptions = new HashSet<IActorRef>();
            _cancelPublishing = new Cancelable(Context.System.Scheduler);

        }

        /// <inheritdoc />
        protected override void PreStart()
        {
            _counter = _performanceCounterGenerator();
            Context.System.Scheduler.ScheduleTellRepeatedly(
                TimeSpan.FromMilliseconds(250),
                TimeSpan.FromMilliseconds(250),
                Self,
                new GatherMetrics(), 
                Self,
                _cancelPublishing);
        }

        /// <inheritdoc />
        protected override void PostStop()
        {
            try
            {
                _cancelPublishing.Cancel(false);
                _counter.Dispose();
            }
            finally
            {
            base.PostStop();

            }
        }

        /// <inheritdoc />
        protected override void OnReceive(object message)
        {
            if (message is GatherMetrics)
            {
                var metric = new Metric(_seriesName, _counter.NextValue());
                foreach (var sub in _subscriptions)
                {
                    sub.Tell(metric);
                }
            }
            else if (message is SubscribeCounter)
            {
                var sc = message as SubscribeCounter;
                _subscriptions.Add(sc.Subscriber);

            }
            else if (message is UnsubscribeCounter)
            {
                var uc = message as UnsubscribeCounter;
                _subscriptions.Remove(uc.Subscriber);
            }
            {
                
            }
        }
    }
}