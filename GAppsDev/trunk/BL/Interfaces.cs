﻿using DA;
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
            const int EXTERNALID = 0;
            const int NAME = 1;
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
                    if (!(int.Parse(lineValues[EXTERNALID]) > 0))
                    {
                        errorType = Loc.Dic.error_invalid_form;
                        break;
                    }
                    if (lineValues[NAME] == null)
                    {
                        errorType = Loc.Dic.error_invalid_form;
                        break;
                    }
                    try
                    {
                        newSupplier = new Supplier()
                        {
                            CompanyId = companyId,
                            ExternalId = lineValues[EXTERNALID],
                            Name = lineValues[NAME],
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
                            supplier.Name = lineValues[NAME];
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
            const int EXTERNALID = 0;
            const int NAME = 1;
            const int AMOUNT = 2;
            const int JANUARY = 1;
            const int FEBRUARY = 2;
            const int MONTHESINYEAR = 12;

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
                    if (!(int.Parse(lineValues[EXTERNALID]) > 0))
                    {
                        errorType = Loc.Dic.error_invalid_form;
                        break;
                    }
                    if (lineValues[NAME] == null)
                    {
                        errorType = Loc.Dic.error_invalid_form;
                        break;
                    }
                    if (!(decimal.Parse(lineValues[AMOUNT]) >= 0))
                    {
                        errorType = Loc.Dic.error_invalid_form;
                        break;
                    }

                    Budget budget = budgetsRepository.GetList().SingleOrDefault(x => x.Id == budgetId);

                    Budgets_Allocations newAllocation;

                    if (lineValues[EXTERNALID].Length != 8 || lineValues[NAME].Length > 100)
                        return Loc.Dic.Error_FileParseError;

                    newAllocation = new Budgets_Allocations()
                    {
                        CompanyId = companyId,
                        BudgetId = budget.Id,
                        ExternalId = lineValues[EXTERNALID],
                        Name = lineValues[NAME],
                        CreationDate = DateTime.Now
                    };

                    if (allocationRep.GetList().SingleOrDefault(x => x.CompanyId == companyId && x.ExternalId == newAllocation.ExternalId && x.BudgetId == budgetId) == null)
                    {
                        allocationRep.Create(newAllocation);
                        tempAmountList.Add(newAllocation.Id, decimal.Parse(lineValues[AMOUNT]));
                    }
                    else
                    {
                        Budgets_Allocations existingAllocation = allocationRep.GetList().SingleOrDefault(x => x.CompanyId == companyId && x.ExternalId == newAllocation.ExternalId && x.BudgetId == budgetId);

                        existingAllocation.Name = newAllocation.Name;
                        allocationRep.Update(existingAllocation);
                        tempAmountList.Add(existingAllocation.Id, decimal.Parse(lineValues[AMOUNT]));
                    }
                }

                List<Budgets_AllocationToMonth> toAddAllocationMonthList = new List<Budgets_AllocationToMonth>();
                foreach (var item in tempAmountList)
                {

                    if (allocationMonthRepository.GetList().Where(x => x.AllocationId == item.Key).SingleOrDefault(x => x.MonthId == JANUARY) == null)
                    {
                        Budgets_AllocationToMonth toAddallocationMonth = new Budgets_AllocationToMonth();
                        toAddallocationMonth.AllocationId = item.Key;
                        toAddallocationMonth.MonthId = JANUARY;
                        toAddallocationMonth.Amount = item.Value;
                        toAddAllocationMonthList.Add(toAddallocationMonth);
                    }
                    else
                    {
                        Budgets_AllocationToMonth existingAllocationMonth = allocationMonthRepository.GetList().Where(x => x.AllocationId == item.Key).SingleOrDefault(x => x.MonthId == JANUARY);
                        existingAllocationMonth.Amount = item.Value;
                        allocationMonthRepository.Update(existingAllocationMonth);
                    }
                    for (int i = FEBRUARY; i <= MONTHESINYEAR; i++)
                    {
                        Budgets_AllocationToMonth toAddZeroallocationMonth = new Budgets_AllocationToMonth();
                        toAddZeroallocationMonth.AllocationId = item.Key;
                        toAddZeroallocationMonth.MonthId = i;
                        toAddZeroallocationMonth.Amount = 0;
                        toAddAllocationMonthList.Add(toAddZeroallocationMonth);
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
            Budget budget;
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

                    if (lineValues[1].Length != 8 || lineValues[2].Length > 100)
                        return Loc.Dic.Error_FileParseError;

                    newAllocation = new Budgets_Allocations()
                    {
                        ExternalId = lineValues[1],
                        Name = lineValues[2],
                        BudgetId = budget.Id,
                        CompanyId = companyId,
                        CreationDate = DateTime.Now,
                        IncomeId = null,
                        ExpenseId = null
                    };

                    Budgets_Allocations existingAllocation = allocationsRep.GetList().SingleOrDefault(x => x.CompanyId == companyId && x.ExternalId == newAllocation.ExternalId && x.BudgetId == budgetId);
                    bool allocationExists = existingAllocation != null;

                    if (!allocationExists)
                    {
                        if (!allocationsRep.Create(newAllocation))
                            return Loc.Dic.error_database_error;

                        createdAllocations.Add(newAllocation);
                    }
                    else
                    {
                        existingAllocation.Name = newAllocation.Name;
                        allocationsRep.Update(existingAllocation);
                    }

                    for (int month = 1, valueIndex = 3; month <= 12; month++, valueIndex += 2)
                    {
                        string monthAmountString = lineValues[valueIndex];
                        if (String.IsNullOrEmpty(monthAmountString))
                        {
                            monthAmountString = "0";
                        }

                        decimal amount;
                        if (!Decimal.TryParse(monthAmountString, out amount))
                        {
                            noErros = false;
                            errorType = Loc.Dic.Error_FileParseError;
                            break;
                        }

                        if (!allocationExists)
                        {
                            Budgets_AllocationToMonth newAllocationMonth = new Budgets_AllocationToMonth()
                            {
                                AllocationId = newAllocation.Id,
                                MonthId = month,
                                Amount = amount < 0 ? 0 : amount
                            };

                            if (!allocationMonthsRep.Create(newAllocationMonth))
                            {
                                noErros = false;
                                errorType = Loc.Dic.error_database_error;
                                break;
                            }

                            createdAllocationMonths.Add(newAllocationMonth);
                        }
                        else
                        {
                            Budgets_AllocationToMonth UpdaedMonth = new Budgets_AllocationToMonth();
                            Budgets_AllocationToMonth existingMonth = existingAllocation.Budgets_AllocationToMonth.SingleOrDefault(x => x.MonthId == month);

                            UpdaedMonth.Id = existingMonth.Id;
                            UpdaedMonth.AllocationId = existingMonth.AllocationId;
                            UpdaedMonth.MonthId = existingMonth.MonthId;
                            UpdaedMonth.Amount = amount;

                            if (allocationMonthsRep.Update(UpdaedMonth) == null)
                            {
                                noErros = false;
                                errorType = Loc.Dic.error_database_error;
                                break;
                            }
                        }
                    }
                }
            }

            if (!noErros)
                return errorType;

            return "OK";
        }
    }
}