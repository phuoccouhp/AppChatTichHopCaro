using System;
using System.Security.Cryptography;
using System.Text;

namespace ChatAppServer
{
    public static class PasswordHelper
    {
        // Số lần lặp cho PBKDF2 (càng cao càng an toàn nhưng chậm hơn)
        private const int Iterations = 10000;
        private const int SaltSize = 16; // 16 bytes = 128 bits
        private const int HashSize = 32; // 32 bytes = 256 bits

        /// <summary>
        /// Hash password với salt sử dụng PBKDF2
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            // Tạo salt ngẫu nhiên
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash password với salt
            byte[] hash = HashPasswordWithSalt(password, salt, Iterations);

            // Kết hợp salt và hash thành một chuỗi (format: salt:hash)
            // Convert sang Base64 để lưu vào database
            string saltBase64 = Convert.ToBase64String(salt);
            string hashBase64 = Convert.ToBase64String(hash);

            return $"{saltBase64}:{hashBase64}";
        }

        /// <summary>
        /// Verify password có khớp với hash đã lưu không
        /// </summary>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            try
            {
                // Tách salt và hash từ chuỗi đã lưu
                string[] parts = hashedPassword.Split(':');
                if (parts.Length != 2)
                    return false; // Format không đúng

                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] storedHash = Convert.FromBase64String(parts[1]);

                // Hash password nhập vào với salt đã lưu
                byte[] computedHash = HashPasswordWithSalt(password, salt, Iterations);

                // So sánh hash (constant-time comparison để tránh timing attack)
                return ConstantTimeEquals(computedHash, storedHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Hash password với salt sử dụng PBKDF2
        /// </summary>
        private static byte[] HashPasswordWithSalt(string password, byte[] salt, int iterations)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(HashSize);
            }
        }

        /// <summary>
        /// So sánh hai mảng byte với thời gian cố định (constant-time) để tránh timing attack
        /// </summary>
        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }
    }
}

