namespace OneTouch.Utils.enums
{
    public static class RoleEnumExtensions
    {
        public static string GetName(this RoleEnum role)
        {
            return role switch
            {
                RoleEnum.Admin => "Admin",
                RoleEnum.Doctor => "Doctor",
                RoleEnum.Patient => "Patient",
                _ => role.ToString().ToUpper()
            };
        }
    }
}
