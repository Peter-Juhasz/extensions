# System.Extensions
Adds missing overloads and more convenient ways to improve usability and readability of existing .NET APIs.

A few examples:
 - `Process.WaitForExit(int milliseconds)` requests a timeout as an `int`, in number of milliseconds, while `TimeSpan` is the preferred way to express time intervals.
 - Some old but still frequently used methods were implemented as static before the concept of extension methods have ever existed. While they would be much more convenient to use as extension methods, they have never been upgraded for compatibility reasons. Like it `if (String.IsNullOrWhiteSpace(str))` is easier to use as `if (str.IsNullOrWhiteSpace())`.
 - Some APIs had limited customizations, because they missed overloads. Like while we could specify `StringComparison` for `str.IndexOf(string, StringComparison)`, we couldn't for `str.Contains(string)`.
 - .NET has more and more focus on performance, but some APIs are missing to be conscious about performance. Like when `ToList()` or `ToArray()` don't know about count of elements, they have to resize their target storage over and over while they finally arrive at the final size, which is expensive. So, we provide an overload that accepts a hint `ToList(int)` for storage size.
 - APIs that are not implemented yet, but they clearly fit into the future of the framework, like reading the lines from a `TextReader` as an `IAsyncEnumerable<string>`.

## Full list of APIs

