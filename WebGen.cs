using RazorLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagmaDataMiner
{
    internal class WebGen
    {
        private static RazorLightEngine engine;

        static WebGen()
        {
            engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(Environment.CurrentDirectory)
                .UseMemoryCachingProvider()
                .EnableDebugMode()
                .Build();

            Console.WriteLine(Environment.CurrentDirectory);
        }

        public static async void Bob(AbilitiesModel model)
        {
            Console.WriteLine("a");

            var html = await engine.CompileRenderAsync("bob.cshtml", model);
            Console.WriteLine("b");
            File.WriteAllText(@"D:\stuff.html", html);
        }
    }

    public class Bobber
    {
        public int BobValue { get; set; }
    }
}
