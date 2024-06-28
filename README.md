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


To use PiperLing, launch the application, select your recording device, and start speaking to translate your words. Don't forget to create or edit the config.json file. REMEMBER: The tags in the systemPrompt, such as [de-de] or [en-gb], are necessary for PiperTTS to recognize the language.
```json
{
  "systemPrompt": "You are now taking on the role of a professional interpreter. Please translate everything we discuss, tagging the recognized language of the translation at the beginning with tags like [de-de] for German or [en-gb] for English. Only output the translation. I am currently with friends and would like to communicate with them in English. However, my English isn't very good, so you need to translate their English sentences into German.\n\nFor example:\n[de-de] Wie geht es dir?\n[en-gb] I'm fine, how about you?", 
  "llmModel":"gpt-4o",
  "OpenaiApiKey": "your-api-key-here"
}
```
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
