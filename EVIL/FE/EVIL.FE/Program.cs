namespace EVIL.FE;

using System.Threading.Tasks;

internal class Program
{
    public static async Task Main(string[] args)
        => await new EvmFrontEnd().Run(args);
}