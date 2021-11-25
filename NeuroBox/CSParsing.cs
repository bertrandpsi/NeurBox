using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace NeuroBox
{
    // Taken from https://laurentkempe.com/2019/02/18/dynamically-compile-and-run-code-using-dotNET-Core-3.0/
    public static class CSParsing
    {
        public class SimpleUnloadableAssemblyLoadContext : AssemblyLoadContext
        {
            public SimpleUnloadableAssemblyLoadContext()
                : base(true)
            {
            }

            protected override Assembly Load(AssemblyName assemblyName)
            {
                return null;
            }
        }

        public static CSharpCompilation GenerateCode(string sourceCode)
        {
            var codeString = SourceText.From(sourceCode);
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp10);

            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);


            MetadataReference[] references;

            try
            {
                references = new MetadataReference[]
                {
                    MetadataReference.CreateFromFile(System.AppContext.BaseDirectory+"NeurBox.dll"),
                    MetadataReference.CreateFromFile(System.AppContext.BaseDirectory+"System.dll"),
                    MetadataReference.CreateFromFile(System.AppContext.BaseDirectory+"System.Core.dll"),
                    MetadataReference.CreateFromFile(System.AppContext.BaseDirectory+"System.Runtime.dll"),
                    MetadataReference.CreateFromFile(System.AppContext.BaseDirectory+"System.Private.CoreLib.dll"),
                };
            }
            catch
            {
                references = new MetadataReference[]
                {
                    MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location),
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Math).Assembly.Location),
                };
            }

            return CSharpCompilation.Create("temp.dll", new[] { parsedSyntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
        }

        public static byte[] Compile(string sourceCode)
        {
            using (var peStream = new MemoryStream())
            {
                var result = GenerateCode(sourceCode).Emit(peStream);

                if (!result.Success)
                {
                    var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
                    var message = string.Join("\n", failures.Select(row => row.GetMessage()));
                    throw new Exception(message);
                }

                peStream.Seek(0, SeekOrigin.Begin);

                return peStream.ToArray();
            }
        }

        public static void UnloadAssembly(WeakReference weakReference)
        {
            for (var i = 0; i < 8 && weakReference.IsAlive; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            if (weakReference.IsAlive)
                throw new Exception("Cannot unload assembly");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static WeakReference LoadAndExecute(string sourceCode, string[] args = null)
        {
            var buff = Compile(sourceCode);
            using (var asm = new MemoryStream(buff))
            {
                var assemblyLoadContext = new SimpleUnloadableAssemblyLoadContext();
                var assembly = assemblyLoadContext.LoadFromStream(asm);
                return new WeakReference(assemblyLoadContext);
            }
        }
    }
}
