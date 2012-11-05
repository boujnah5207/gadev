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
        public const string ORDER_GET_ERROR = "קרתה שגיאה בזמן משיכת ההזמנה מבסיס הנתונים. אנא נסה שוב.";
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
        public const string ORDER_INSUFFICIENT_ALLOCATION = "סכום ההזמנה עולה על הסכום שנותר להקצאה שנבחרה.";

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
        public const string USER_ALREADY_HAS_PERMISSIONS = "למשתמש כבר יש  את ההרשאה שצויינה.";
        public const string USER_EDIT_PERMISSIONS_ERROR = "היו שגיאות בזמן השמירה לבסיס הנתונים. לא כל ההרשאות נשמרו.";

        //Inventory Errors
        public const string INVENTORY_CREATE_ERROR = "קרתה שגיאה בזמן השמירה למסד הנתונים. הפריטים לא הוספו למלאי.";

        //Budgets Errors
        public const string BUDGETS_CREATE_ERROR = "קרתה שגיאה בזמן השמירה למסד הנתונים. התקציב לא נשמר במערכת.";
        public const string BUDGETS_GET_ERROR = "קרתה שגיאה בזמן הגישה לתקציבים בבסיס הנתונים. אנא נסה שוב.";
        public const string BUDGETS_YEAR_PASSED = "לא ניתן ליצור או לערוך תקציב לשנים קודמות.";
        public const string BUDGETS_YEAR_EXISTS = "נוצר כבר תקציב לשנה זאת.";
        public const string BUDGETS_ALREADY_ACTIVE = "תקציב זה כבר פעיל.";

        //Incomes Errors
        public const string INCOME_CREATE_ERROR = "קרתה שגיאה בזמן השמירה למסד הנתונים. ההכנסה לא נשמרה במערכת.";
        public const string INCOME_GET_ERROR = "קרתה שגיאה בזמן הגישה להכנסות בבסיס הנתונים. אנא נסה שוב.";
        public const string INCOME_ALLOCATIONS_EXEEDS_AMOUNT = "להכנסה זאת כבר הוקצה להוצאה סכום העולה על סך ההכנסה החדש. ההכנסה לא עודכנה. אנא ערוך את ההקצאות בהתאם ונסה שוב.";
        public const string INCOME_FULL_ALLOCATION = "לא נותר מספיק סכום להכנסה שצויינה על מנת להקצות את הסכום שבוקש.";
        public const string INCOME_DELETE_HAS_APPROVED_ORDERS = "להכנסה זאת קיימות הזמנות מאושרות. לא הייתה אפשרות לבטל את ההכנסה.";

        //Expenses Errors
        public const string EXPENSES_CREATE_ERROR = "קרתה שגיאה בזמן השמירה למסד הנתונים. ההוצאה לא נשמרה במערכת.";
        public const string EXPENSES_GET_ERROR = "קרתה שגיאה בזמן הגישה להוצאות בבסיס הנתונים. אנא נסה שוב.";
        public const string EXPENSES_ALLOCATIONS_EXEEDS_AMOUNT = "להוצאה זאת כבר הוקצה להוצאה סכום העולה על סך ההוצאה החדש. ההוצאה לא עודכנה. אנא ערוך את ההקצאות בהתאם ונסה שוב.";
        public const string EXPENSES_FULL_ALLOCATION = "לא נותר מספיק סכום להוצאה שצויינה על מנת להקצות את הסכום שבוקש.";
        public const string EXPENSES_DELETE_HAS_APPROVED_ORDERS = "להוצאה זאת קיימות הזמנות מאושרות. לא הייתה אפשרות לבטל את ההוצאה.";

        //Allocations Errors
        public const string ALLOCATIONS_AMOUNT_IS_USED = "סך ההזמנות מהקצאה זאת עולה על סכום ההקצאה שצויין.";
        public const string ALLOCATIONS_GET_ERROR = "קרתה שגיאה בזמן הגישה להקצאות בבסיס הנתונים. אנא נסה שוב.";
        public const string ALLOCATIONS_EDIT_ERROR = "קרתה שגיאה בזמן הגישה להקצאות בבסיס הנתונים. אנא נסה שוב.";
        public const string ALLOCATIONS_HAS_APPROVED_ORDERS = "להקצאה זאת קיימות הזמנות מאושרות. לא הייתה אפשרות לבטל את ההקצאה.";
        public const string ALLOCATIONS_DELETE_ERROR = "קרתה שגיאה בזמן הסרת ההקצאה, הפעולה לא הושלמה.";

        //Projects Errors
        public const string PROJECTS_CREATE_ERROR = "קרתה שגיאה בזמן השמירה למסד הנתונים. הפרוייקט לא נשמר במערכת.";
        public const string PROJECTS_GET_ERROR = "קרתה שגיאה בזמן הגישה לפרוייקטים בבסיס הנתונים. אנא נסה שוב.";

        //SubProjects Errors
        public const string SUB_PROJECTS_CREATE_ERROR = "קרתה שגיאה בזמן השמירה למסד הנתונים. התת-פרוייקט לא נשמר במערכת.";
        public const string SUB_PROJECTS_GET_ERROR = "קרתה שגיאה בזמן הגישה לתת-פרוייקטים בבסיס הנתונים. אנא נסה שוב.";

        //Permissions Errors
        public const string PERMISSIONS_CREATE_ERROR = "קרתה שגיאה בזמן השמירה למסד הנתונים. ההרשאה לא נשמר במערכת.";
        public const string PERMISSIONS_GET_ERROR = "קרתה שגיאה בזמן הגישה להרשאות בבסיס הנתונים. אנא נסה שוב.";
        public const string PERMISSIONS_DELETE_ERROR = "קרתה שגיאה בזמן הסרת ההרשאה, הפעולה לא הושלמה.";


    }
}