using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Threading.Tasks; // Ensure this is present
using System; // Ensure this is present

public sealed class WebViewBridge {
    private readonly WebView2 _webView;

    // JS -> C# Handlers
    private readonly ConcurrentDictionary<string, Func<Dictionary<string, object>, Task<object?>>> _handlers = new();

    // C# -> JS Callbacks (NEW: Tracks pending C# calls awaiting JS results)
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _jsCallbacks = new();

    private readonly List<Func<Task>> _onLoadHandlers = new();

    public WebViewBridge(WebView2 webView) {
        _webView = webView;
        this.AddOnLoad(async () => {
            await this.ExecuteJsAsync("""
                console.log("✅ Page loaded — message from C#");
            """);
        });
    }

    /// <summary>
    /// Execute raw JavaScript (C# → JS)
    /// </summary>
    public Task ExecuteJsAsync(string js) {
        return _webView.CoreWebView2.ExecuteScriptAsync(js);
    }

    public async Task InitializeAsync() {
        // 🚨 IMPORTANT: You must attach OnMessageReceived *before* any calls are made
        _webView.CoreWebView2.WebMessageReceived += OnMessageReceived;

        // 🔹 Page load hook
        _webView.CoreWebView2.NavigationCompleted += async (s, e) => {
            if (!e.IsSuccess) return;

            foreach (var handler in _onLoadHandlers) {
                try { await handler(); } catch (Exception ex) {
                    // Log any exceptions from onLoad handlers
                    Console.WriteLine($"OnLoad Handler Error: {ex.Message}");
                }
            }
        };

        // JS bridge for JS -> C# (Your existing, correct bridge)
        await _webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
            (function () {
                window.cs = {
                    _callbacks: {}, // For JS -> C#
                    _jsCallbacks: {}, // NEW: For C# -> JS callbacks
                    call(name, payload) {
                        return new Promise((resolve, reject) => {
                            const id = crypto.randomUUID();
                            this._callbacks[id] = { resolve, reject };
                            // Type 'call' for JS -> C#
                            window.chrome.webview.postMessage({ type: 'call', name: name, payload, callId: id }); 
                        });
                    },
                    // Used by C# to return result to a pending JS call
                    _resolve(id, result) {
                        if (!this._callbacks[id]) return;
                        this._callbacks[id].resolve(result);
                        delete this._callbacks[id];
                    },
                    _reject(id, error) {
                        if (!this._callbacks[id]) return;
                        this._callbacks[id].reject(error);
                        delete this._callbacks[id];
                    },
                    // NEW: Used by C# to return result to a pending C# call
                    _csharpResolve(id, result) {
                        // Type 'resolve' for C# -> JS
                        window.chrome.webview.postMessage({ type: 'resolve', callId: id, result: result }); 
                    },
                    _csharpReject(id, error) {
                        window.chrome.webview.postMessage({ type: 'reject', callId: id, error: error });
                    }
                };
            })();
        ");
    }

