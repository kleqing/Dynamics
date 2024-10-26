using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamics.Utility
{
    public class MySettingSession
    {
        public const string SESSION_Organization_Member_KEY = "OrganizationMember";// table OrganizationMember

        public const string SESSION_Current_Organization_KEY = "CurrentOrganization"; // current Organization

        public const string SESSION_Current_Project_KEY = "CurrentProject";//current Project

        public const string SESSION_Current_Organization_Resource_KEY = "CurrentOrganizationResource";//current Organization Resource

        public const string SESSION_OrganizzationToProjectHistory_For_Organization_Pending_Key = "OTPHistory In project pending"; // get OTPHistory In project pending

        public const string SESSION_OrganizzationToProjectHistory_For_Organization_Accepting_Key = "OTPHistory In project accepting"; // get OTPHistory In project accepting





        public const string SESSION_Resources_In_A_PRoject_KEY = "ResourcesInProject"; //all resource in a project

        public const string SESSION_User_In_A_OrganizationID_KEY = "UserMemberInAOrganizationId";//organization that user join 

        public const string Session_Organization_User_Ceo_Key = "OrganizationUserIsCeo";// organization that you are ceo

        public const string SESSION_Projects_In_A_OrganizationID_Key = "ProjectsInAOrganization";// project of a organization

        public const string SESSION_Organization_Resource_Current_Key = "CurrentOrganizationResource"; // resource which user is donating

        public const string SESSION_ResourceName_For_UserToOrganizationHistory_Key = "resourceNameUTOHistory"; //get resource name for  UTO history

        public const string SESSION_UserName_For_UserToOrganizationHistory_Key = "userNameUTOHistory"; //get userName for UTO history

        public const string SESSION_ResourceName_For_OrganizationToProjectHistory_Key = "resourceNameOTPHistory"; //get resource name for  OTP history

        public const string SESSION_ProjectName_For_OrganizzationToProjectHistory_Key = "projectNameOTPHistory"; //get projectName for OTP history
    }
}
