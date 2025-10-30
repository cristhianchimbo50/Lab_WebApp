using Lab_APIRest.Infrastructure.EF.Models;
using Lab_Contracts.Ordenes;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Lab_APIRest.Services.PDF
{
    public class PdfTicketService
    {
        public byte[] GenerarTicketOrden(OrdenTicketDto orden)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(200, 9999);
                    page.Margin(5);

                    page.Content().Column(col =>
                    {
                        col.Spacing(4);

                        col.Item().AlignCenter().Text("LA INMACULADA").Bold().FontSize(12);
                        col.Item().AlignCenter().Text("LABORATORIO CLINICO DE BAJA COMPLEJIDAD").FontSize(8);
                        col.Item().AlignCenter().Text("Dir: Av. 20 de Diciembre y López de Galarza");
                        col.Item().AlignCenter().Text("Guano - Chimborazo");
                        col.Item().AlignCenter().Text("Tel.: 099 505 5992 / 098 323 9788");
                        col.Item().Text("--------------------------------------------------").FontSize(7);

                        col.Item().AlignCenter().Text("Orden").Bold();
                        col.Item().Text($"Orden.: {orden.NumeroOrden}");
                        col.Item().Text($"Fecha: {orden.FechaOrden:dd/MM/yyyy}");
                        col.Item().Text($"Paciente: {orden.NombrePaciente}");
                        col.Item().Text($"Cédula: {orden.CedulaPaciente}");
                        col.Item().Text($"Edad: {orden.EdadPaciente} años");
                        col.Item().Text($"Médico: Dr. {orden.NombreMedico}");

                        col.Item().Text("--------------------------------------------------").FontSize(7);

                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Examen").Bold();
                            r.RelativeItem().AlignRight().Text("Precio").Bold();
                        });

                        foreach (var examen in orden.Examenes)
                        {
                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Text(examen.NombreExamen);
                                r.RelativeItem().AlignRight().Text($"${examen.Precio:F2}");
                            });
                        }

                        col.Item().Text("--------------------------------------------------").FontSize(7);

                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Total:").Bold();
                            r.RelativeItem().AlignRight().Text($"${orden.Total:F2}");
                        });

                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Pagado:");
                            r.RelativeItem().AlignRight().Text($"${orden.TotalPagado:F2}");
                        });

                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Saldo:");
                            r.RelativeItem().AlignRight().Text($"${orden.SaldoPendiente:F2}");
                        });

                        col.Item().Text("--------------------------------------------------").FontSize(7);

                        col.Item().Text($"Forma de Pago: {orden.TipoPago}");
                        col.Item().AlignCenter().Text("¡Gracias por su preferencia!").FontSize(8);
                        col.Item().AlignCenter().Text("*** Documento sin valor tributario ***").FontSize(6);
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
