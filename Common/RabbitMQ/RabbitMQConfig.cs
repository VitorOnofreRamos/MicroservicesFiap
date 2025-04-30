namespace Common.RabbitMQ;

public static class RabbitMQConfig
{
    // RabbitMQ Connection
    public const string HostName = "localhost";
    public const string UserName = "guest";
    public const string Password = "guest";

    // Exchange Names
    public const string FrutasExchange = "frutas_exchange";
    public const string UsuariosExchange = "usuarios_exchange";
    public const string ValidationExchange = "validation_exchange";

    // Queue Names
    public const string FrutasToValidationQueue = "frutas_to_validation_queue";
    public const string UsuariosToValidationQueue = "usuairos_to_validation_queue";
    public const string ValidatedFrutasQueue = "validated_frutas_queue";
    public const string ValidatedUsuariosQueue = "validated_usuarios_queue";

    // Routing Keys
    public const string FrutasToValidationKey = "frutas.validation";
    public const string UsuariosToValidationKey = "usuairos.validation";
    public const string ValidatedFrutasKey = "frutas.validated";
    public const string ValidatedUsuariosKey = "usuarios.validated";
}
