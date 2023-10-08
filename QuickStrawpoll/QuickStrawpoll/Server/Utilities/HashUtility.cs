namespace QuickStrawpoll.Server.Utilities
{
    public static class HashUtility
    {
        private static Random random = new Random();

        public static string GenerateUniqueHash(int length)
        {
            // ensure that before saving to the database,
            // check if the hash already exists and if by the rare chance it does, regenerate it

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
