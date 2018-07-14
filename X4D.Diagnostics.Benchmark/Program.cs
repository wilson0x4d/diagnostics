using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Reports;
using System.Configuration;
using X4D.Diagnostics.Logging;

namespace X4D.Diagnostics.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = ConfigurationManager.AppSettings["test"];
            if (test != "test")
            {
                throw new Exception("test");
            }
            else
            {
                test.Log();
            }
            X4DBenchmarkRunner.Run();
        }
    }

    public static class X4DBenchmarkRunner
    {
        public static IEnumerable<Summary> Run(
            Assembly assembly)
        {
            var benchmarkTypes = assembly
                .GetTypes();
            benchmarkTypes = benchmarkTypes
                .Where(type => type
                    .GetMethods()
                    .Any(method => method
                        .GetCustomAttributes()
                        .Any(attribute => attribute is BenchmarkAttribute)))
                .ToArray();
            var summaries = new List<Summary>();
            foreach (var type in benchmarkTypes)
            {
                summaries.Add(
                    BenchmarkRunner.Run(type));
            }
            return summaries;
        }

        internal static IEnumerable<Summary> Run()
        {
            var appDomain = AppDomain.CurrentDomain;
            var assemblies = appDomain.GetAssemblies();
            var summaries = new List<Summary>();
            foreach (var assembly in assemblies)
            {
                summaries.AddRange(
                    X4DBenchmarkRunner.Run(assembly));
            }
            return summaries;
        }

        //public static IEnumerable<Summary> Run()
        //{
        //}
    }
}
