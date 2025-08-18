using LocalGov360.Data;
using LocalGov360.Data.Models;
using LocalGov360.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace LocalGov360.Services
{
    // ===== INTERFACE =====
    public interface IValuationUploadService
    {
        Task<ValuationUploadResult> UploadValuationRollAsync(IBrowserFile file, string councilName, int year, string uploadedBy);
        Task<List<PropertyImport>> PreviewDataAsync(IBrowserFile file);

        // Property Evaluation Methods
        Task<EvaluationResult> RunEvaluationAsync(int valuationRollId, string evaluatedBy);
        Task<bool> UpdatePropertyPoundageRateAsync(int propertyId, decimal rate);
    }
}

// ===== IMPLEMENTATION =====
public class ValuationUploadService : IValuationUploadService
{
    private readonly ApplicationDbContext _context;
    private const int MaxFileSize = 10 * 1024 * 1024; // 10MB
    private const int MinRequiredColumns = 10; // Minimum columns required (excluding remarks)

    public ValuationUploadService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
      
    }

    public async Task<ValuationUploadResult> UploadValuationRollAsync(IBrowserFile file, string councilName, int year, string uploadedBy)
    {
        var result = new ValuationUploadResult();

        try
        {
            // Validate inputs
            var validationError = ValidateUploadInputs(file, councilName, year);
            if (!string.IsNullOrEmpty(validationError))
            {
                result.Success = false;
                result.ErrorMessage = validationError;
                return result;
            }

            var extension = Path.GetExtension(file.Name).ToLower();
            List<PropertyImport> importData;

            if (extension == ".csv")
            {
                importData = await ReadCsvDataAsync(file);
            }
            else
            {
                importData = await ReadExcelDataAsync(file);
            }

            if (!importData.Any())
            {
                result.Success = false;
                result.ErrorMessage = "No data found in file";
                return result;
            }

            ValidateImportData(importData);
            var validData = importData.Where(d => d.IsValid).ToList();

            if (!validData.Any())
            {
                result.Success = false;
                result.ErrorMessage = "No valid data found";
                result.InvalidRows = importData.Where(d => !d.IsValid).ToList();
                return result;
            }

            // Check for duplicate property numbers within the file
            var duplicatePropertyNumbers = validData
                .GroupBy(p => p.PropertyNumber?.ToUpper())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatePropertyNumbers.Any())
            {
                result.Success = false;
                result.ErrorMessage = $"Duplicate property numbers found: {string.Join(", ", duplicatePropertyNumbers)}";
                return result;
            }

            var roll = new ValuationRoll
            {
                Council = councilName.Trim(),
                ValuationDate = DateTime.Now.Date,
                Year = year,
                RollNumber = GenerateRollNumber(year),
                CreatedDate = DateTime.UtcNow
            };

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.CouncilValuationRolls.Add(roll);
                await _context.SaveChangesAsync();

                var properties = await ConvertToPropertiesAsync(validData, roll.Id);
                _context.CouncilProperties.AddRange(properties);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                result.Success = true;
                result.ValuationRollId = roll.Id;
                result.TotalRowsProcessed = importData.Count;
                result.SuccessfulRows = validData.Count;
                result.InvalidRows = importData.Where(d => !d.IsValid).ToList();

                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Upload failed: {ex.Message}";
            return result;
        }
    }

    public async Task<List<PropertyImport>> PreviewDataAsync(IBrowserFile file)
    {
        try
        {
            if (file == null || file.Size == 0)
            {
                return new List<PropertyImport>
                    {
                        new PropertyImport
                        {
                            Errors = new List<string> { "No file provided or file is empty." },
                            RowNumber = 0
                        }
                    };
            }

            if (file.Size > MaxFileSize)
            {
                return new List<PropertyImport>
                    {
                        new PropertyImport
                        {
                            Errors = new List<string> { $"File size exceeds {MaxFileSize / (1024 * 1024)}MB limit." },
                            RowNumber = 0
                        }
                    };
            }

            var extension = Path.GetExtension(file.Name).ToLower();
            List<PropertyImport> data;

            if (extension == ".csv")
            {
                data = await ReadCsvDataAsync(file);
            }
            else if (extension == ".xlsx" || extension == ".xls")
            {
                data = await ReadExcelDataAsync(file);
            }
            else
            {
                return new List<PropertyImport>
                    {
                        new PropertyImport
                        {
                            Errors = new List<string> { "Unsupported file format. Please use CSV (.csv) or Excel (.xlsx, .xls) files." },
                            RowNumber = 0
                        }
                    };
            }

            ValidateImportData(data);
            return data;
        }
        catch (Exception ex)
        {
            return new List<PropertyImport>
                {
                    new PropertyImport
                    {
                        Errors = new List<string> { $"Error reading file: {ex.Message}" },
                        RowNumber = 0
                    }
                };
        }
    }

    private string? ValidateUploadInputs(IBrowserFile file, string councilName, int year)
    {
        if (file == null || file.Size == 0)
            return "No file uploaded";

        if (string.IsNullOrWhiteSpace(councilName))
            return "Council name is required";

        if (year < 2000 || year > 2100)
            return "Please enter a valid year";

        var extension = Path.GetExtension(file.Name).ToLower();
        if (extension != ".csv" && extension != ".xlsx" && extension != ".xls")
            return "Only CSV and Excel files (.csv, .xlsx, .xls) are supported";

        if (file.Size > MaxFileSize)
            return $"File size cannot exceed {MaxFileSize / (1024 * 1024)}MB";

        return null;
    }

    private string GenerateRollNumber(int year)
    {
        return $"ROLL_{year}_{DateTime.Now:MMdd}_{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    }

    private async Task<List<PropertyImport>> ReadExcelDataAsync(IBrowserFile file)
    {
        var data = new List<PropertyImport>();

        using var stream = file.OpenReadStream(maxAllowedSize: MaxFileSize);
        using var package = new ExcelPackage(stream);

        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
        if (worksheet == null)
        {
            throw new InvalidOperationException("No worksheet found in the Excel file");
        }

        if (worksheet.Dimension == null)
        {
            return data; // Empty worksheet
        }

        var rowCount = worksheet.Dimension.Rows;
        var colCount = worksheet.Dimension.Columns;

        if (rowCount < 2) // At least header + 1 data row
        {
            throw new InvalidOperationException("Excel file must contain at least a header row and one data row");
        }

        if (colCount < MinRequiredColumns)
        {
            throw new InvalidOperationException($"Excel file must contain at least {MinRequiredColumns} columns");
        }

        // Start from row 2 (skip header)
        for (int row = 2; row <= rowCount; row++)
        {
            // Check if the row has any data in the first few critical columns
            if (IsEmptyRow(worksheet, row, Math.Min(colCount, 6)))
                continue;

            var item = new PropertyImport
            {
                RowNumber = row,
                PropertyNumber = GetExcelValue(worksheet, row, 1),
                Description = GetExcelValue(worksheet, row, 2),
                StreetAddress = GetExcelValue(worksheet, row, 3),
                Location = GetExcelValue(worksheet, row, 4),
                Leaseholder = GetExcelValue(worksheet, row, 5),
                Use = GetExcelValue(worksheet, row, 6),
                LandExtHa = GetExcelValue(worksheet, row, 7),
                LandValueK = GetExcelValue(worksheet, row, 8),
                ValueOfImprovementsK = GetExcelValue(worksheet, row, 9),
                TotalRateableValueK = GetExcelValue(worksheet, row, 10),
                Remarks = colCount >= 11 ? GetExcelValue(worksheet, row, 11) : null
            };

            data.Add(item);
        }

        return data;
    }

    private bool IsEmptyRow(ExcelWorksheet worksheet, int row, int checkColumns)
    {
        for (int col = 1; col <= checkColumns; col++)
        {
            if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, col].Text))
            {
                return false;
            }
        }
        return true;
    }

    private string? GetExcelValue(ExcelWorksheet worksheet, int row, int col)
    {
        try
        {
            var cell = worksheet.Cells[row, col];
            var value = cell.Text?.Trim();

            // Handle null, empty, or dash values
            if (string.IsNullOrEmpty(value) || value == "-" || value.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return value;
        }
        catch
        {
            return null;
        }
    }

    private async Task<List<PropertyImport>> ReadCsvDataAsync(IBrowserFile file)
    {
        var data = new List<PropertyImport>();

        using var stream = file.OpenReadStream(maxAllowedSize: MaxFileSize);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        string? headerLine = await reader.ReadLineAsync();
        if (headerLine == null)
        {
            throw new InvalidOperationException("CSV file is empty or cannot be read");
        }

        // Check if header has minimum required columns
        var headerColumns = ParseCsvLine(headerLine);
        if (headerColumns.Length < MinRequiredColumns)
        {
            throw new InvalidOperationException($"CSV file must contain at least {MinRequiredColumns} columns");
        }

        int rowNumber = 1;
        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            rowNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = ParseCsvLine(line);

            // Skip rows that don't have enough columns or are completely empty
            if (values.Length < MinRequiredColumns || values.All(string.IsNullOrWhiteSpace))
                continue;

            var item = new PropertyImport
            {
                RowNumber = rowNumber,
                PropertyNumber = GetValue(values, 0),
                Description = GetValue(values, 1),
                StreetAddress = GetValue(values, 2),
                Location = GetValue(values, 3),
                Leaseholder = GetValue(values, 4),
                Use = GetValue(values, 5),
                LandExtHa = GetValue(values, 6),
                LandValueK = GetValue(values, 7),
                ValueOfImprovementsK = GetValue(values, 8),
                TotalRateableValueK = GetValue(values, 9),
                Remarks = values.Length > 10 ? GetValue(values, 10) : null
            };
            data.Add(item);
        }

        return data;
    }

    private string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        bool inQuotes = false;
        var currentValue = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                // Handle escaped quotes (double quotes)
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentValue.Append('"');
                    i++; // Skip the next quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue.ToString().Trim());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(c);
            }
        }

        values.Add(currentValue.ToString().Trim());
        return values.ToArray();
    }

    private string? GetValue(string[] values, int index)
    {
        if (index < values.Length)
        {
            var value = values[index].Trim();
            return string.IsNullOrEmpty(value) || value == "-" || value.Equals("null", StringComparison.OrdinalIgnoreCase)
                ? null
                : value;
        }
        return null;
    }

    private void ValidateImportData(List<PropertyImport> data)
    {
        var propertyNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in data)
        {
            // Required field validations
            if (string.IsNullOrWhiteSpace(item.PropertyNumber))
                item.Errors.Add("Property Number is required");
            else if (propertyNumbers.Contains(item.PropertyNumber))
                item.Errors.Add($"Duplicate Property Number: {item.PropertyNumber}");
            else
                propertyNumbers.Add(item.PropertyNumber);

            if (string.IsNullOrWhiteSpace(item.Description))
                item.Errors.Add("Description is required");

            if (string.IsNullOrWhiteSpace(item.Use))
                item.Errors.Add("Use is required");

            if (string.IsNullOrWhiteSpace(item.TotalRateableValueK))
                item.Errors.Add("Total Rateable Value is required");

            // Numeric field validations
            if (!string.IsNullOrWhiteSpace(item.LandExtHa) && !IsValidDecimal(item.LandExtHa))
                item.Errors.Add("Land Ext (HA) must be a valid number");

            if (!string.IsNullOrWhiteSpace(item.LandValueK) && !IsValidDecimal(item.LandValueK))
                item.Errors.Add("Land Value must be a valid number");

            if (!string.IsNullOrWhiteSpace(item.ValueOfImprovementsK) && !IsValidDecimal(item.ValueOfImprovementsK))
                item.Errors.Add("Value of Improvements must be a valid number");

            if (!string.IsNullOrWhiteSpace(item.TotalRateableValueK) && !IsValidDecimal(item.TotalRateableValueK))
                item.Errors.Add("Total Rateable Value must be a valid number");

            // Use type validation
            if (!string.IsNullOrWhiteSpace(item.Use))
            {
                var validUseTypes = PropertyUseTypes.GetAll();
                if (!validUseTypes.Contains(item.Use.ToUpper()))
                    item.Errors.Add($"Use type '{item.Use}' is not valid. Valid types: {string.Join(", ", validUseTypes)}");
            }

            // Business rule validations
            if (!string.IsNullOrWhiteSpace(item.TotalRateableValueK) && IsValidDecimal(item.TotalRateableValueK))
            {
                var totalValue = ParseDecimal(item.TotalRateableValueK);
                if (totalValue <= 0)
                    item.Errors.Add("Total Rateable Value must be greater than zero");
            }

            // Property number format validation (optional - adjust based on your requirements)
            if (!string.IsNullOrWhiteSpace(item.PropertyNumber) && item.PropertyNumber.Length > 50)
                item.Errors.Add("Property Number cannot exceed 50 characters");

            // Description length validation
            if (!string.IsNullOrWhiteSpace(item.Description) && item.Description.Length > 500)
                item.Errors.Add("Description cannot exceed 500 characters");
        }
    }

    private bool IsValidDecimal(string value)
    {
        return ParseDecimal(value).HasValue;
    }

    private async Task<List<Property>> ConvertToPropertiesAsync(List<PropertyImport> data, int rollId)
    {
        var propertyTypes = await _context.CouncilPropertyTypes
            .Where(pt => pt.IsActive)
            .ToListAsync();

        if (!propertyTypes.Any())
        {
            throw new InvalidOperationException("No active property types found in the system. Please ensure property types are configured.");
        }

        var properties = new List<Property>();

        foreach (var item in data)
        {
            var propertyType = propertyTypes.FirstOrDefault(pt =>
                pt.ShortName.Equals(item.Use?.ToUpper(), StringComparison.OrdinalIgnoreCase));

            if (propertyType == null)
            {
                // Fall back to the first available property type
                propertyType = propertyTypes.First();
            }

            var property = new Property
            {
                ValuationRollId = rollId,
                PropertyTypeId = propertyType.Id,
                PropertyNumber = item.PropertyNumber!.Trim(),
                Description = item.Description!.Trim(),
                StreetAddress = item.StreetAddress?.Trim(),
                Location = item.Location?.Trim(),
                Leaseholder = item.Leaseholder?.Trim(),
                Use = item.Use!.ToUpper().Trim(),
                LandExtHa = ParseDecimal(item.LandExtHa),
                LandValue = ParseDecimal(item.LandValueK),
                ValueOfImprovements = ParseDecimal(item.ValueOfImprovementsK),
                TotalRateableValue = ParseDecimal(item.TotalRateableValueK) ?? 0,
                Remarks = item.Remarks?.Trim(),
                CreatedDate = DateTime.UtcNow
            };

            properties.Add(property);
        }

        return properties;
    }

    private decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        // Clean the value
        value = value.Replace(",", "")
                     .Replace(" ", "")
                     .Replace("ZMW", "", StringComparison.OrdinalIgnoreCase)
                     .Replace("K", "", StringComparison.OrdinalIgnoreCase)
                     .Trim();

        // Handle negative values
        if (value.StartsWith("(") && value.EndsWith(")"))
        {
            value = "-" + value.Substring(1, value.Length - 2);
        }

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    // ===== PROPERTY EVALUATION METHODS =====

    public async Task<EvaluationResult> RunEvaluationAsync(int valuationRollId, string evaluatedBy)
    {
        var result = new EvaluationResult();

        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get the valuation roll
                var roll = await _context.CouncilValuationRolls
                    .FirstOrDefaultAsync(r => r.Id == valuationRollId);

                if (roll == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "Valuation roll not found";
                    return result;
                }

                // Get all properties for this roll
                var properties = await _context.CouncilProperties
                    .Include(p => p.PropertyType)
                    .Where(p => p.ValuationRollId == valuationRollId)
                    .ToListAsync();

                if (!properties.Any())
                {
                    result.Success = false;
                    result.ErrorMessage = "No properties found for this valuation roll";
                    return result;
                }

                // Get current poundage rates
                var currentDate = DateTime.UtcNow.Date;
                var poundageRates = await _context.CouncilPoundageRates
                    .Where(r => r.EffectiveFrom <= currentDate &&
                               (r.EffectiveTo == null || r.EffectiveTo >= currentDate) &&
                               r.IsCurrent)
                    .ToListAsync();

                int updatedCount = 0;
                var errors = new List<string>();

                foreach (var property in properties)
                {
                    try
                    {
                        // Find the appropriate poundage rate
                        var rate = poundageRates.FirstOrDefault(r => r.PropertyTypeId == property.PropertyTypeId);

                        if (rate == null)
                        {
                            // Try to get rate from PropertyUseTypes as fallback
                            if (PropertyUseTypes.ApprovedPoundageRates.TryGetValue(property.Use, out var fallbackRate))
                            {
                                property.PoundageRate = fallbackRate;
                            }
                            else
                            {
                                errors.Add($"No poundage rate found for property {property.PropertyNumber} (Type: {property.Use})");
                                continue;
                            }
                        }
                        else
                        {
                            property.PoundageRate = rate.Rate;
                        }

                        // Calculate poundage (annual amount)
                        property.Poundage = property.TotalRateableValue * property.PoundageRate.Value;

                        // Calculate evaluation amount (bi-annual amount = poundage / 2)
                        property.EvaluationAmount = property.Poundage / 2;

                        updatedCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error processing property {property.PropertyNumber}: {ex.Message}");
                    }
                }

                // Save all changes
                await _context.SaveChangesAsync();

                // Create evaluation records for the current period
                await CreateEvaluationRecordsAsync(properties.Where(p => p.PoundageRate.HasValue).ToList(), roll.Year, evaluatedBy);

                await transaction.CommitAsync();

                result.Success = true;
                result.Message = $"Evaluation completed successfully. Updated {updatedCount} of {properties.Count} properties.";
                result.UpdatedPropertiesCount = updatedCount;
                result.TotalPropertiesCount = properties.Count;
                result.Errors = errors;

                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Evaluation failed: {ex.Message}";
            return result;
        }
    }

    public async Task<bool> UpdatePropertyPoundageRateAsync(int propertyId, decimal rate)
    {
        try
        {
            var property = await _context.CouncilProperties
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null)
                return false;

            property.PoundageRate = rate;
            property.Poundage = property.TotalRateableValue * rate;
            property.EvaluationAmount = property.Poundage / 2;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<PropertyEvaluation>> GetPropertyEvaluationsAsync(int propertyId)
    {
        return await _context.CouncilPropertyEvaluations
            .Where(e => e.PropertyId == propertyId)
            .OrderByDescending(e => e.Year)
            .ThenByDescending(e => e.Period)
            .ToListAsync();
    }

    public async Task<PropertyEvaluation?> GetCurrentEvaluationAsync(int propertyId, int year, int period)
    {
        return await _context.CouncilPropertyEvaluations
            .FirstOrDefaultAsync(e => e.PropertyId == propertyId &&
                                     e.Year == year &&
                                     e.Period == period);
    }

    public async Task<bool> UpdateEvaluationPaymentAsync(int evaluationId, decimal amountPaid, DateTime paymentDate, string? invoiceNumber = null)
    {
        try
        {
            var evaluation = await _context.CouncilPropertyEvaluations
                .FirstOrDefaultAsync(e => e.Id == evaluationId);

            if (evaluation == null)
                return false;

            evaluation.AmountPaid = amountPaid;
            evaluation.PaymentDate = paymentDate;
            evaluation.Balance = evaluation.EvaluationAmount - amountPaid;
            evaluation.IsPaid = evaluation.Balance <= 0;
            evaluation.InvoiceNumber = invoiceNumber;
            evaluation.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ===== PRIVATE HELPER METHODS =====

    private async Task CreateEvaluationRecordsAsync(List<Property> properties, int year, string createdBy)
    {
        var currentPeriod = DateTime.Now.Month <= 6 ? 1 : 2;
        var existingEvaluations = await _context.CouncilPropertyEvaluations
            .Where(e => properties.Select(p => p.Id).Contains(e.PropertyId) &&
                       e.Year == year &&
                       e.Period == currentPeriod)
            .Select(e => e.PropertyId)
            .ToListAsync();

        var newEvaluations = new List<PropertyEvaluation>();

        foreach (var property in properties)
        {
            // Skip if evaluation already exists for this period
            if (existingEvaluations.Contains(property.Id))
                continue;

            if (!property.PoundageRate.HasValue || !property.EvaluationAmount.HasValue)
                continue;

            var evaluation = new PropertyEvaluation
            {
                PropertyId = property.Id,
                ValuationRollId = property.ValuationRollId,
                Year = year,
                Period = currentPeriod,
                RateableValue = property.TotalRateableValue,
                PoundageRate = property.PoundageRate.Value,
                EvaluationAmount = property.EvaluationAmount.Value,
                AmountPaid = 0,
                Balance = property.EvaluationAmount.Value,
                IsPaid = false,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow
            };

            newEvaluations.Add(evaluation);
        }

        if (newEvaluations.Any())
        {
            _context.CouncilPropertyEvaluations.AddRange(newEvaluations);
            await _context.SaveChangesAsync();
        }
    }
}
