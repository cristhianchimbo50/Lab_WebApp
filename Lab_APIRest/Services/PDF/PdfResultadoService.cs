using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Lab_Contracts.Resultados;
using Lab_Contracts.Examenes;

namespace Lab_APIRest.Services.PDF
{
    public class PdfResultadoService
    {
        public byte[] GenerarResultadosPdf(IEnumerable<ResultadoCompletoDto> resultados)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "encabezado_lab.png");

            var document = Document.Create(container =>
            {
                foreach (var resultado in resultados)
                {
                    container.Page(page =>
                    {
                        page.Margin(20);
                        page.DefaultTextStyle(TextStyle.Default.FontSize(10));

                        page.Header().Height(120).AlignCenter().AlignMiddle().Element(header =>
                        {
                            header.Image(Image.FromFile(imagePath)).FitWidth();
                        });

                        page.Content().Column(col =>
                        {
                            col.Spacing(10);

                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"Orden: {resultado.NumeroOrden}").SemiBold();
                                row.RelativeItem().AlignCenter().Text($"Resultado: {resultado.NumeroResultado}").SemiBold();
                                row.RelativeItem().AlignRight().Text($"Fecha: {resultado.FechaResultado:dd/MM/yyyy}").SemiBold();
                            });

                            col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(info =>
                            {
                                info.Spacing(4);
                                info.Item().Text("Datos del Paciente").Bold().FontSize(11);
                                info.Item().Row(r =>
                                {
                                    r.RelativeItem().Text($"Nombres: {resultado.NombrePaciente}");
                                    r.RelativeItem().AlignCenter().Text($"Cédula: {resultado.CedulaPaciente}");
                                    r.RelativeItem().AlignRight().Text($"Género: {resultado.GeneroPaciente ?? "-"}");
                                });
                                info.Item().Row(r =>
                                {
                                    r.RelativeItem().Text($"Edad: {resultado.EdadPaciente} años");
                                    r.RelativeItem().AlignRight().Text($"Médico Solicitante: {resultado.MedicoSolicitante}");
                                });
                            });

                            var tituloExamen = resultado.Detalles
                                .SelectMany(d => d.Detalles)
                                .FirstOrDefault()?.TituloExamen ?? "Resultados";

                            col.Item().AlignCenter().Text(tituloExamen).Bold().FontSize(13).Underline();

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("Parámetro").Bold();
                                    header.Cell().Element(HeaderCellStyle).Text("Resultado").Bold();
                                    header.Cell().Element(HeaderCellStyle).Text("Unidad").Bold();
                                    header.Cell().Element(HeaderCellStyle).Text("Valor de Referencia").Bold();
                                });

                                foreach (var detalle in resultado.Detalles)
                                {
                                    foreach (var examen in detalle.Detalles)
                                    {
                                        table.Cell().Element(BodyCellStyle).Text(examen.NombreExamen);
                                        table.Cell().Element(BodyCellStyle).Text(string.IsNullOrWhiteSpace(examen.Valor) ? "-" : examen.Valor);
                                        table.Cell().Element(BodyCellStyle).Text(string.IsNullOrWhiteSpace(examen.Unidad) ? "-" : examen.Unidad);
                                        table.Cell().Element(BodyCellStyle).Text(string.IsNullOrWhiteSpace(examen.ValorReferencia) ? "-" : examen.ValorReferencia);
                                    }
                                }

                                static IContainer HeaderCellStyle(IContainer container) =>
                                    container.DefaultTextStyle(TextStyle.Default.FontSize(10).SemiBold())
                                            .PaddingVertical(6)
                                            .PaddingHorizontal(4)
                                            .Background(Colors.Grey.Lighten3)
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Grey.Lighten2);

                                static IContainer BodyCellStyle(IContainer container) =>
                                    container.PaddingVertical(4)
                                            .PaddingHorizontal(4)
                                            .BorderBottom(0.5f)
                                            .BorderColor(Colors.Grey.Lighten4);
                            });

                            var observaciones = string.Join("\n",
                                resultado.Detalles
                                    .Select(d => d.Observaciones)
                                    .Where(o => !string.IsNullOrWhiteSpace(o)));

                            if (!string.IsNullOrWhiteSpace(observaciones))
                            {
                                col.Item().PaddingTop(8).Text("Observaciones:").Bold();
                                col.Item().Text(observaciones);
                            }
                        });

                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.Span("Laboratorio Clínico La Inmaculada").SemiBold();
                            text.Span("  |  ");
                            text.Span("Documento generado automáticamente").FontSize(9);
                        });
                    });
                }
            });

            return document.GeneratePdf();
        }
    }
}
