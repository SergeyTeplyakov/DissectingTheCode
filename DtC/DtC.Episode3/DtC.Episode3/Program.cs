using System.Runtime.CompilerServices;

namespace DtC.Episode3
{
    public record BuildGraph;

    public record ProjectFiles(List<string> Files);

    public record EvaluationResult(List<string> EvaluatedFiles);

    internal class Program
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static async Task Build()
        {
            // Parsing project files generates 1GB of data and takes 10 seconds.
            var buildGraph = BuildGraph();

            ForceGcCollectCheck();

            // Evaluating the build graph generates another 1GB and takes 10 seconds.
            var evaluationResult = EvaluateBuildGraph(buildGraph);
            
            // Execution phase: running the compilers to produce the build results
            await RunCompilers(evaluationResult);
        }

        static WeakReference? s_projectFilesWeakReference = null;
        private static BuildGraph BuildGraph()
        {
            ProjectFiles projectFiles = GetAllProjectFiles();
            s_projectFilesWeakReference = new WeakReference(projectFiles);
            var buildGraph = ComputeBuildGraph(projectFiles);
            return buildGraph;
        }

        static void ForceGcCollectCheck()
        {
            GC.Collect();
            if (s_projectFilesWeakReference?.IsAlive == true)
            {
                throw new InvalidOperationException("Project files were not collected! Potential memory leak detected. Please investigate!");
            }
            s_projectFilesWeakReference = null; // Reset to avoid false positives.
        }

        private static ProjectFiles GetAllProjectFiles()
        {
            const int stringCount = 1_048_576; // 1GB / 1KB
            const int stringSize = 512; // 1KB per string
            
            var files = new List<string>(stringCount);

            for (int i = 0; i < stringCount; i++)
            {
                string sample = new string('A', stringSize);
                files.Add(sample);
            }
            
            Thread.Sleep(5_000); // Simulate 10 seconds of work
            return new ProjectFiles(files);
        }

        private static BuildGraph ComputeBuildGraph(ProjectFiles projectFiles)
        {
            Thread.Sleep(2_000); // Simulate 5 seconds of work
            return new BuildGraph();
        }

        private static EvaluationResult EvaluateBuildGraph(BuildGraph buildGraph)
        {
            // Just calling the same method to allocate the same 1Gb of memory in 10 seconds.
            var list = GetAllProjectFiles();
            return new EvaluationResult(list.Files);
        }

        private static async Task RunCompilers(EvaluationResult evaluationResult)
        {
            await Task.Yield();
        }

        static void Main(string[] args)
        {
            Build().GetAwaiter().GetResult();
        }
    }
}
