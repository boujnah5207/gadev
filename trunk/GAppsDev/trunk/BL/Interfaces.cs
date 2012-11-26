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
                    List<Supplier> existingSuppliers = suppliersRep.GetList().Where(x => x.CompanyId == companyId && x.ExternalId == newSupplier.ExternalId).ToList();
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
        public static string ImportYearBudget(Stream stream, int companyId, int budgetId)
        {
            const int FIRST_COLOUMN = 0;
            const int SECOND_COLOUMN = 1;
            const int THIRD_COLOUMN = 2;
            const int JANUARY = 1;

            List<Budgets_Allocations> toAddAllocations = new List<Budgets_Allocations>();
            Dictionary<int, decimal> tempAmountList = new Dictionary<int, decimal>();

            byte[] fileBytes = new byte[stream.Length];
            stream.Read(fileBytes, 0, Convert.ToInt32(stream.Length));
            string fileContent = System.Text.Encoding.Default.GetString(fileBytes);
            string[] fileLines = fileContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            int firstValuesLine = 0;
            bool noErros = true;
            string errorType = String.Empty;

            using (AllocationMonthsRepository allocationMonthRepository = new AllocationMonthsRepository())
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



                    Budget budget = budgetsRepository.GetList().SingleOrDefault(x => x.Id == budgetId);

                    Budgets_Allocations newAllocation;
                    try
                    {
                        newAllocation = new Budgets_Allocations()
                        {
                            CompanyId = companyId,
                            BudgetId = budget.Id,
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

                    if (allocationRep.GetList().SingleOrDefault(x => x.CompanyId == companyId && x.ExternalId == newAllocation.ExternalId) == null) toAddAllocations.Add(newAllocation);
                    else
                    {
                        Budgets_Allocations existingAllocation = allocationRep.GetList().SingleOrDefault(x => x.CompanyId == companyId && x.ExternalId == newAllocation.ExternalId);
                        existingAllocation.Name = newAllocation.Name;
                        allocationRep.Update(existingAllocation);
                    }
                    tempAmountList.Add(int.Parse(newAllocation.ExternalId), decimal.Parse(lineValues[THIRD_COLOUMN]));
                }
                if (!allocationRep.AddList(toAddAllocations))
                {
                    noErros = false;
                    errorType = Loc.Dic.error_database_error;
                }

                Budgets_AllocationToMonth toAddallocationMonth = new Budgets_AllocationToMonth();
                List<Budgets_AllocationToMonth> toAddAllocationMonthList = new List<Budgets_AllocationToMonth>();
                foreach (var item in tempAmountList)
                {
                    string externalIdstring = item.Key.ToString();
                    Budgets_Allocations allocationFromDb = allocationRep.GetList().SingleOrDefault(x => x.ExternalId == externalIdstring);
                    toAddallocationMonth.AllocationId = allocationFromDb.Id;
                    toAddallocationMonth.MonthId = JANUARY;
                    toAddallocationMonth.Amount = item.Value;
                    if (allocationMonthRepository.GetList().SingleOrDefault(x => x.AllocationId == allocationFromDb.Id) == null)
                        toAddAllocationMonthList.Add(toAddallocationMonth);
                    else
                    {
                        Budgets_AllocationToMonth existingAllocationMonth = allocationMonthRepository.GetList().SingleOrDefault(x => x.AllocationId == allocationFromDb.Id);
                        existingAllocationMonth.MonthId = toAddallocationMonth.MonthId;
                        existingAllocationMonth.Amount = toAddallocationMonth.Amount;
                        allocationMonthRepository.Update(existingAllocationMonth);
                    }
                }
                allocationMonthRepository.AddList(toAddAllocationMonthList);
            }
            if (!noErros) return errorType;
            return "OK";
        }
        public static string ImportMonthBudget(Stream stream, int companyId, int budgetId)
        {
            string errorType = String.Empty;
            Budget budget = new Budget();
            using (BudgetsRepository budgetsRepository = new BudgetsRepository())
            {
                budget = budgetsRepository.GetList().SingleOrDefault(x => x.Id == budgetId);
            }

            if (budget.Year < DateTime.Now.Year - 1)
                return errorType = Loc.Dic.error_budgets_year_passed;

            List<Budgets_Allocations> createdAllocations = new List<Budgets_Allocations>();
            List<Budgets_AllocationToMonth> createdAllocationMonths = new List<Budgets_AllocationToMonth>();

            byte[] fileBytes = new byte[stream.Length];
            stream.Read(fileBytes, 0, Convert.ToInt32(stream.Length));
            string fileContent = System.Text.Encoding.Default.GetString(fileBytes);

            string[] fileLines = fileContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            int firstValuesLine = 3;

            bool noErros = true;

            using (ExpensesToIncomeRepository allocationsRep = new ExpensesToIncomeRepository())
            using (AllocationMonthsRepository allocationMonthsRep = new AllocationMonthsRepository())
            {
                for (int i = firstValuesLine; i < fileLines.Length; i++)
                {
                    string[] lineValues = fileLines[i].Split('\t');
                    for (int vIndex = 0; vIndex < lineValues.Length; vIndex++)
                    {
                        lineValues[vIndex] = lineValues[vIndex].Replace("\"", "");
                    }

                    Budgets_Allocations newAllocation;

                    try
                    {
                        newAllocation = new Budgets_Allocations()
                        {
                            Name = lineValues[2],
                            BudgetId = budget.Id,
                            CompanyId = companyId,
                            IncomeId = null,
                            ExpenseId = null,
                            Amount = null
                        };
                    }
                    catch
                    {
                        noErros = false;
                        errorType = Loc.Dic.Error_FileParseError;
                        break;
                    }

                    if (!allocationsRep.Create(newAllocation))
                        return Loc.Dic.error_database_error;

                    createdAllocations.Add(newAllocation);

                    for (int month = 1, valueIndex = 3; month <= 12; month++, valueIndex += 2)
                    {
                        string monthAmountString = lineValues[valueIndex];
                        if (String.IsNullOrEmpty(monthAmountString))
                        {
                            noErros = false;
                            break;
                        }

                        decimal amount;
                        if (!Decimal.TryParse(monthAmountString, out amount))
                        {
                            noErros = false;
                            break;
                        }

                        Budgets_AllocationToMonth newAllocationMonth = new Budgets_AllocationToMonth()
                        {
                            AllocationId = newAllocation.Id,
                            MonthId = month,
                            Amount = amount < 0 ? 0 : amount
                        };

                        if (!allocationMonthsRep.Create(newAllocationMonth))
                        {
                            noErros = false;
                            break;
                        }

                        createdAllocationMonths.Add(newAllocationMonth);
                    }
                }
            }
            if (!noErros) return errorType;
            return "OK";
        }
    }
}