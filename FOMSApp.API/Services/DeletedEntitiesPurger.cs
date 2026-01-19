using Microsoft.EntityFrameworkCore;
using FOMSApp.API.Data;
using FOMSApp.Shared.Models;

namespace FOMSApp.API.Services;

public sealed record PurgeDeletedResult(
    TimeSpan Retention,
    DateTimeOffset CutoffUtc,
    int VaultsPurged,
    int MidpointsPurged,
    int CablesPurged,
    int PhotoFilesDeletedAttempts,
    int PhotoFilesDeletedSucceeded);

/// <summary>
/// Permanently purges soft-deleted entities older than a retention window.
/// Deletes DB rows and deletes associated photo files via the configured storage provider.
/// </summary>
public sealed class DeletedEntitiesPurger(
    AppDbContext db,
    IStorageService storage,
    ILogger<DeletedEntitiesPurger> logger)
{
    private readonly AppDbContext _db = db;
    private readonly IStorageService _storage = storage;
    private readonly ILogger<DeletedEntitiesPurger> _logger = logger;

    public async Task<PurgeDeletedResult> PurgeAsync(TimeSpan retention, CancellationToken cancellationToken = default)
    {
        if (retention <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(retention), "Retention must be > 0.");

        var cutoffUtc = DateTimeOffset.UtcNow.Subtract(retention);

        // Vaults + photos
        var vaultsToPurge = await _db.Vaults
            .Include(v => v.Photos)
            .Where(v => v.IsDeleted && v.DeletedAt != null && v.DeletedAt < cutoffUtc)
            .ToListAsync(cancellationToken);

        // Midpoints + photos
        var midpointsToPurge = await _db.Midpoints
            .Include(m => m.Photos)
            .Where(m => m.IsDeleted && m.DeletedAt != null && m.DeletedAt < cutoffUtc)
            .ToListAsync(cancellationToken);

        // Cables (no photos)
        var cablesToPurge = await _db.Cables
            .Where(c => c.IsDeleted && c.DeletedAt != null && c.DeletedAt < cutoffUtc)
            .ToListAsync(cancellationToken);

        int deleteAttempts = 0;
        int deleteSucceeded = 0;

        async Task DeletePhotoFilesAsync(IEnumerable<Photo> photos)
        {
            foreach (var photo in photos)
            {
                if (string.IsNullOrWhiteSpace(photo.FileName))
                    continue;

                deleteAttempts++;
                bool deleted;
                try
                {
                    deleted = await _storage.DeleteFileAsync(photo.FileName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete photo file '{FileName}' during purge.", photo.FileName);
                    deleted = false;
                }

                if (deleted)
                {
                    deleteSucceeded++;
                    _logger.LogInformation(
                        "Purged photo file '{FileName}' (PhotoId={PhotoId}, VaultId={VaultId}, MidpointId={MidpointId}).",
                        photo.FileName,
                        photo.Id,
                        photo.VaultId,
                        photo.MidpointId);
                }
            }
        }

        // Delete files first (best-effort). DB deletes will cascade Photo rows for vaults/midpoints.
        foreach (var v in vaultsToPurge)
            await DeletePhotoFilesAsync(v.Photos);

        foreach (var m in midpointsToPurge)
            await DeletePhotoFilesAsync(m.Photos);

        // Remove DB rows (cascades photos)
        if (vaultsToPurge.Count > 0)
            _db.Vaults.RemoveRange(vaultsToPurge);

        if (midpointsToPurge.Count > 0)
            _db.Midpoints.RemoveRange(midpointsToPurge);

        if (cablesToPurge.Count > 0)
            _db.Cables.RemoveRange(cablesToPurge);

        await _db.SaveChangesAsync(cancellationToken);

        return new PurgeDeletedResult(
            Retention: retention,
            CutoffUtc: cutoffUtc,
            VaultsPurged: vaultsToPurge.Count,
            MidpointsPurged: midpointsToPurge.Count,
            CablesPurged: cablesToPurge.Count,
            PhotoFilesDeletedAttempts: deleteAttempts,
            PhotoFilesDeletedSucceeded: deleteSucceeded);
    }
}

