// Archivo: CRM.Datos\Seed\SeedCatalog.cs
// Catalogo semilla con peliculas, retos e insignias iniciales del sistema.

using CRM.Entidad.Enums;

namespace CRM.Datos.Seed;

// Representa la responsabilidad de SeedCatalog dentro de la aplicacion.

internal static class SeedCatalog
{
    // Define los generos iniciales que se cargan en la base de datos.
    internal static readonly SeedGenre[] Genres =
    [
        new(1, "Ciencia ficciÃ³n"),
        new(2, "Drama"),
        new(3, "Comedia"),
        new(4, "Aventura"),
        new(5, "AcciÃ³n"),
        new(6, "AnimaciÃ³n"),
        new(7, "Thriller"),
        new(8, "Romance"),
        new(9, "HistÃ³rico"),
        new(10, "FantasÃ­a")
    ];

    // Define los directores iniciales disponibles para relacionar peliculas.
    internal static readonly SeedPerson[] Directors =
    [
        new(1, "Sidney Lumet"),
        new(2, "James Cameron"),
        new(3, "Greta Gerwig"),
        new(4, "Ryan Coogler"),
        new(5, "Denis Villeneuve"),
        new(6, "Daniel Kwan & Daniel Scheinert"),
        new(7, "Steven Spielberg"),
        new(8, "Martin Scorsese"),
        new(9, "Christopher Nolan"),
        new(10, "Quentin Tarantino"),
        new(11, "Sam Raimi"),
        new(12, "Taika Waititi"),
        new(13, "Paul King")
    ];

    // Define los actores iniciales disponibles para relacionar peliculas.
    internal static readonly SeedPerson[] Actors =
    [
        new(1, "Henry Fonda"),
        new(2, "Sam Worthington"),
        new(3, "Zoe Saldana"),
        new(4, "Margot Robbie"),
        new(5, "Ryan Gosling"),
        new(6, "Letitia Wright"),
        new(7, "Timothee Chalamet"),
        new(8, "Zendaya"),
        new(9, "Michelle Yeoh"),
        new(10, "Leonardo DiCaprio"),
        new(11, "Cillian Murphy"),
        new(12, "Robert Downey Jr."),
        new(13, "John Travolta"),
        new(14, "Samuel L. Jackson"),
        new(15, "Tobey Maguire"),
        new(16, "Chris Hemsworth"),
        new(17, "Calah Lane"),
        new(18, "Ke Huy Quan"),
        new(19, "Gabriel LaBelle"),
        new(20, "Natalie Portman")
    ];

