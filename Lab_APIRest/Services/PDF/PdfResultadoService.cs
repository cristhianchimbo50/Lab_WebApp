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

                        page.Header().Height(120).AlignCenter().AlignMiddle().Element(header =>
                        {
                            header.Image(Image.FromFile(imagePath)).FitWidth();
                        });

                        page.Content().Column(col =>
                        {
                            col.Spacing(8);

                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"Orden: {resultado.NumeroOrden}").FontSize(10);
                                r.RelativeItem().AlignRight().Text($"Fecha: {resultado.FechaResultado:dd/M/yyyy}").FontSize(10);
                            });

                            col.Item().Text($"Resultado: {resultado.NumeroResultado}").FontSize(10);

                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"Cédula: {resultado.CedulaPaciente}").FontSize(10);
                                r.RelativeItem().AlignRight().Text($"Edad: {resultado.EdadPaciente} Años").FontSize(10);
                            });

                            col.Item().Text($"Nombres: {resultado.NombrePaciente}").FontSize(10);
                            col.Item().Text($"Medico Solicitante: {resultado.MedicoSolicitante}").FontSize(10);

                            col.Item().PaddingVertical(10);

                            var tituloExamen = resultado.Detalles
                                .SelectMany(d => d.Detalles)
                                .FirstOrDefault()?.TituloExamen ?? "Resultados";

                            col.Item().AlignCenter().Text(tituloExamen).Bold().FontSize(14);

                            col.Item().PaddingBottom(5);

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
                                    header.Cell().Element(CellStyle).Text("Parámetro").Bold();
                                    header.Cell().Element(CellStyle).Text("Resultado").Bold();
                                    header.Cell().Element(CellStyle).Text("Unidad").Bold();
                                    header.Cell().Element(CellStyle).Text("Valor de Referencia").Bold();
                                });

                                foreach (var detalle in resultado.Detalles)
                                {
                                    foreach (var examen in detalle.Detalles )
                                    {
                                        table.Cell().Element(CellStyle).Text(examen.NombreExamen);
                                        table.Cell().Element(CellStyle).Text(examen.Valor.ToString());
                                        table.Cell().Element(CellStyle).Text(examen.Unidad ?? "-");
                                        table.Cell().Element(CellStyle).Text(examen.ValorReferencia ?? "-");
                                    }
                                }

                                static IContainer CellStyle(IContainer container) =>
                                    container.PaddingVertical(3)
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Grey.Lighten3);
                            });

                            var observaciones = string.Join("\n",
                                resultado.Detalles
                                    .Select(d => d.Observaciones)
                                    .Where(o => !string.IsNullOrWhiteSpace(o)));

                            if (!string.IsNullOrWhiteSpace(observaciones))
                            {
                                col.Item().PaddingTop(10).Text("Observaciones:").Bold().FontSize(10);
                                col.Item().Text(observaciones).FontSize(10);
                            }

                        });
                    });
                }
            });

            return document.GeneratePdf();
        }
    }
}
