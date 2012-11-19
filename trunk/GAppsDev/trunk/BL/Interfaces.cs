using DA;
using DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BL
{
    public static class Interfaces
    {
        public static string ImportSuppliers(Stream stream, int companyId)
        {
            const int FIRST_COLOUMN = 0;
            const int SECOND_COLOUMN = 1;
            List<Supplier> toAddSuppliers = new List<Supplier>();
            byte[] fileBytes = new byte[stream.Length];
            stream.Read(fileBytes, 0, Convert.ToInt32(stream.Length));
            string fileContent = System.Text.Encoding.Default.GetString(fileBytes);

            string[] fileLines = fileContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            int firstValuesLine = 0;
            bool noErros = true;
            string errorType = String.Empty;
            using (SuppliersRepository suppliersRep = new SuppliersRepository())
            {
                for (int i = firstValuesLine; i < fileLines.Length; i++)
                {
                    string[] lineValues = fileLines[i].Split('\t');
                    for (int vIndex = 0; vIndex < lineValues.Length; vIndex++)
                    {
                        lineValues[vIndex] = lineValues[vIndex].Replace("\"", "");
                    }

                    Supplier newSupplier;
                    if (!(int.Parse(lineValues[FIRST_COLOUMN]) > 0))
                    {
                        errorType = Loc.Dic.error_invalid_form;
                        break;
                    }
                    if (lineValues[SECOND_COLOUMN] == null)
                    {
                        errorType = Loc.Dic.error_invalid_form;
                        break;
                    }
                    try
                    {
                        newSupplier = new Supplier()
                        {
                            CompanyId = companyId,
                            ExternalId = lineValues[FIRST_COLOUMN],
                            Name = lineValues[SECOND_COLOUMN],
                        };
                    }
                    catch
                    {
                        noErros = false;
                        errorType = Loc.Dic.Error_FileParseError;
                        break;
                    }
                    List<Supplier> existingSuppliers = suppliersRep.GetList().Where(x=>x.CompanyId == companyId && x.ExternalId == newSupplier.ExternalId).ToList();
                    if (existingSuppliers.Count == 0) toAddSuppliers.Add(newSupplier);
                    else
                    {
                        foreach (Supplier supplier in existingSuppliers)
                        {
                            supplier.Name = lineValues[SECOND_COLOUMN];
                            suppliersRep.Update(supplier);
                        }
                    }
                }
                if (!suppliersRep.AddList(toAddSuppliers))
                {
                    noErros = false;
                    errorType = Loc.Dic.error_database_error;
                }
            }
            if (!noErros) return errorType;
            return "OK";
        }
        public static string ImportYearBudget(Stream stream, int companyId, int budgetYear)
        {
            const int FIRST_COLOUMN = 0;
            const int SECOND_COLOUMN = 1;
            const int THIRD_COLOUMN = 2;
            List<Budgets_Allocations> toAddAllocations = new List<Budgets_Allocations>();
            byte[] fileBytes = new byte[stream.Length];
            stream.Read(fileBytes, 0, Convert.ToInt32(stream.Length));
            string fileContent = System.Text.Encoding.Default.GetString(fileBytes);

            string[] fileLines = fileContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            int firstValuesLine = 0;
            bool noErros = true;
            string errorType = String.Empty;

            using (BudgetsRepository budgetsRepository = new BudgetsRepository())
            using (AllocationRepository allocationRep = new AllocationRepository())
            {
                for (int i = firstValuesLine; i < fileLines.Length; i++)
                {
                    string[] lineValues = fileLines[i].Split('\t');
                    for (int vIndex = 0; vIndex < lineValues.Length; vIndex++)
                    {
                        lineValues[vIndex] = lineValues[vIndex].Replace("\"", "");
                    }
                    if (!(int.Parse(lineValues[FIRST_COLOUMN]) > 0))
                    {
                        errorType = Loc.Dic.error_invalid_form;
                        break;
                    }
                    if (lineValues[SECOND_COLOUMN] == null)
                    {
                        errorType = Loc.Dic.error_invalid_form;
                        break;
                    }
                    if (!(decimal.Parse(lineValues[THIRD_COLOUMN]) >= 0))
                    {
                        errorType = Loc.Dic.error_invalid_form;
                        break;
                    }

                    Budget budget = budgetsRepository.GetList().SingleOrDefault(x => x.Year == budgetYear);
                    Budgets_Allocations newAllocation;
                    try
                    {
                        newAllocation = new Budgets_Allocations()
                        {
                            CompanyId = companyId,
                            BudgetId = budget.Id,
                            ExternalId = lineValues[FIRST_COLOUMN],
                            Name = lineValues[SECOND_COLOUMN],
                            Amount = decimal.Parse(lineValues[THIRD_COLOUMN]),
                        };
                    }
                    catch
                    {
                        noErros = false;
                        errorType = Loc.Dic.Error_FileParseError;
                        break;
                    }
                    List<Budgets_Allocations> existingAllocations = allocationRep.GetList().Where(x => x.CompanyId == companyId && x.ExternalId == newAllocation.ExternalId).ToList();
                    if (existingAllocations.Count == 0) toAddAllocations.Add(newAllocation);
                    else
                    {
                        foreach (Budgets_Allocations allocation in existingAllocations)
                        {
                            allocation.Name = lineValues[SECOND_COLOUMN];
                            allocation.BudgetId = budget.Id;
                            allocation.Amount = decimal.Parse(lineValues[THIRD_COLOUMN]);
                            allocationRep.Update(allocation);
                        }
                    }
                }
                if (!allocationRep.AddList(toAddAllocations))
                {
                    noErros = false;
                    errorType = Loc.Dic.error_database_error;
                }
            }
            if (!noErros) return errorType;
            return "OK";
        }
    }
}