    // Define las peliculas de demostracion incluidas en el catalogo inicial.
    internal static readonly SeedMovie[] Movies =
    [
        new(1001, "12 Angry Men", 1957, 97, "en", "Doce jurados deciden el destino de un joven acusado de asesinato mientras un Ãºnico voto siembra la duda razonable.", "/images/posters/12angrymen.jpg"),
        new(1002, "Avatar: El sentido del agua", 2022, 192, "en", "Jake Sully y Neytiri luchan por proteger a su familia en las profundidades de Pandora.", "/images/posters/Avatar2.jpg"),
        new(1003, "Barbie", 2023, 114, "en", "Barbie abandona Barbieland para descubrir quiÃ©n es de verdad en el mundo real.", "/images/posters/Barbie.jpg"),
        new(1004, "Black Panther: Wakanda Forever", 2022, 161, "en", "Wakanda se enfrenta a una nueva amenaza mientras honra el legado de su rey.", "/images/posters/BlackPanther2.jpg"),
        new(1005, "Dune", 2021, 155, "en", "Paul Atreides inicia un viaje Ã©pico en Arrakis, entre profecÃ­as, intrigas y supervivencia.", "/images/posters/Dune1.jpg"),
        new(1006, "Dune: Parte Dos", 2024, 166, "en", "Paul abraza su destino y lidera la rebeliÃ³n contra quienes destruyeron a su familia.", "/images/posters/Dune2.jpg"),
        new(1007, "Everything Everywhere All at Once", 2022, 139, "en", "Una propietaria de lavanderÃ­a debe conectar universos imposibles para salvar a su familia.", "/images/posters/EverythingEveryWhere.jpg"),
        new(1008, "The Fabelmans", 2022, 151, "en", "Un adolescente descubre el poder del cine mientras su familia se transforma.", "/images/posters/Fabelmans.jpg"),
        new(1009, "Killers of the Flower Moon", 2023, 206, "en", "Una oleada de asesinatos en la naciÃ³n osage revela una conspiraciÃ³n devastadora.", "/images/posters/KillerFlowerMoon.jpg"),
        new(1010, "Oppenheimer", 2023, 180, "en", "El padre de la bomba atÃ³mica encara el peso moral de su creaciÃ³n en plena guerra.", "/images/posters/Openhaimmer.jpg"),
        new(1011, "Pulp Fiction", 1994, 154, "en", "Historias criminales y diÃ¡logos afilados se entrelazan en Los Ãngeles.", "/images/posters/pulpfiction.jpg"),
        new(1012, "Spider-Man 3", 2007, 139, "en", "Peter Parker ve cÃ³mo el traje negro y sus enemigos llevan su vida al lÃ­mite.", "/images/posters/SpiderMan3.jpg"),
        new(1013, "Thor: Love and Thunder", 2022, 119, "en", "Thor busca sentido a su vida mientras un nuevo villano amenaza a los dioses.", "/images/posters/Thor4.jpg"),
        new(1014, "Wonka", 2023, 116, "en", "El joven Willy Wonka persigue su sueÃ±o de abrir la chocolaterÃ­a mÃ¡s imaginativa del mundo.", "/images/posters/Wonka.jpg")
    ];

    // Define la relacion inicial entre peliculas y generos.
    internal static readonly (int MovieId, int GenreId)[] MovieGenres =
    [
        (1001, 2), (1001, 7),
        (1002, 1), (1002, 4), (1002, 5),
        (1003, 3), (1003, 10),
        (1004, 5), (1004, 4), (1004, 2),
        (1005, 1), (1005, 4), (1005, 2),
        (1006, 1), (1006, 4), (1006, 2),
        (1007, 1), (1007, 3), (1007, 5),
        (1008, 2),
        (1009, 2), (1009, 9), (1009, 7),
        (1010, 2), (1010, 7), (1010, 9),
        (1011, 2), (1011, 3), (1011, 7),
        (1012, 5), (1012, 4),
        (1013, 5), (1013, 3), (1013, 4),
        (1014, 10), (1014, 3)
    ];

    // Define la relacion inicial entre peliculas y directores.
    internal static readonly (int MovieId, int DirectorId)[] MovieDirectors =
    [
        (1001, 1), (1002, 2), (1003, 3), (1004, 4), (1005, 5), (1006, 5), (1007, 6),
        (1008, 7), (1009, 8), (1010, 9), (1011, 10), (1012, 11), (1013, 12), (1014, 13)
    ];

    // Define la relacion inicial entre peliculas y actores.
    internal static readonly (int MovieId, int ActorId)[] MovieActors =
    [
        (1001, 1),
        (1002, 2), (1002, 3),
        (1003, 4), (1003, 5),
        (1004, 6),
        (1005, 7), (1005, 8),
        (1006, 7), (1006, 8),
        (1007, 9), (1007, 18),
        (1008, 19),
        (1009, 10),
        (1010, 11), (1010, 12),
        (1011, 13), (1011, 14),
        (1012, 15),
        (1013, 16), (1013, 20),
        (1014, 7), (1014, 17)
    ];