### Primitive types
 - `ReadOnlySpan<byte>`
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `string ToBase64String()`: converts a span of bytes to a Base 64 encoded string.
 - `byte[]`
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `MemoryStream ToMemoryStream()`: constructs a MemoryStream from an array of bytes.
 - `DateTime` and `DateTimeOffset`
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `DateTimeOffset Floor(TimeSpan unit)`: rounds down a date and time to a specific time interval.
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `DateTimeOffset Round(TimeSpan unit)`: rounds up or down a date and time to the nearest specific time interval.															   
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `DateTimeOffset Ceiling(TimeSpan unit)`: rounds up a date and time to a specific time interval.
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `DateTimeOffset StartOfWeek(DayOfWeek firstDayOfWeek)`: calculates the start of the week.
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `string ToRfc3339Format()`: formats a `DateTimeOffset` as RFC-3339 format.
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `DateTimeOffset ToTimeZone(TimeZoneInfo destinationTimeZone)`: converts a date and time to another time zone.
 - `Guid`
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `Guid? TreatEmptyAsNull()`: returns `null` when a `Guid` is `Guid.Empty`.
 - `String`
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `bool IsEmpty()`: determines whether the length of a `string` is `0`.
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `bool IsNullOrEmpty()`: shortcut extension method for `String.IsNullOrEmpty`.
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `bool IsNullOrWhiteSpace()`: shortcut extension method for `String.IsNullOrWhiteSpace`.
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `bool IsWhiteSpace()`: determines whether a `string` contains only whitespace characters.
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `string? TreatEmptyAsNull()`: returns `null` when a `string` is empty.
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `string? TreatWhiteSpaceAsNull()`: returns `null` when a `string` contains only whitespace.
   - [performance](https://github.com/Peter-Juhasz/extensions/labels/performance) `StringSegment Subsegment(int start, int length)`: constructs a `StringSegment` from a part of a `string`.
   - [performance](https://github.com/Peter-Juhasz/extensions/labels/performance) `StringSegment Subsegment(Range range)`: constructs a `StringSegment` from a part of a `string`.
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `string Ellipsis(int maxLength, string ellipsis)`: trims a `string` to a specific length and appends `ellipsis` when it overflows.
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `string TrimPrefix(string prefix)`: trims a specific `string` from the beginning of a `string`.
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `string TrimSuffix(string suffix)`: trims a specific `string` from the end of a `string`.
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `Uri ToUri(UriKind kind)`: constructs a `Uri` from a `string`.
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `string Remove(string subject, StringComparison comparison)`: removes all occurrences of a substring from a string.																 
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `string Remove(string subject)`: removes all occurrences of a substring from a string.
 - `StringSegment`
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `IEnumerable<char> AsEnumerable()`: enumerates the characters of the `StringSegment`.
   - [missing overload](https://github.com/Peter-Juhasz/extensions/labels/missing%20overload) `StringSegment TrimStart(char ch)`: overload with specific `char` to trim.
   - [missing overload](https://github.com/Peter-Juhasz/extensions/labels/missing%20overload) `StringSegment TrimStart(params char[] characters)`: overload with specific `char`s to trim.
   - [missing overload](https://github.com/Peter-Juhasz/extensions/labels/missing%20overload) `StringSegment TrimEnd(char ch)`: overload with specific `char` to trim.
   - [missing overload](https://github.com/Peter-Juhasz/extensions/labels/missing%20overload) `StringSegment TrimEnd(params char[] characters)`: overload with specific `char`s to trim.
   - [missing overload](https://github.com/Peter-Juhasz/extensions/labels/missing%20overload) `StringSegment Contains(char ch)`: overload with `char`.
   - [missing overload](https://github.com/Peter-Juhasz/extensions/labels/missing%20overload) `StringSegment StartsWith(char ch)`: overload with `char`.
   - [missing overload](https://github.com/Peter-Juhasz/extensions/labels/missing%20overload) `StringSegment EndsWith(char ch)`: overload with `char`.
   - [missing overload](https://github.com/Peter-Juhasz/extensions/labels/missing%20overload) `int LastIndexOfAny(int startIndex, params char[] characters)`: overload with `char[]`.
   - [missing overload](https://github.com/Peter-Juhasz/extensions/labels/missing%20overload) `int LastIndexOfAny(params char[] characters)`: overload with `char[]`.
   - [missing overload](https://github.com/Peter-Juhasz/extensions/labels/missing%20overload) `StringTokenizer Split(char ch)`: overload with `char`.
 - `Uri`
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `bool IsSchemeRelative()`: determines whether the scheme of the `Uri` is relative.
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `Uri Append(Uri relative)`: appends an `Uri` to another.
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `Uri AppendQueryString(string name, string? value)`: appends a name-value pair as query string to a `Uri`.
 - `char`
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `bool IsDigit()`
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `bool IsLetter()`
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `bool IsWhiteSpace()`
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `char ToLower()`
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `char ToUpper()`

### Collection types
 - `IEnumerable<T>`
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `bool IsEmpty<T>()`: determines whether the enumerable is empty.
   - [performance](https://github.com/Peter-Juhasz/extensions/labels/performance) `IList<T> ToList<T>(int initialCapacity)`: provides a hint for the size of the list to be constructed, so the list doesn't have to be resized multiple times during the copy.
   - [performance](https://github.com/Peter-Juhasz/extensions/labels/performance) `T[] ToArray<T>(int sizeHint)`: provides a hint for the size of the array to be constructed, so the list doesn't have to be resized multiple times during the copy.
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `bool TryFirst(out T element)`: determines whether the enumerable consists of at least one element, and if yes, gets the first element.										  
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `bool TrySingle(out T element)`: determines whether the enumerable consists of a single element, and if yes, gets the element.									  
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `bool TryLast(out T element)`: determines whether the enumerable consists of at least one element, and if yes, gets the last element.
 - `ICollection<T>`
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `void AddRange<T>(IEnumerable<T> elements)`: adds a range of elements to a collection.
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `void RemoveRange<T>(IEnumerable<T> elements)`: removes a range of elements from a collection.
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `IReadOnlyCollection<T> AsReadOnly<T>()`
 - `IList<T>`
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `IReadOnlyList<T> AsReadOnly<T>()`
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `bool CanMoveUp<T>(T element)`: determines whether an element can be moved to the next index in a list.
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `bool CanMoveDown<T>(T element)`: determines whether an element can be moved to the previous index in a list.																    
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `void MoveUp<T>(T element)`: moves an element to the next index in a list.
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `void MoveDown<T>(T element)`: moves an element to the previous index in a list.
 - `IReadOnlyList<T>`
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `int IndexOf<T>(Predicate<T> predicate, int startIndex)`
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `int IndexOf<T>(Predicate<T> predicate)`
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `int LastIndexOf<T>(Predicate<T> predicate, int startIndex)`
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `int LastIndexOf<T>(Predicate<T> predicate)`
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `int IndexOf<T>(T element, int startIndex, IEqualityComparer<T> comparer)`
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `int IndexOf<T>(T element)`: finds the position of an element in a list, starting from the beginning, using the default comparer.											   
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `int LastIndexOf<T>(T element, int startIndex, IEqualityComparer<T> comparer)`
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `int LastIndexOf<T>(T element)`: finds the last position of an element in a list, starting from the beginning, using the default comparer.
 - `IDictionary<TKey, TValue>`
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>()`: converts a dictionary to read-only.
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `SortedDictionary<TKey, TValue> AsSorted<TKey, TValue>()`: converts a dictionary to a `SortedDictionary`.
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `void AddRange<TKey, TValue>(IEnumerable<KeyVaulePair<TKey, TValue>> elements)`: adds a range of elements to a dictionary.
 - `IEnumerator<T>`
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `bool TryMoveNext<T>(out T current)`: wraps `MoveNext()` and `Current` to a Try-pattern.

### Streams
 - `Stream`
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `Stream Rewind()`: sets its position to `0`.
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `Task<MemoryStream> BufferAsync(int bufferSize, CancellationToken cancellationToken)`: copies the contents of a stream into a `MemoryStream`.									   
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `Task<byte[]> ToByteArrayAsync(CancellationToken cancellationToken)`: copies the contents of a stream into a byte array.
 - `TextReader`
   - [new api](https://github.com/Peter-Juhasz/extensions/labels/new%20api) `IAsyncEnumerable<string> EnumerateLinesAsync()`: reads a `TextReader` line by line, asynchronously.

### Miscellaneous
 - `Process`
   - [missing overload](https://github.com/Peter-Juhasz/extensions/labels/missing%20overload) `bool WaitForExit(TimeSpan timeout)`: provides an overload with `TimeSpan` instead of `int milliseconds`.
 - `IPAddress`
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `bool IsIPv4()`: determines whether an IP address is version 4.
   - [helper](https://github.com/Peter-Juhasz/extensions/labels/helper) `bool IsIPv6()`: determines whether an IP address is version 6.
 - `RandomNumberGenerator`
   - [missing overload](https://github.com/Peter-Juhasz/extensions/labels/missing%20overload) `int GetInt32()`: gets a random integer.
