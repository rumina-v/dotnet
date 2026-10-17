using System.Net;

namespace UserRegistration.Api.Services;

public record ApprovalReply(HttpStatusCode StatusCode, string Json, string? Location);

public class ApprovalServiceClient(HttpClient client)
{
    public async Task<ApprovalReply> SendAsync(
        HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body != null) { request.Content = JsonContent.Create(body); }
        using var response = await client.SendAsync(request, cancellationToken);
        if (response.StatusCode is not (HttpStatusCode.OK or HttpStatusCode.Created
            or HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.Conflict))
        {
            throw new HttpRequestException("Approval service returned an unexpected status.", null, response.StatusCode);
        }
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var location = response.Headers.Location;
        return new ApprovalReply(response.StatusCode, json,
            location?.IsAbsoluteUri == true ? location.PathAndQuery : location?.ToString());
    }
}
