using System.Threading.Tasks;

namespace EVIL.evil
{
    internal class Program
    {
        public static async Task Main(string[] args)
            => await new EvmFrontEnd().Run(args);
    }
}