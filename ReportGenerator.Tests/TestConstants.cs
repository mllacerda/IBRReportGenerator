using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ReportGenerator.Tests;

public static class TestConstants
{
    //Webhook
    public const string UrlTest = "https://test.webhook.com";

    //Reports
    public const string ReportIdTest = "123";
    public const string SuccessMessageTest = "Operation successful";
    public const string Base64ImageTest = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8cX3QAAAABJRU5ErkJggg==";

    public static readonly object ObjectTest = new { Name = "Person", Age = 30 };
    public static readonly byte[] PdfBytesTest = new byte[] { 1, 2, 3 };
    public static readonly Dictionary<string, object> DictTest = new() { { "key1", "value1" } };
    public static readonly Dictionary<string, object> DictionaryTwoValuesTest = new() { { "key1", "value1" }, { "key2", 42 } };

    
    //RabbitMQ
    public const string RabbitMQHostTest = "localhost";
    public const string RabbitMQUserTest = "guest";
    public const string RabbitMQPasswordTest = "guest";
    public const string RabbitMQQueueTest = "report_requests";
}