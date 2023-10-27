namespace Utility.cs
{
    public static class StringExtensions
    {
        public static string? GetThisEnvironmentVariable(this string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
