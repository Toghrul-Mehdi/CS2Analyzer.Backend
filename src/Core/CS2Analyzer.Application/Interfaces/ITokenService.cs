
using CS2Analyzer.Application.DTOs;

namespace CS2Analyzer.Application.Interfaces;

public interface ITokenService
{
    /// <summary>İstifadəçi məlumatını (SteamID, ad, avatar) imzalanmış JWT token kimi qaytarır.</summary>
    string CreateAccessToken(SteamProfileDto profile);
}