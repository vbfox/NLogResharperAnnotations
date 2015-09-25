using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using ExternalAnnotationsGenerator;
using NDesk.Options;
using NLog;
using NLog.Common;
using NLog.Config;
using static ExternalAnnotationsGenerator.Annotations;
#pragma warning disable 612
#pragma warning disable 618

namespace NLogResharperAnnotations
{
    [SuppressMessage("ReSharper", "PossibleUnintendedReferenceComparison")]
    class Program
    {
        [SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod")]
        static int Main(string[] args)
        {
            var annotator = Annotator.Create();

            AnnotateLogger(annotator);
            AnnotateFactories(annotator);

            var nuspec = new NugetSpec(
                id: "NLog.Annotations",
                title: "NLog Annotations",
                authors: "Julien Roncaglia",
                owners: "Julien Roncaglia",
                projectUrl: "https://github.com/vbfox/NLogResharperAnnotations",
                iconUrl: "https://raw.github.com/vbfox/NLogResharperAnnotations/master/nlog.png",
                description: "Annotations for the NLog.dll file",
                tags: "NLog Annotations");

            return RunApp("NLogResharperAnnotations", args, annotator, nuspec);
        }

        private static NugetSpec SpecWithVersion(NugetSpec spec, Version version)
        {
            return new NugetSpec(
                spec.Id,
                version.ToString(),
                spec.Title,
                spec.Authors,
                spec.Owners,
                spec.ProjectUrl,
                spec.IconUrl,
                spec.Description,
                spec.Tags);
        }

        private const int SuccessExitCode = 0;
        private const int ExceptionExitCode = 1;
        private const int InvalidArgumentsExitCode = 2;

        private static int RunApp(string exeName, string[] args, IAnnotator annotator, NugetSpec nuspec)
        {
            try
            {
                var parsedArgs = ParseArgs(args);
                if (parsedArgs.ShowHelp)
                {
                    ShowHelp(exeName, parsedArgs);
                    return parsedArgs.ParseError == null ? SuccessExitCode : InvalidArgumentsExitCode;
                }

                RunGeneration(annotator, nuspec, parsedArgs);
                return SuccessExitCode;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                return ExceptionExitCode;
            }
        }

        private static void RunGeneration(IAnnotator annotator, NugetSpec nuspec, Args parsedArgs)
        {
            var version = parsedArgs.Version ?? new Version("1.0.0.0");
            var dir = parsedArgs.Directory ?? new DirectoryInfo(Environment.CurrentDirectory);
            var fixedSpec = SpecWithVersion(nuspec, version);
            annotator.CreateNugetPackage(fixedSpec, dir);

            Console.WriteLine($"Generated version {version}  in {dir.FullName}");
        }

        private static void ShowHelp(string exeName, Args args)
        {
            var set = GetOptionSet(new Args());
            if (args.ParseError != null)
            {
                Console.WriteLine(args.ParseError);
                Console.WriteLine();
            }
            Console.WriteLine($"Usage: {exeName} [OPTIONS]+");
            Console.WriteLine("Generate the nuget annotation package");
            Console.WriteLine();
            Console.WriteLine("Options:");
            set.WriteOptionDescriptions(Console.Out);
        }

        private class Args
        {
            public bool ShowHelp { get; set; }
            public Version Version { get; set; }
            public string ParseError { get; set; }
            public DirectoryInfo Directory { get; set; }
        }

        private static OptionSet GetOptionSet(Args args)
        {
            return new OptionSet
            {
                {
                    "h|?|help",
                    "show this message and exit",
                    v => args.ShowHelp = v != null
                },
                {
                    "v|version=",
                    "Version of the generated package (Default: 1.0.0.0)",
                    v => args.Version = new Version(v)
                },
                {
                    "d|directory=",
                    "The root directory of the NuGet package (Default: Current directory)",
                    v => args.Directory = new DirectoryInfo(v)
                }
            };
        }

        private static Args ParseArgs(string[] args)
        {
            var result = new Args();
            try
            {
                var set = GetOptionSet(result);

                var extra = set.Parse(args);
                if (extra.Count != 0)
                {
                    result.ShowHelp = true;
                    result.ParseError = "Unknown parameters : " + string.Join(" ", extra);
                }

                return result;
            }
            catch(Exception exception)
            {
                result.ShowHelp = true;
                result.ParseError = "Error : " + exception.Message;
                return result;
            }
        }

        private static void AnnotateLogger(IAnnotator annotator)
        {
            annotator.Annotate<ILogger>(type =>
            {
                AnnotateFatal(type);
                AnnotateError(type);
                AnnotateWarn(type);
                AnnotateInfo(type);
                AnnotateDebug(type);
                AnnotateTrace(type);
            });
        }

        static void AnnotateDebug(ITypeAnnotator<ILogger> t)
        {
            // Modern interface
            t.Annotate(l => l.Debug(CanBeNull<TClass>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Debug(NotNull<LogMessageGenerator>()));
            t.Annotate(l => l.DebugException(NotNull<string>(), NotNull<Exception>()));
            t.Annotate(l => l.Debug(NotNull<Exception>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Debug(NotNull<Exception>(), NotNull<IFormatProvider>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Debug(NotNull<string>()));
            t.Annotate(l => l.Debug(FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Debug(NotNull<string>(), NotNull<Exception>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>()));
            t.Annotate(l => l.Debug(FormatString(), CanBeNull<TClass>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Debug(FormatString(), CanBeNull<TClass>(), NotNull<TClass>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Debug(FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>(), CanBeNull<TClass>()));

            // NLog-V1
            t.Annotate(l => l.Debug(CanBeNull<object>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), CanBeNull<object>()));
            t.Annotate(l => l.Debug(FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Debug(FormatString(), CanBeNull<object>(), CanBeNull<object>()));
            t.Annotate(l => l.Debug(FormatString(), CanBeNull<object>(), CanBeNull<object>(), CanBeNull<object>()));
            t.Annotate(l => l.Debug(FormatString(), Some<bool>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), FormatString(), Some<bool>()));
            t.Annotate(l => l.Debug(FormatString(), Some<char>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), FormatString(), Some<char>()));
            t.Annotate(l => l.Debug(FormatString(), Some<byte>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), FormatString(), Some<byte>()));
            t.Annotate(l => l.Debug(FormatString(), Some<string>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), FormatString(), Some<string>()));
            t.Annotate(l => l.Debug(FormatString(), Some<int>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), FormatString(), Some<int>()));
            t.Annotate(l => l.Debug(FormatString(), Some<long>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), FormatString(), Some<long>()));
            t.Annotate(l => l.Debug(FormatString(), Some<float>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), FormatString(), Some<float>()));
            t.Annotate(l => l.Debug(FormatString(), Some<double>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), FormatString(), Some<double>()));
            t.Annotate(l => l.Debug(FormatString(), Some<decimal>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), FormatString(), Some<decimal>()));
            t.Annotate(l => l.Debug(FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Debug(FormatString(), Some<sbyte>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), FormatString(), Some<sbyte>()));
            t.Annotate(l => l.Debug(FormatString(), Some<uint>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), FormatString(), Some<uint>()));
            t.Annotate(l => l.Debug(FormatString(), Some<ulong>()));
            t.Annotate(l => l.Debug(NotNull<IFormatProvider>(), FormatString(), Some<ulong>()));
        }

        static void AnnotateInfo(ITypeAnnotator<ILogger> t)
        {
            // Modern interface
            t.Annotate(l => l.Info(CanBeNull<TClass>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Info(NotNull<LogMessageGenerator>()));
            t.Annotate(l => l.InfoException(NotNull<string>(), NotNull<Exception>()));
            t.Annotate(l => l.Info(NotNull<Exception>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Info(NotNull<Exception>(), NotNull<IFormatProvider>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Info(NotNull<string>()));
            t.Annotate(l => l.Info(FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Info(NotNull<string>(), NotNull<Exception>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>()));
            t.Annotate(l => l.Info(FormatString(), CanBeNull<TClass>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Info(FormatString(), CanBeNull<TClass>(), NotNull<TClass>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Info(FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>(), CanBeNull<TClass>()));

            // NLog-V1
            t.Annotate(l => l.Info(CanBeNull<object>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), CanBeNull<object>()));
            t.Annotate(l => l.Info(FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Info(FormatString(), CanBeNull<object>(), CanBeNull<object>()));
            t.Annotate(l => l.Info(FormatString(), CanBeNull<object>(), CanBeNull<object>(), CanBeNull<object>()));
            t.Annotate(l => l.Info(FormatString(), Some<bool>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), FormatString(), Some<bool>()));
            t.Annotate(l => l.Info(FormatString(), Some<char>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), FormatString(), Some<char>()));
            t.Annotate(l => l.Info(FormatString(), Some<byte>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), FormatString(), Some<byte>()));
            t.Annotate(l => l.Info(FormatString(), Some<string>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), FormatString(), Some<string>()));
            t.Annotate(l => l.Info(FormatString(), Some<int>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), FormatString(), Some<int>()));
            t.Annotate(l => l.Info(FormatString(), Some<long>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), FormatString(), Some<long>()));
            t.Annotate(l => l.Info(FormatString(), Some<float>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), FormatString(), Some<float>()));
            t.Annotate(l => l.Info(FormatString(), Some<double>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), FormatString(), Some<double>()));
            t.Annotate(l => l.Info(FormatString(), Some<decimal>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), FormatString(), Some<decimal>()));
            t.Annotate(l => l.Info(FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Info(FormatString(), Some<sbyte>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), FormatString(), Some<sbyte>()));
            t.Annotate(l => l.Info(FormatString(), Some<uint>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), FormatString(), Some<uint>()));
            t.Annotate(l => l.Info(FormatString(), Some<ulong>()));
            t.Annotate(l => l.Info(NotNull<IFormatProvider>(), FormatString(), Some<ulong>()));
        }

        static void AnnotateFatal(ITypeAnnotator<ILogger> t)
        {
            // Modern interface
            t.Annotate(l => l.Fatal(CanBeNull<TClass>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Fatal(NotNull<LogMessageGenerator>()));
            t.Annotate(l => l.FatalException(NotNull<string>(), NotNull<Exception>()));
            t.Annotate(l => l.Fatal(NotNull<Exception>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Fatal(NotNull<Exception>(), NotNull<IFormatProvider>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Fatal(NotNull<string>()));
            t.Annotate(l => l.Fatal(FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Fatal(NotNull<string>(), NotNull<Exception>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>()));
            t.Annotate(l => l.Fatal(FormatString(), CanBeNull<TClass>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Fatal(FormatString(), CanBeNull<TClass>(), NotNull<TClass>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Fatal(FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>(), CanBeNull<TClass>()));

            // NLog-V1
            t.Annotate(l => l.Fatal(CanBeNull<object>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), CanBeNull<object>()));
            t.Annotate(l => l.Fatal(FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Fatal(FormatString(), CanBeNull<object>(), CanBeNull<object>()));
            t.Annotate(l => l.Fatal(FormatString(), CanBeNull<object>(), CanBeNull<object>(), CanBeNull<object>()));
            t.Annotate(l => l.Fatal(FormatString(), Some<bool>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), FormatString(), Some<bool>()));
            t.Annotate(l => l.Fatal(FormatString(), Some<char>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), FormatString(), Some<char>()));
            t.Annotate(l => l.Fatal(FormatString(), Some<byte>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), FormatString(), Some<byte>()));
            t.Annotate(l => l.Fatal(FormatString(), Some<string>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), FormatString(), Some<string>()));
            t.Annotate(l => l.Fatal(FormatString(), Some<int>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), FormatString(), Some<int>()));
            t.Annotate(l => l.Fatal(FormatString(), Some<long>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), FormatString(), Some<long>()));
            t.Annotate(l => l.Fatal(FormatString(), Some<float>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), FormatString(), Some<float>()));
            t.Annotate(l => l.Fatal(FormatString(), Some<double>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), FormatString(), Some<double>()));
            t.Annotate(l => l.Fatal(FormatString(), Some<decimal>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), FormatString(), Some<decimal>()));
            t.Annotate(l => l.Fatal(FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Fatal(FormatString(), Some<sbyte>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), FormatString(), Some<sbyte>()));
            t.Annotate(l => l.Fatal(FormatString(), Some<uint>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), FormatString(), Some<uint>()));
            t.Annotate(l => l.Fatal(FormatString(), Some<ulong>()));
            t.Annotate(l => l.Fatal(NotNull<IFormatProvider>(), FormatString(), Some<ulong>()));
        }

        static void AnnotateWarn(ITypeAnnotator<ILogger> t)
        {
            // Modern interface
            t.Annotate(l => l.Warn(CanBeNull<TClass>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Warn(NotNull<LogMessageGenerator>()));
            t.Annotate(l => l.WarnException(NotNull<string>(), NotNull<Exception>()));
            t.Annotate(l => l.Warn(NotNull<Exception>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Warn(NotNull<Exception>(), NotNull<IFormatProvider>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Warn(NotNull<string>()));
            t.Annotate(l => l.Warn(FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Warn(NotNull<string>(), NotNull<Exception>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>()));
            t.Annotate(l => l.Warn(FormatString(), CanBeNull<TClass>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Warn(FormatString(), CanBeNull<TClass>(), NotNull<TClass>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Warn(FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>(), CanBeNull<TClass>()));

            // NLog-V1
            t.Annotate(l => l.Warn(CanBeNull<object>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), CanBeNull<object>()));
            t.Annotate(l => l.Warn(FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Warn(FormatString(), CanBeNull<object>(), CanBeNull<object>()));
            t.Annotate(l => l.Warn(FormatString(), CanBeNull<object>(), CanBeNull<object>(), CanBeNull<object>()));
            t.Annotate(l => l.Warn(FormatString(), Some<bool>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), FormatString(), Some<bool>()));
            t.Annotate(l => l.Warn(FormatString(), Some<char>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), FormatString(), Some<char>()));
            t.Annotate(l => l.Warn(FormatString(), Some<byte>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), FormatString(), Some<byte>()));
            t.Annotate(l => l.Warn(FormatString(), Some<string>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), FormatString(), Some<string>()));
            t.Annotate(l => l.Warn(FormatString(), Some<int>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), FormatString(), Some<int>()));
            t.Annotate(l => l.Warn(FormatString(), Some<long>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), FormatString(), Some<long>()));
            t.Annotate(l => l.Warn(FormatString(), Some<float>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), FormatString(), Some<float>()));
            t.Annotate(l => l.Warn(FormatString(), Some<double>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), FormatString(), Some<double>()));
            t.Annotate(l => l.Warn(FormatString(), Some<decimal>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), FormatString(), Some<decimal>()));
            t.Annotate(l => l.Warn(FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Warn(FormatString(), Some<sbyte>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), FormatString(), Some<sbyte>()));
            t.Annotate(l => l.Warn(FormatString(), Some<uint>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), FormatString(), Some<uint>()));
            t.Annotate(l => l.Warn(FormatString(), Some<ulong>()));
            t.Annotate(l => l.Warn(NotNull<IFormatProvider>(), FormatString(), Some<ulong>()));
        }

        static void AnnotateTrace(ITypeAnnotator<ILogger> t)
        {
            // Modern interface
            t.Annotate(l => l.Trace(CanBeNull<TClass>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Trace(NotNull<LogMessageGenerator>()));
            t.Annotate(l => l.TraceException(NotNull<string>(), NotNull<Exception>()));
            t.Annotate(l => l.Trace(NotNull<Exception>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Trace(NotNull<Exception>(), NotNull<IFormatProvider>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Trace(NotNull<string>()));
            t.Annotate(l => l.Trace(FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Trace(NotNull<string>(), NotNull<Exception>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>()));
            t.Annotate(l => l.Trace(FormatString(), CanBeNull<TClass>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Trace(FormatString(), CanBeNull<TClass>(), NotNull<TClass>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Trace(FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>(), CanBeNull<TClass>()));

            // NLog-V1
            t.Annotate(l => l.Trace(CanBeNull<object>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), CanBeNull<object>()));
            t.Annotate(l => l.Trace(FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Trace(FormatString(), CanBeNull<object>(), CanBeNull<object>()));
            t.Annotate(l => l.Trace(FormatString(), CanBeNull<object>(), CanBeNull<object>(), CanBeNull<object>()));
            t.Annotate(l => l.Trace(FormatString(), Some<bool>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), FormatString(), Some<bool>()));
            t.Annotate(l => l.Trace(FormatString(), Some<char>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), FormatString(), Some<char>()));
            t.Annotate(l => l.Trace(FormatString(), Some<byte>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), FormatString(), Some<byte>()));
            t.Annotate(l => l.Trace(FormatString(), Some<string>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), FormatString(), Some<string>()));
            t.Annotate(l => l.Trace(FormatString(), Some<int>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), FormatString(), Some<int>()));
            t.Annotate(l => l.Trace(FormatString(), Some<long>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), FormatString(), Some<long>()));
            t.Annotate(l => l.Trace(FormatString(), Some<float>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), FormatString(), Some<float>()));
            t.Annotate(l => l.Trace(FormatString(), Some<double>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), FormatString(), Some<double>()));
            t.Annotate(l => l.Trace(FormatString(), Some<decimal>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), FormatString(), Some<decimal>()));
            t.Annotate(l => l.Trace(FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Trace(FormatString(), Some<sbyte>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), FormatString(), Some<sbyte>()));
            t.Annotate(l => l.Trace(FormatString(), Some<uint>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), FormatString(), Some<uint>()));
            t.Annotate(l => l.Trace(FormatString(), Some<ulong>()));
            t.Annotate(l => l.Trace(NotNull<IFormatProvider>(), FormatString(), Some<ulong>()));
        }

        static void AnnotateError(ITypeAnnotator<ILogger> t)
        {
            // Modern interface
            t.Annotate(l => l.Error(CanBeNull<TClass>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Error(NotNull<LogMessageGenerator>()));
            t.Annotate(l => l.ErrorException(NotNull<string>(), NotNull<Exception>()));
            t.Annotate(l => l.Error(NotNull<Exception>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Error(NotNull<Exception>(), NotNull<IFormatProvider>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Error(NotNull<string>()));
            t.Annotate(l => l.Error(FormatString(), NotNull<object[]>()));
            t.Annotate(l => l.Error(NotNull<string>(), NotNull<Exception>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>()));
            t.Annotate(l => l.Error(FormatString(), CanBeNull<TClass>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Error(FormatString(), CanBeNull<TClass>(), NotNull<TClass>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>(), CanBeNull<TClass>()));
            t.Annotate(l => l.Error(FormatString(), CanBeNull<TClass>(), CanBeNull<TClass>(), CanBeNull<TClass>()));

            // NLog-V1
            t.Annotate(l => l.Error(CanBeNull<object>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), CanBeNull<object>()));
            t.Annotate(l => l.Error(FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Error(FormatString(), CanBeNull<object>(), CanBeNull<object>()));
            t.Annotate(l => l.Error(FormatString(), CanBeNull<object>(), CanBeNull<object>(), CanBeNull<object>()));
            t.Annotate(l => l.Error(FormatString(), Some<bool>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), FormatString(), Some<bool>()));
            t.Annotate(l => l.Error(FormatString(), Some<char>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), FormatString(), Some<char>()));
            t.Annotate(l => l.Error(FormatString(), Some<byte>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), FormatString(), Some<byte>()));
            t.Annotate(l => l.Error(FormatString(), Some<string>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), FormatString(), Some<string>()));
            t.Annotate(l => l.Error(FormatString(), Some<int>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), FormatString(), Some<int>()));
            t.Annotate(l => l.Error(FormatString(), Some<long>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), FormatString(), Some<long>()));
            t.Annotate(l => l.Error(FormatString(), Some<float>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), FormatString(), Some<float>()));
            t.Annotate(l => l.Error(FormatString(), Some<double>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), FormatString(), Some<double>()));
            t.Annotate(l => l.Error(FormatString(), Some<decimal>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), FormatString(), Some<decimal>()));
            t.Annotate(l => l.Error(FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), FormatString(), CanBeNull<object>()));
            t.Annotate(l => l.Error(FormatString(), Some<sbyte>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), FormatString(), Some<sbyte>()));
            t.Annotate(l => l.Error(FormatString(), Some<uint>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), FormatString(), Some<uint>()));
            t.Annotate(l => l.Error(FormatString(), Some<ulong>()));
            t.Annotate(l => l.Error(NotNull<IFormatProvider>(), FormatString(), Some<ulong>()));
        }

        private static void AnnotateFactories(IAnnotator annotator)
        {
            annotator.Annotate<LogManager>(type =>
            {
                type.Annotate(_ => LogManager.CreateNullLogger() == NotNull<ILogger>());
                type.Annotate(_ => LogManager.GetCurrentClassLogger() == NotNull<ILogger>());
                type.Annotate(_ => LogManager.GetCurrentClassLogger(CanBeNull<Type>()) == NotNull<ILogger>());
                type.Annotate(_ => LogManager.GetLogger(NotNull<string>()) == NotNull<ILogger>());
                type.Annotate(_ => LogManager.GetLogger(NotNull<string>(), CanBeNull<Type>()) == NotNull<ILogger>());
                type.Annotate(_ => LogManager.DefaultCultureInfo == NotNull<LogManager.GetCultureInfo>());
                type.Annotate(_ => LogManager.Configuration == CanBeNull<LoggingConfiguration>());
                type.Annotate(_ => LogManager.DisableLogging() == NotNull<IDisposable>());
                type.Annotate(_ => LogManager.AddHiddenAssembly(NotNull<Assembly>()));
                type.Annotate(_ => LogManager.Flush(NotNull<AsyncContinuation>()));
                type.Annotate(_ => LogManager.Flush(NotNull<AsyncContinuation>(), Some<TimeSpan>()));
                type.Annotate(_ => LogManager.Flush(NotNull<AsyncContinuation>(), Some<int>()));
            });

            annotator.Annotate<LogFactory>(type =>
            {
                type.Annotate(x => x.CreateNullLogger() == NotNull<ILogger>());
                type.Annotate(x => x.GetCurrentClassLogger() == NotNull<ILogger>());
                type.Annotate(x => x.GetCurrentClassLogger(CanBeNull<Type>()) == NotNull<ILogger>());
                type.Annotate(x => x.GetLogger(NotNull<string>()) == NotNull<ILogger>());
                type.Annotate(x => x.GetLogger(NotNull<string>(), CanBeNull<Type>()) == NotNull<ILogger>());
                type.Annotate(x => x.DefaultCultureInfo == CanBeNull<CultureInfo>());
                type.Annotate(x => x.Configuration == CanBeNull<LoggingConfiguration>());
                type.Annotate(x => x.DisableLogging() == NotNull<IDisposable>());
                type.Annotate(x => x.SuspendLogging() == NotNull<IDisposable>());
                type.Annotate(x => x.Flush(NotNull<AsyncContinuation>()));
                type.Annotate(x => x.Flush(NotNull<AsyncContinuation>(), Some<TimeSpan>()));
                type.Annotate(x => x.Flush(NotNull<AsyncContinuation>(), Some<int>()));
            });
        }
    }
}