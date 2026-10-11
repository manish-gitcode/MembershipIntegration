using System.Net;
using System.Text.Json;
using MembershipIntegration.Models;
using MembershipIntegration.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace MembershipIntegration;

public class MapRegistrationFunction
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly RegistrationMapper _mapper;
    private readonly ILogger<MapRegistrationFunction> _logger;

    public MapRegistrationFunction(RegistrationMapper mapper, ILogger<MapRegistrationFunction> logger)
    {
        _mapper = mapper;
        _logger = logger;
    }

    [Function("MapRegistration")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "map-registration")] HttpRequestData req)
    {
        _logger.LogInformation("MapRegistration triggered");

        CrmRegistration? registration;
        try
        {
            registration = await JsonSerializer.DeserializeAsync<CrmRegistration>(req.Body, JsonOpts);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Bad JSON");
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteAsJsonAsync(new { error = "Invalid JSON payload.", detail = ex.Message });
            return bad;
        }

        var (validation, payload) = _mapper.Map(registration);

        if (!validation.IsValid)
        {
            _logger.LogWarning("Validation failed: {Errors}", string.Join("; ", validation.Errors));
            var err = req.CreateResponse(HttpStatusCode.BadRequest);
            await err.WriteAsJsonAsync(new { error = "Validation failed.", details = validation.Errors });
            return err;
        }

        var ok = req.CreateResponse(HttpStatusCode.OK);
        ok.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await ok.WriteStringAsync(JsonSerializer.Serialize(payload, JsonOpts));
        return ok;
    }
}
