
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ClienteApp.Tests.Api;

public class ClientesApiTests
{
    [Fact]
    public async Task Registrar_ClienteValido_DebeRetornar201()
    {
        // Arrange
        using var factory = new ClienteApiFactory();
        using var client = factory.CreateClient();

        var solicitud = new
        {
            Nombre = "Ana López",
            Correo = $"ana_{Guid.NewGuid():N}@example.com"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/Clientes",
            solicitud);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var cliente = await response.Content
            .ReadFromJsonAsync<ClienteRespuesta>();

        Assert.NotNull(cliente);
        Assert.Equal(solicitud.Nombre, cliente.Nombre);
        Assert.Equal(solicitud.Correo, cliente.Correo);
    }

    [Fact]
    public async Task Registrar_NombreVacio_DebeRetornar400()
    {
        // Arrange
        using var factory = new ClienteApiFactory();
        using var client = factory.CreateClient();

        var solicitud = new
        {
            Nombre = "",
            Correo = $"correo_{Guid.NewGuid():N}@example.com"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/Clientes",
            solicitud);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    //Consultar un cliente existente y comprobar que la API devuelve 200 OK
    [Fact]
    public async Task ObtenerPorId_ClienteExistente_DebeRetornar200()
    {
        //Arrange: levantar la API y registrar un cliente para poder consultarlo
        using var factory = new ClienteApiFactory();
        using var client = factory.CreateClient();

        var solicitud = new
        {
            Nombre = "Angelica Mejía",
            Correo = $"amejiat4_{Guid.NewGuid():N}@miumg.edu.gt"
        };

        var respuestaRegistro = await client.PostAsJsonAsync(
            "/api/Clientes",
            solicitud
        );

        Assert.Equal(
            HttpStatusCode.Created,
            respuestaRegistro.StatusCode
        );

        var clienteCreado = await respuestaRegistro.Content
            .ReadFromJsonAsync<ClienteRespuesta>();

        Assert.NotNull(clienteCreado);
        
        //Act: consultar el cliente recién creado por su id
        var response = await client.GetAsync(
            $"/api/Clientes/{clienteCreado.Id}"
        );

        //Assert: La API responde 200 OK con los datos del cliente
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var clienteConsultado = await response.Content
            .ReadFromJsonAsync<ClienteRespuesta>();

        Assert.NotNull(clienteConsultado);
        Assert.Equal(clienteCreado.Id, clienteConsultado.Id);
        Assert.Equal(solicitud.Nombre, clienteConsultado.Nombre);
        Assert.Equal(solicitud.Correo, clienteConsultado.Correo);
    }

    //Consultar un cliente inexistente y comprobar que la API devuelve 404 Not Found
    [Fact]
    public async Task ObtenerPorId_ClienteInexistente_DebeRetornar404()
    {
        //Arrange: levantar la API con una base de datos vacía
        using var factory = new ClienteApiFactory();
        using var client = factory.CreateClient();

        //Act: consultar un id que no existe
        var response = await client.GetAsync("/api/Clientes/21");

        //Assert: la API responde 404 Not Found
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    private sealed record ClienteRespuesta(
        int Id,
        string Nombre,
        string Correo);
}

public sealed class ClienteApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(),
        $"clientes_api_test_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:ClientesDb"] =
                        $"Data Source={_dbPath};Pooling=False"
                });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
