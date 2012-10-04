using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GAppsDev.Models.ErrorModels
{
    public class ErrorModel
    {
        public string ErrorMessage { get; set; }

        public ErrorModel(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}