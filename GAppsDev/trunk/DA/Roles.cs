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
        Employee = 1,
        OrdersApprover = 2 | Viewer,
        Viewer = 4,
        SystemManager = 8,
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
    }
}
