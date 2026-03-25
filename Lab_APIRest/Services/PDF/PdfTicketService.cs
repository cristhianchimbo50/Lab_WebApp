using Lab_Contracts.Ordenes;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Linq;

namespace Lab_APIRest.Services.PDF
{
    public class PdfTicketService
    {
        public byte[] GenerarTicketOrden(OrdenTicketDto orden)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var culture = CultureInfo.GetCultureInfo("es-EC");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.ContinuousSize(58, Unit.Millimetre);
                    page.Margin(3, Unit.Millimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(TextStyle.Default.FontSize(7).FontFamily("Arial"));

                    page.Content().Column(column =>
                    {
                        column.Spacing(3);

                        column.Item().AlignCenter().Text("LA INMACULADA").Bold().FontSize(10);
                        column.Item().AlignCenter().Text("LABORATORIO CLÍNICO").SemiBold().FontSize(7);
                        column.Item().AlignCenter().Text("DE BAJA COMPLEJIDAD").SemiBold().FontSize(7);
                        column.Item().AlignCenter().Text("Guano - Chimborazo").FontSize(6);
                        column.Item().AlignCenter().Text("099 505 5992 / 098 323 9788").FontSize(6);

                        column.Item().PaddingTop(2).PaddingBottom(1).LineHorizontal(0.7f);
                        column.Item().AlignCenter().Text("ORDEN DE LABORATORIO").Bold().FontSize(8);
                        column.Item().AlignCenter().Text($"N° {orden.NumeroOrden}").SemiBold().FontSize(7);
                        column.Item().PaddingBottom(1).LineHorizontal(0.7f);

                        Dato(column, "Fecha", orden.FechaOrden.ToString("dd/MM/yyyy HH:mm", culture));
                        Dato(column, "Paciente", orden.NombrePaciente);
                        Dato(column, "Cédula", orden.CedulaPaciente);
                        Dato(column, "Edad", $"{orden.EdadPaciente} años");
                        Dato(column, "Médico", $"Dr. {orden.NombreMedico}");

                        column.Item().PaddingTop(1).PaddingBottom(1).LineHorizontal(0.5f);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("EXAMEN").Bold().FontSize(7);
                            row.ConstantItem(45).AlignRight().Text("VALOR").Bold().FontSize(7);
                        });

                        if (orden.Examenes != null && orden.Examenes.Any())
                        {
                            foreach (var examen in orden.Examenes)
                            {
                                column.Item().PaddingTop(1).Row(row =>
                                {
                                    row.RelativeItem().Text(examen.NombreExamen).FontSize(7);
                                    row.ConstantItem(45).AlignRight().Text(examen.Precio.ToString("C2", culture)).FontSize(7);
                                });
                            }
                        }
                        else
                        {
                            column.Item().Text("Sin exámenes registrados").FontSize(7);
                        }

                        column.Item().PaddingTop(2).PaddingBottom(1).LineHorizontal(0.7f);

                        column.Item().Background(Colors.Grey.Lighten4).PaddingVertical(2).PaddingHorizontal(3).Column(total =>
                        {
                            total.Spacing(1);

                            total.Item().Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL").Bold().FontSize(9);
                                row.ConstantItem(45).AlignRight().Text(orden.Total.ToString("C2", culture)).Bold().FontSize(9);
                            });

                            total.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Pagado").FontSize(7);
                                row.ConstantItem(45).AlignRight().Text(orden.TotalPagado.ToString("C2", culture)).FontSize(7);
                            });

                            total.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Saldo").SemiBold().FontSize(7);
                                row.ConstantItem(45).AlignRight().Text(orden.SaldoPendiente.ToString("C2", culture)).SemiBold().FontSize(7);
                            });
                        });

                        column.Item().PaddingTop(2).LineHorizontal(0.5f);
                        var tiposPago = (orden.TipoPago != null && orden.TipoPago.Any())
                            ? string.Join(", ", orden.TipoPago)
                            : "-";
                        column.Item().Text($"Forma de pago: {tiposPago}").FontSize(7);
                        column.Item().AlignCenter().PaddingTop(3).Text("Gracias por su preferencia").SemiBold().FontSize(7);
                        column.Item().AlignCenter().Text("Documento sin valor tributario").Italic().FontSize(6);
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static void Dato(ColumnDescriptor column, string etiqueta, string valor)
        {
            column.Item().Row(row =>
            {
                row.ConstantItem(42).Text($"{etiqueta}:").SemiBold().FontSize(7);
                row.RelativeItem().AlignRight().Text(valor ?? string.Empty).FontSize(7);
            });
        }
    }
}