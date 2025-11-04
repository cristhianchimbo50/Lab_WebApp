namespace Lab_APIRest.Infrastructure.Services
{
    /// <summary>
    /// Clase que genera una clave única para la sesión del servidor.
    /// Cada reinicio de la API genera una nueva clave, invalidando los tokens previos.
    /// </summary>
    public class ServerSessionKey
    {
        public string CurrentKey { get; } = Guid.NewGuid().ToString();
    }
}
