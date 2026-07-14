using System.Threading;
using OllamaSharp;

class Jarvis
{
public static void ShowHelp()
    {
        Console.WriteLine("""
            ================================
            jarvis - Local AI Chat Assistant
            ================================

            This tool lets you chat with a locally running AI model via Ollama.

            Commands:
            /help     Show this help message
            /quit     Exit the program
            /clear    Clear the chat history

            Usage:
            Just type your message and press Enter to chat with the AI.
            The conversation history is remembered during this session.

            Notes:
            - Make sure Ollama is running in the background ('ollama serve').
            - Current model in use: llama3.2:3b
        """);
    }
    public async static Task StopSpinner(CancellationTokenSource cts, Task task, bool firstTokenReceived)
    {
        cts.Cancel();
        await task;
        firstTokenReceived = true;
    }

public static async Task Main(string[] args)
{
    System.Console.WriteLine("Welcome to Jarvis.");
    System.Console.WriteLine("Your local AI chat assistant powered by Ollama.");
    System.Console.WriteLine("Type /help for a list of commands.");
    System.Console.WriteLine();
    var ollama = new OllamaApiClient( new Uri( "http://localhost:11434"));
    ollama.SelectedModel = "llama3.2:3b";
    var chat = new Chat(ollama);

    
        while (true)
        {
            Console.Write("==> ");
            string? inputMessage = Console.ReadLine();
            string? input = inputMessage?.ToLower().Trim();
            switch (input)
            {
                case "/help":
                ShowHelp();
                continue;
                case "/clear":
                chat = new Chat(ollama);
                System.Console.WriteLine("Chat history has been cleared");
                continue;

                case "/model":
                System.Console.WriteLine($"Current model: {ollama.SelectedModel}");
                continue;
                case "/quit":
                System.Console.WriteLine("Bye");
                return;
            }
            if(string.IsNullOrWhiteSpace(inputMessage))
            {
                continue;
            }
            Console.Write("Jarvis: ");
            var SpinnerCTS = new CancellationTokenSource();
            var SpinnerTask = ShowSpinnerAsync(SpinnerCTS.Token);
            bool firstTokenReceived = false;
            try
            {
            await foreach (var part in chat.SendAsync(inputMessage))
            {
                    if (!firstTokenReceived)
                    {
                    await StopSpinner(SpinnerCTS, SpinnerTask, firstTokenReceived);
                    }
                    System.Console.Write(part);
            }
            } catch (HttpRequestException ex)
            {
                if (!firstTokenReceived) { await StopSpinner(SpinnerCTS, SpinnerTask, firstTokenReceived); }
                System.Console.WriteLine();
                System.Console.WriteLine($"[Error] Could not connect to Ollama. Make sure 'ollama serve' is running. ({ex.Message}");
            } catch (TaskCanceledException ex)
            {
                if (!firstTokenReceived) { await StopSpinner(SpinnerCTS, SpinnerTask, firstTokenReceived); }
                System.Console.WriteLine();
                System.Console.WriteLine($"[Error] The request timed out. The model might be taking too long to respond. {ex.Message}");
            } catch (Exception ex)
            {
                if (!firstTokenReceived) { await StopSpinner(SpinnerCTS, SpinnerTask, firstTokenReceived); }
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
            // Intentionally left empty to handle cancellation without throwing an exception
        }
        System.Console.Write("\b");
    } 
}