namespace Ncc.Authorization.Roles
{
    public static class StaticRoleNames
    {
        public static class Host
        {
            public const string Admin = "Admin";
            public const string ProjectAdmin = "ProjectAdmin";
            public const string BasicUser = "BasicUser";
            //public const string Post = "PostCreator";
            public const string Supervisor = "Supervisor";
            //public const string AbsenceAdmin = "AbsenceAdmin";
            //public const string NewsAdmin = "NewsAdmin";
            public const string BranchDirector = "BranchDirector";
        }

        public static class Tenants
        {
            public const string Admin = "Admin";
        }
    }
}
