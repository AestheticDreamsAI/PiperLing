using System;
using System.Threading.Tasks;

class Program
{

    static async Task Main(string[] args)
    {
            Task.Run(async () => ApiEndpoint.Run());
            await PiperLing.Init();


    }
}

