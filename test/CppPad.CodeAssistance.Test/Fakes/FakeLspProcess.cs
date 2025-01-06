#region

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CppPad.LspClient;
using CppPad.LspClient.Model;

#endregion

namespace CppPad.CodeAssistance.Test.Fakes
{
    public class FakeLspProcess : ILspProcess, IDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly ProducerConsumerStream _inputStream = new();
        private readonly List<JsonObject> _jsonObjects = [];
        private readonly ProducerConsumerStream _outputStream = new();
        private StreamWriter? _inputWriter;
        private Thread? _processingThread;

        public FakeLspProcess()
        {
            OutputReader = new StreamReader(_outputStream, Encoding.UTF8);
        }

        public IList<string> CompletionItems { get; set; } = ["CompletionItem"];

        public IList<PositionInFile> DefinitionPositions { get; set; } = new List<PositionInFile>();

        public TextReader OutputReader { get; }

        public TextWriter InputWriter =>
            _inputWriter ??= new StreamWriter(_inputStream, Encoding.UTF8) { AutoFlush = true };

        public bool HasExited { get; private set; }

        public Task StartAsync()
        {
            _processingThread = new Thread(ProcessJsonObjects);
            _processingThread.Start();
            return Task.CompletedTask;
        }

        public Task KillAsync()
        {
            Kill();
            return Task.CompletedTask;
        }
        
        public void Dispose()
        {
            Kill();
            GC.SuppressFinalize(this);
        }

        private void Kill()
        {
            _cts.Cancel();
            _inputStream.Complete();
            _outputStream.Complete();
            _processingThread?.Join();
            HasExited = true;
        }

        private void ProcessInputMessage(JsonObject json)
        {
            if (json.TryGetPropertyValue("method", out var method))
            {
                if (
                    method?.ToString() == "initialize"
                    && json.TryGetPropertyValue("id", out var id)
                )
                {
                    var response = CreateInitializeResponse(id!.GetValue<int>());
                    WriteToOutput(response);
                }
                else if (
                    method?.ToString() == "textDocument/completion"
                    && json.TryGetPropertyValue("id", out var completionId)
                )
                {
                    var response = CreateCompletionResponse(completionId!.GetValue<int>());
                    WriteToOutput(response);
                }
                else if (
                    method?.ToString() == "textDocument/definition"
                    && json.TryGetPropertyValue("id", out var definitionId)
                )
                {
                    var response = CreateDefinitionResponse(definitionId!.GetValue<int>());
                    WriteToOutput(response);
                }
            }
        }

        private object CreateDefinitionResponse(int definitionId)
        {
            var definitions = DefinitionPositions
                .Select(pos => new
                {
                    uri = pos.FileName,
                    range = new
                    {
                        start = new
                        {
                            line = pos.Position.Line,
                            character = pos.Position.Character,
                        },
                        end = new { line = pos.Position.Line, character = pos.Position.Character },
                    },
                })
                .ToArray();

            return new
            {
                jsonrpc = "2.0",
                id = definitionId,
                result = definitions,
            };
        }

        public List<JsonObject> GetJsonObjects()
        {
            return _jsonObjects;
        }

        private void ProcessJsonObjects()
        {
            try
            {
                var reader = new StreamReader(_inputStream, Encoding.UTF8);

                while (!_cts.Token.IsCancellationRequested)
                {
                    var message = ReadMessage(reader);
                    if (message != null)
                    {
                        ProcessMessage(message);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                _outputStream.Complete();
            }
        }

        private string? ReadMessage(StreamReader reader)
        {
            var contentLength = 0;
            while (reader.ReadLine() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                var headerParts = line.Split(':', 2);
                if (headerParts.Length == 2)
                {
                    var headerName = headerParts[0].Trim();
                    var headerValue = headerParts[1].Trim();

                    if (headerName.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                    {
                        int.TryParse(headerValue, out contentLength);
                    }
                }
            }

            if (contentLength > 0)
            {
                var buffer = new char[contentLength];
                var totalRead = 0;
                while (totalRead < contentLength)
                {
                    var read = reader.Read(buffer, totalRead, contentLength - totalRead);
                    if (read == 0)
                    {
                        return null;
                    }

                    totalRead += read;
                }

                return new string(buffer);
            }

            return null;
        }

        private void ProcessMessage(string message)
        {
            var jsonObject = JsonSerializer.Deserialize<JsonObject>(message);
            if (jsonObject != null)
            {
                _jsonObjects.Add(jsonObject);
                ProcessInputMessage(jsonObject);
            }
        }

        private static object CreateInitializeResponse(int id)
        {
            return new
            {
                jsonrpc = "2.0",
                id,
                result = new
                {
                    capabilities = new
                    {
                        textDocumentSync = 1,
                        completionProvider = new { triggerCharacters = new[] { ".", ">", "::" } },
                    },
                },
            };
        }

        private object CreateCompletionResponse(int completionId)
        {
            var completions = CompletionItems
                .Select(item => new
                {
                    label = item,
                    kind = 1,
                    detail = $"Detail for {item}",
                })
                .ToArray();

            return new
            {
                jsonrpc = "2.0",
                id = completionId,
                result = new { isIncomplete = false, items = completions },
            };
        }

        private void WriteToOutput(object obj)
        {
            var responseString = JsonSerializer.Serialize(obj);
            var responseBytes = Encoding.UTF8.GetBytes(responseString);
            var contentLength = responseBytes.Length;

            var header = $"Content-Length: {contentLength}\r\n\r\n";
            var headerBytes = Encoding.UTF8.GetBytes(header);

            _outputStream.Write(headerBytes, 0, headerBytes.Length);
            _outputStream.Write(responseBytes, 0, responseBytes.Length);
        }
    }
}
