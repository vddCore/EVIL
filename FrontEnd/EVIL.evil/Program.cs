namespace EVIL.evil;

using System.Threading.Tasks;

internal class Program
{
    public static async Task Main(string[] args)
        => await new EvmFrontEnd().Run(args);
}