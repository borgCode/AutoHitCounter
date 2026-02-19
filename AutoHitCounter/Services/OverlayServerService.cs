// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AutoHitCounter.Models;
using Fleck;

namespace AutoHitCounter.Services;

public class OverlayServerService : IDisposable
{
    private WebSocketServer _server;
    private readonly List<IWebSocketConnection> _clients = new();
    private readonly object _lock = new();

    private string _lastStateJson;

    private const int Port = 16200;

    public void Start()
    {
        try
        {
            _server = new WebSocketServer($"ws://0.0.0.0:{Port}");
            _server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    lock (_lock) _clients.Add(socket);

                    if (_lastStateJson != null)
                        socket.Send(_lastStateJson);
                };
                socket.OnClose = () =>
                {
                    lock (_lock) _clients.Remove(socket);
                };
                socket.OnError = ex =>
                {
                    lock (_lock) _clients.Remove(socket);
                };
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Overlay server failed to start: {ex.Message}");
        }
    }

    public void BroadcastState(OverlayState state)
    {
        var json = JsonSerializer.Serialize(
            new { type = "state", data = state },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        _lastStateJson = json;
        Broadcast(json);
    }

    public void BroadcastIgt(string formatted)
    {
        var json = JsonSerializer.Serialize(new { type = "igt", data = formatted });
        Broadcast(json);
    }

    private void Broadcast(string json)
    {
        List<IWebSocketConnection> snapshot;
        lock (_lock) snapshot = _clients.ToList();

        foreach (var client in snapshot)
        {
            try
            {
                client.Send(json);
            }
            catch
            {
                lock (_lock) _clients.Remove(client);
            }
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            foreach (var client in _clients.ToList())
            {
                try
                {
                    client.Close();
                }
                catch
                {
                    // ignored
                }
            }

            _clients.Clear();
        }

        _server?.Dispose();
        _server = null;
    }

    public void Dispose()
    {
        Stop();
    }
}