namespace Lab_Contracts.Common
{
    public static class EstadoIds
    {
        public static class Orden
        {
            public const int EnProceso = 1;
            public const int Finalizada = 2;
            public const int Anulada = 3;
        }

        public static class Pago
        {
            public const int Pendiente = 1;
            public const int Abonado = 2;
            public const int Pagado = 3;
        }

        public static class Resultado
        {
            public const int Pendiente = 1;
            public const int EnRevision = 2;
            public const int Correccion = 3;
            public const int Aprobado = 4;
            public const int Anulada = 5;
        }
    }
}
