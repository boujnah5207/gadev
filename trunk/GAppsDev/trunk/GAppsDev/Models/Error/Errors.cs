using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GAppsDev.Models.ErrorModels
{
    public static class Errors
    {
        // General Errors
        public const string NO_PERMISSION = "אין לך את ההרשאה המתאימה לביצוע פעולה זאת. אנא פנה למנהל המערכת.";
        public const string DATABASE_ERROR = "הייתה שגיאה בזמן הגישה לבסיס הנתונים. אנא נסה שנית.";
        public const string INVALID_FORM = "נמצאו שגיאות בטופס, לא כל השדות תקינים.";

        // Orders Errors
        public const string ORDERS_CREATE_ERROR = "קרתה שגיאה בזמן השמירה למסד הנתונים. ההזמנה לא נשמרה במערכת.";

        // Suppliers Errors
        public const string SUPPLIERS_GET_ERROR = "קרתה שגיאה בזמן הגישה לספקים בבסיס הנתונים. אנא נסה שוב.";
        public const string SUPPLIERS_CREATE_ERROR = "קרתה שגיאה בזמן השמירה למסד הנתונים. הספק לא נשמר במערכת.";

        //OrderItems Errors
        public const string ORDERITEMS_GET_ERROR = "קרתה שגיאה בזמן הגישה לפריטים להזמנה בבסיס הנתונים. אנא נסה שוב.";

    }
}