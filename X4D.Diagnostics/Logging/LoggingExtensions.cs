﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace X4D.Diagnostics.Logging
{
    /// <summary>
    /// Extension methods for tracing <see cref="Exception"/>, <see
    /// cref="ExpandoObject"/>, <see cref="String"/>, <see
    /// cref="StringBuilder"/> and <see cref="TextReader"/> objects.
    /// <para>
    /// <see cref="String"/>, <see cref="StringBuilder"/> and <see
    /// cref="TextReader"/> objects are used as a 'trace message'.
    /// </para>
    /// <para>
    /// <see cref="Exception"/> and <see cref="ExpandoObject"/> objects are
    /// used as 'trace data'.
    /// </para>
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// a lookup for <see cref="TraceSource"/> instances
        /// <para>
        /// These are look-ups for configured sources. If a source is not
        /// explicitly configured it is not logged to.
        /// </para>
        /// </summary>
        private static readonly ConcurrentDictionary<string/*TraceContext*/, TraceSource> _sources =
            new ConcurrentDictionary<string, TraceSource>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// A regex to normalize log events such that they appear on a single line.
        /// </summary>
        private static readonly Regex _normalizeRegex = new Regex(@"[\s]+");

        public static class Settings
        {
            /// <summary>
            /// A boolean indicating whether or not `Log` methods will normalize
            /// message content before tracing.
            /// <para>Default is true.</para>
            /// <para>
            /// Normalization reduces all whitespace down to a single character
            /// (tabs, carriage returns, line feeds, etc.)
            /// </para>
            /// </summary>
            public static bool ShouldNormalizeMessages { get; set; } = true;
        }
        private static readonly char[] s_crlfCharacters = Environment.NewLine.ToCharArray();

        /// <summary>
        /// A unique ID associated with each log event generated by these extensions.
        /// </summary>
        private static int _id = 0;

        /// <summary>
        /// <para>Check if a particular .NET `Trace Event Type` is enabled for the current trace source.</para>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsEnabled(
            TraceEventType eventType,
            string traceSourceName = null)
        {
            // only return for specified source when source is specified
            if (!string.IsNullOrWhiteSpace(traceSourceName))
            {
                return
                    GetTraceSource(traceSourceName)
                    ?.Switch
                    ?.ShouldTrace(eventType) == true;
            }

            // otherwise, infer TraceSource from calling type/namespace
            var callingType = (new StackFrame(1))
                .GetMethod()
                .DeclaringType;
            return
                GetTraceSource(callingType.Namespace, true)
                ?.Switch
                ?.ShouldTrace(eventType) != false
                ||
                GetTraceSource(DemangleTypeName(callingType.FullName))
                ?.Switch
                ?.ShouldTrace(eventType) != false;
        }

        /// <summary>
        /// Log the specified <see cref="Exception"/>, optionally specifying
        /// <paramref name="type"/> and <paramref name="traceSourceName"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type">Default is <see cref="TraceEventType.Error"/></param>
        /// <param name="traceSourceName"></param>
        /// <returns></returns>
        public static Exception Log(
            this Exception data,
            TraceEventType type = TraceEventType.Error,
            string traceSourceName = null)
        {
            var callingType = (new StackFrame(1))
                .GetMethod()
                .DeclaringType;
            return TraceData(data, type, callingType, traceSourceName);
        }

        /// <summary>
        /// Log the specified <see cref="string"/>, optionally specifying
        /// <paramref name="type"/> and <paramref name="traceSourceName"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type">Default is <see cref="TraceEventType.Information"/></param>
        /// <param name="traceSourceName"></param>
        /// <returns></returns>
        public static string Log(
            this String data,
            TraceEventType type = TraceEventType.Information,
            string traceSourceName = null)
        {
            var callingType = (new StackFrame(1))
                .GetMethod()
                .DeclaringType;
            TraceData(data, type, callingType, traceSourceName);
            return data;
        }

        /// <summary>
        /// Log the specified <see cref="StringBuilder"/>, optionally specifying
        /// <paramref name="type"/> and <paramref name="traceSourceName"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type">Default is <see cref="TraceEventType.Information"/></param>
        /// <param name="traceSourceName"></param>
        /// <returns></returns>
        public static StringBuilder Log(
            this StringBuilder data,
            TraceEventType type = TraceEventType.Information,
            string traceSourceName = null)
        {
            var callingType = (new StackFrame(1))
                .GetMethod()
                .DeclaringType;
            TraceData(data.ToString(), type, callingType, traceSourceName);
            return data;
        }

        /// <summary>
        /// Log the specified <see cref="TextReader"/>, optionally specifying
        /// <paramref name="type"/> and <paramref name="traceSourceName"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type">Default is <see cref="TraceEventType.Information"/></param>
        /// <param name="traceSourceName"></param>
        /// <returns></returns>
        public static TextReader Log(
            this TextReader data,
            TraceEventType type = TraceEventType.Information,
            string traceSourceName = null)
        {
            var callingType = (new StackFrame(1))
                .GetMethod()
                .DeclaringType;
            TraceData(data.ReadToEnd(), type, callingType, traceSourceName);
            return data;
        }

        internal static TraceSource GetTraceSource(
            string traceSourceName,
            bool isNamespaceName = false)
        {
            if (string.IsNullOrWhiteSpace(traceSourceName))
            {
                throw new ArgumentException($"IsNullOrWhiteSpace", nameof(traceSourceName));
            }
            return _sources.GetOrAdd(
                traceSourceName,
                (L_traceSourceName) =>
                {
                    var traceSource = default(TraceSource);
                    var bootstrapperType = Type.GetType("X4D.Diagnostics.Configuration.SystemDiagnosticsBootstrapper,X4D.Diagnostics.Configuration");
                    if (bootstrapperType != null)
                    {
                        var getConfiguredSource = bootstrapperType.GetMethod("GetConfiguredSource", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                        traceSource = getConfiguredSource.Invoke(null, new object[] { L_traceSourceName }) as TraceSource;
                    }
                    if (traceSource == null)
                    {
                        traceSource = new TraceSource(
                            L_traceSourceName,
                            isNamespaceName
                                ? SourceLevels.Off
                                : SourceLevels.All);
                    }
                    return traceSource;
                });
        }

        private static string DemangleTypeName(string fullName)
        {
            var idx = fullName.IndexOf("+<");
            return idx >= 0
                ? fullName.Remove(idx)
                : fullName;
        }

        private static T TraceData<T>(
            T data,
            TraceEventType type,
            Type callingType,
            string traceSourceName = null)
        {
            var id = Interlocked.Increment(ref _id);
            var message = BuildMessage<T>(data);
            if (!string.IsNullOrWhiteSpace(traceSourceName))
            {
                GetTraceSource(traceSourceName)
                    ?.TraceData(
                        type,
                        id,
                        message);
            }
            else
            {
                GetTraceSource(callingType.Namespace, true)
                    ?.TraceData(
                        type,
                        id,
                        message);
                GetTraceSource(DemangleTypeName(callingType.FullName))
                    ?.TraceData(
                        type,
                        id,
                        message);
            }
            return data;
        }

        private static string Normalize(string input)
        {
            return _normalizeRegex.Replace(input, " ");
        }

        private static object BuildMessage<T>(T data)
        {
            // flatten exception into an easy-to-serialize object
            if (data is Exception ex)
            {
                if (ex.Data?.Count > 0)
                {
                    // include `Data` property
                    return new
                    {
                        Type = ex.GetType().FullName,
                        Message = Normalize(ex.Message),
                        StackTrace = BuildFullStackTrace(ex),
                        //
                        Data = FullDataDump(ex)
                    };
                }
                else
                {
                    // omit `Data` property
                    return new
                    {
                        Type = ex.GetType().FullName,
                        Message = Normalize(ex.Message),
                        StackTrace = BuildFullStackTrace(ex)
                    };
                }
            }
            // else return a string
            return LoggingExtensions.Settings.ShouldNormalizeMessages
                ? Normalize(Convert.ToString(data))
                : Convert.ToString(data);
        }

        private static string FullDataDump(Exception ex)
        {
            if (ex.Data == null || ex.Data.Count == 0)
            {
                return null;
            }
            var sb = new StringBuilder();
            try
            {
                foreach (var key in ex.Data.Keys)
                {
                    sb.AppendLine($"{key}, {ex.Data[key]}");
                }
            }
            catch
            {
                // NOP
            }
            return sb.ToString();
        }

        /// <summary>
        /// <para>Produces a "deep" Stack Trace for exceptions, and includes
        /// </summary>
        /// <param name="ex">`Exception` and/or `FaultException`</param>
        /// <returns></returns>
        private static string BuildFullStackTrace(Exception ex)
        {
            var sb = new StringBuilder();
            try
            {
                while (ex != null)
                {
                    AppendException(sb, ex);
                    ex = ex.InnerException;
                }
            }
            catch (Exception L_ex)
            {
                sb.AppendLine("!!! ERROR BUILDING STACK TRACE - EXCEPTION FOLLOWS !!!");
                AppendException(sb, L_ex);
            }
            return sb.ToString().TrimEnd(s_crlfCharacters);
        }

        private static void AppendException(StringBuilder sb, Exception ex)
        {
            if (sb != null && ex != null)
            {
                sb.AppendLine("==(" + ex.GetType().FullName + "|" + (ex.Message ?? "") + ")==");
                if (ex.StackTrace != null)
                {
                    sb.AppendLine(ex.StackTrace);
                }
            }
        }
    }
}
