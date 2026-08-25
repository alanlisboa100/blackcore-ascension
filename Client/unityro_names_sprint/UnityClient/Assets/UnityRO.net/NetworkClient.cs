using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using static PacketSerializer;

public enum NetworkServerRole {
    Unknown,
    Login,
    Character,
    Map
}

public enum NetworkConnectionState {
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    Suspended
}

public class NetworkClient : MonoBehaviour, IPacketHandler {

    public static UnityAction<NetworkPacket, bool> OnPacketEvent;
    public static UnityAction OnDisconnected;
    public static UnityAction OnReconnected;
    public static UnityAction<NetworkConnectionState> OnConnectionStateChanged;
    public static UnityAction<int, int> OnReconnectAttempt;

    #region Singleton
    private static NetworkClient _instance;
    private static NetworkClient Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<NetworkClient>();
            }

            return _instance;
        }
    }
    #endregion

    #region Members
    public bool IsConnected => CurrentConnection?.IsConnected() ?? false;
    public bool IsReconnecting => ConnectionState == NetworkConnectionState.Reconnecting;
    public static int CLIENT_ID = new System.Random().Next();

    private readonly Dictionary<PacketHeader, List<OnPacketReceived>> PacketHooks = new Dictionary<PacketHeader, List<OnPacketReceived>>();

    private bool IsPaused = false;
    private bool IsApplicationPaused;
    private bool ReconnectInProgress;
    private TaskCompletionSource<bool> ReconnectHandshakeCompletion;
    private Coroutine HeartBeatCoroutine;

    private string CurrentHost;
    private int CurrentPort;
    private NetworkServerRole CurrentServerRole = NetworkServerRole.Unknown;

    private const int MAX_AUTO_RECONNECT_ATTEMPTS = 4;
    private const int RECONNECT_HANDSHAKE_TIMEOUT_MS = 5000;
    private static readonly int[] RECONNECT_DELAYS_MS = { 500, 1000, 2000, 4000 };

    public NetworkClientState State;
    public Connection CurrentConnection;
    public NetworkConnectionState ConnectionState { get; private set; } = NetworkConnectionState.Disconnected;

    private Queue<OutPacket> OutPacketQueue;
    private Queue<InPacket> InPacketQueue;
    #endregion

    #region Lifecycle
    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Start() {
        CurrentConnection = new Connection(this);
        State = new NetworkClientState();

        OutPacketQueue = new Queue<OutPacket>();
        InPacketQueue = new Queue<InPacket>();
        Connection.OnDisconnect += HandleConnectionDisconnected;
    }

    private void Update() {
        if (IsPaused) {
            return;
        }
        TrySendPacket();
        TryHandleReceivedPacket();
    }

    private void OnApplicationPause(bool paused) {
        IsApplicationPaused = paused;

        if (paused) {
            StopHeartBeat();
            SetConnectionState(NetworkConnectionState.Suspended);
            return;
        }

        _ = RestoreConnectionAfterResumeAsync();
    }

    private async Task RestoreConnectionAfterResumeAsync() {
        if (IsConnected) {
            SetConnectionState(NetworkConnectionState.Connected);
            StartHeartBeat();
            new CZ.REQUEST_TIME2().Send();
            return;
        }

        await TryReconnectCurrentServerAsync();
    }

    private void OnApplicationQuit() {
        Disconnect();
    }

    private void OnDestroy() {
        Connection.OnDisconnect -= HandleConnectionDisconnected;
        StopHeartBeat();
        if (_instance == this) {
            _instance = null;
        }
    }
    #endregion

    public async Task ChangeServer(string ip, int port, NetworkServerRole role = NetworkServerRole.Unknown) {
        StopHeartBeat();
        ReconnectHandshakeCompletion?.TrySetResult(false);
        ReconnectHandshakeCompletion = null;
        ReconnectInProgress = false;
        CurrentHost = ip;
        CurrentPort = port;
        CurrentServerRole = role;
        SetConnectionState(NetworkConnectionState.Connecting);

        try {
            await CurrentConnection.Connect(ip, port);
            ClearPacketQueues();
            SetConnectionState(NetworkConnectionState.Connected);
        } catch {
            SetConnectionState(NetworkConnectionState.Disconnected);
            throw;
        }
    }

    public void StartHeatBeat() {
        if (HeartBeatCoroutine == null && !IsApplicationPaused) {
            HeartBeatCoroutine = StartCoroutine(ServerHeartBeat());
        }
    }

    // Correctly-spelled alias for new call sites while preserving compatibility.
    public void StartHeartBeat() {
        StartHeatBeat();
    }

    public void StopHeartBeat() {
        if (HeartBeatCoroutine == null) {
            return;
        }

        StopCoroutine(HeartBeatCoroutine);
        HeartBeatCoroutine = null;
    }

    public void Disconnect() {
        ReconnectHandshakeCompletion?.TrySetResult(false);
        ReconnectHandshakeCompletion = null;
        ReconnectInProgress = false;
        StopHeartBeat();
        CurrentConnection?.Disconnect();
        SetConnectionState(NetworkConnectionState.Disconnected);
    }

    private void HandleConnectionDisconnected() {
        // Socket callbacks run off the Unity main thread. Keep UI/gameplay listeners safe.
        ThreadManager.ExecuteOnMainThread(() => {
            StopHeartBeat();
            SetConnectionState(NetworkConnectionState.Disconnected);
            OnDisconnected?.Invoke();

            if (!IsApplicationPaused) {
                _ = TryReconnectCurrentServerAsync();
            }
        });
    }

    public async Task<bool> TryReconnectCurrentServerAsync() {
        if (ReconnectInProgress || IsConnected || IsApplicationPaused) {
            return IsConnected;
        }

        if (string.IsNullOrWhiteSpace(CurrentHost) || CurrentPort <= 0 || !CanAutomaticallyReconnect(CurrentServerRole)) {
            return false;
        }

        ReconnectInProgress = true;
        SetConnectionState(NetworkConnectionState.Reconnecting);

        try {
            for (int attempt = 1; attempt <= MAX_AUTO_RECONNECT_ATTEMPTS; attempt++) {
                if (IsApplicationPaused) {
                    break;
                }

                OnReconnectAttempt?.Invoke(attempt, MAX_AUTO_RECONNECT_ATTEMPTS);

                if (attempt > 1) {
                    await Task.Delay(RECONNECT_DELAYS_MS[Math.Min(attempt - 2, RECONNECT_DELAYS_MS.Length - 1)]);
                }

                try {
                    await CurrentConnection.Connect(CurrentHost, CurrentPort);
                    ClearPacketQueues();
                    ReconnectHandshakeCompletion = new TaskCompletionSource<bool>();
                    SendReconnectHandshake();

                    var completed = await Task.WhenAny(
                        ReconnectHandshakeCompletion.Task,
                        Task.Delay(RECONNECT_HANDSHAKE_TIMEOUT_MS)
                    );

                    if (completed == ReconnectHandshakeCompletion.Task && ReconnectHandshakeCompletion.Task.Result) {
                        return true;
                    }

                    Debug.LogWarning($"Reconnect attempt {attempt}/{MAX_AUTO_RECONNECT_ATTEMPTS} timed out waiting for server handshake");
                    CurrentConnection.Disconnect();
                } catch (Exception ex) {
                    Debug.LogWarning($"Reconnect attempt {attempt}/{MAX_AUTO_RECONNECT_ATTEMPTS} failed: {ex.Message}");
                } finally {
                    ReconnectHandshakeCompletion = null;
                }
            }
        } finally {
            ReconnectInProgress = false;
        }

        SetConnectionState(IsApplicationPaused ? NetworkConnectionState.Suspended : NetworkConnectionState.Disconnected);
        return false;
    }

    private static bool CanAutomaticallyReconnect(NetworkServerRole role) {
        // Login reconnect would require retaining the user's password, which we deliberately do not do.
        return role == NetworkServerRole.Character || role == NetworkServerRole.Map;
    }

    private void SendReconnectHandshake() {
        switch (CurrentServerRole) {
            case NetworkServerRole.Character:
                if (State.LoginInfo == null) {
                    return;
                }
                CurrentConnection.SkipBytes(4);
                new CH.ENTER(State.LoginInfo.AccountID, State.LoginInfo.LoginID1, State.LoginInfo.LoginID2, State.LoginInfo.Sex).Send();
                break;

            case NetworkServerRole.Map:
                if (State.LoginInfo == null || State.SelectedCharacter == null) {
                    return;
                }
                new CZ.ENTER2(
                    State.LoginInfo.AccountID,
                    State.SelectedCharacter.GID,
                    State.LoginInfo.LoginID1,
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                    State.LoginInfo.Sex
                ).Send();
                break;
        }
    }

    private void MarkReconnectAccepted() {
        if (ConnectionState != NetworkConnectionState.Reconnecting) {
            return;
        }

        ReconnectHandshakeCompletion?.TrySetResult(true);
        ReconnectInProgress = false;
        SetConnectionState(NetworkConnectionState.Connected);
        StartHeartBeat();
        OnReconnected?.Invoke();
    }

    public void HookPacket(PacketHeader cmd, OnPacketReceived onPackedReceived) {
        if (onPackedReceived == null) {
            return;
        }

        if (!PacketHooks.TryGetValue(cmd, out var hooks)) {
            hooks = new List<OnPacketReceived>();
            PacketHooks[cmd] = hooks;
        }

        if (!hooks.Contains(onPackedReceived)) {
            hooks.Add(onPackedReceived);
        }
    }

    public bool UnhookPacket(PacketHeader cmd, OnPacketReceived onPacketReceived) {
        if (!PacketHooks.TryGetValue(cmd, out var hooks)) {
            return false;
        }

        bool removed = hooks.Remove(onPacketReceived);
        if (hooks.Count == 0) {
            PacketHooks.Remove(cmd);
        }

        return removed;
    }

    public void SkipBytes(int bytesToSkip) {
        CurrentConnection?.SkipBytes(bytesToSkip);
    }

    #region Packet Handling
    public void PausePacketHandling() {
        IsPaused = true;
    }

    public void ResumePacketHandling() {
        IsPaused = false;
    }

    public void OnPacketReceived(InPacket packet) {
        InPacketQueue.Enqueue(packet);
    }

    public static void SendPacket(OutPacket packet) {
        Instance?.OutPacketQueue.Enqueue(packet);
    }

    private void TrySendPacket() {
        if (OutPacketQueue.Count == 0 || !IsConnected) {
            return;
        }

        var stream = CurrentConnection.GetStream();
        if (stream == null || !stream.CanWrite) {
            return;
        }

        var packet = OutPacketQueue.Dequeue();
        OnPacketEvent?.Invoke(packet, false);
        packet.Send(stream);
    }

    private void TryHandleReceivedPacket() {
        if (InPacketQueue.Count == 0) {
            return;
        }

        var packet = InPacketQueue.Dequeue();
        bool isHandled = false;

        if (PacketHooks.TryGetValue(packet.Header, out var hooks) && hooks.Count > 0) {
            // Copy so handlers may safely unhook themselves during callbacks.
            var snapshot = hooks.ToArray();
            foreach (var hook in snapshot) {
                if (hook == null) {
                    continue;
                }
                isHandled = true;
                hook.Invoke((ushort) packet.Header, -1, packet);
            }
        }

        if (ConnectionState == NetworkConnectionState.Reconnecting) {
            if ((CurrentServerRole == NetworkServerRole.Map && packet.Header == ZC.ACCEPT_ENTER2.HEADER)
                || (CurrentServerRole == NetworkServerRole.Character && packet.Header == HC.ACCEPT_ENTER.HEADER)) {
                MarkReconnectAccepted();
            }
        }

        OnPacketEvent?.Invoke(packet, isHandled);
    }
    #endregion

    private void ClearPacketQueues() {
        OutPacketQueue?.Clear();
        InPacketQueue?.Clear();
    }

    private void SetConnectionState(NetworkConnectionState state) {
        if (ConnectionState == state) {
            return;
        }

        ConnectionState = state;
        OnConnectionStateChanged?.Invoke(state);
    }

    private IEnumerator ServerHeartBeat() {
        for (; ; ) {
            if (IsConnected) {
                new CZ.REQUEST_TIME2().Send();
            }
            yield return new WaitForSecondsRealtime(10f);
        }
    }

    public struct NetworkClientState {
        public MapLoginInfo MapLoginInfo;
        public CharServerInfo CharServer;
        public CharacterData SelectedCharacter;
        public AC.ACCEPT_LOGIN3 LoginInfo;
        public HC.ACCEPT_ENTER CurrentCharactersInfo;
    }
}
