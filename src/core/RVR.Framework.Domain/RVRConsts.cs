namespace RVR.Framework.Domain;

/// <summary>
/// Constantes globales du framework RVR
/// </summary>
public static class RVRConsts
{
    /// <summary>
    /// Préfixe pour toutes les tables de la base de données
    /// </summary>
    public const string TablePrefix = "RVR.";

    /// <summary>
    /// Longueur maximale par défaut pour les champs de texte courts
    /// </summary>
    public const int MaxNameLength = 256;

    /// <summary>
    /// Longueur maximale pour les descriptions
    /// </summary>
    public const int MaxDescriptionLength = 1024;

    /// <summary>
    /// Longueur maximale pour les emails
    /// </summary>
    public const int MaxEmailLength = 256;

    /// <summary>
    /// Longueur maximale pour les numéros de téléphone
    /// </summary>
    public const int MaxPhoneLength = 24;
}
