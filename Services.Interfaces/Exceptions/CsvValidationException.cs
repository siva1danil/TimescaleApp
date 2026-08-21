namespace Services.Interfaces.Exceptions;

public sealed class CsvValidationException(string message) : Exception(message);
