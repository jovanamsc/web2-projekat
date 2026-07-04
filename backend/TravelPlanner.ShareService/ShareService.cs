using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace TravelPlanner.ShareService;

public sealed class ShareService : StatefulService
{
    private const string ShareTokenDictionary = "shareTokens";

    public ShareService(StatefulServiceContext context) : base(context) { }

    protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners() =>
        new ServiceReplicaListener[]
        {
            new(serviceContext =>
                new KestrelCommunicationListener(serviceContext, (url, listener) =>
                {
                    var builder = WebApplication.CreateBuilder();
                    builder.WebHost.UseUrls(url);
                    builder.WebHost.UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl);

                    builder.Services.AddControllers();
                    builder.Services.AddSingleton(this);

                    var app = builder.Build();
                    app.MapControllers();

                    return app;
                }), "ServiceEndpoint", listenOnSecondary: false)
        };

    public async Task<string> CreateShareTokenAsync(int travelPlanId, string accessType, TimeSpan expiry)
    {
        var tokens = await StateManager.GetOrAddAsync<IReliableDictionary<string, ShareToken>>(ShareTokenDictionary);
        var token = Guid.NewGuid().ToString("N");

        using var tx = StateManager.CreateTransaction();
        await tokens.AddAsync(tx, token, new ShareToken
        {
            Token = token,
            TravelPlanId = travelPlanId,
            AccessType = accessType.ToUpper(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(expiry)
        });
        await tx.CommitAsync();

        return token;
    }

    public async Task<ShareToken?> ValidateTokenAsync(string token)
    {
        var tokens = await StateManager.GetOrAddAsync<IReliableDictionary<string, ShareToken>>(ShareTokenDictionary);

        using var tx = StateManager.CreateTransaction();
        var result = await tokens.TryGetValueAsync(tx, token);

        if (!result.HasValue) return null;
        if (result.Value.ExpiresAt < DateTime.UtcNow)
        {
            await tokens.TryRemoveAsync(tx, token);
            await tx.CommitAsync();
            return null;
        }

        return result.Value;
    }

    public async Task RevokeTokenAsync(string token)
    {
        var tokens = await StateManager.GetOrAddAsync<IReliableDictionary<string, ShareToken>>(ShareTokenDictionary);

        using var tx = StateManager.CreateTransaction();
        await tokens.TryRemoveAsync(tx, token);
        await tx.CommitAsync();
    }

    public async Task<List<ShareToken>> GetTokensByPlanAsync(int planId)
    {
        var tokens = await StateManager.GetOrAddAsync<IReliableDictionary<string, ShareToken>>(ShareTokenDictionary);
        var result = new List<ShareToken>();

        using var tx = StateManager.CreateTransaction();
        var enumerator = (await tokens.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

        while (await enumerator.MoveNextAsync(CancellationToken.None))
        {
            var t = enumerator.Current.Value;
            if (t.TravelPlanId == planId && t.ExpiresAt >= DateTime.UtcNow)
                result.Add(t);
        }

        return result;
    }

    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await CleanupExpiredTokensAsync();
            await Task.Delay(TimeSpan.FromMinutes(30), cancellationToken);
        }
    }

    private async Task CleanupExpiredTokensAsync()
    {
        var tokens = await StateManager.GetOrAddAsync<IReliableDictionary<string, ShareToken>>(ShareTokenDictionary);

        using var tx = StateManager.CreateTransaction();
        var enumerator = (await tokens.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
        var expiredKeys = new List<string>();

        while (await enumerator.MoveNextAsync(CancellationToken.None))
        {
            if (enumerator.Current.Value.ExpiresAt < DateTime.UtcNow)
                expiredKeys.Add(enumerator.Current.Key);
        }

        foreach (var key in expiredKeys)
            await tokens.TryRemoveAsync(tx, key);

        await tx.CommitAsync();
    }
}
