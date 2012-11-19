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
        public static bool ImportSuppliers(Stream stream)
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
                        errorType = Loc.Dic.error_in INVALID_FORM;
                        break;
                    }
                    if (int.Parse(lineValues[SECOND_COLOUMN]) == null)
                    {
                        errorType = Errors.INVALID_FORM;
                        break;
                    }
                    try
                    {
                        newSupplier = new Supplier()
                        {
                            CompanyId = CurrentUser.CompanyId,
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
                    toAddSuppliers.Add(newSupplier);
                }
                if (!suppliersRep.AddList(toAddSuppliers))
                {
                    noErros = false;
                    errorType = Errors.DATABASE_ERROR;
                }
            }
            if (!noErros) return Error(Errors.BUDGETS_CREATE_ERROR);
        }
    }
}
