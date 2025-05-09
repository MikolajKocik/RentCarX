namespace RentCarX.Application.Interfaces.PasswordHasher
{
    public interface IPasswordHasher
    {
        void CreatePasswordHash(string password, out byte[] hash, out byte[] salt);
        bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt);
    }
}