    // Define las insignias iniciales que se pueden desbloquear.
    internal static readonly SeedBadge[] Badges =
    [
        new(1, "Cinefilo Inicial", "/images/badges/badge-gold.svg"),
        new(2, "Cinefilo Intermedio", "/images/badges/badge-gold.svg"),
        new(3, "Explorador Sci-Fi", "/images/badges/badge-blue.svg"),
        new(4, "Fan de la Comedia", "/images/badges/badge-rose.svg"),
        new(5, "Nolanverse", "/images/badges/badge-red.svg"),
        new(6, "Barbiecore", "/images/badges/badge-rose.svg"),
        new(7, "Seguidor de Timothee", "/images/badges/badge-gold.svg"),
        new(8, "Duo del desierto", "/images/badges/badge-blue.svg"),
        new(9, "Radar 2023", "/images/badges/badge-red.svg"),
        new(10, "Critico Constante", "/images/badges/badge-blue.svg"),
        new(11, "Desafiante", "/images/badges/badge-red.svg"),
        new(12, "Cinefilo de Elite", "/images/badges/badge-gold.svg")
    ];

    // Define los logros iniciales asociados a reglas de progreso.
    internal static readonly SeedAchievement[] Achievements =
    [
        new(1, "Ver 5 peliculas", "Completa cinco peliculas valoradas en la plataforma.", 5, 1, RuleType.General, "views"),
        new(2, "Ver 10 peliculas", "Llega a diez peliculas vistas para desbloquear el siguiente nivel.", 10, 2, RuleType.General, "views"),
        new(3, "Explorador de Ciencia Ficcion", "Valora tres peliculas de ciencia ficcion.", 3, 3, RuleType.Genre, "Ciencia ficciÃ³n"),
        new(4, "Fan de la Comedia", "Valora tres peliculas de comedia.", 3, 4, RuleType.Genre, "Comedia"),
        new(5, "Seguidor de Christopher Nolan", "Valora dos peliculas dirigidas por Christopher Nolan.", 2, 5, RuleType.Director, "Christopher Nolan"),
        new(6, "Seguidor de Greta Gerwig", "Valora una pelicula dirigida por Greta Gerwig.", 1, 6, RuleType.Director, "Greta Gerwig"),
        new(7, "Fan de Timothee Chalamet", "Valora dos peliculas con Timothee Chalamet.", 2, 7, RuleType.Actor, "Timothee Chalamet"),
        new(8, "Fan de Zendaya", "Valora dos peliculas con Zendaya.", 2, 8, RuleType.Actor, "Zendaya"),
        new(9, "Cinefilo del 2023", "Valora tres estrenos del aÃ±o 2023.", 3, 9, RuleType.Year, "2023"),
        new(10, "Critico constante", "Registra ocho valoraciones en total.", 8, 10, RuleType.General, "ratings"),
        new(11, "Completa 2 retos", "Finaliza dos retos activos dentro del plazo.", 2, 11, RuleType.General, "completed_challenges"),
        new(12, "Cinefilo de Elite", "Acumula quince peliculas vistas.", 15, 12, RuleType.General, "views")
    ];

    // Define los retos iniciales que se asignan a los usuarios.
    internal static readonly SeedChallenge[] Challenges =
    [
        new(1, "Cinefilo novato", "Mira tu primera pelicula en la nueva plataforma.", ChallengeType.Daily, RuleType.General, "views", 1),
        new(2, "Maraton de Comedia", "Valora dos peliculas del genero comedia.", ChallengeType.Weekly, RuleType.Genre, "Comedia", 2),
        new(3, "Explorador de Culto", "Valora una pelicula estrenada antes de 2000.", ChallengeType.Thematic, RuleType.Year, "1999", 1),
        new(4, "Universo Nolan", "Valora dos peliculas de Christopher Nolan.", ChallengeType.Thematic, RuleType.Director, "Christopher Nolan", 2),
        new(5, "Sci-Fi en marcha", "Valora dos peliculas de ciencia ficcion.", ChallengeType.Weekly, RuleType.Genre, "Ciencia ficciÃ³n", 2),
        new(6, "Radar 2023", "Valora dos peliculas del aÃ±o 2023.", ChallengeType.Weekly, RuleType.Year, "2023", 2),
        new(7, "Talento Chalamet", "Valora dos peliculas con Timothee Chalamet.", ChallengeType.Thematic, RuleType.Actor, "Timothee Chalamet", 2),
        new(8, "Tormenta de accion", "Valora dos peliculas de accion.", ChallengeType.Weekly, RuleType.Genre, "AcciÃ³n", 2)
    ];

