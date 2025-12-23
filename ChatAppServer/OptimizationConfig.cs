using System;

namespace ChatAppServer
{
    /// <summary>
    /// C?u hình t?i ?u hóa cho nh?n/g?i gói tin
    /// </summary>
    public static class OptimizationConfig
    {
        /// <summary>
        /// Kích th??c buffer cho NetworkStream (m?c ??nh 64KB, t?ng lên ?? gi?m system calls)
        /// </summary>
        public const int NETWORK_BUFFER_SIZE = 131072; // 128 KB

        /// <summary>
        /// Timeout cho socket options (ms)
        /// </summary>
        public const int SOCKET_RECEIVE_TIMEOUT = 60000;
        public const int SOCKET_SEND_TIMEOUT = 60000;

        /// <summary>
        /// TCP Keep Alive configuration
        /// </summary>
        public const int TCP_KEEP_ALIVE_TIME = 30000;      // 30 segundos
        public const int TCP_KEEP_ALIVE_INTERVAL = 1000;   // 1 segundo

        /// <summary>
        /// T?i ?u TCP_NODELAY ?? gi?m latency cho gói tin nh? (chat messages)
        /// </summary>
        public const bool ENABLE_TCP_NODELAY = true;

        /// <summary>
        /// T?i ?u MaxPoolSize cho connection pooling
        /// </summary>
        public const int MAX_CONNECTION_POOL_SIZE = 200;

        /// <summary>
        /// Kích th??c t?i thi?u cho payload (bytes)
        /// </summary>
        public const int MIN_PAYLOAD_SIZE = 0;

        /// <summary>
        /// Kích th??c t?i ?a cho payload (bytes) - 50MB
        /// </summary>
        public const int MAX_PAYLOAD_SIZE = 52428800;
    }
}
