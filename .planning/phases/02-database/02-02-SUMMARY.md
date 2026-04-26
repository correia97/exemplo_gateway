# 02-02 Summary — Entities + DbContexts + Migrations

**Status**: ✅ Complete  
**Date**: 2026-04-24  

## Deliverables
- 7 entity classes: BaseEntity (abstract), Character, Genre, Artist, ArtistGenre, Album, Track
- 2 DbContexts: DragonBallContext (`HasDefaultSchema("dragonball")`), MusicContext (`HasDefaultSchema("music")`)
- 2 Design-time factories for CLI migrations
- Initial EF Core migrations for both contexts

## Key Design Decisions
- BaseEntity provides Id, CreatedAt, UpdatedAt on all main entities
- ArtistGenre join entity does NOT inherit BaseEntity (composite PK join table)
- Genre ↔ Artist Many-to-Many via ArtistGenre join table
- Track.AlbumId nullable (int?) with IsStandalone flag for singles support
- Album → Track: OnDelete(DeleteBehavior.SetNull) to preserve singles when album deleted
- TimeSpan maps to PostgreSQL `interval`, DateOnly maps to PostgreSQL `date`

## Schema Isolation Verified
- DragonBall migration: only `dragonball` schema, only Characters table
- Music migration: only `music` schema, Genres + Artists + ArtistGenres + Albums + Tracks tables
- No cross-schema contamination
