
using ClienteApp.Core.Data;
using ClienteApp.Core.Repositories;
using ClienteApp.Core.Services;

namespace ClienteApp.Tests.Integracion;

public class ClienteRepositoryTests : IDisposable
{
    private readonly string _dbPath;
    private readonly string _connectionString;
    private readonly ClienteRepository _repository;

    public ClienteRepositoryTests()
    {
        // Crear una base de datos exclusiva para esta prueba.
        _dbPath = Path.Combine(
            Path.GetTempPath(),
            $"clientes_test_{Guid.NewGuid():N}.db");

        _connectionString = $"Data Source={_dbPath};Pooling=False";

        // Crear la tabla en la base de datos de prueba.
        Database.Inicializar(_connectionString);

        // Utilizar el repositorio real.
        _repository = new ClienteRepository(_connectionString);
    }

    [Fact]
    public void Registrar_ClienteValido_DebePersistirEnSQLite()
    {
        // Arrange: preparar el servicio y los datos.
        var service = new ClienteService(_repository);

        // Act: registrar un cliente.
        var cliente = service.Registrar(
            "Ana López",
            "ana@example.com");

        // Assert: recuperar el cliente desde SQLite.
        var clienteGuardado =
            _repository.ObtenerPorId(cliente.Id);

        Assert.NotNull(clienteGuardado);
        Assert.Equal(cliente.Id, clienteGuardado.Id);
        Assert.Equal("Ana López", clienteGuardado.Nombre);
        Assert.Equal(
            "ana@example.com",
            clienteGuardado.Correo);
    }

    [Fact]
    public void Registrar_CorreoDuplicado_DebeRechazarSegundoCliente()
    {
        // Arrange: preparar el servicio y registrar el primer cliente.
        var service = new ClienteService(_repository);

        service.Registrar(
            "Ana López",
            "ana@example.com");

        // Act + Assert: el segundo registro debe ser rechazado.
        var excepcion = Assert.Throws<InvalidOperationException>(
            () => service.Registrar(
                "Carlos Pérez",
                "ana@example.com"));

        Assert.Equal(
            "El correo ya está registrado.",
            excepcion.Message);
    }

    //Prueba de integración: Implemente una prueba que compruebe que, 
    // al consultar un identificador inexistente mediante el servicio y
    // el repositorio real, utilizando SQLite, se obtiene un resultado nulo.
    [Fact]
    public void ObtenerPorId_IdInexistente_DeberRetornarNulo()
    {
        //Arrange: servicio real con repositorio real y base SQLite temporal vacía
        var service = new ClienteService(_repository);

        //Act: consultar un identificador que no existe
        var resultado = service.ObtenerPorId(999);

        //Assert: no se encontró ningún cliente
        Assert.Null(resultado);
        
    }    


    public void Dispose()
    {
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
