namespace EkoaGym.Domain.Enums;

// Recorded on a membership at creation time by comparing its tier to the user's previous week's tier —
// drives the M-10.1 "promoted/relegated since last cycle" banner.
public enum LeagueMovement
{
    None,
    Promoted,
    Relegated
}
