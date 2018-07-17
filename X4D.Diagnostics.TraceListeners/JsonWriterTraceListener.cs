using Newtonsoft.Json;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace X4D.Diagnostics.TraceListeners
{
    /// <summary>
    /// A `TraceListener` implementation which outputs data as line-delimited
    /// JSON fragments.
    /// <para>
    /// Used as the basis for other, similar `JsonXxxTraceListener` implementations.
    /// </para>
    /// </summary>
    public abstract class JsonWriterTraceListener :
        TraceListener
    {
        private static readonly JsonSerializerSettings s_defaultSerializerSettings =
            new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None
            };

        private readonly int _processId = Process.GetCurrentProcess().Id;

        protected readonly JsonSerializerSettings _serializerSettings;

        private StringBuilder _messageBuilder = new StringBuilder();

        public JsonWriterTraceListener()
            : base()
        {
            _serializerSettings = s_defaultSerializerSettings;
        }

        public JsonWriterTraceListener(string name)
            : this(name, s_defaultSerializerSettings)
        {
        }

        public JsonWriterTraceListener(string name, JsonSerializerSettings serializerSettings)
            : base(name)
        {
            _serializerSettings = serializerSettings;
        }

        public override bool IsThreadSafe => true;

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            TraceJsonEventData(eventCache, source, eventType, id, data);
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            TraceJsonEventData(eventCache, source, eventType, id, data);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            TraceJsonEventData(eventCache, source, eventType, id);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            TraceJsonEventData(eventCache, source, eventType, id, string.Format(format, args));
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            TraceJsonEventData(eventCache, source, eventType, id, message);
        }

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            TraceJsonEventData(eventCache, source, TraceEventType.Transfer, id, new JsonTraceTransferData
            {
                ActivityId = Trace.CorrelationManager.ActivityId,
                RelatedActivityId = relatedActivityId,
                Message = message
            });
        }

        protected void TraceJsonEventData(
            TraceEventCache eventCache,
            string source,
            TraceEventType eventType,
            int id,
            object data = null)
        {
            string json = BuildJsonEventData(
                eventCache.DateTime,
                source,
                eventType,
                id,
                data,
                eventCache.Callstack,
                eventCache.LogicalOperationStack,
                eventCache.ProcessId,
                int.TryParse(eventCache.ThreadId, out int threadId)
                    ? threadId
                    : 0);
            WriteLine(json);
        }

        protected string BuildJsonEventData(
            DateTime timestamp,
            string source, 
            TraceEventType eventType, 
            int id, 
            object data,
            string callstack = null,
            Stack logicalOperationStack = null,
            int processId = 0,
            int threadId = 0)
        {
            var traceData = new JsonTraceData
            {
                Timestamp = timestamp,
                Source = source,
                Id = id,
                Type = eventType.ToString(),
                Host = Environment.MachineName,
                User = Environment.UserName,
            };
            if (TraceOutputOptions != TraceOptions.None)
            {
                var traceOptions = Enum.GetValues(typeof(TraceOptions)).Cast<TraceOptions>();
                foreach (var traceOption in traceOptions)
                {
                    if (TraceOutputOptions.HasFlag(traceOption))
                    {
                        switch (traceOption)
                        {
                            case TraceOptions.Callstack:
                                traceData.Callstack = callstack;
                                break;

                            case TraceOptions.LogicalOperationStack:
                                traceData.LogicalOperationStack = logicalOperationStack;
                                break;

                            case TraceOptions.ProcessId:
                                traceData.ProcessId = processId > 0
                                    ? processId
                                    : _processId;
                                break;

                            case TraceOptions.ThreadId:
                                traceData.ThreadId = threadId > 0
                                    ? threadId
                                    : System.Threading.Thread.CurrentThread.ManagedThreadId;
                                break;

                            case TraceOptions.DateTime:
                            case TraceOptions.Timestamp:
                            default:
                                // NOP: not supported
                                break;
                        }
                    }
                }
            }
            if (data != null)
            {
                traceData.Data = data;
            }
            traceData = ExtendOrReplaceJsonTraceData(traceData);
            var json = JsonConvert.SerializeObject(traceData, _serializerSettings);
            return json;
        }

        /// <summary>
        /// Subclasses can override this method to return an
        /// extended/modified <see cref="JsonTraceData"/> object.
        /// </summary>
        /// <param name="traceData"></param>
        /// <returns></returns>
        protected virtual JsonTraceData ExtendOrReplaceJsonTraceData(JsonTraceData traceData)
        {
            return traceData;
        }

        [DataContract]
        protected class JsonTraceTransferData
        {
            [DataMember(Name = "from")]
            public Guid ActivityId { get; set; }

            [DataMember(Name = "to")]
            public Guid RelatedActivityId { get; set; }

            [DataMember(Name = "message")]
            public string Message { get; set; }
        }

        [DataContract]
        protected class JsonTraceData
        {
            [DataMember(Name = "ts", Order = 1)]
            public DateTime Timestamp { get; set; }

            [DataMember(Name = "source", Order = 2)]
            public string Source { get; internal set; }

            [DataMember(Name = "type", Order = 3)]
            public string Type { get; internal set; }

            [DataMember(Name = "id", Order = 4)]
            public int Id { get; internal set; }

            [DataMember(Name = "data", Order = 5)]
            public object Data { get; internal set; }

            [DataMember(Name = "tid", Order = 6)]
            public int ThreadId { get; internal set; }

            [DataMember(Name = "pid", Order = 7)]
            public int ProcessId { get; internal set; }

            [DataMember(Name = "host", Order = 8)]
            public string Host { get; set; }

            [DataMember(Name = "user", Order = 9)]
            public string User { get; set; }

            [DataMember(Name = "lstack", Order = 10)]
            public Stack LogicalOperationStack { get; internal set; }

            [DataMember(Name = "stack", Order = 11)]
            public string Callstack { get; internal set; }
        }
    }
}
