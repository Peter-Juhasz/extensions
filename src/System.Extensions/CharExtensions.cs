namespace System
{
    public static class CharExtensions
    {
        public static bool IsDigit(this char c) => Char.IsDigit(c);

        public static bool IsLetter(this char c) => Char.IsLetter(c);

        public static bool IsWhiteSpace(this char c) => Char.IsWhiteSpace(c);

        public static char ToLower(this char c) => Char.ToLower(c);

        public static char ToUpper(this char c) => Char.ToUpper(c);
    }
}
