using System;
using System.Threading.Tasks;

class Program
{

    static async Task Main(string[] args)
    {
        try
        {
            Task.Run(async () => ApiEndpoint.Run());
            await PiperLing.Init();
        }
        catch (Exception ex)
        {
Console.WriteLine($"Error - {ex.Message}");        
            Console.ReadLine();
        }

    }
}

