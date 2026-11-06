using System;

namespace EcoDriveTcc.Models.Constants
{
    public static class TarifaConstant
    {
        public const decimal ValorPorMinuto = 1.00m;

        public static int CalcularMinutos(DateTime inicio, DateTime fim)
        {
            int minutos = (int)Math.Ceiling((fim - inicio).TotalMinutes);
            return minutos < 1 ? 1 : minutos;
        }

        public static decimal CalcularValor(int minutos)
        {
            return minutos * ValorPorMinuto;
        }
    }
}