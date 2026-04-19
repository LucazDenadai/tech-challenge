using System.Text.RegularExpressions;

namespace TechChallenge.Domain.Validators;

public static class CnpjValidator
{
    public static void Validar(string cnpj)
    {
        if (!Valido(cnpj))
            throw new ArgumentException("CNPJ inválido.", nameof(cnpj));
    }

    public static bool Valido(string cnpj)
    {
        var digits = Regex.Replace(cnpj ?? "", @"\D", "", RegexOptions.None, TimeSpan.FromMilliseconds(100));

        if (digits.Length != 14)
            return false;

        if (digits.Distinct().Count() == 1)
            return false;

        return CalcularDigito(digits, [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]) == int.Parse(digits[12].ToString())
            && CalcularDigito(digits, [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]) == int.Parse(digits[13].ToString());
    }

    private static int CalcularDigito(string digits, int[] pesos)
    {
        var soma = pesos.Select((p, i) => int.Parse(digits[i].ToString()) * p).Sum();
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
