using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinanceiroApi.Infrastructure.Reports;

public sealed class PayslipGeneratorService : IPayslipGeneratorService
{
    private readonly ILogger<PayslipGeneratorService> _logger;

    public PayslipGeneratorService(ILogger<PayslipGeneratorService> logger)
    {
        _logger = logger;
    }

    public Task<byte[]> GeneratePdfAsync(
        PayrollResponse payroll,
        EmployeeResponse employee,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Generating payslip PDF for Employee {EmployeeId} | Month: {Month}/{Year}",
            employee.Id, payroll.Month, payroll.Year);

        var html = BuildPayslipHtml(payroll, employee);
        var pdf = HtmlToPdf(html);
        return Task.FromResult(pdf);
    }

    private static string BuildPayslipHtml(PayrollResponse payroll, EmployeeResponse employee) => $$"""
        <!DOCTYPE html>
        <html lang="pt-BR">
        <head>
          <meta charset="utf-8"/>
          <style>
            body { font-family: Arial, sans-serif; font-size: 12px; margin: 40px; }
            h1   { font-size: 18px; text-align: center; }
            table { width: 100%; border-collapse: collapse; margin-top: 16px; }
            th, td { border: 1px solid #ccc; padding: 6px 10px; }
            th { background: #f0f0f0; font-weight: bold; }
            .total { font-weight: bold; background: #e8f4e8; }
          </style>
        </head>
        <body>
          <h1>HOLERITE – {{payroll.Month:D2}}/{{payroll.Year}}</h1>
          <p><strong>Funcionário:</strong> {{employee.FirstName}} {{employee.LastName}}</p>
          <p><strong>CPF:</strong> {{employee.Cpf}}</p>
          <p><strong>Departamento:</strong> {{employee.DepartmentName}}</p>
          <p><strong>Cargo:</strong> {{employee.Position}}</p>
          <table>
            <tr><th>Descrição</th><th>Proventos</th><th>Descontos</th></tr>
            <tr><td>Salário Bruto</td><td>{{payroll.TotalGross:C}}</td><td>–</td></tr>
            <tr><td>Descontos</td><td>–</td><td>{{payroll.TotalDiscounts:C}}</td></tr>
            <tr class="total">
              <td>SALÁRIO LÍQUIDO</td>
              <td colspan="2" style="text-align:center">{{payroll.TotalNet:C}}</td>
            </tr>
          </table>
          <p style="margin-top:40px; font-size:10px; color:#888;">
            Gerado em {{DateTime.Now:dd/MM/yyyy HH:mm}} – Documento válido sem assinatura.
          </p>
        </body>
        </html>
        """;

    private static byte[] HtmlToPdf(string html)
    {
        return System.Text.Encoding.UTF8.GetBytes(html);
    }
}