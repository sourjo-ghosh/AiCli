using System.Threading;
using OllamaSharp;

class AiCli
{
public static async Task Main(string[] args)
{
    System.Console.WriteLine("Welcome to AiCli.");
    System.Console.WriteLine("Type /quit or /help");

    var ollama = new OllamaApiClient( new Uri( "http://localhost:11434"));
    ollama.SelectedModel = "llama3.2:3b";
    var chat = new Chat(ollama);


        while (true)
        {
            Console.Write("You: ");
            string? inputMessage = Console.ReadLine();
            string? input = inputMessage?.ToLower().Trim();
            if(input == "/quit")
            {   
                System.Console.WriteLine("Bye");
                break;
            }
            if(input == "/help")
            {
                Console.WriteLine("""
            ================================
            AiCli - Local AI Chat Assistant
            ================================

            This tool lets you chat with a locally running AI model via Ollama.

            Commands:
            /help     Show this help message
            /quit     Exit the program

            Usage:
            Just type your message and press Enter to chat with the AI.
            The conversation history is remembered during this session.

            Notes:
            - Make sure Ollama is running in the background ('ollama serve').
            - Current model in use: llama3.2:3b
            """);
            continue;
            }
            if(input == "/clear")
            {
                chat = new Chat(ollama);
                System.Console.WriteLine("Chat history has been cleared");
                continue;
            }
            Console.Write("AI: ");
            var SpinnerCTS = new CancellationTokenSource();
            var SpinnerTask = ShowSpinnerAsync(SpinnerCTS.Token);
            bool firstTokenReceived = false;

            try
            {
            await foreach (var part in chat.SendAsync(inputMessage))
            {
                    if (!firstTokenReceived)
                    {
                    SpinnerCTS.Cancel();
                    await SpinnerTask;
                    firstTokenReceived = true;
                    }
                    System.Console.Write(part);
            }
            if (!firstTokenReceived)
            {
            SpinnerCTS.Cancel();
            await SpinnerTask;
            }
            } catch (HttpRequestException ex)
            {
                if (!firstTokenReceived) { SpinnerCTS.Cancel(); await SpinnerTask; }
                System.Console.WriteLine();
                System.Console.WriteLine($"[Error] Could not connect to Ollama. Make sure 'ollama serve' is running. ({ex.Message}");
            } catch (TaskCanceledException ex)
            {
                if (!firstTokenReceived) { SpinnerCTS.Cancel(); await SpinnerTask; }
                System.Console.WriteLine();
                System.Console.WriteLine($"[Error] The request timed out. The model might be taking too long to respond. {ex.Message}");
            } catch (Exception ex)
            {
                if (!firstTokenReceived) { SpinnerCTS.Cancel(); await SpinnerTask; }
                System.Console.WriteLine();
                System.Console.WriteLine($"[Unexpected Error] {ex.GetType().Name}: {ex.Message}");
            }
            
            System.Console.WriteLine();
        }
}

static async Task ShowSpinnerAsync(CancellationToken token)
    {
        string[] frames = { "|", "/", "-", "\\" };
        int i = 0;

        try
        {
            while (!token.IsCancellationRequested)
            {
                System.Console.Write(frames[i % frames.Length]);
                await Task.Delay(100, token);
                System.Console.Write("\b");
                i++;
            }
        }
        catch (TaskCanceledException)
        {
            
        }
        System.Console.Write("\b");
    } 

}