    // ============================
    // C# -> JS (FIXED)
    // ============================
    public async Task<T?> CallJsAsync<T>(string functionName, object? payload = null) {
        var callId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<string>();

        // 1. Store the TaskCompletionSource to await the result later
        if (!_jsCallbacks.TryAdd(callId, tcs)) {
            throw new Exception("Failed to register C# -> JS callback ID.");
        }

        var json = payload == null ? "undefined" : JsonSerializer.Serialize(payload);

        // 2. Build the script: await the JS call, then message the result back to C#
        var script = $@"
            (async function() {{
                const id = '{callId}';
                try {{
                    const fn = window['{functionName}'];
                    if (typeof fn !== 'function') {{
                        throw new Error('JS function {functionName} not found');
                    }}
                    
                    // Await the asynchronous JS function call
                    const ret = await fn({json});
                    
                    // Send the result back to C# via postMessage
                    window.cs._csharpResolve(id, ret); 
                }} catch (error) {{
                    // Send error back to C# via postMessage
                    const errorMessage = (error && error.message) ? error.message : 'Unknown JS Error';
                    window.cs._csharpReject(id, errorMessage);
                }}
            }})()
        ";

        // 3. Execute the script (it returns immediately, but the JS runs asynchronously)
        // We ignore the direct return value (which would be a Promise).
        await _webView.CoreWebView2.ExecuteScriptAsync(script);

        // 4. Await the TaskCompletionSource (this waits until OnMessageReceived 
        // receives the response from window.cs._csharpResolve)
        var resultJson = await tcs.Task;

        // 5. Cleanup the callback tracking
        _jsCallbacks.TryRemove(callId, out _);

        // 6. Deserialize the result (Simplified, removing the manual string logic 
        // as JsonSerializer handles strings enclosed in quotes fine).
        if (string.IsNullOrWhiteSpace(resultJson) || resultJson == "undefined")
            return default;

        try {
            return JsonSerializer.Deserialize<T>(resultJson);
        } catch (JsonException ex) {
            Console.WriteLine($"Deserialization Error for type {typeof(T).Name}: {ex.Message} JSON: {resultJson}");
            // Handle primitives that don't need JSON parsing (e.g., if JS returned a raw number or string that wasn't JSON-encoded)
            if (typeof(T) == typeof(string))
                return (T)(object)resultJson.Trim('"');

            return default;
        }
    }


    // ============================
    // JS -> C# and C# -> JS (Unified Message Handler)
    // ============================
    private async void OnMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e) {
        using var doc = JsonDocument.Parse(e.WebMessageAsJson);
        var root = doc.RootElement;

        // Check message type to determine if it's a JS->C# call or a C#->JS response
        var type = root.GetProperty("type").GetString();
        var callId = root.GetProperty("callId").GetString();

        if (callId == null) return;

        // --- 1. C# -> JS Response (Completes the C# -> JS call initiated by CallJsAsync) ---
        if (type == "resolve" || type == "reject") {
            if (_jsCallbacks.TryGetValue(callId, out var tcs)) {
                if (type == "resolve") {
                    var resultJson = root.TryGetProperty("result", out var r) ? r.GetRawText() : "null";
                    // Resolve the task with the JSON string result
                    tcs.SetResult(resultJson);
                } else // type == "reject"
                  {
                    var error = root.TryGetProperty("error", out var err) ? err.GetString() : "Unknown Error from JS";
                    tcs.SetException(new Exception($"JS Call Failed: {error}"));
                }
            }
            return;
        }

        // --- 2. JS -> C# Call (Your existing logic) ---

        var name = root.GetProperty("name").GetString(); // Changed from 'type' to 'name' for clarity

        // Safely extract payload for JS -> C# calls
        var payload = root.TryGetProperty("payload", out var p)
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(p.GetRawText()) ?? new()
            : new();

        if (name == null) return;

        if (!_handlers.TryGetValue(name, out var handler)) {
            await _webView.CoreWebView2.ExecuteScriptAsync(
                $"window.cs._reject('{callId}', 'C# handler {name} not found');");
            return;
        }

        try {
            var result = await handler(payload);
            var json = JsonSerializer.Serialize(result);
            await _webView.CoreWebView2.ExecuteScriptAsync(
                // Use window.cs._resolve to send result back to JS Promise
                $"window.cs._resolve('{callId}', {json});");
        } catch (Exception ex) {
            var msg = JsonSerializer.Serialize(ex.Message);
            await _webView.CoreWebView2.ExecuteScriptAsync(
                // Use window.cs._reject to send error back to JS Promise
                $"window.cs._reject('{callId}', {msg});");
        }
    }

    // ============================
    // Page load
    // ============================
    public void AddOnLoad(Func<Task> handler) {
        _onLoadHandlers.Add(handler);
    }
}