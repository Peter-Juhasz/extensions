namespace System;

public class ConflictException : Exception
{
	public ConflictException(string value)
		: base($"Conflicting value '{value}'")
	{

	}
}
