# PiperLing

PiperLing is a powerful open-source interpreter AI based on Piper TTS, Ollama (LLama3), and C#. Designed to provide simple and fast real-time speech translation and synthesis.

## Features

- **Real-time Translation:** Fast and accurate translations in multiple languages.
- **Text-to-Speech:** Natural and clear speech synthesis for various use cases.
- **Extensible and Customizable:** Open-source code that can be easily extended and customized.
- **Cutting-edge Technology:** Utilizes advanced models like Piper TTS and Ollama (LLama3).

## Installation

To install PiperLing on Windows, follow these steps:

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/yourusername/PiperLing.git
   cd PiperLing
   ```

2. **Install Dependencies:**
   Ensure you have the required dependencies installed. This may include .NET SDK, necessary libraries for Piper TTS, and Ollama (LLama3) components.

3. **Build the Project:**
   ```bash
   dotnet build
   ```

4. **Run the Application:**
   ```bash
   dotnet run
   ```

## Usage

To use PiperLing, run the application and input the text you want to translate. The application will handle the translation in real-time. Here is a brief example of the `Program.cs` usage:

```csharp
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            await PiperLing.Init();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error - {ex.Message}");
            Console.ReadLine();
        }
    }
}
```

## Project Structure

- **AIHelper.cs:** Contains helper methods for AI-related tasks.
- **AudioHelper.cs:** Provides functionalities for handling audio input and output.
- **PiperHelper.cs:** Integrates Piper TTS functionalities.
- **PiperLing.csproj:** Project file for the PiperLing application.
- **Program.cs:** Main entry point of the application.

## Contributing

We welcome contributions from the community! If you'd like to contribute to PiperLing, please follow these steps:

1. Fork the repository.
2. Create a new branch: `git checkout -b feature-branch`.
3. Make your changes and commit them: `git commit -m 'Add new feature'`.
4. Push to the branch: `git push origin feature-branch`.
5. Submit a pull request.

For more details, please refer to our [Contribution Guidelines](CONTRIBUTING.md).

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

## Contact

For any questions or suggestions, feel free to open an issue or contact us at [contact@aestheticdreams.net](mailto:contact@aestheticdreams.net).