    // Define los usuarios de demostracion para poblar la aplicacion.
    internal static readonly SeedUser[] Users =
    [
        new(1, "demo", "Demo CineRoad", "demo@cineroadmap.local", "600100100"),
        new(2, "marta", "Marta Frames", "marta@cineroadmap.local", "600200200"),
        new(3, "diego", "Diego Noir", "diego@cineroadmap.local", "600300300"),
        new(4, "lucia", "Lucia Scope", "lucia@cineroadmap.local", "600400400")
    ];

    // Define las valoraciones iniciales de los usuarios de demostracion.
    internal static readonly SeedRating[] Ratings =
    [
        new(1, 1005, 5, 25),
        new(1, 1006, 5, 7),
        new(1, 1003, 4, 11),
        new(1, 1010, 5, 16),
        new(1, 1014, 4, 3),
        new(1, 1002, 4, 29),
        new(2, 1003, 5, 9),
        new(2, 1014, 4, 4),
        new(2, 1013, 3, 13),
        new(2, 1012, 4, 26),
        new(2, 1007, 5, 19),
        new(3, 1010, 5, 10),
        new(3, 1009, 4, 14),
        new(3, 1008, 4, 31),
        new(3, 1001, 5, 65),
        new(3, 1011, 5, 6),
        new(4, 1004, 4, 17),
        new(4, 1002, 5, 12),
        new(4, 1005, 4, 5),
        new(4, 1012, 4, 22)
    ];

    // Define las peliculas pendientes iniciales de usuarios de demostracion.
    internal static readonly SeedWatchlist[] WatchlistEntries =
    [
        new(1, 1009),
        new(1, 1013),
        new(2, 1005),
        new(3, 1006)
    ];

    // Define los retos asignados inicialmente a usuarios de demostracion.
    internal static readonly SeedUserChallenge[] UserChallenges =
    [
        new(1, 2, 6), new(1, 5, 8), new(1, 7, 20),
        new(2, 1, 1), new(2, 2, 7), new(2, 6, 9),
        new(3, 3, 18), new(3, 4, 25), new(3, 6, 12),
        new(4, 1, 1), new(4, 5, 6), new(4, 8, 10)
    ];
}

// Transporta los datos de SeedGenre entre capas.
internal sealed record SeedGenre(int Id, string Name);

// Transporta los datos de SeedPerson entre capas.
internal sealed record SeedPerson(int Id, string Name);

// Transporta los datos de SeedMovie entre capas.
internal sealed record SeedMovie(int Id, string Title, int Year, int DurationMinutes, string OriginalLanguage, string Synopsis, string PosterPath);

// Transporta los datos de SeedBadge entre capas.
internal sealed record SeedBadge(int Id, string Name, string ImagePath);

// Transporta los datos de SeedAchievement entre capas.
internal sealed record SeedAchievement(int Id, string Title, string Description, int Objective, int BadgeId, RuleType RequirementType, string RequirementValue);

// Transporta los datos de SeedChallenge entre capas.
internal sealed record SeedChallenge(int Id, string Name, string Description, ChallengeType Type, RuleType RuleType, string RuleValue, int TargetProgress);

// Transporta los datos de SeedUser entre capas.
internal sealed record SeedUser(int Id, string UserName, string NickName, string Email, string Phone);

// Transporta los datos de SeedRating entre capas.
internal sealed record SeedRating(int UserId, int MovieId, int Score, int DaysAgo);

// Transporta los datos de SeedWatchlist entre capas.
internal sealed record SeedWatchlist(int UserId, int MovieId);

// Transporta los datos de SeedUserChallenge entre capas.
internal sealed record SeedUserChallenge(int UserId, int ChallengeId, int DaysAgoAssigned);
