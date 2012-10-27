using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB;

namespace DA
{
    [Flags]
    public enum RoleType : int
    {
        None = 0,
        OrdersWriter = 1,
        OrdersApprover = 2 | OrdersViewer,
        OrdersViewer = 4,
        SystemManager = 8 | OrdersViewer | OrdersWriter,
        UsersManager = 16,
        admin = 32 | UsersManager | SystemManager | OrdersApprover | OrdersWriter,
        BudgetViewer = 64,
        SuperAdmin = int.MaxValue
    }
    public static class Roles
    {
        public static bool HasRole(int userRoles, RoleType role)
        {
            return ((RoleType)userRoles & role) == role;
        }

        public static bool HasRole(RoleType roles, RoleType role)
        {
            return (roles & role) == role;
        }

        public static void AddRole(User user, RoleType role)
        {
            user.Roles = (int)((RoleType)user.Roles | role);
        }

        public static void RemoveRole(User user, RoleType role)
        {
            user.Roles = (int)((RoleType)user.Roles & ~role);
        }

        public static RoleType CombineRoles(params RoleType[] roles)
        {
            RoleType resultRole = RoleType.None;

            foreach (RoleType role in roles)
            {
                resultRole |= role;
            }

            return resultRole;
        }

        public static List<RoleType> GetAllRoles(RoleType roles)
        {
            List<RoleType> allRoles = new List<RoleType>();
            int[] values = (int[])Enum.GetValues(typeof(RoleType));

            foreach (int value in values)
            {
                if(HasRole(roles, (RoleType)value))
                    allRoles.Add((RoleType)value);
            }

            return allRoles;
        }

        public static string Describe(this RoleType e)
        {
            var allRoleType = Enum.GetValues(typeof(RoleType));
            StringBuilder builder = new StringBuilder();
            string description = String.Empty;

            if (e == RoleType.SuperAdmin)
            {
                return "אדמיניסטרטור ראשי";
            }
            else
            {
                foreach (var role in allRoleType)
                {
                    switch ((RoleType)role)
                    {
                        case RoleType.OrdersWriter:
                            if (Roles.HasRole(e, (RoleType)role))
                                builder.Append("יוצר הזמנות, ");
                            break;
                        case RoleType.OrdersApprover:
                            if (Roles.HasRole(e, (RoleType)role))
                                builder.Append("מאשר הזמנות, ");
                            break;
                        case RoleType.OrdersViewer:
                            if (Roles.HasRole(e, (RoleType)role))
                                builder.Append("צופה בהזמנות, ");
                            break;
                        case RoleType.SystemManager:
                            if (Roles.HasRole(e, (RoleType)role))
                                builder.Append("מנהל מערכת, ");
                            break;
                        case RoleType.SuperAdmin:
                            if (Roles.HasRole(e, (RoleType)role))
                                builder.Append("אדמיניסטרטור ראשי, ");
                            break;
                    }
                }
            }

            description = builder.ToString();
            if (description.Length != 0)
                description = description.Remove(description.Length - 2);

            return description;
        }
    }
}
