namespace ROIO {
    public class LocalConfiguration {
        // Optional HTTPS/HTTP endpoint that returns RemoteConfiguration JSON.
        // Leave empty during local/alpha builds to use the bundled fallback below.
        public string remoteConfigLocation;

        public string fallbackLoginServer = "127.0.0.1";
        public string fallbackLoginPort = "6900";
        public bool fallbackUseSameIpForEveryServer = true;
    }
}
