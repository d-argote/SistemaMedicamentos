namespace SistemaMedicamentos.Services;

// Patrón Singleton con thread safety (Lazy<T> + lock con doble comprobación).
// Servicio de configuración que lee appsettings.json. Se inyecta en controladores.
public sealed class AppConfigService
{
    private static AppConfigService? _instance;
    private static readonly object _lock = new();
    private static readonly Lazy<AppConfigService> _lazy =
        new(() => new AppConfigService(), LazyThreadSafetyMode.ExecutionAndPublication);

    // Acceso global thread-safe (doble comprobación + Lazy).
    public static AppConfigService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= _lazy.Value;
                }
            }
            return _instance;
        }
    }

    // Solo para pruebas: permite reiniciar el singleton.
    internal static void ResetForTests()
    {
        lock (_lock)
        {
            _instance = null;
        }
    }

    private AppConfigService()
    {
    }

    public string NombreApp { get; private set; } = "Sistema de Medicamentos";
    public string Version { get; private set; } = "1.0";
    public int RegistrosCargados { get; private set; }

    // Carga valores desde IConfiguration (appsettings.json).
    public void Configure(IConfiguration configuration)
    {
        lock (_lock)
        {
            NombreApp = configuration["AppConfig:NombreApp"] ?? NombreApp;
            Version = configuration["AppConfig:Version"] ?? Version;
            RegistrosCargados++;
        }
    }

    public string Descripcion => $"{NombreApp} v{Version}";
}
