namespace PetGuardian.Domain.Helpers;

/// <summary>
/// Utilitário criptográfico para geração de hash de senhas com Salt utilizando BCrypt.
/// </summary>
public static class HashHelper
{
    public static string Hash(string newPassword, string salt)
    {
        return BCrypt.Net.BCrypt.HashPassword(newPassword + salt);
    }
    
    public static bool Verify(string rawPassword, string salt, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword) || string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        return BCrypt.Net.BCrypt.Verify(rawPassword + salt, hashedPassword);
    }
}
