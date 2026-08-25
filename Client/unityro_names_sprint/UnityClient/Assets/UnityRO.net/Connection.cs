using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public class Connection {

    public const int DATA_BUFFER_SIZE = 16 * 1024;
    public const int CONNECT_TIMEOUT_MS = 10_000;

    public static Action OnDisconnect;

    private TcpClient TcpClient;
    private NetworkStream Stream;
    private BinaryWriter BinaryWriter;
    private readonly PacketSerializer PacketSerializer;
    private readonly byte[] ReceiveBuffer;
    private CancellationTokenSource ReceiveCancellation;
    private Task ReceiveTask;

    public bool IsConnected() => TcpClient?.Connected == true && Stream != null;
    public BinaryWriter GetBinaryWriter() => BinaryWriter;
    public NetworkStream GetStream() => Stream;

    public Connection(IPacketHandler packetHandler) {
        PacketSerializer = new PacketSerializer(packetHandler);
        ReceiveBuffer = new byte[DATA_BUFFER_SIZE];
        TcpClient = CreateTcpClient();
    }

    public async Task Connect(string target, int port) {
        Disconnect();

        TcpClient = CreateTcpClient();
        var connectTask = TcpClient.ConnectAsync(target, port);
        var completedTask = await Task.WhenAny(connectTask, Task.Delay(CONNECT_TIMEOUT_MS));
        if (completedTask != connectTask) {
            Disconnect();
            throw new TimeoutException($"Timed out connecting to {target}:{port}");
        }

        // Observe any connection exception after the timeout race.
        await connectTask;

        Stream = TcpClient.GetStream();
        BinaryWriter = new BinaryWriter(Stream);
        ReceiveCancellation = new CancellationTokenSource();
        ReceiveTask = ReceiveLoopAsync(ReceiveCancellation.Token);
    }

    public void SkipBytes(int bytesToSkip) {
        PacketSerializer.BytesToSkip = bytesToSkip;
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken) {
        try {
            while (!cancellationToken.IsCancellationRequested && Stream != null) {
                int size = await Stream.ReadAsync(
                    ReceiveBuffer,
                    0,
                    ReceiveBuffer.Length,
                    cancellationToken
                );

                // ReadAsync returning zero means the remote peer closed gracefully.
                if (size == 0) {
                    HandleUnexpectedDisconnect();
                    return;
                }

                PacketSerializer.EnqueueBytes(ReceiveBuffer, size);
            }
        } catch (OperationCanceledException) {
            // Expected during an intentional Disconnect().
        } catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested) {
            // Expected when the stream is disposed while cancelling.
        } catch (Exception) {
            if (!cancellationToken.IsCancellationRequested) {
                HandleUnexpectedDisconnect();
            }
        }
    }

    private void HandleUnexpectedDisconnect() {
        Disconnect();
        OnDisconnect?.Invoke();
    }

    public void Disconnect() {
        ReceiveCancellation?.Cancel();
        ReceiveCancellation?.Dispose();
        ReceiveCancellation = null;
        ReceiveTask = null;

        try {
            BinaryWriter?.Dispose();
        } catch { }

        try {
            Stream?.Dispose();
        } catch { }

        try {
            TcpClient?.Close();
        } catch { }

        Stream = null;
        BinaryWriter = null;
        PacketSerializer.Reset();
    }

    private static TcpClient CreateTcpClient() {
        return new TcpClient {
            NoDelay = true
        };
    }
}
