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
        public const string ORDERS_GET_ERROR = "קרתה שגיאה בזמן משיכת ההזמנות מבסיס הנתונים. אנא נסה שוב.";
        public const string ORDER_HAS_NO_ITEMS = "אין אפשרות ליצור הזמנה ללא פריטים. אנא הוסף פריט ונסה שוב.";
        public const string ORDERS_DELETE_ITEMS_ERROR = "קרתה שגיאה בזמן הגישה למסד הנתונים. לא הייתה אפשרות למחוק פריטים מההזמנה.";
        public const string ORDERS_DELETE_ERROR = "קרתה שגיאה בזמן הגישה למסד הנתונים. לא הייתה אפשרות למחוק את ההזמנה.";
        public const string ORDER_NOT_FOUND = "לא הייתה אפשרות למצוא הזמנה בבסיס הנתונים.";
        public const string ORDER_UPDATE_ITEMS_ERROR = "קרתה שגיאה בזמן השמירה לבסיס הנתונים, לא כל פריטי ההזמנה עודכנו.";
        public const string ORDER_SAVE_ITEMS_ERROR = "קרתה שגיאה בזמן השמירה לבסיס הנתונים, לא כל פריטי ההזמנה נשמרו.";
        public const string ORDER_EDIT_AFTER_APPROVAL = "אין לערוך הזמנה לאחר אישורה\\ביטולה, אנא צור הזמנה חדשה במקום.";
        public const string ORDER_DELETE_AFTER_APPROVAL = "אין למחוק הזמנה לאחר אישורה\\ביטולה.";
        public const string ORDER_NOT_APPROVED = "ההזמנה עדיין לא אושרה. אנא המתן עד לאישור ההזמנה.";
        public const string ORDER_ALREADY_HAS_INVOICE = "כבר נסרקה חשבונית להזמנה זאת. ";


        // Suppliers Errors
        public const string SUPPLIERS_GET_ERROR = "קרתה שגיאה בזמן הגישה לספקים בבסיס הנתונים. אנא נסה שוב.";
        public const string SUPPLIERS_CREATE_ERROR = "קרתה שגיאה בזמן השמירה למסד הנתונים. הספק לא נשמר במערכת.";

        //OrderItems Errors
        public const string ORDERITEMS_GET_ERROR = "קרתה שגיאה בזמן הגישה לפריטים להזמנה בבסיס הנתונים. אנא נסה שוב.";

        //Users Errors
        public const string USERS_CREATE_ERROR = "קרתה שגיאה בזמן השמירה למסד הנתונים. המשתמש לא נשמר במערכת.";
        public const string USERS_EXIST_ERROR = "משתמש עם אימייל זהה כבר רשום במערכת.";
        public const string USERS_GET_ERROR = "קרתה שגיאה בזמן משיכת המשתמשים מבסיס הנתונים. אנא נסה שוב.";
        public const string USERS_LIMIT_REACHED = "חברתך הגיעה לגבול מספר המשתמשים שלה. אין אפשרות ליצור משתמשים חדשים.";
        public const string USERS_DELETE_ERROR = "לא הייתה אפשרות להסיר את המשתמש. אנא נסה שוב.";
        public const string USER_CANNOT_DELETE_SELF = "למשתמש אין אפשרות להסיר את עצמו.";
        public const string USER_NOT_FOUND = "לא הייתה אפשרות למצוא משתמש בבסיס הנתונים.";
        public const string USER_WAS_CANCELED = "חשבון משתשתמש זה בוטל. אנא פנה למנהל המערכת.";
        public const string USER_NOT_REGISTERD = "חשבון משתשתמש זה אינו רשום במערכת.";

        //Inventory Errors
        public const string INVENTORY_CREATE_ERROR = "קרתה שגיאה בזמן השמירה למסד הנתונים. הפריטים לא הוספו למלאי.";

        //Budgets Errors
        public const string BUDGETS_CREATE_ERROR = "קרתה שגיאה בזמן השמירה למסד הנתונים. התקציב לא נשמר במערכת.";
        public const string BUDGETS_YEAR_PASSED = "לא ניתן ליצור תקציב לשנים קודמות.";
        public const string BUDGETS_YEAR_EXISTS = "נוצר כבר תקציב לשנה זאת.";

        //Incomes Errors
        public const string INCOME_CREATE_ERROR = "קרתה שגיאה בזמן השמירה למסד הנתונים. ההכנסה לא נשמרה במערכת.";
        public const string INCOME_GET_ERROR = "קרתה שגיאה בזמן הגישה להכנסות בבסיס הנתונים. אנא נסה שוב.";


    }
}