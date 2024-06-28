using System;
using System.Threading.Tasks;

class Program
{

    static async Task Main(string[] args)
    {
        try
        {
            await PiperLing.Init("llama3");
        }
        catch (Exception ex)
        {
Console.WriteLine($"Error - {ex.Message}");        
            Console.ReadLine();
        }

    }
}

