namespace CS2Analyzer.Application.Interfaces;

public interface ISteamAuthService
{
    /// <summary>
    /// Steam OpenID cavabını Steam-in özündən yoxlayır.
    /// Uğurludursa SteamID64 qaytarır, əks halda null.
    /// </summary>
    Task<string?> VerifyLoginAsync(
        IReadOnlyDictionary<string, string> callbackParameters,
        string expectedReturnTo,
        CancellationToken cancellationToken);
}