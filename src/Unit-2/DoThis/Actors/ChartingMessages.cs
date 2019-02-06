using Akka.Actor;

namespace ChartApp.Actors
{
    #region Reporting

    /// <summary>
    /// Signal used to indicate that it's time to sample all counters
    /// </summary>
    public class GatherMetrics
    {

    }

    public class Metric
    {
        public string Series { get; }
        public float CounterValue { get; }

        /// <inheritdoc />
        public Metric(string series, float counterValue)
        {
            Series = series;
            CounterValue = counterValue;
        }
    }
    #endregion

    #region Performance Counter Management

    public enum CounterType
    {
        Cpu,
        Memory,
        Disk
    }

    public class SubscribeCounter
    {
        public CounterType Counter { get; }
        public IActorRef Subscriber { get; }

        /// <inheritdoc />
        public SubscribeCounter(CounterType counter, IActorRef subscriber)
        {
            Counter = counter;
            Subscriber = subscriber;
        }
    }


    public class UnsubscribeCounter
    {
        public CounterType Counter { get; }
        public IActorRef Subscriber { get; }

        /// <inheritdoc />
        public UnsubscribeCounter(CounterType counter, IActorRef subscriber)
        {
            Counter = counter;
            Subscriber = subscriber;
        }
    }

    #endregion
}