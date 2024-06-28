using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        AIHelper.Init();
        while (true)
        {
            try
            {
                var text = Console.ReadLine();
                await AIHelper.DoJob(text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ein Fehler ist aufgetreten: {ex.Message}");
            }
        }
    }
}

