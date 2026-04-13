using System.Text.RegularExpressions;

namespace TechChallenge.Domain.Validators;

public static class PlacaValidator
{
    // Formato antigo: ABC1234 | Mercosul: ABC1D23
    private static readonly Regex _regex = new(@"^[A-Z]{3}[0-9][0-9A-Z][0-9]{2}$", RegexOptions.Compiled);

    public static string Normalizar(string placa)
        => Regex.Replace(placa ?? "", @"[-\s]", "").ToUpperInvariant();

    public static void Validar(string placa)
    {
        if (!EhValido(placa))
            throw new ArgumentException("Placa de veículo inválida. Use o formato ABC1234 ou Mercosul ABC1D23.", nameof(placa));
    }

    public static bool EhValido(string placa)
    {
        var normalizada = Normalizar(placa);
        return _regex.IsMatch(normalizada);
    }
}
