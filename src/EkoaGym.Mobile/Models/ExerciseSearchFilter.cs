namespace EkoaGym.Mobile.Models;

// Query filter for GET api/exercises — mirrors the API's query params (muscleGroup/equipment/difficulty/q/page/pageSize).
public record ExerciseSearchFilter(
    string? MuscleGroup,
    string? Equipment,
    string? Difficulty,
    string? Query,
    int Page,
    int PageSize);
