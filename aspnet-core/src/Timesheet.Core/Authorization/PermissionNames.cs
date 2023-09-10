using Abp.MultiTenancy;
using System.Collections.Generic;
using Timesheet.Entities;
using static Ncc.Authorization.Roles.StaticRoleNames;

namespace Ncc.Authorization
{
    public static class PermissionNames
    {
        //public const string Home = "Home";
        public const string Admin = "Admin";
        public const string Admin_Users = "Admin.Users";
        public const string Admin_Users_View = "Admin.Users.View";
        public const string Admin_Users_AddNew = "Admin.Users.AddNew";
        public const string Admin_Users_Edit = "Admin.Users.Edit";
        public const string Admin_Users_EditRole = "Admin.Users.EditRole";
        public const string Admin_Users_Delete = "Admin.Users.Delete";
        public const string Admin_Users_ResetPassword = "Admin.Users.ResetPassword";
        public const string Admin_Users_UploadAvatar = "Admin.Users.UploadAvatar";
        public const string Admin_Users_ChangeStatus = "Admin.Users.ChangeStatus";
        public const string Admin_Users_ImportWorkingTime = "Admin.Users.UploadWorkingTime";
        public const string Admin_Users_UpdateUserWorkingTime = "Admin.Users.UpdateUserWorkingTime";
        public const string Admin_Users_ViewLevelUser = "Admin.Users.ViewLevelUser";
        public const string Admin_Roles = "Admin.Roles";
        public const string Admin_Roles_View = "Admin.Roles.View";
        public const string Admin_Roles_ViewDetail = "Admin.Roles.ViewDetail";
        public const string Admin_Roles_AddNew = "Admin.Roles.AddNew";
        public const string Admin_Roles_Edit = "Admin.Roles.Edit";
        public const string Admin_Roles_Delete = "Admin.Roles.Delete";
        public const string Admin_Configuration = "Admin.Configuration";
        public const string Admin_Configuration_Email = "Admin.Configuration.Email";
        public const string Admin_Configuration_WorkingDay = "Admin.Configuration.WorkingDay";
        public const string Admin_Configuration_GoogleSignOn = "Admin.Configuration.GoogleSignOn";
        public const string Admin_Configuration_AutoLockTimesheet = "Admin.Configuration.AutoLockTimesheet";
        public const string Admin_Configuration_SercurityCode = "Admin.Configuration.SercurityCode";
        public const string Admin_Configuration_LogTimesheetInFuture = "Admin.Configuration.LogTimesheetInFuture";
        public const string Admin_Configuration_AutoSubmitTimesheet = "Admin.Configuration.AutoSubmitTimesheet";
        public const string Admin_Configuration_EmailSaoDo = "Admin.Configuration.EmailSaoDo";
        public const string Admin_Configuration_HRMConfig = "Admin.Configuration.HRMConfig";
        public const string Admin_Configuration_ProjectConfig = "Admin.Configuration.ProjectConfig";
        public const string Admin_Configuration_KomuConfig = "Admin.Configuration.KomuConfig";
        public const string Admin_Configuration_LevelSetting = "Admin.Configuration.LevelSetting";
        public const string Admin_Configuration_CheckInCheckOutPunishmentSetting = "Admin.Configuration.CheckInCheckOutPunishmentSetting";
        public const string Admin_Configuration_NotificationSetting = "Admin.Configuration.NotificationSetting";
        public const string Admin_Configuration_CheckInSetting = "Admin.Configuration.CheckInSetting";
        public const string Admin_Configuration_RemoteSetting = "Admin.Configuration.RemoteSetting";
        public const string Admin_Configuration_SpecialProjectTaskSetting = "Admin.Configuration.SpecialProjectTaskSetting";
        public const string Admin_Configuration_NRITConfig = "Admin.Configuration.NRITConfig";
        public const string Admin_Configuration_UnlockTimesheetSetting = "Admin.Configuration.UnlockTimesheetSetting";
        public const string Admin_Configuration_TeamBuilding = "Admin.Configuration.TeamBuilding";
        public const string Admin_Configuration_TimeStartChangingCheckInToCheckOutSetting = "Admin.Configuration.TimeStartChangingCheckInToCheckOutSetting";
        public const string Admin_Configuration_SettingWorkerNoticeKomuPunishmentUserNoCheckInOut = "Admin.Configuration.SettingWorkerNoticeKomuPunishmentUserNoCheckInOut";
        public const string Admin_Configuration_SettingWorkerNoticeKomuPunishmentUserNoCheckInOut_View = "Admin.Configuration.SettingWorkerNoticeKomuPunishmentUserNoCheckInOut.View";
        public const string Admin_Configuration_SettingWorkerNoticeKomuPunishmentUserNoCheckInOut_Update = "Admin.Configuration.SettingWorkerNoticeKomuPunishmentUserNoCheckInOut.Update";
        public const string Admin_Configuration_Email_View = "Admin.Configuration.Email.ViewEmail";
        public const string Admin_Configuration_WorkingDay_View = "Admin.Configuration.WorkingDay.ViewWorkingDay";
        public const string Admin_Configuration_GoogleSignOn_View = "Admin.Configuration.GoogleSignOn.ViewGoogleSignOn";
        public const string Admin_Configuration_AutoLockTimesheet_View = "Admin.Configuration.AutoLockTimesheet.ViewAutoLockTimesheet";
        public const string Admin_Configuration_SercurityCode_View = "Admin.Configuration.SercurityCode.ViewSercurityCode";
        public const string Admin_Configuration_LogTimesheetInFuture_View = "Admin.Configuration.LogTimesheetInFuture.ViewLogTimesheetInFuture";
        public const string Admin_Configuration_AutoSubmitTimesheet_View = "Admin.Configuration.AutoSubmitTimesheet.ViewAutoSubmitTimesheet";
        public const string Admin_Configuration_EmailSaoDo_View = "Admin.Configuration.EmailSaoDo.ViewEmailSaoDo";
        public const string Admin_Configuration_SercurityCode_Edit = "Admin.Configuration.SercurityCode.EditSercurityCode";
        public const string Admin_Configuration_Email_Edit = "Admin.Configuration.Email.EditEmail";
        public const string Admin_Configuration_WorkingDay_Edit = "Admin.Configuration.WorkingDay.EditWorkingDay";
        public const string Admin_Configuration_GoogleSignOn_Edit = "Admin.Configuration.GoogleSignOn.EditGoogleSignOn";
        public const string Admin_Configuration_AutoLockTimesheet_Edit = "Admin.Configuration.AutoLockTimesheet.EditAutoLockTimesheet";
        public const string Admin_Configuration_LogTimesheetInFuture_Edit = "Admin.Configuration.LogTimesheetInFuture.EditLogTimesheetInFuture";
        public const string Admin_Configuration_AutoSubmitTimesheet_Edit = "Admin.Configuration.AutoSubmitTimesheet.EditAutoSubmitTimesheet";
        public const string Admin_Configuration_EmailSaoDo_Edit = "Admin.Configuration.EmailSaoDo.EditEmailSaoDo";
        public const string Admin_Configuration_HRMConfig_View = "Admin.Configuration.HRMConfig.ViewHRMConfig";
        public const string Admin_Configuration_HRMConfig_Update = "Admin.Configuration.HRMConfig.UpdateHRMConfig";
        public const string Admin_Configuration_ProjectConfig_View = "Admin.Configuration.ProjectConfig.ViewProjectConfig";
        public const string Admin_Configuration_ProjectConfig_Update = "Admin.Configuration.ProjectConfig.UpdateProjectConfig";
        public const string Admin_Configuration_KomuConfig_View = "Admin.Configuration.KomuConfig.ViewKomuConfig";
        public const string Admin_Configuration_KomuConfig_Update = "Admin.Configuration.KomuConfig.UpdateKomuConfig";
        public const string Admin_Configuration_LevelSetting_View = "Admin.Configuration.LevelSetting.ViewLevelSetting";
        public const string Admin_Configuration_LevelSetting_Edit = "Admin.Configuration.LevelSetting.EditLevelSetting";
        public const string Admin_Configuration_CheckInCheckOutPunishmentSetting_View = "Admin.Configuration.CheckInCheckOutPunishmentSetting.ViewCheckInCheckOutPunishmentSetting";
        public const string Admin_Configuration_CheckInCheckOutPunishmentSetting_Edit = "Admin.Configuration.CheckInCheckOutPunishmentSetting.EditCheckInCheckOutPunishmentSetting";
        public const string Admin_Configuration_NotificationSetting_View = "Admin.Configuration.NotificationSetting.ViewNotificationSetting";
        public const string Admin_Configuration_NotificationSetting_Edit = "Admin.Configuration.NotificationSetting.EditNotificationSetting";
        public const string Admin_Configuration_CheckInSetting_View = "Admin.Configuration.CheckInSetting.ViewCheckInSetting";
        public const string Admin_Configuration_CheckInSetting_Update = "Admin.Configuration.CheckInSetting.UpdateCheckInSetting";
        public const string Admin_Configuration_RemoteSetting_View = "Admin.Configuration.RemoteSetting.ViewRemoteSetting";
        public const string Admin_Configuration_RemoteSetting_Edit = "Admin.Configuration.RemoteSetting.EditRemoteSetting";
        public const string Admin_Configuration_SpecialProjectTaskSetting_View = "Admin.Configuration.SpecialProjectTaskSetting.ViewSpecialProjectTaskSetting";
        public const string Admin_Configuration_SpecialProjectTaskSetting_Edit = "Admin.Configuration.SpecialProjectTaskSetting.EditSpecialProjectTaskSetting";
        public const string Admin_Configuration_NRITConfig_View = "Admin.Configuration.NRITConfig.ViewNRITConfig";
        public const string Admin_Configuration_NRITConfig_Update = "Admin.Configuration.NRITConfig.UpdateNRITConfig";
        public const string Admin_Configuration_UnlockTimesheetSetting_View = "Admin.Configuration.UnlockTimesheetSetting.ViewUnlockTimesheetSetting";
        public const string Admin_Configuration_UnlockTimesheetSetting_Update = "Admin.Configuration.UnlockTimesheetSetting.UpdateUnlockTimesheetSetting";
        public const string Admin_Configuration_RetroNotifyConfig = "Admin.Configuration.RetroNotifyConfig";
        public const string Admin_Configuration_RetroNotifyConfig_View = "Admin.Configuration.RetroNotifyConfig.ViewRetroNotifyConfig";
        public const string Admin_Configuration_RetroNotifyConfig_Update = "Admin.Configuration.RetroNotifyConfig.UpdateRetroNotifyConfig";
        public const string Admin_Configuration_TimesCanLateAndEarlyInMonthSetting = "Admin.Configuration.TimesCanLateAndEarlyInMonthSetting";
        public const string Admin_Configuration_TimesCanLateAndEarlyInMonthSetting_View = "Admin.Configuration.TimesCanLateAndEarlyInMonthSetting.ViewTimesCanLateAndEarlyInMonthSetting";
        public const string Admin_Configuration_TimesCanLateAndEarlyInMonthSetting_Update = "Admin.Configuration.TimesCanLateAndEarlyInMonthSetting.EditTimesCanLateAndEarlyInMonthSetting";

        public const string Admin_Configuration_TeamBuilding_View = "Admin.Configuration.TeamBuilding.ViewTeamBuildingConfig";
        public const string Admin_Configuration_TeamBuilding_Update = "Admin.Configuration.TeamBuilding.UpdateTeamBuildingConfig";
        public const string Admin_Configuration_TimeStartChangingCheckInToCheckOutSetting_View = "Admin.Configuration.TimeStartChangingCheckInToCheckOutSetting.ViewTimeStartChangingCheckInToCheckOut";
        public const string Admin_Configuration_TimeStartChangingCheckInToCheckOutSetting_Update = "Admin.Configuration.TimeStartChangingCheckInToCheckOutSetting.UpdateTimeStartChangingCheckInToCheckOut";
        public const string Admin_Configuration_ApproveTimesheetNotifyConfig = "Admin.Configuration.ApproveTimesheetNotifyConfig";
        public const string Admin_Configuration_ApproveTimesheetNotifyConfig_View = "Admin.Configuration.ApproveTimesheetNotifyConfig.ViewApproveTimesheetNotifyConfig";
        public const string Admin_Configuration_ApproveTimesheetNotifyConfig_Update = "Admin.Configuration.ApproveTimesheetNotifyConfig.UpdateApproveTimesheetNotifyConfig";
        public const string Admin_Configuration_ApproveRequestOffNotifyConfig = "Admin.Configuration.ApproveRequestOffNotifyConfig";
        public const string Admin_Configuration_ApproveRequestOffNotifyConfig_View = "Admin.Configuration.ApproveRequestOffNotifyConfig.ViewApproveRequestOffNotifyConfig";
        public const string Admin_Configuration_ApproveRequestOffNotifyConfig_Update = "Admin.Configuration.ApproveRequestOffNotifyConfig.UpdateApproveRequestOffNotifyConfig";
        public const string Admin_Configuration_SendMessageRequestPendingTeamBuildingToHRConfig = "Admin.Configuration.SendMessageRequestPendingTeamBuildingToHRConfig";
        public const string Admin_Configuration_SendMessageRequestPendingTeamBuildingToHRConfig_View = "Admin.Configuration.SendMessageRequestPendingTeamBuildingToHRConfig.ViewSendMessageRequestPendingTeamBuildingToHRConfig";
        public const string Admin_Configuration_SendMessageRequestPendingTeamBuildingToHRConfig_Update = "Admin.Configuration.SendMessageRequestPendingTeamBuildingToHRConfig.UpdateSendMessageRequestPendingTeamBuildingToHRConfig";
        public const string Admin_Configuration_NotifyHRTheEmployeeMayHaveLeftConfig = "Admin.Configuration.NotifyHRTheEmployeeMayHaveLeftConfig";
        public const string Admin_Configuration_NotifyHRTheEmployeeMayHaveLeftConfig_View = "Admin.Configuration.NotifyHRTheEmployeeMayHaveLeftConfig.ViewNotifyHRTheEmployeeMayHaveLeftConfig";
        public const string Admin_Configuration_NotifyHRTheEmployeeMayHaveLeftConfig_Update = "Admin.Configuration.NotifyHRTheEmployeeMayHaveLeftConfig.UpdateNotifyHRTheEmployeeMayHaveLeftConfig";
        public const string Admin_Configuration_MoneyPMUnlockTimeSheetConfig = "Admin.Configuration.MoneyPMUnlockTimeSheetConfig";
        public const string Admin_Configuration_MoneyPMUnlockTimeSheetConfig_View = "Admin.Configuration.MoneyPMUnlockTimeSheetConfig.ViewMoneyPMUnlockTimeSheetConfig";
        public const string Admin_Configuration_MoneyPMUnlockTimeSheetConfig_Update = "Admin.Configuration.MoneyPMUnlockTimeSheetConfig.UpdateMoneyPMUnlockTimeSheetConfig";
        public const string Admin_Configuration_SendMessageToPunishUserConfig = "Admin.Configuration.SendMessageToPunishUserConfig";
        public const string Admin_Configuration_SendMessageToPunishUserConfig_View = "Admin.Configuration.SendMessageToPunishUserConfig.ViewSendMessageToPunishUserConfig";
        public const string Admin_Configuration_SendMessageToPunishUserConfig_Update = "Admin.Configuration.SendMessageToPunishUserConfig.UpdateSendMessageToPunishUserConfig";



        public const string Admin_Clients = "Admin.Clients";
        public const string Admin_Clients_View = "Admin.Clients.View";
        public const string Admin_Clients_AddNew = "Admin.Clients.AddNew";
        public const string Admin_Clients_Edit = "Admin.Clients.Edit";
        public const string Admin_Clients_Delete = "Admin.Clients.Delete";
        public const string Admin_Tasks = "Admin.Tasks";
        public const string Admin_Tasks_View = "Admin.Tasks.View";
        public const string Admin_Tasks_AddNew = "Admin.Tasks.AddNew";
        public const string Admin_Tasks_Edit = "Admin.Tasks.Edit";
        public const string Admin_Tasks_Delete = "Admin.Tasks.Delete";
        public const string Admin_Tasks_ChangeStatus = "Admin.Tasks.ChangeStatus";
        public const string Admin_LeaveTypes = "Admin.LeaveTypes";
        public const string Admin_LeaveTypes_View = "Admin.LeaveTypes.View";
        public const string Admin_LeaveTypes_AddNew = "Admin.LeaveTypes.AddNew";
        public const string Admin_LeaveTypes_Edit = "Admin.LeaveTypes.Edit";
        public const string Admin_LeaveTypes_Delete = "Admin.LeaveTypes.Delete";
        public const string Admin_Branchs = "Admin.Branchs";
        public const string Admin_Branchs_View = "Admin.Branchs.View";
        public const string Admin_Branchs_AddNew = "Admin.Branchs.AddNew";
        public const string Admin_Branchs_Edit = "Admin.Branchs.Edit";
        public const string Admin_Branchs_Delete = "Admin.Branchs.Delete";

        public const string Admin_Position = "Admin.Position";
        public const string Admin_Position_View = "Admin.Position.View";
        public const string Admin_Position_AddNew = "Admin.Position.AddNew";
        public const string Admin_Position_Edit = "Admin.Position.Edit";
        public const string Admin_Position_Delete = "Admin.Position.Delete";

        public const string Admin_Capability = "Admin.Capability";
        public const string Admin_Capability_View = "Admin.Capability.View";
        public const string Admin_Capability_AddNew = "Admin.Capability.AddNew";
        public const string Admin_Capability_Edit = "Admin.Capability.Edit";
        public const string Admin_Capability_Delete = "Admin.Capability.Delete";
        public const string Admin_CapabilitySetting = "Admin.CapabilitySetting";
        public const string Admin_CapabilitySetting_View = "Admin.CapabilitySetting.View";
        public const string Admin_CapabilitySetting_AddNew = "Admin.CapabilitySetting.AddNew";
        public const string Admin_CapabilitySetting_Edit = "Admin.CapabilitySetting.Edit";
        public const string Admin_CapabilitySetting_Delete = "Admin.CapabilitySetting.Delete";
        public const string Admin_CapabilitySetting_Clone = "Admin.CapabilitySetting.Clone";

        //Bg Job
        public const string Admin_BackgroundJob = "Admin.BackgroundJob";
        public const string Admin_BackgroundJob_View = "Admin.BackgroundJob.View";
        public const string Admin_BackgroundJob_Delete = "Admin.BackgroundJob.Delete";
        public const string Admin_AuditLog = "Admin.AuditLog";
        public const string Admin_AuditLog_View = "Admin.AuditLog.View";

        public const string Project = "Project";
        public const string Project_View = "Project.View";
        public const string Project_View_All = "Project.ViewAll";
        public const string Project_AddNew = "Project.AddNew";
        public const string Project_Edit = "Project.Edit";
        public const string Project_Delete = "Project.Delete";
        public const string Project_ChangeStatus = "Project.ChangeStatus";
        public const string Project_UpdateDefaultProjectTask = "Project.UpdateDefaultProjectTask";
        public const string Project_ViewDetail = "Project.ViewDetail";
        public const string Project_Export = "Project.Export";
        public const string Project_Edit_Team_WorkType = "Project.EditTeamWorkType";

        public const string MyTimesheet = "MyTimesheet";
        public const string MyTimesheet_View = "MyTimesheet.View";
        public const string MyTimesheet_AddNew = "MyTimesheet.AddNew";
        public const string MyTimesheet_Edit = "MyTimesheet.Edit";
        public const string MyTimesheet_Delete = "MyTimesheet.Delete";
        public const string MyTimesheet_Submit = "MyTimesheet.Submit";
        public const string MyProfile = "MyProfile";
        public const string MyProfile_View = "MyProfile.View";
        public const string MyProfile_RequestUpdateInfo = "MyProfile.RequestUpdateInfo";
        public const string Timesheet = "Timesheet";
        public const string Timesheet_View = "Timesheet.View";
        public const string Timesheet_ViewStatus = "Timesheet.ViewStatus";
        public const string Timesheet_Approval = "Timesheet.Approval";
        public const string Timesheet_Export = "Timesheet.Export";
        public const string TimesheetSupervision = "TimesheetSupervision";
        public const string TimesheetSupervision_View = "TimesheetSupervision.View";
        public const string MyAbsenceDay = "MyAbsenceDay";
        public const string MyAbsenceDay_View = "MyAbsenceDay.View";
        public const string MyAbsenceDay_AddNew = "MyAbsenceDay.AddNew";
        public const string MyAbsenceDay_SendRequest = "MyAbsenceDay.SendRequest";
        public const string MyAbsenceDay_CancelRequest = "MyAbsenceDay.CancelRequest";
        public const string AbsenceDay = "AbsenceDay";
        public const string AbsenceDay_View = "AbsenceDay.View";
        public const string AbsenceDay_Approval = "AbsenceDay.Approval";
        public const string AbsenceDayByProject = "AbsenceDayByProject";
        public const string AbsenceDayByProject_View = "AbsenceDayByProject.View";
        public const string AbsenceDayByProject_ViewByBranch = "AbsenceDayByProject.ViewByBranch";
        public const string AbsenceDayByProject_ViewDetail = "AbsenceDayByProject.ViewDetail";
        public const string AbsenceDayByProject_Approval = "AbsenceDayByProject.Approval";
        public const string AbsenceDayOfTeam = "AbsenceDayOfTeam";
        public const string AbsenceDayOfTeam_View = "AbsenceDayOfTeam.View";
        public const string AbsenceDayOfTeam_ViewDetail = "AbsenceDayOfTeam.ViewDetail";
        public const string AbsenceDayOfTeam_NotifyPm = "AbsenceDayOfTeam.NotifyPm";
        public const string AbsenceDayOfTeam_ExportTeamWorkingCalender = "AbsenceDayOfTeam.ExportTeamWorkingCalender";
        public const string DayOff = "DayOff";
        public const string DayOff_View = "DayOff.View";
        public const string DayOff_AddNew = "DayOff.AddNew";
        public const string DayOff_Edit = "DayOff.Edit";
        public const string DayOff_Delete = "DayOff.Delete";
        public const string MyWorkingTime = "MyWorkingTime";
        public const string MyWorkingTime_View = "MyWorkingTime.View";
        public const string MyWorkingTime_RegistrationTime = "MyWorkingTime.RegistrationTime";
        public const string MyWorkingTime_Edit = "MyWorkingTime.Edit";
        public const string MyWorkingTime_Delete = "MyWorkingTime.Delete";
        public const string ManageWorkingTime = "ManageWorkingTime";
        public const string ManageWorkingTime_ViewAll = "ManageWorkingTime.ViewAll";
        public const string ManageWorkingTime_ViewDetail = "ManageWorkingTime.ViewDetail";
        public const string ManageWorkingTime_Approval = "ManageWorkingTime.Approval";
        public const string Report = "Report";
        public const string Report_InternsInfo = "Report.InternsInfo";
        public const string Report_InternsInfo_View = "Report.InternsInfo.View";
        public const string Report_InternsInfo_ViewLevelIntern = "Report.InternsInfo.ViewLevelIntern";
        public const string Report_NormalWorking = "Report.NormalWorking";
        public const string Report_NormalWorking_View = "Report.NormalWorking.View";
        public const string Report_NormalWorking_Export = "Report.NormalWorking.Export";
        public const string Report_NormalWorking_LockUnlockTimesheet = "Report.NormalWorking.LockUnlockTimesheet";
        public const string Report_OverTime = "Report.OverTime";
        public const string Report_OverTime_View = "Report.OverTime.View";
        public const string Report_KomuTracker = "Report.KomuTracker";
        public const string Report_KomuTracker_View = "Report.KomuTracker.View";
        public const string Report_TardinessLeaveEarly = "Report.TardinessLeaveEarly";
        public const string Report_TardinessLeaveEarly_View = "Report.TardinessLeaveEarly.View";
        public const string MyTimeSheet_ViewMyTardinessDetail = "MyTimeSheet.ViewMyTardinessDetail";
        public const string Report_TardinessLeaveEarly_GetData = "Report.TardinessLeaveEarly.GetData";
        public const string Report_TardinessLeaveEarly_ExportExcel = "Report.TardinessLeaveEarly.ExportExcel";
        public const string Report_TardinessLeaveEarly_Edit = "Report.TardinessLeaveEarly.Edit";
        public const string Timekeeping_UserNote = "Timekeeping.UserNote";
        public const string Timekeeping_ReplyUserNote = "Timekeeping.ReplyUserNote";

        public const string ReviewIntern = "ReviewIntern";
        public const string ReviewIntern_ViewAllReport = "ReviewIntern.ViewAllReport";
        public const string ReviewIntern_ExportReport = "ReviewIntern.ExportReport";
        public const string ReviewIntern_AddNewReview = "ReviewIntern.AddNewReview";
        public const string ReviewIntern_AddNewReviewByCapability = "ReviewIntern.AddNewReviewByCapability";
        public const string ReviewIntern_ViewAll = "ReviewIntern.ViewAll";
        public const string ReviewIntern_Delete = "ReviewIntern.Delete";
        public const string ReviewIntern_Active = "ReviewIntern.Active";
        public const string ReviewIntern_DeActive = "ReviewIntern.DeActive";
        public const string ReviewIntern_ApproveAll = "ReviewIntern.ApproveAll";

        public const string ReviewIntern_ReviewDetail = "ReviewIntern.ReviewDetail";
        public const string ReviewIntern_ReviewDetail_ViewAll = "ReviewIntern.ReviewDetail.ViewAll";
        public const string ReviewIntern_ReviewDetail_AddNew = "ReviewIntern.ReviewDetail.AddNew";
        public const string ReviewIntern_ReviewDetail_Update = "ReviewIntern.ReviewDetail.Update";
        public const string ReviewIntern_ReviewDetail_ChangeReviewer = "ReviewIntern.ReviewDetail.ChangeReviewer";
        public const string ReviewIntern_ReviewDetail_Delete = "ReviewIntern.ReviewDetail.Delete";
        public const string ReviewIntern_ReviewDetail_ReviewForOneIntern = "ReviewIntern.ReviewDetail.ReviewForOneIntern";
        public const string ReviewIntern_ReviewDetail_SendEmailForOneIntern = "ReviewIntern.ReviewDetail.SendEmailForOneIntern";
        public const string ReviewIntern_ReviewDetail_UpdateToHRMForOneIntern = "ReviewIntern.ReviewDetail.UpdateToHRMForOneIntern";
        public const string ReviewIntern_ReviewDetail_SendAllEmailsIntern = "ReviewIntern.ReviewDetail.SendAllEmailsIntern";
        public const string ReviewIntern_ReviewDetail_SendAllEmailsOffical = "ReviewIntern.ReviewDetail.SendAllEmailsOffical";
        public const string ReviewIntern_ReviewDetail_UpdateAllToHRMs = "ReviewIntern.ReviewDetail.SendAllToHRM";
        public const string ReviewIntern_ReviewDetail_UpdateStarToProject = "ReviewIntern.ReviewDetail.UpdateStarToProject";
        public const string ReviewIntern_ReviewDetail_ApproveForOneIntern = "ReviewIntern.ReviewDetail.ApproveForOneIntern";
        public const string ReviewIntern_ReviewDetail_ConfirmSalaryForOneIntern = "ReviewIntern.ReviewDetail.ConfirmSalaryForOneIntern";
        public const string ReviewIntern_ReviewDetail_RejectForOneIntern = "ReviewIntern.ReviewDetail.RejectForOneIntern";
        public const string ReviewIntern_ReviewDetail_RejectSentEmailForOneIntern = "ReviewIntern.ReviewDetail.RejectSentEmailForOneIntern";
        public const string ReviewIntern_ReviewDetail_ViewDetailSubLevel = "ReviewIntern.ReviewDetail.ViewDetailSubLevel";
        public const string ReviewIntern_ReviewDetail_ViewDetailLevel = "ReviewIntern.ReviewDetail.ViewDetailLevel";
        public const string ReviewIntern_ReviewDetail_ViewFullSalary = "ReviewIntern.ReviewDetail.ViewFullSalary";
        public const string ReviewIntern_ReviewDetail_UpdateDetailSubLevel = "ReviewIntern.ReviewDetail.UpdateDetailSubLevel";
        public const string ReviewIntern_ReviewDetail_UpdateDetailFullSalary = "ReviewIntern.ReviewDetail.UpdateDetailFullSalary";
        public const string ReviewIntern_ReviewDetail_ReviewByCapabilityForOneIntern = "ReviewIntern.ReviewDetail.ReviewByCapabilityForOneIntern";

        public const string OverTimeSetting = "OverTimeSetting";
        public const string OverTimeSetting_View = "OverTimeSetting.View";
        public const string OverTimeSetting_AddNew = "OverTimeSetting.AddNew";
        public const string OverTimeSetting_Edit = "OverTimeSetting.Edit";
        public const string OverTimeSetting_Delete = "OverTimeSetting.Delete";

        public const string Retro = "Retro";
        public const string Retro_View = "Retro.View";
        public const string Retro_AddNew = "Retro.AddNew";
        public const string Retro_Edit = "Retro.Edit";
        public const string Retro_Delete = "Retro.Delete";
        public const string Retro_ChangeStatus = "Retro.ChangeStatus";

        public const string Retro_RetroDetail = "Retro.ManageRetro.RetroDetail";
        public const string Retro_RetroDetail_ViewAllTeam = "Retro.RetroDetail.ViewAllTeam";
        public const string Retro_RetroDetail_ViewMyTeam = "Retro.RetroDetail.ViewMyTeam";
        public const string Retro_RetroDetail_AddEmployeeAllTeam = "Retro.RetroDetail.AddEmployeeAllTeam";
        public const string Retro_RetroDetail_AddEmployeeMyTeam = "Retro.RetroDetail.AddEmployeeMyTeam";
        public const string Retro_RetroDetail_Delete = "Retro.RetroDetail.Delete";
        public const string Retro_RetroDetail_Edit = "Retro.RetroDetail.Edit";
        public const string Retro_RetroDetail_Import = "Retro.RetroDetail.Import";
        public const string Retro_RetroDetail_DownloadTemplate = "Retro.RetroDetail.DownloadTemplate";
        public const string Retro_RetroDetail_Export = "Retro.RetroDetail.Export";
        public const string Retro_RetroDetail_GenerateData = "Retro.RetroDetail.GenerateData";
        public const string Retro_RetroDetail_ViewLevel = "Retro.RetroDetail.ViewLevel";

        public const string TeamBuilding = "TeamBuilding";
        public const string TeamBuilding_DetailHR = "TeamBuilding.DetailHR";
        public const string TeamBuilding_DetailHR_ViewAllProject = "TeamBuilding.DetailHR.ViewAllProject";
        public const string TeamBuilding_DetailHR_GenerateData = "TeamBuilding.DetailHR.GenerateData";
        public const string TeamBuilding_DetailHR_Management = "TeamBuilding.DetailHR.Management";
        public const string TeamBuilding_DetailPM = "TeamBuilding.DetailPM";
        public const string TeamBuilding_DetailPM_ViewMyProject = "TeamBuilding.DetailPM.ViewMyProject";
        public const string TeamBuilding_DetailPM_CreateRequest = "TeamBuilding.DetailPM.CreateRequest";
        public const string TeamBuilding_Request = "TeamBuilding.Request";
        public const string TeamBuilding_Request_ViewAllRequest = "TeamBuilding.Request.ViewAllRequest";
        public const string TeamBuilding_Request_ViewMyRequest = "TeamBuilding.Request.ViewMyRequest";
        public const string TeamBuilding_Request_DisburseRequest = "TeamBuilding.Request.DisburseRequest";
        public const string TeamBuilding_Request_EditRequest = "TeamBuilding.Request.EditRequest";
        public const string TeamBuilding_Request_ReOpenRequest = "TeamBuilding.Request.ReOpenRequest";
        public const string TeamBuilding_Request_RejectRequest = "TeamBuilding.Request.RejectRequest";
        public const string TeamBuilding_Request_CancelRequest = "TeamBuilding.Request.CancelRequest";
        public const string TeamBuilding_Request_ViewDetailRequest = "TeamBuilding.Request.ViewDetailRequest";
        public const string TeamBuilding_Project = "TeamBuilding.Project";
        public const string TeamBuilding_Project_SelectProjectTeamBuilding = "TeamBuilding.Project.SelectProjectTeamBuilding";
        
    }

    public class GrantPermissionRoles
    {
        public static Dictionary<string, List<string>> PermissionRoles = new Dictionary<string, List<string>>()
        {
            {
                Host.Admin,
                new List<string>()
                {
                    //PermissionNames.Home,
                    PermissionNames.Admin,
                    PermissionNames.Admin_Users,
                    PermissionNames.Admin_Users_View,
                    PermissionNames.Admin_Users_AddNew,
                    PermissionNames.Admin_Users_Edit,
                    PermissionNames.Admin_Users_EditRole,
                    PermissionNames.Admin_Users_Delete,
                    PermissionNames.Admin_Users_ResetPassword,
                    PermissionNames.Admin_Users_UploadAvatar,
                    PermissionNames.Admin_Users_ChangeStatus,
                    PermissionNames.Admin_Users_ImportWorkingTime,
                    PermissionNames.Admin_Users_UpdateUserWorkingTime,
                    PermissionNames.Admin_Users_ViewLevelUser,
                    PermissionNames.Admin_Roles,
                    PermissionNames.Admin_Roles_View,
                    PermissionNames.Admin_Roles_ViewDetail,
                    PermissionNames.Admin_Roles_AddNew,
                    PermissionNames.Admin_Roles_Edit,
                    PermissionNames.Admin_Roles_Delete,
                    PermissionNames.Admin_Configuration,
                    PermissionNames.Admin_Configuration_Email,
                    PermissionNames.Admin_Configuration_WorkingDay,
                    PermissionNames.Admin_Configuration_GoogleSignOn,
                    PermissionNames.Admin_Configuration_AutoLockTimesheet,
                    PermissionNames.Admin_Configuration_SercurityCode,
                    PermissionNames.Admin_Configuration_EmailSaoDo,
                    PermissionNames.Admin_Configuration_AutoLockTimesheet,
                    PermissionNames.Admin_Configuration_LogTimesheetInFuture,
                    PermissionNames.Admin_Configuration_AutoSubmitTimesheet,
                    PermissionNames.Admin_Configuration_CheckInSetting,
                    PermissionNames.Admin_Configuration_HRMConfig,
                    PermissionNames.Admin_Configuration_ProjectConfig,
                    PermissionNames.Admin_Configuration_KomuConfig,
                    PermissionNames.Admin_Configuration_LevelSetting,
                    PermissionNames.Admin_Configuration_CheckInCheckOutPunishmentSetting,
                    PermissionNames.Admin_Configuration_RemoteSetting,
                    PermissionNames.Admin_Configuration_SpecialProjectTaskSetting,
                    PermissionNames.Admin_Configuration_NotificationSetting,
                    PermissionNames.Admin_Configuration_Email_View,
                    PermissionNames.Admin_Configuration_WorkingDay_View,
                    PermissionNames.Admin_Configuration_GoogleSignOn_View,
                    PermissionNames.Admin_Configuration_Email_Edit,
                    PermissionNames.Admin_Configuration_WorkingDay_Edit,
                    PermissionNames.Admin_Configuration_GoogleSignOn_Edit,
                    PermissionNames.Admin_Configuration_AutoLockTimesheet_View,
                    PermissionNames.Admin_Configuration_SercurityCode_View,
                    PermissionNames.Admin_Configuration_EmailSaoDo_View,
                    PermissionNames.Admin_Configuration_EmailSaoDo_Edit,
                    PermissionNames.Admin_Configuration_AutoLockTimesheet_Edit,
                    PermissionNames.Admin_Configuration_SercurityCode_Edit,
                    PermissionNames.Admin_Configuration_LogTimesheetInFuture_View,
                    PermissionNames.Admin_Configuration_LogTimesheetInFuture_Edit,
                    PermissionNames.Admin_Configuration_AutoSubmitTimesheet_View,
                    PermissionNames.Admin_Configuration_AutoSubmitTimesheet_Edit,
                    PermissionNames.Admin_Configuration_CheckInSetting_View,
                    PermissionNames.Admin_Configuration_CheckInSetting_Update,
                    PermissionNames.Admin_Configuration_HRMConfig_View,
                    PermissionNames.Admin_Configuration_HRMConfig_Update,
                    PermissionNames.Admin_Configuration_ProjectConfig_View,
                    PermissionNames.Admin_Configuration_ProjectConfig_Update,
                    PermissionNames.Admin_Configuration_KomuConfig_View,
                    PermissionNames.Admin_Configuration_KomuConfig_Update,
                    PermissionNames.Admin_Configuration_LevelSetting_View,
                    PermissionNames.Admin_Configuration_LevelSetting_Edit,
                    PermissionNames.Admin_Configuration_CheckInCheckOutPunishmentSetting_View,
                    PermissionNames.Admin_Configuration_CheckInCheckOutPunishmentSetting_Edit,
                    PermissionNames.Admin_Configuration_RemoteSetting_View,
                    PermissionNames.Admin_Configuration_RemoteSetting_Edit,
                    PermissionNames.Admin_Configuration_SpecialProjectTaskSetting_View,
                    PermissionNames.Admin_Configuration_SpecialProjectTaskSetting_Edit,
                    PermissionNames.Admin_Configuration_NotificationSetting_View,
                    PermissionNames.Admin_Configuration_NotificationSetting_Edit,
                    PermissionNames.Admin_Configuration_NRITConfig_View,
                    PermissionNames.Admin_Configuration_NRITConfig_Update,
                    PermissionNames.Admin_Configuration_UnlockTimesheetSetting_View,
                    PermissionNames.Admin_Configuration_UnlockTimesheetSetting_Update,
                    PermissionNames.Admin_Configuration_SettingWorkerNoticeKomuPunishmentUserNoCheckInOut_View,
                    PermissionNames.Admin_Configuration_SettingWorkerNoticeKomuPunishmentUserNoCheckInOut_Update,
                    PermissionNames.Admin_Configuration_RetroNotifyConfig_View,
                    PermissionNames.Admin_Configuration_RetroNotifyConfig_Update,
                    PermissionNames.Admin_Configuration_TeamBuilding_View,
                    PermissionNames.Admin_Configuration_TeamBuilding_Update,
                    PermissionNames.Admin_Configuration_TimesCanLateAndEarlyInMonthSetting,
                    PermissionNames.Admin_Configuration_TimesCanLateAndEarlyInMonthSetting_View,
                    PermissionNames.Admin_Configuration_TimesCanLateAndEarlyInMonthSetting_Update,
                    PermissionNames.Admin_Configuration_TimeStartChangingCheckInToCheckOutSetting_View,
                    PermissionNames.Admin_Configuration_TimeStartChangingCheckInToCheckOutSetting_Update,
                    PermissionNames.Admin_Configuration_ApproveTimesheetNotifyConfig_View,
                    PermissionNames.Admin_Configuration_ApproveTimesheetNotifyConfig_Update,
                    PermissionNames.Admin_Configuration_ApproveRequestOffNotifyConfig_View,
                    PermissionNames.Admin_Configuration_ApproveRequestOffNotifyConfig_Update,
                    PermissionNames.Admin_Configuration_SendMessageRequestPendingTeamBuildingToHRConfig_View,
                    PermissionNames.Admin_Configuration_SendMessageRequestPendingTeamBuildingToHRConfig_Update,
                    PermissionNames.Admin_Configuration_NotifyHRTheEmployeeMayHaveLeftConfig_View,
                    PermissionNames.Admin_Configuration_NotifyHRTheEmployeeMayHaveLeftConfig_Update,
                    PermissionNames.Admin_Configuration_MoneyPMUnlockTimeSheetConfig_View,
                    PermissionNames.Admin_Configuration_MoneyPMUnlockTimeSheetConfig_Update,
                    PermissionNames.Admin_Configuration_SendMessageToPunishUserConfig_View,
                    PermissionNames.Admin_Configuration_SendMessageToPunishUserConfig_Update,
                    PermissionNames.Admin_Clients,
                    PermissionNames.Admin_Clients_View,
                    PermissionNames.Admin_Clients_AddNew,
                    PermissionNames.Admin_Clients_Edit,
                    PermissionNames.Admin_Clients_Delete,
                    PermissionNames.Admin_Tasks,
                    PermissionNames.Admin_Tasks_View,
                    PermissionNames.Admin_Tasks_AddNew,
                    PermissionNames.Admin_Tasks_Edit,
                    PermissionNames.Admin_Tasks_Delete,
                    PermissionNames.Admin_Tasks_ChangeStatus,
                    PermissionNames.Admin_LeaveTypes,
                    PermissionNames.Admin_LeaveTypes_View,
                    PermissionNames.Admin_LeaveTypes_AddNew,
                    PermissionNames.Admin_LeaveTypes_Edit,
                    PermissionNames.Admin_LeaveTypes_Delete,
                    PermissionNames.Admin_Branchs,
                    PermissionNames.Admin_Branchs_View,
                    PermissionNames.Admin_Branchs_AddNew,
                    PermissionNames.Admin_Branchs_Edit,
                    PermissionNames.Admin_Branchs_Delete,
                    PermissionNames.Admin_Position,
                    PermissionNames.Admin_Position_View,
                    PermissionNames.Admin_Position_AddNew,
                    PermissionNames.Admin_Position_Edit,
                    PermissionNames.Admin_Position_Delete,

                    PermissionNames.Admin_Capability,
                    PermissionNames.Admin_Capability_View,
                    PermissionNames.Admin_Capability_AddNew,
                    PermissionNames.Admin_Capability_Edit,
                    PermissionNames.Admin_Capability_Delete,
                    PermissionNames.Admin_CapabilitySetting,
                    PermissionNames.Admin_CapabilitySetting_View,
                    PermissionNames.Admin_CapabilitySetting_AddNew,
                    PermissionNames.Admin_CapabilitySetting_Edit,
                    PermissionNames.Admin_CapabilitySetting_Delete,
                    PermissionNames.Admin_CapabilitySetting_Clone,
                    PermissionNames.Admin_BackgroundJob,
                    PermissionNames.Admin_BackgroundJob_View,
                    PermissionNames.Admin_BackgroundJob_Delete,

                    PermissionNames.Admin_AuditLog,
                    PermissionNames.Admin_AuditLog_View,

                    PermissionNames.Project,
                    PermissionNames.Project_View,
                    PermissionNames.Project_View_All,
                    PermissionNames.Project_AddNew,
                    PermissionNames.Project_Edit,
                    PermissionNames.Project_Delete,
                    PermissionNames.Project_ChangeStatus,
                    PermissionNames.Project_UpdateDefaultProjectTask,
                    PermissionNames.Project_ViewDetail,
                    PermissionNames.Project_Export,
                    PermissionNames.Project_Edit_Team_WorkType,
                    PermissionNames.MyTimesheet,
                    PermissionNames.MyTimesheet_View,
                    PermissionNames.MyTimesheet_AddNew,
                    PermissionNames.MyTimesheet_Edit,
                    PermissionNames.MyTimesheet_Delete,
                    PermissionNames.MyTimesheet_Submit,
                    PermissionNames.MyProfile,
                    PermissionNames.MyProfile_View,
                    PermissionNames.MyProfile_RequestUpdateInfo,
                    PermissionNames.Timesheet,
                    PermissionNames.Timesheet_View,
                    PermissionNames.Timesheet_ViewStatus,
                    PermissionNames.Timesheet_Approval,
                    PermissionNames.Timesheet_Export,
                    PermissionNames.TimesheetSupervision,
                    PermissionNames.TimesheetSupervision_View,
                    PermissionNames.MyAbsenceDay,
                    PermissionNames.MyAbsenceDay_View,
                    PermissionNames.MyAbsenceDay_AddNew,
                    PermissionNames.MyAbsenceDay_SendRequest,
                    PermissionNames.MyAbsenceDay_CancelRequest,
                    PermissionNames.AbsenceDay,
                    PermissionNames.AbsenceDay_View,
                    PermissionNames.AbsenceDay_Approval,
                    PermissionNames.AbsenceDayByProject,
                    PermissionNames.AbsenceDayByProject_View,
                    PermissionNames.AbsenceDayByProject_Approval,
                    PermissionNames.AbsenceDayByProject_ViewDetail,
                    PermissionNames.AbsenceDayOfTeam,
                    PermissionNames.AbsenceDayOfTeam_View,
                    PermissionNames.AbsenceDayOfTeam_ViewDetail,
                    PermissionNames.AbsenceDayOfTeam_ExportTeamWorkingCalender,
                    PermissionNames.DayOff,
                    PermissionNames.DayOff_View,
                    PermissionNames.DayOff_AddNew,
                    PermissionNames.DayOff_Edit,
                    PermissionNames.DayOff_Delete,
                    PermissionNames.MyWorkingTime,
                    PermissionNames.MyWorkingTime_View,
                    PermissionNames.MyWorkingTime_RegistrationTime,
                    PermissionNames.MyWorkingTime_Edit,
                    PermissionNames.MyWorkingTime_Delete,
                    PermissionNames.ManageWorkingTime,
                    PermissionNames.ManageWorkingTime_ViewDetail,
                    PermissionNames.ManageWorkingTime_Approval,
                    PermissionNames.Report,
                    PermissionNames.Report_InternsInfo,
                    PermissionNames.Report_InternsInfo_View,
                    PermissionNames.Report_InternsInfo_ViewLevelIntern,
                    PermissionNames.Report_NormalWorking,
                    PermissionNames.Report_NormalWorking_View,
                    PermissionNames.Report_NormalWorking_Export,
                    PermissionNames.Report_NormalWorking_LockUnlockTimesheet,
                    PermissionNames.Report_OverTime,
                    PermissionNames.Report_OverTime_View,
                    PermissionNames.Report_KomuTracker,
                    PermissionNames.Report_KomuTracker_View,
                    PermissionNames.Report_TardinessLeaveEarly,
                    PermissionNames.Report_TardinessLeaveEarly_View,
                    PermissionNames.MyTimeSheet_ViewMyTardinessDetail,
                    PermissionNames.Report_TardinessLeaveEarly_GetData,
                    PermissionNames.Report_TardinessLeaveEarly_ExportExcel,
                    PermissionNames.Report_TardinessLeaveEarly_Edit,
                    PermissionNames.ReviewIntern,
                    PermissionNames.ReviewIntern_ViewAllReport,
                    PermissionNames.ReviewIntern_ExportReport,
                    PermissionNames.ReviewIntern_AddNewReview,
                    PermissionNames.ReviewIntern_AddNewReviewByCapability,
                    PermissionNames.ReviewIntern_ViewAll,
                    PermissionNames.ReviewIntern_Delete,
                    PermissionNames.ReviewIntern_Active,
                    PermissionNames.ReviewIntern_DeActive,
                    PermissionNames.ReviewIntern_ApproveAll,
                    PermissionNames.ReviewIntern_ReviewDetail,
                    PermissionNames.ReviewIntern_ReviewDetail_ViewAll,
                    PermissionNames.ReviewIntern_ReviewDetail_AddNew,
                    PermissionNames.ReviewIntern_ReviewDetail_Update,
                    PermissionNames.ReviewIntern_ReviewDetail_ChangeReviewer,
                    PermissionNames.ReviewIntern_ReviewDetail_Delete,
                    PermissionNames.ReviewIntern_ReviewDetail_ReviewForOneIntern,
                    PermissionNames.ReviewIntern_ReviewDetail_SendEmailForOneIntern,
                    PermissionNames.ReviewIntern_ReviewDetail_UpdateToHRMForOneIntern,
                    PermissionNames.ReviewIntern_ReviewDetail_SendAllEmailsIntern,
                    PermissionNames.ReviewIntern_ReviewDetail_SendAllEmailsOffical,
                    PermissionNames.ReviewIntern_ReviewDetail_UpdateAllToHRMs,
                    PermissionNames.ReviewIntern_ReviewDetail_UpdateStarToProject,
                    PermissionNames.ReviewIntern_ReviewDetail_ApproveForOneIntern,
                    PermissionNames.ReviewIntern_ReviewDetail_ConfirmSalaryForOneIntern,
                    PermissionNames.ReviewIntern_ReviewDetail_RejectForOneIntern,
                    PermissionNames.ReviewIntern_ReviewDetail_RejectSentEmailForOneIntern,
                    PermissionNames.ReviewIntern_ReviewDetail_ViewDetailSubLevel,
                    PermissionNames.ReviewIntern_ReviewDetail_ViewDetailLevel,
                    PermissionNames.ReviewIntern_ReviewDetail_ViewFullSalary,
                    PermissionNames.ReviewIntern_ReviewDetail_UpdateDetailSubLevel,
                    PermissionNames.ReviewIntern_ReviewDetail_UpdateDetailFullSalary,

                    PermissionNames.ReviewIntern_ReviewDetail_ReviewByCapabilityForOneIntern,

                    PermissionNames.Timekeeping_UserNote,
                    PermissionNames.ManageWorkingTime_ViewAll,

                    PermissionNames.OverTimeSetting,
                    PermissionNames.OverTimeSetting_View,
                    PermissionNames.OverTimeSetting_AddNew,
                    PermissionNames.OverTimeSetting_Edit,
                    PermissionNames.OverTimeSetting_Delete,

                    PermissionNames.Retro,
                    PermissionNames.Retro_View,
                    PermissionNames.Retro_AddNew,
                    PermissionNames.Retro_Edit,
                    PermissionNames.Retro_Delete,
                    PermissionNames.Retro_ChangeStatus,
                    PermissionNames.Retro_RetroDetail,
                    PermissionNames.Retro_RetroDetail_ViewAllTeam,
                    PermissionNames.Retro_RetroDetail_ViewMyTeam,
                    PermissionNames.Retro_RetroDetail_AddEmployeeAllTeam,
                    PermissionNames.Retro_RetroDetail_AddEmployeeMyTeam,
                    PermissionNames.Retro_RetroDetail_Delete,
                    PermissionNames.Retro_RetroDetail_Edit,
                    PermissionNames.Retro_RetroDetail_DownloadTemplate,
                    PermissionNames.Retro_RetroDetail_Export,
                    PermissionNames.Retro_RetroDetail_Import,
                    PermissionNames.Retro_RetroDetail_GenerateData,
                    PermissionNames.Retro_RetroDetail_ViewLevel,

                    PermissionNames.TeamBuilding,
                    PermissionNames.TeamBuilding_DetailHR,
                    PermissionNames.TeamBuilding_DetailHR_ViewAllProject,
                    PermissionNames.TeamBuilding_DetailHR_GenerateData,
                    PermissionNames.TeamBuilding_DetailHR_Management,
                    PermissionNames.TeamBuilding_DetailPM,
                    PermissionNames.TeamBuilding_DetailPM_ViewMyProject,
                    PermissionNames.TeamBuilding_DetailPM_CreateRequest,
                    PermissionNames.TeamBuilding_Request,
                    PermissionNames.TeamBuilding_Request_ViewAllRequest,
                    PermissionNames.TeamBuilding_Request_ViewMyRequest,
                    PermissionNames.TeamBuilding_Request_DisburseRequest,
                    PermissionNames.TeamBuilding_Request_EditRequest,
                    PermissionNames.TeamBuilding_Request_ReOpenRequest,
                    PermissionNames.TeamBuilding_Request_RejectRequest,
                    PermissionNames.TeamBuilding_Request_CancelRequest,
                    PermissionNames.TeamBuilding_Request_ViewDetailRequest,
                    PermissionNames.TeamBuilding_Project,
                    PermissionNames.TeamBuilding_Project_SelectProjectTeamBuilding,
                }
            },

            {
                Host.ProjectAdmin,
                new List<string>()
                {
                    PermissionNames.Project,
                    PermissionNames.Project_View,
                    PermissionNames.Project_AddNew,
                    PermissionNames.Project_Edit,
                    PermissionNames.Project_Delete,
                    PermissionNames.Project_ChangeStatus,
                    PermissionNames.Project_UpdateDefaultProjectTask,
                    PermissionNames.Project_ViewDetail,
                    PermissionNames.Project_Export,
                    PermissionNames.MyTimesheet,
                    PermissionNames.MyTimesheet_View,
                    PermissionNames.MyTimesheet_AddNew,
                    PermissionNames.MyTimesheet_Edit,
                    PermissionNames.MyTimesheet_Delete,
                    PermissionNames.MyTimesheet_Submit,
                    PermissionNames.MyProfile,
                    PermissionNames.MyProfile_View,
                    PermissionNames.MyProfile_RequestUpdateInfo,
                    PermissionNames.Timesheet,
                    PermissionNames.Timesheet_View,
                    PermissionNames.Timesheet_ViewStatus,
                    PermissionNames.Timesheet_Approval,
                    PermissionNames.Timesheet_Export,
                    PermissionNames.TimesheetSupervision,
                    PermissionNames.TimesheetSupervision_View,
                    PermissionNames.MyAbsenceDay,
                    PermissionNames.MyAbsenceDay_View,
                    PermissionNames.MyAbsenceDay_AddNew,
                    PermissionNames.MyAbsenceDay_SendRequest,
                    PermissionNames.MyAbsenceDay_CancelRequest,
                    PermissionNames.AbsenceDay,
                    PermissionNames.AbsenceDay_View,
                    PermissionNames.AbsenceDay_Approval,
                    PermissionNames.AbsenceDayByProject,
                    PermissionNames.AbsenceDayByProject_View,
                    PermissionNames.AbsenceDayByProject_Approval,
                    PermissionNames.AbsenceDayByProject_ViewDetail,
                    PermissionNames.AbsenceDayOfTeam,
                    PermissionNames.AbsenceDayOfTeam_View,
                    PermissionNames.AbsenceDayOfTeam_ViewDetail,
                    PermissionNames.AbsenceDayOfTeam_NotifyPm,
                    PermissionNames.MyWorkingTime,
                    PermissionNames.MyWorkingTime_View,
                    PermissionNames.MyWorkingTime_RegistrationTime,
                    PermissionNames.MyWorkingTime_Edit,
                    PermissionNames.MyWorkingTime_Delete,
                    PermissionNames.ManageWorkingTime,
                    PermissionNames.ManageWorkingTime_ViewDetail,
                    PermissionNames.ManageWorkingTime_Approval,
                }
            },
            {
                Host.BasicUser,
                new List<string>()
                {
                    PermissionNames.MyTimesheet,
                    PermissionNames.MyTimesheet_View,
                    PermissionNames.MyTimesheet_AddNew,
                    PermissionNames.MyTimesheet_Edit,
                    PermissionNames.MyTimesheet_Delete,
                    PermissionNames.MyTimesheet_Submit,
                    PermissionNames.MyProfile,
                    PermissionNames.MyProfile_View,
                    PermissionNames.MyProfile_RequestUpdateInfo,
                    PermissionNames.MyAbsenceDay,
                    PermissionNames.MyAbsenceDay_View,
                    PermissionNames.MyAbsenceDay_AddNew,
                    PermissionNames.MyAbsenceDay_SendRequest,
                    PermissionNames.MyAbsenceDay_CancelRequest,
                    PermissionNames.AbsenceDayOfTeam,
                    PermissionNames.AbsenceDayOfTeam_View,
                    PermissionNames.AbsenceDayOfTeam_ViewDetail,
                    PermissionNames.MyWorkingTime,
                    PermissionNames.MyWorkingTime_View,
                    PermissionNames.MyWorkingTime_RegistrationTime,
                    PermissionNames.MyWorkingTime_Edit,
                    PermissionNames.MyWorkingTime_Delete,
                    PermissionNames.Timekeeping_UserNote,
                    PermissionNames.MyTimeSheet_ViewMyTardinessDetail,
                }
            },
            {
                Host.Supervisor,
                new List<string>()
                {
                    PermissionNames.TimesheetSupervision,
                    PermissionNames.TimesheetSupervision_View,
                }
            },
            {
                Host.BranchDirector,
                new List<string>()
                {
                    PermissionNames.Project,
                    PermissionNames.Project_View,
                    PermissionNames.Project_AddNew,
                    PermissionNames.Project_Edit,
                    PermissionNames.Project_Delete,
                    PermissionNames.Project_ChangeStatus,
                    PermissionNames.Project_UpdateDefaultProjectTask,
                    PermissionNames.Project_ViewDetail,
                    PermissionNames.Project_Export,
                    PermissionNames.MyTimesheet,
                    PermissionNames.MyTimesheet_View,
                    PermissionNames.MyTimesheet_AddNew,
                    PermissionNames.MyTimesheet_Edit,
                    PermissionNames.MyTimesheet_Delete,
                    PermissionNames.MyTimesheet_Submit,
                    PermissionNames.MyTimeSheet_ViewMyTardinessDetail,
                    PermissionNames.MyProfile,
                    PermissionNames.MyProfile_View,
                    PermissionNames.MyProfile_RequestUpdateInfo,
                    PermissionNames.Timesheet,
                    PermissionNames.Timesheet_View,
                    PermissionNames.Timesheet_ViewStatus,
                    PermissionNames.Timesheet_Approval,
                    PermissionNames.Timesheet_Export,
                    PermissionNames.TimesheetSupervision,
                    PermissionNames.TimesheetSupervision_View,
                    PermissionNames.MyAbsenceDay,
                    PermissionNames.MyAbsenceDay_View,
                    PermissionNames.MyAbsenceDay_AddNew,
                    PermissionNames.MyAbsenceDay_SendRequest,
                    PermissionNames.MyAbsenceDay_CancelRequest,
                    PermissionNames.AbsenceDay,
                    PermissionNames.AbsenceDay_View,
                    PermissionNames.AbsenceDay_Approval,
                    PermissionNames.AbsenceDayByProject,
                    PermissionNames.AbsenceDayByProject_Approval,
                    PermissionNames.AbsenceDayByProject_ViewDetail,
                    PermissionNames.AbsenceDayByProject_ViewByBranch,
                    PermissionNames.AbsenceDayOfTeam,
                    PermissionNames.AbsenceDayOfTeam_View,
                    PermissionNames.AbsenceDayOfTeam_ViewDetail,
                    PermissionNames.AbsenceDayOfTeam_ExportTeamWorkingCalender,
                    PermissionNames.MyWorkingTime,
                    PermissionNames.MyWorkingTime_View,
                    PermissionNames.MyWorkingTime_RegistrationTime,
                    PermissionNames.MyWorkingTime_Edit,
                    PermissionNames.MyWorkingTime_Delete,
                    PermissionNames.ManageWorkingTime,
                    PermissionNames.ManageWorkingTime_ViewDetail,
                    PermissionNames.ManageWorkingTime_Approval,
                }
            },
        };
    }

    public class SystemPermission
    {
        public string Name { get; set; }
        public MultiTenancySides MultiTenancySides { get; set; }
        public string DisplayName { get; set; }
        public bool IsConfiguration { get; set; }
        public List<SystemPermission> Childrens { get; set; }

        public static List<SystemPermission> ListPermissions = new List<SystemPermission>()
        {
            //new SystemPermission{ Name =  PermissionNames.Home, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Home" },
            new SystemPermission{ Name =  PermissionNames.Admin, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Admin" },
            new SystemPermission{ Name =  PermissionNames.Admin_Users, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Users" },
            new SystemPermission{ Name =  PermissionNames.Admin_Users_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View users" },
            new SystemPermission{ Name =  PermissionNames.Admin_Users_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new user" },
            new SystemPermission{ Name =  PermissionNames.Admin_Users_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit user" },
            new SystemPermission{ Name =  PermissionNames.Admin_Users_EditRole, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit user role" },
            new SystemPermission{ Name =  PermissionNames.Admin_Users_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete user" },
            new SystemPermission{ Name =  PermissionNames.Admin_Users_ResetPassword, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Reset password" },
            new SystemPermission{ Name =  PermissionNames.Admin_Users_UploadAvatar, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Upload avatar" },
            new SystemPermission{ Name =  PermissionNames.Admin_Users_ChangeStatus, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Change status user" },
            new SystemPermission{ Name =  PermissionNames.Admin_Users_ImportWorkingTime, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Import working time" },
            new SystemPermission{ Name =  PermissionNames.Admin_Users_UpdateUserWorkingTime, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update user's working time" },
            new SystemPermission{ Name =  PermissionNames.Admin_Users_ViewLevelUser, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View level user" },
            new SystemPermission{ Name =  PermissionNames.Admin_Roles, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Roles" },
            new SystemPermission{ Name =  PermissionNames.Admin_Roles_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View roles" },
            new SystemPermission{ Name =  PermissionNames.Admin_Roles_ViewDetail, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View detail role" },
            new SystemPermission{ Name =  PermissionNames.Admin_Roles_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new role" },
            new SystemPermission{ Name =  PermissionNames.Admin_Roles_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit role" },
            new SystemPermission{ Name =  PermissionNames.Admin_Roles_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete role" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Configuration" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_Email, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Email setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_Email_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Email setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_WorkingDay, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Working time setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_WorkingDay_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Working time setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_GoogleSignOn, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Google single sign on setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_GoogleSignOn_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Google single sign on setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_AutoLockTimesheet, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Auto lock timesheet setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_AutoLockTimesheet_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Auto lock timesheet setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SercurityCode, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Sercurity code setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SercurityCode_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Sercurity code setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_EmailSaoDo, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Email Sao Do setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_EmailSaoDo_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Email Sao Do setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_LogTimesheetInFuture, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Log timesheet in future" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_LogTimesheetInFuture_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Log timesheet in future" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_AutoSubmitTimesheet, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Auto submit timesheet" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_AutoSubmitTimesheet_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Auto submit timesheet" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_Email_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit Email setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_WorkingDay_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit Working time setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_GoogleSignOn_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit Google single sign on setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_AutoLockTimesheet_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit Auto lock timesheet setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SercurityCode_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit Sercurity code setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_EmailSaoDo_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit Email Sao Do setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_LogTimesheetInFuture_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit Log timesheet in future" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_AutoSubmitTimesheet_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit Auto submit timesheet" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_CheckInSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View CheckIn Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_CheckInSetting_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update CheckIn Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_CheckInSetting, MultiTenancySides = MultiTenancySides.Host , DisplayName = "CheckIn Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_HRMConfig, MultiTenancySides = MultiTenancySides.Host , DisplayName = "HRM Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_HRMConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View HRM Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_HRMConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update HRM Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_ProjectConfig, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Project Tool Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_ProjectConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Project Tool Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_ProjectConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Project Tool Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_LevelSetting, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Level setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_LevelSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View level setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_LevelSetting_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update level setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_CheckInCheckOutPunishmentSetting, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Check in check out punishment setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_CheckInCheckOutPunishmentSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Check in check out punishment setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_CheckInCheckOutPunishmentSetting_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Check in check out punishment setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_KomuConfig, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Komu setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_KomuConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Komu setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_KomuConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Komu setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_RemoteSetting, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Remote Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_RemoteSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Remote Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_RemoteSetting_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Remote Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SpecialProjectTaskSetting, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Special Project Task Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SpecialProjectTaskSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Special Project Task Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SpecialProjectTaskSetting_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Special Project Task Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_NotificationSetting, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Notification Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_NotificationSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Notification Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_NotificationSetting_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Notification Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_NRITConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Notify Review Intern Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_NRITConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Notify Review Intern Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_UnlockTimesheetSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Unlock Timesheet Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_UnlockTimesheetSetting_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Unlock Timesheet Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SettingWorkerNoticeKomuPunishmentUserNoCheckInOut_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Punished Check In Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SettingWorkerNoticeKomuPunishmentUserNoCheckInOut_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Punished Check In Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_RetroNotifyConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Notify Retro Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_RetroNotifyConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Notify Retro Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_TeamBuilding_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Team Building Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_TeamBuilding_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Team Building Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_TimesCanLateAndEarlyInMonthSetting, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Times Can Late And Early In Month Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_TimesCanLateAndEarlyInMonthSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Times Can Late And Early In Month Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_TimesCanLateAndEarlyInMonthSetting_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Times Can Late And Early In Month Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_TimeStartChangingCheckInToCheckOutSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Time Start Changing Checkin To Checkout Setting" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_TimeStartChangingCheckInToCheckOutSetting_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Time Start Changing Checkin To Checkout Setting" },

            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_ApproveTimesheetNotifyConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Approve Timesheet Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_ApproveTimesheetNotifyConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Approve Timesheet Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_ApproveRequestOffNotifyConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Approve Request Off Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_ApproveRequestOffNotifyConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Approve Request Off Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SendMessageRequestPendingTeamBuildingToHRConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Send Message Request Pending Team Building To HR Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SendMessageRequestPendingTeamBuildingToHRConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Send Message Request Pending Team Building To HR Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_NotifyHRTheEmployeeMayHaveLeftConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Notify HR The Employee May Have Left Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_NotifyHRTheEmployeeMayHaveLeftConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Notify HR The Employee May Have Left Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_MoneyPMUnlockTimeSheetConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Money PM Unlock TimeSheets Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_MoneyPMUnlockTimeSheetConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Money PM Unlock TimeSheets Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SendMessageToPunishUserConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Send Message To Punish User Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SendMessageToPunishUserConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Send Message To Punish User Config" },
            new SystemPermission{ Name =  PermissionNames.Admin_Clients, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Clients" },
            new SystemPermission{ Name =  PermissionNames.Admin_Clients_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View clients" },
            new SystemPermission{ Name =  PermissionNames.Admin_Clients_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new client" },
            new SystemPermission{ Name =  PermissionNames.Admin_Clients_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit client" },
            new SystemPermission{ Name =  PermissionNames.Admin_Clients_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete client" },
            new SystemPermission{ Name =  PermissionNames.Admin_Tasks, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Tasks" },
            new SystemPermission{ Name =  PermissionNames.Admin_Tasks_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View tasks" },
            new SystemPermission{ Name =  PermissionNames.Admin_Tasks_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new task" },
            new SystemPermission{ Name =  PermissionNames.Admin_Tasks_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit task" },
            new SystemPermission{ Name =  PermissionNames.Admin_Tasks_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete task" },
            new SystemPermission{ Name =  PermissionNames.Admin_Tasks_ChangeStatus, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Change status task" },
            new SystemPermission{ Name =  PermissionNames.Admin_LeaveTypes, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Leave types" },
            new SystemPermission{ Name =  PermissionNames.Admin_LeaveTypes_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View leave types" },
            new SystemPermission{ Name =  PermissionNames.Admin_LeaveTypes_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new leave type" },
            new SystemPermission{ Name =  PermissionNames.Admin_LeaveTypes_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit leave type" },
            new SystemPermission{ Name =  PermissionNames.Admin_LeaveTypes_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete leave type" },
            new SystemPermission{ Name =  PermissionNames.Admin_Branchs, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Branchs" },
            new SystemPermission{ Name =  PermissionNames.Admin_Branchs_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View branchs" },
            new SystemPermission{ Name =  PermissionNames.Admin_Branchs_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new branch" },
            new SystemPermission{ Name =  PermissionNames.Admin_Branchs_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit branch" },
            new SystemPermission{ Name =  PermissionNames.Admin_Branchs_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete branch" },
            new SystemPermission{ Name =  PermissionNames.Admin_Position, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Position" },
            new SystemPermission{ Name =  PermissionNames.Admin_Position_View, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View position"},
            new SystemPermission{ Name =  PermissionNames.Admin_Position_AddNew, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Add new position"},
            new SystemPermission{ Name =  PermissionNames.Admin_Position_Edit, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Edit position"},
            new SystemPermission{ Name =  PermissionNames.Admin_Position_Delete, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Delete position"},

            new SystemPermission{ Name =  PermissionNames.Admin_Capability, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Capability" },
            new SystemPermission{ Name =  PermissionNames.Admin_Capability_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View capability" },
            new SystemPermission{ Name =  PermissionNames.Admin_Capability_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new capability" },
            new SystemPermission{ Name =  PermissionNames.Admin_Capability_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit capability" },
            new SystemPermission{ Name =  PermissionNames.Admin_Capability_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete capability" },
            new SystemPermission{ Name =  PermissionNames.Admin_CapabilitySetting, MultiTenancySides = MultiTenancySides.Host , DisplayName = "CapabilitySetting" },
            new SystemPermission{ Name =  PermissionNames.Admin_CapabilitySetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View capabilitySetting" },
            new SystemPermission{ Name =  PermissionNames.Admin_CapabilitySetting_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new capabilitySetting" },
            new SystemPermission{ Name =  PermissionNames.Admin_CapabilitySetting_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit capabilitySetting" },
            new SystemPermission{ Name =  PermissionNames.Admin_CapabilitySetting_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete capabilitySetting" },
            new SystemPermission{ Name =  PermissionNames.Admin_CapabilitySetting_Clone, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Clone capabilitySetting" },

            new SystemPermission{ Name =  PermissionNames.Admin_BackgroundJob ,MultiTenancySides = MultiTenancySides.Host, DisplayName = "Background Job"},
            new SystemPermission{ Name =  PermissionNames.Admin_BackgroundJob_View ,MultiTenancySides = MultiTenancySides.Host, DisplayName = "View"},
            new SystemPermission{ Name =  PermissionNames.Admin_BackgroundJob_Delete ,MultiTenancySides = MultiTenancySides.Host, DisplayName = "Delete"},
            new SystemPermission{ Name =  PermissionNames.Admin_AuditLog, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Auditlogs" },
            new SystemPermission{ Name =  PermissionNames.Admin_AuditLog_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View AuditLogs" },

            new SystemPermission{ Name =  PermissionNames.Project, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Projects" },
            new SystemPermission{ Name =  PermissionNames.Project_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View my projects" },
            new SystemPermission{ Name =  PermissionNames.Project_View_All, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View all projects" },
            new SystemPermission{ Name =  PermissionNames.Project_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new project" },
            new SystemPermission{ Name =  PermissionNames.Project_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit project" },
            new SystemPermission{ Name =  PermissionNames.Project_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete project" },
            new SystemPermission{ Name =  PermissionNames.Project_ViewDetail, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View detail of project" },
            new SystemPermission{ Name =  PermissionNames.Project_ChangeStatus, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Change status of project" },
            new SystemPermission{ Name =  PermissionNames.Project_UpdateDefaultProjectTask, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Set default project task" },
            new SystemPermission{ Name =  PermissionNames.Project_Edit_Team_WorkType, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit Team Work Type (Temp/Official)" },
            new SystemPermission{ Name =  PermissionNames.Project_Export, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Export excel" },
            new SystemPermission{ Name =  PermissionNames.MyTimesheet, MultiTenancySides = MultiTenancySides.Host , DisplayName = "My timesheets" },
            new SystemPermission{ Name =  PermissionNames.MyTimesheet_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View my timesheet by day/week" },
            new SystemPermission{ Name =  PermissionNames.MyTimesheet_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new timesheet" },
            new SystemPermission{ Name =  PermissionNames.MyTimesheet_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit timesheet" },
            new SystemPermission{ Name =  PermissionNames.MyTimesheet_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete timesheet" },
            new SystemPermission{ Name =  PermissionNames.MyTimesheet_Submit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Submit timesheet" },
            new SystemPermission{ Name =  PermissionNames.MyProfile, MultiTenancySides = MultiTenancySides.Host , DisplayName = "My Profile" },
            new SystemPermission{ Name =  PermissionNames.MyProfile_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
            new SystemPermission{ Name =  PermissionNames.MyProfile_RequestUpdateInfo, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Request Update Info" },
            new SystemPermission{ Name =  PermissionNames.Timesheet, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Timesheets" },
            new SystemPermission{ Name =  PermissionNames.Timesheet_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View timesheets" },
            new SystemPermission{ Name =  PermissionNames.Timesheet_ViewStatus, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View status timesheets" },
            new SystemPermission{ Name =  PermissionNames.Timesheet_Approval, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Approval timesheet" },
            new SystemPermission{ Name =  PermissionNames.Timesheet_Export, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Export excel" },
            new SystemPermission{ Name =  PermissionNames.TimesheetSupervision, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Timesheet mornitoring" },
            new SystemPermission{ Name =  PermissionNames.TimesheetSupervision_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View timesheets" },
            new SystemPermission{ Name =  PermissionNames.MyAbsenceDay, MultiTenancySides = MultiTenancySides.Host, DisplayName = "My leave day / onsite" },
            new SystemPermission{ Name =  PermissionNames.MyAbsenceDay_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View my leave/onsite requests" },
            new SystemPermission{ Name =  PermissionNames.MyAbsenceDay_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new my leave/onsite day" },
            new SystemPermission{ Name =  PermissionNames.MyAbsenceDay_SendRequest, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Send my leave/onsite request" },
            new SystemPermission{ Name =  PermissionNames.MyAbsenceDay_CancelRequest, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Cancel my leave/onsite request" },
            new SystemPermission{ Name =  PermissionNames.AbsenceDay, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Leave days" },
            new SystemPermission{ Name =  PermissionNames.AbsenceDay_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View leave/onsite requests" },
            new SystemPermission{ Name =  PermissionNames.AbsenceDay_Approval, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Approval requests" },
            new SystemPermission{ Name =  PermissionNames.AbsenceDayByProject, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Manage team working calendar" },
            new SystemPermission{ Name =  PermissionNames.AbsenceDayByProject_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View users leave / onsite by project" },
            new SystemPermission{ Name =  PermissionNames.AbsenceDayByProject_ViewByBranch, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View users leave / onsite by branch" },
            new SystemPermission{ Name =  PermissionNames.AbsenceDayByProject_Approval, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Approval requests" },
            new SystemPermission{ Name =  PermissionNames.AbsenceDayByProject_ViewDetail, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View detail request" },
            new SystemPermission{ Name =  PermissionNames.AbsenceDayOfTeam, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Team working calendar" },
            new SystemPermission{ Name =  PermissionNames.AbsenceDayOfTeam_View, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View your team member leave / onsite by project" },
            new SystemPermission{ Name =  PermissionNames.AbsenceDayOfTeam_ViewDetail, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View detail leave / onsite of your team member" },
            new SystemPermission{ Name =  PermissionNames.AbsenceDayOfTeam_ExportTeamWorkingCalender, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Export team working calender" },
            new SystemPermission{ Name =  PermissionNames.AbsenceDayOfTeam_NotifyPm, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Allow to push notify to PM"},
            new SystemPermission{ Name =  PermissionNames.DayOff, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Setting off days" },
            new SystemPermission{ Name =  PermissionNames.DayOff_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View day offs" },
            new SystemPermission{ Name =  PermissionNames.DayOff_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new day off" },
            new SystemPermission{ Name =  PermissionNames.DayOff_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit day off" },
            new SystemPermission{ Name =  PermissionNames.DayOff_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete day off" },
            new SystemPermission{ Name =  PermissionNames.MyWorkingTime, MultiTenancySides = MultiTenancySides.Host , DisplayName = "My working time" },
            new SystemPermission{ Name =  PermissionNames.MyWorkingTime_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View my working time" },
            new SystemPermission{ Name =  PermissionNames.MyWorkingTime_RegistrationTime, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Registration new working time" },
            new SystemPermission{ Name =  PermissionNames.MyWorkingTime_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit registration working time" },
            new SystemPermission{ Name =  PermissionNames.MyWorkingTime_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete registration working time" },
            new SystemPermission{ Name =  PermissionNames.ManageWorkingTime, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Manage working time" },
            new SystemPermission{ Name =  PermissionNames.ManageWorkingTime_ViewAll, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View all user working time " },
            new SystemPermission{ Name =  PermissionNames.ManageWorkingTime_ViewDetail, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View my team member Working Time" },
            new SystemPermission{ Name =  PermissionNames.ManageWorkingTime_Approval, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Approval working time of member" },
            new SystemPermission{ Name =  PermissionNames.Report, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Report" },
            new SystemPermission{ Name =  PermissionNames.Report_InternsInfo, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Interns info" },
            new SystemPermission{ Name =  PermissionNames.Report_InternsInfo_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View interns info" },
            new SystemPermission{ Name =  PermissionNames.Report_InternsInfo_ViewLevelIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View level intern" },
            new SystemPermission{ Name =  PermissionNames.Report_NormalWorking, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Normal working" },
            new SystemPermission{ Name =  PermissionNames.Report_NormalWorking_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View normal working" },
            new SystemPermission{ Name =  PermissionNames.Report_NormalWorking_Export, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Export excel" },
            new SystemPermission{ Name =  PermissionNames.Report_NormalWorking_LockUnlockTimesheet, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Lock/unlock timesheet" },
            new SystemPermission{ Name =  PermissionNames.Report_OverTime, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Over time" },
            new SystemPermission{ Name =  PermissionNames.Report_OverTime_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View overtime" },
            new SystemPermission{ Name =  PermissionNames.Report_KomuTracker, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Komu tracker" },
            new SystemPermission{ Name =  PermissionNames.Report_KomuTracker_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Komu tracker" },
            new SystemPermission{ Name =  PermissionNames.Report_TardinessLeaveEarly, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Tardisness / leave early" },
            new SystemPermission{ Name =  PermissionNames.Report_TardinessLeaveEarly_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View user tardiness" },
            new SystemPermission{ Name =  PermissionNames.MyTimeSheet_ViewMyTardinessDetail, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View my tardiness detail" },
            new SystemPermission{ Name =  PermissionNames.Report_TardinessLeaveEarly_GetData, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Process tardiness data from FaceID" },
            new SystemPermission{ Name =  PermissionNames.Report_TardinessLeaveEarly_ExportExcel, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Export excel" },
            new SystemPermission{ Name =  PermissionNames.Report_TardinessLeaveEarly_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit checkin, checkout time of user" },
            new SystemPermission{ Name =  PermissionNames.Timekeeping_UserNote, MultiTenancySides = MultiTenancySides.Host , DisplayName = "User khiếu lại đi muộn" },
            new SystemPermission{ Name =  PermissionNames.Timekeeping_ReplyUserNote, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Trả lời khiếu lại, chốt phạt user" },

            new SystemPermission{ Name =  PermissionNames.ReviewIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Review interns" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ViewAllReport, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View all report" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ExportReport, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Export report" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_AddNewReview, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Create review phase" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_AddNewReviewByCapability, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Create review by capability phase" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ViewAll, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View all review phase" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete review phase" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_Active, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Active review phase" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_DeActive, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Deactive review phase" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ApproveAll, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Approve all" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Review Detail" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ViewAll, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View all review detail" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new review detail" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update review detail" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ChangeReviewer, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Change reviewer review detail" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete review detail" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ReviewForOneIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Review (for one intern)" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ReviewByCapabilityForOneIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Review by capability(for one intern)"},
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_SendEmailForOneIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Send email (for one intern)"},
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_UpdateToHRMForOneIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update to hrm (for one intern)"},
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_SendAllEmailsIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Send all emails Intern" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_SendAllEmailsOffical, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Send all emails Offical" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_UpdateAllToHRMs, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Send to HRM" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_UpdateStarToProject, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Star Rating to Project" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ApproveForOneIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Approve review (for one intern)" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ConfirmSalaryForOneIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Chốt lương (for one intern)" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_RejectForOneIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Reject review (for one intern)" },
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_RejectSentEmailForOneIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Reject sent mail (for one intern) by CEO"},
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ViewDetailLevel, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Level"},
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ViewDetailSubLevel, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Sublevel"},
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ViewFullSalary, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Full Lương"},
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_UpdateDetailSubLevel, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Sublevel"},
            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_UpdateDetailFullSalary, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Full Lương"},
            new SystemPermission{ Name =  PermissionNames.OverTimeSetting, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Setting overtime" },
            new SystemPermission{ Name =  PermissionNames.OverTimeSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View overtime settings" },
            new SystemPermission{ Name =  PermissionNames.OverTimeSetting_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new overtime setting" },
            new SystemPermission{ Name =  PermissionNames.OverTimeSetting_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit overtime setting" },
            new SystemPermission{ Name =  PermissionNames.OverTimeSetting_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete overtime setting" },

            new SystemPermission{ Name =  PermissionNames.Retro, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Retro" },
            new SystemPermission{ Name =  PermissionNames.Retro_View, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View retro"},
            new SystemPermission{ Name =  PermissionNames.Retro_AddNew, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Add new retro"},
            new SystemPermission{ Name =  PermissionNames.Retro_Edit, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Edit retro"},
            new SystemPermission{ Name =  PermissionNames.Retro_Delete, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Delete retro"},
            new SystemPermission{ Name =  PermissionNames.Retro_ChangeStatus, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Change status retro"},

            new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Retro detail" },
            new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_ViewAllTeam, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View all team"},
            new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_ViewMyTeam, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View my team"},
            new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_AddEmployeeAllTeam, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add employee all team" },
            new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_AddEmployeeMyTeam, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add employee my team" },
            new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit employee" },
            new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete employee" },
            new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_DownloadTemplate, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Download template" },
            new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_Import, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Import employee" },
            new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_Export, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Export employee"},
            new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_GenerateData, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Generate data employee"},
            new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_ViewLevel, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View level"},

            new SystemPermission{ Name =  PermissionNames.TeamBuilding, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Team building" },
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_DetailHR, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Team building detail HR"},
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_DetailHR_ViewAllProject, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View all project"},
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_DetailHR_GenerateData, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Generate data"},
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_DetailHR_Management, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Management"},
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_DetailPM, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Team building detail PM"},
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_DetailPM_ViewMyProject, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View my project"},
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_DetailPM_CreateRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Create request money"},
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Team building request"},
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request_ViewAllRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View all request"},
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request_ViewMyRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View my request"},
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request_DisburseRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Disburse request"},
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request_EditRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Edit request"},
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request_ReOpenRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Re-open request"},
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request_RejectRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Reject request"},
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request_CancelRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Cancel request"},
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request_ViewDetailRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View detail request"},
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Project, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Team building project"},
            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Project_SelectProjectTeamBuilding, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Select project team building"},
        };

        public static List<SystemPermission> TreePermissions = new List<SystemPermission>()
        {
            //new SystemPermission{ Name =  PermissionNames.Home, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Home" },
            new SystemPermission{ Name =  PermissionNames.Admin, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Admin",
                Childrens = new List<SystemPermission>() {
                    new SystemPermission{ Name =  PermissionNames.Admin_Users, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Users",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.Admin_Users_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View users" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Users_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new user" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Users_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit user" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Users_EditRole, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit user role" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Users_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete user" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Users_ResetPassword, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Reset password" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Users_UploadAvatar, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Upload avatar" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Users_ChangeStatus, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Change status user" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Users_ImportWorkingTime, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Import working time" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Users_UpdateUserWorkingTime, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update user's working time" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Users_ViewLevelUser, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View level user" },
                        }
                    },
                    new SystemPermission{ Name =  PermissionNames.Admin_Roles, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Roles",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.Admin_Roles_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View roles" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Roles_ViewDetail, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View detail role" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Roles_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new role" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Roles_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit role" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Roles_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete role" },
                        }
                    },
                    new SystemPermission{ Name =  PermissionNames.Admin_Configuration, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Configuration",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_Email, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Email Setting",
                                Childrens = new List<SystemPermission>()
                                {
                                    new SystemPermission{ Name =  PermissionNames.Admin_Configuration_Email_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                    new SystemPermission{ Name =  PermissionNames.Admin_Configuration_Email_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                }
                            },
                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_WorkingDay, MultiTenancySides = MultiTenancySides.Host , DisplayName = " Working Time Setting",
                                Childrens = new List<SystemPermission>()
                                {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_WorkingDay_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_WorkingDay_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                }
                            },
                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_GoogleSignOn, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Google Single Sign On Setting",
                                Childrens = new List<SystemPermission>()
                                {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_GoogleSignOn_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_GoogleSignOn_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                }
                            },

                              new SystemPermission{ Name =  PermissionNames.Admin_Configuration_AutoLockTimesheet, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Auto Lock Timesheet Setting",
                                Childrens = new List<SystemPermission>()
                                {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_AutoLockTimesheet_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_AutoLockTimesheet_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                }
                            },
                                new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SercurityCode, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Sercurity Code Setting",
                                Childrens = new List<SystemPermission>()
                                {
                                      new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SercurityCode_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                      new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SercurityCode_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                }
                            },
                                new SystemPermission{ Name =  PermissionNames.Admin_Configuration_LogTimesheetInFuture, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Log Timesheet Setting",
                                Childrens = new List<SystemPermission>()
                                 {
                                      new SystemPermission{ Name =  PermissionNames.Admin_Configuration_LogTimesheetInFuture_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                      new SystemPermission{ Name =  PermissionNames.Admin_Configuration_LogTimesheetInFuture_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },
                                new SystemPermission{ Name =  PermissionNames.Admin_Configuration_AutoSubmitTimesheet, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Auto Submit Timesheet",
                                Childrens = new List<SystemPermission>()
                                 {
                                      new SystemPermission{ Name =  PermissionNames.Admin_Configuration_AutoSubmitTimesheet_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                      new SystemPermission{ Name =  PermissionNames.Admin_Configuration_AutoSubmitTimesheet_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },
                                 new SystemPermission{ Name =  PermissionNames.Admin_Configuration_CheckInSetting, MultiTenancySides = MultiTenancySides.Host , DisplayName = "CheckIn Setting",
                                 Childrens = new List<SystemPermission>()
                                 {
                                    new SystemPermission{ Name =  PermissionNames.Admin_Configuration_CheckInSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                    new SystemPermission{ Name =  PermissionNames.Admin_Configuration_CheckInSetting_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },
                                 new SystemPermission{ Name =  PermissionNames.Admin_Configuration_HRMConfig, MultiTenancySides = MultiTenancySides.Host , DisplayName = "HRM Config",
                                 Childrens = new List<SystemPermission>()
                                 {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_HRMConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_HRMConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },

                                 new SystemPermission{ Name =  PermissionNames.Admin_Configuration_ProjectConfig ,MultiTenancySides = MultiTenancySides.Host , DisplayName = "Project Config",
                                 Childrens = new List<SystemPermission>()
                                 {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_ProjectConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_ProjectConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },
                                 new SystemPermission{ Name =  PermissionNames.Admin_Configuration_LevelSetting ,MultiTenancySides = MultiTenancySides.Host , DisplayName = "Level setting",
                                 Childrens = new List<SystemPermission>()
                                 {
                                      new SystemPermission{ Name =  PermissionNames.Admin_Configuration_LevelSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                      new SystemPermission{ Name =  PermissionNames.Admin_Configuration_LevelSetting_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },
                                 new SystemPermission{ Name =  PermissionNames.Admin_Configuration_CheckInCheckOutPunishmentSetting ,MultiTenancySides = MultiTenancySides.Host , DisplayName = "Check in check out punishment setting",
                                 Childrens = new List<SystemPermission>()
                                 {
                                      new SystemPermission{ Name =  PermissionNames.Admin_Configuration_CheckInCheckOutPunishmentSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                      new SystemPermission{ Name =  PermissionNames.Admin_Configuration_CheckInCheckOutPunishmentSetting_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },
                                  new SystemPermission{ Name =  PermissionNames.Admin_Configuration_EmailSaoDo ,MultiTenancySides = MultiTenancySides.Host , DisplayName = "Email Sao Do Setting",
                                  Childrens = new List<SystemPermission>()
                                  {
                                      new SystemPermission{ Name =  PermissionNames.Admin_Configuration_EmailSaoDo_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                      new SystemPermission{ Name =  PermissionNames.Admin_Configuration_EmailSaoDo_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                  }
                            },
                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_RemoteSetting ,MultiTenancySides = MultiTenancySides.Host , DisplayName = "Remote Setting",
                                  Childrens = new List<SystemPermission>()
                                  {
                                       new SystemPermission{ Name =  PermissionNames.Admin_Configuration_RemoteSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                        new SystemPermission{ Name =  PermissionNames.Admin_Configuration_RemoteSetting_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                  }
                            },

                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_KomuConfig ,MultiTenancySides = MultiTenancySides.Host , DisplayName = "Komu Setting",
                                  Childrens = new List<SystemPermission>()
                                  {
                                      new SystemPermission{ Name =  PermissionNames.Admin_Configuration_KomuConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                      new SystemPermission{ Name =  PermissionNames.Admin_Configuration_KomuConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                  }
                            },
                             new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SpecialProjectTaskSetting ,MultiTenancySides = MultiTenancySides.Host , DisplayName = "Special Project Task Setting",
                                  Childrens = new List<SystemPermission>()
                                  {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SpecialProjectTaskSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                    new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SpecialProjectTaskSetting_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                  }
                            },
                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_NotificationSetting ,MultiTenancySides = MultiTenancySides.Host , DisplayName = "Notification Setting",
                                  Childrens = new List<SystemPermission>()
                                  {
                                       new SystemPermission{ Name =  PermissionNames.Admin_Configuration_NotificationSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                       new SystemPermission{ Name =  PermissionNames.Admin_Configuration_NotificationSetting_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                  }
                            },
                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_NRITConfig, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Notify Review Intern Setting",
                                 Childrens = new List<SystemPermission>()
                                 {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_NRITConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_NRITConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },
                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_UnlockTimesheetSetting, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Unlock Timesheet Setting",
                                 Childrens = new List<SystemPermission>()
                                 {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_UnlockTimesheetSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_UnlockTimesheetSetting_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },
                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SettingWorkerNoticeKomuPunishmentUserNoCheckInOut, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Setting Worker Notice Komu Punishment User No Check In/Out",
                                 Childrens = new List<SystemPermission>()
                                 {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SettingWorkerNoticeKomuPunishmentUserNoCheckInOut_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SettingWorkerNoticeKomuPunishmentUserNoCheckInOut_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },
                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_RetroNotifyConfig, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Notify Retro Setting",
                                 Childrens = new List<SystemPermission>()
                                 {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_RetroNotifyConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_RetroNotifyConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },
                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_TeamBuilding, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Team Building Setting",
                                 Childrens = new List<SystemPermission>()
                                 {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_TeamBuilding_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_TeamBuilding_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },
                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_TimesCanLateAndEarlyInMonthSetting, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Times Can Late And Early In Month Setting",
                                 Childrens = new List<SystemPermission>()
                                 {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_TimesCanLateAndEarlyInMonthSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_TimesCanLateAndEarlyInMonthSetting_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },
                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_TimeStartChangingCheckInToCheckOutSetting, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Time Start Changing Checkin To Checkout Setting",
                                 Childrens = new List<SystemPermission>()
                                 {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_TimeStartChangingCheckInToCheckOutSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_TimeStartChangingCheckInToCheckOutSetting_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },
                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_ApproveTimesheetNotifyConfig, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Setting Worker Notice Approve Timesheet",
                                 Childrens = new List<SystemPermission>()
                                 {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_ApproveTimesheetNotifyConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_ApproveTimesheetNotifyConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },
                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_ApproveRequestOffNotifyConfig, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Setting Worker Notice Approve Request Off",
                                 Childrens = new List<SystemPermission>()
                                 {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_ApproveRequestOffNotifyConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_ApproveRequestOffNotifyConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },
                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SendMessageRequestPendingTeamBuildingToHRConfig, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Send Message Request Pending TeamBuilding To HR",
                                 Childrens = new List<SystemPermission>()
                                 {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SendMessageRequestPendingTeamBuildingToHRConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SendMessageRequestPendingTeamBuildingToHRConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },
                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_NotifyHRTheEmployeeMayHaveLeftConfig, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Notify HR The Employee May Have Left",
                                 Childrens = new List<SystemPermission>()
                                 {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_NotifyHRTheEmployeeMayHaveLeftConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_NotifyHRTheEmployeeMayHaveLeftConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },

                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_MoneyPMUnlockTimeSheetConfig, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Money PM Unlock TimeSheet",
                                 Childrens = new List<SystemPermission>()
                                 {
                                     new SystemPermission{Name = PermissionNames.Admin_Configuration_MoneyPMUnlockTimeSheetConfig_View, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View"},
                                     new SystemPermission{Name = PermissionNames.Admin_Configuration_MoneyPMUnlockTimeSheetConfig_Update, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Edit"},
                                 }
                            },
                            new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SendMessageToPunishUserConfig, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Send Message To Punish User",
                                 Childrens = new List<SystemPermission>()
                                 {
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SendMessageToPunishUserConfig_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                                     new SystemPermission{ Name =  PermissionNames.Admin_Configuration_SendMessageToPunishUserConfig_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit" },
                                 }
                            },
                        }
                    },
                    new SystemPermission{ Name =  PermissionNames.Admin_Clients, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Clients",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.Admin_Clients_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View clients" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Clients_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new client" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Clients_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit client" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Clients_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete client" },
                        }
                    },
                    new SystemPermission{ Name =  PermissionNames.Admin_Tasks, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Tasks",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.Admin_Tasks_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View tasks" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Tasks_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new task" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Tasks_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit task" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Tasks_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete task" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Tasks_ChangeStatus, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Change status task" },
                        }
                    },
                    new SystemPermission{ Name =  PermissionNames.Admin_LeaveTypes, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Leave types",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.Admin_LeaveTypes_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View leave types" },
                            new SystemPermission{ Name =  PermissionNames.Admin_LeaveTypes_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new leave type" },
                            new SystemPermission{ Name =  PermissionNames.Admin_LeaveTypes_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit leave type" },
                            new SystemPermission{ Name =  PermissionNames.Admin_LeaveTypes_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete leave type" },
                        }
                    },
                    new SystemPermission{ Name =  PermissionNames.Admin_Branchs, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Branchs",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.Admin_Branchs_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View branchs" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Branchs_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new branch" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Branchs_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit branch" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Branchs_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete branch" },
                        }
                    },
                    new SystemPermission{ Name =  PermissionNames.Admin_Position, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Position",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.Admin_Position_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View position" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Position_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new position" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Position_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit position" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Position_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete position" },
                        }
                    },
                      new SystemPermission{ Name =  PermissionNames.Admin_Capability, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Capability",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.Admin_Capability_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View capability" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Capability_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new capability" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Capability_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit capability" },
                            new SystemPermission{ Name =  PermissionNames.Admin_Capability_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete capability" },
                        }
                    },
                     new SystemPermission{ Name =  PermissionNames.Admin_CapabilitySetting, MultiTenancySides = MultiTenancySides.Host , DisplayName = "CapabilitySetting",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.Admin_CapabilitySetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View capabilitySetting" },
                            new SystemPermission{ Name =  PermissionNames.Admin_CapabilitySetting_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new capabilitySetting" },
                            new SystemPermission{ Name =  PermissionNames.Admin_CapabilitySetting_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit capabilitySetting" },
                            new SystemPermission{ Name =  PermissionNames.Admin_CapabilitySetting_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete capabilitySetting" },
                            new SystemPermission{ Name =  PermissionNames.Admin_CapabilitySetting_Clone, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Clone capabilitySetting" },
                        }
                    },
                     new SystemPermission{ Name =  PermissionNames.Admin_AuditLog, MultiTenancySides = MultiTenancySides.Host , DisplayName = "AuditLogs",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.Admin_AuditLog_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View AuditLogs" },
                        }
                    },
                    new SystemPermission{ Name =  PermissionNames.DayOff, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Setting off days",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.DayOff_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View day offs" },
                            new SystemPermission{ Name =  PermissionNames.DayOff_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new day off" },
                            new SystemPermission{ Name =  PermissionNames.DayOff_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit day off" },
                            new SystemPermission{ Name =  PermissionNames.DayOff_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete day off" },
                        }
                    },
                     new SystemPermission{ Name =  PermissionNames.OverTimeSetting, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Setting overtime",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.OverTimeSetting_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View overtime settings" },
                            new SystemPermission{ Name =  PermissionNames.OverTimeSetting_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new overtime setting" },
                            new SystemPermission{ Name =  PermissionNames.OverTimeSetting_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit overtime setting" },
                            new SystemPermission{ Name =  PermissionNames.OverTimeSetting_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete overtime setting" },
                        }
                    },
                     new SystemPermission{ Name =  PermissionNames.Admin_BackgroundJob ,MultiTenancySides = MultiTenancySides.Host | MultiTenancySides.Tenant, DisplayName = "Background Job",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.Admin_BackgroundJob_View ,MultiTenancySides = MultiTenancySides.Host | MultiTenancySides.Tenant, DisplayName = "View"},
                            new SystemPermission{ Name =  PermissionNames.Admin_BackgroundJob_Delete ,MultiTenancySides = MultiTenancySides.Host | MultiTenancySides.Tenant, DisplayName = "Delete"},
                        }
                     },
                }
            },
            new SystemPermission{ Name =  PermissionNames.Project, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Projects" ,
                Childrens = new List<SystemPermission>()
                {
                    new SystemPermission{ Name =  PermissionNames.Project_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View my projects" },
                    new SystemPermission{ Name =  PermissionNames.Project_View_All, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View all projects" },
                    new SystemPermission{ Name =  PermissionNames.Project_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new project" },
                    new SystemPermission{ Name =  PermissionNames.Project_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit project" },
                    new SystemPermission{ Name =  PermissionNames.Project_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete project" },
                    new SystemPermission{ Name =  PermissionNames.Project_ChangeStatus, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Change status of project" },
                    new SystemPermission{ Name =  PermissionNames.Project_ViewDetail, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View detail of project" },
                    new SystemPermission{ Name =  PermissionNames.Project_Export, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Export excel" },
                    new SystemPermission{ Name =  PermissionNames.Project_Edit_Team_WorkType, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit Team Work Type (Temp/Official)" },
                }
            },
            new SystemPermission{ Name =  PermissionNames.MyProfile, MultiTenancySides = MultiTenancySides.Host , DisplayName = "My profile",
                Childrens = new List<SystemPermission>()
                {
                    new SystemPermission{ Name =  PermissionNames.MyProfile_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View" },
                    new SystemPermission{ Name =  PermissionNames.MyProfile_RequestUpdateInfo, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Request Update Info" },
                }
            },
            new SystemPermission{ Name =  PermissionNames.MyTimesheet, MultiTenancySides = MultiTenancySides.Host , DisplayName = "My timesheets",
                Childrens = new List<SystemPermission>()
                {
                    new SystemPermission{ Name =  PermissionNames.MyTimesheet_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View my timesheet by day/week" },
                    new SystemPermission{ Name =  PermissionNames.MyTimesheet_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new timesheet" },
                    new SystemPermission{ Name =  PermissionNames.MyTimesheet_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit timesheet" },
                    new SystemPermission{ Name =  PermissionNames.MyTimesheet_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete timesheet" },
                    new SystemPermission{ Name =  PermissionNames.MyTimesheet_Submit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Submit timesheet" },
                    new SystemPermission{ Name =  PermissionNames.Timekeeping_UserNote, MultiTenancySides = MultiTenancySides.Host , DisplayName = "User khiếu lại đi muộn" },
                    new SystemPermission{ Name =  PermissionNames.MyTimeSheet_ViewMyTardinessDetail, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View my tardiness detail" },
                    new SystemPermission{ Name =  PermissionNames.Project_UpdateDefaultProjectTask, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Set default project task" },
                }
            },
            new SystemPermission{ Name =  PermissionNames.MyAbsenceDay, MultiTenancySides = MultiTenancySides.Host, DisplayName = "My request off/remote/onsite",
                Childrens = new List<SystemPermission>()
                {
                    new SystemPermission{ Name =  PermissionNames.MyAbsenceDay_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View my off/remote/onsite requests" },
                    //new SystemPermission{ Name =  PermissionNames.MyAbsenceDay_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add my off/remote/onsite day" },
                    new SystemPermission{ Name =  PermissionNames.MyAbsenceDay_SendRequest, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Send my leave/remote/onsite request" },
                    new SystemPermission{ Name =  PermissionNames.MyAbsenceDay_CancelRequest, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Cancel my leave/remote/onsite request" },
                }
            },
            new SystemPermission{ Name =  PermissionNames.MyWorkingTime, MultiTenancySides = MultiTenancySides.Host, DisplayName = "My working time",
                Childrens = new List<SystemPermission>()
                {
                    new SystemPermission{ Name =  PermissionNames.MyWorkingTime_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View my working time" },
                    new SystemPermission{ Name =  PermissionNames.MyWorkingTime_RegistrationTime, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Registration new working time" },
                    new SystemPermission{ Name =  PermissionNames.MyWorkingTime_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit registration working time" },
                    new SystemPermission{ Name =  PermissionNames.MyWorkingTime_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete registration working time" },
                }
            },
            new SystemPermission{ Name =  PermissionNames.Timesheet, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Manage timesheet",
                Childrens = new List<SystemPermission>()
                {
                    new SystemPermission{ Name =  PermissionNames.Timesheet_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View timesheets" },
                    new SystemPermission{ Name =  PermissionNames.Timesheet_ViewStatus, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View status timesheets" },
                    new SystemPermission{ Name =  PermissionNames.Timesheet_Approval, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Approval timesheet" },
                    new SystemPermission{ Name =  PermissionNames.Timesheet_Export, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Export excel" },
                }
    },
    //new SystemPermission{ Name =  PermissionNames.AbsenceDay, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Leave days",
    //    Childrens = new List<SystemPermission>()
    //    {
    //        new SystemPermission{ Name =  PermissionNames.AbsenceDay_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View leave/onsite requests" },
    //        new SystemPermission{ Name =  PermissionNames.AbsenceDay_Approval, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Approval requests" },
    //    }
    //},
            new SystemPermission{ Name =  PermissionNames.AbsenceDayByProject, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Manage request off/remote/onsite",
                Childrens = new List<SystemPermission>()
                {
                    new SystemPermission{ Name =  PermissionNames.AbsenceDayByProject_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View users leave/remote/onsite by project" },
                    new SystemPermission{ Name =  PermissionNames.AbsenceDayByProject_ViewByBranch, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View users leave/remote/onsite by branch" },
                    new SystemPermission{ Name =  PermissionNames.AbsenceDayByProject_Approval, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Approval requests" },
                    new SystemPermission{ Name =  PermissionNames.AbsenceDayByProject_ViewDetail, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View detail request" },
                }
     },
            new SystemPermission{ Name =  PermissionNames.ManageWorkingTime, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Manage working time",
                Childrens = new List<SystemPermission>()
                {
                    new SystemPermission{ Name =  PermissionNames.ManageWorkingTime_ViewAll, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View all user working time " },
                    new SystemPermission{ Name =  PermissionNames.ManageWorkingTime_ViewDetail, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View my team member Working Time" },
                    new SystemPermission{ Name =  PermissionNames.ManageWorkingTime_Approval, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Approval working time of member" },
                }
     },
            new SystemPermission{ Name =  PermissionNames.AbsenceDayOfTeam, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Team working calendar",
                Childrens = new List<SystemPermission>()
                {
                    new SystemPermission{ Name =  PermissionNames.AbsenceDayOfTeam_View, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View your team member leave / onsite by project" },
                    new SystemPermission{ Name =  PermissionNames.AbsenceDayOfTeam_ViewDetail, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View detail leave / onsite of your team member" },
                    new SystemPermission{ Name =  PermissionNames.AbsenceDayOfTeam_NotifyPm, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Allow to push notify to PM"},
                    new SystemPermission{ Name =  PermissionNames.AbsenceDayOfTeam_ExportTeamWorkingCalender, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Export team working calender" },
                }
            },
            new SystemPermission{ Name =  PermissionNames.TimesheetSupervision, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Timesheet mornitoring",
                Childrens = new List<SystemPermission>()
                {
                    new SystemPermission{ Name =  PermissionNames.TimesheetSupervision_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View timesheets" },
                }
            },
             new SystemPermission{ Name =  PermissionNames.Retro, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Retro",
                Childrens = new List<SystemPermission>()
                {
                    new SystemPermission{ Name = PermissionNames.Retro_View, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View retro"},
                    new SystemPermission{ Name = PermissionNames.Retro_AddNew, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Add new retro"},
                    new SystemPermission{ Name = PermissionNames.Retro_Edit, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Edit retro"},
                    new SystemPermission{ Name = PermissionNames.Retro_Delete, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Delete retro"},
                    new SystemPermission{ Name = PermissionNames.Retro_ChangeStatus, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Change status retro"},
                    new SystemPermission{ Name = PermissionNames.Retro_RetroDetail, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Retro detail",
                        Childrens = new List<SystemPermission>()
                        {
                           new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_ViewAllTeam, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View all team" },
                           new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_ViewMyTeam, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View my team"},
                           new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_AddEmployeeAllTeam, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add employee all team"},
                           new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_AddEmployeeMyTeam, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add employee my team"},
                           new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit employee" },
                           new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete employee" },
                           new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_DownloadTemplate, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Download template" },
                           new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_Import, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Import employee" },
                           new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_Export, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Export employee" },
                           new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_GenerateData, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Generate data employee"},
                           new SystemPermission{ Name =  PermissionNames.Retro_RetroDetail_ViewLevel, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View level"},
                            },
                        }
                    },
                },

             new SystemPermission{ Name =  PermissionNames.ReviewIntern, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Review Interns",
                Childrens = new List<SystemPermission>()
                {
                    new SystemPermission{ Name =  PermissionNames.ReviewIntern_ViewAllReport, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View all report"},
                    new SystemPermission{ Name =  PermissionNames.ReviewIntern_ExportReport, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Export report"},
                    new SystemPermission{ Name =  PermissionNames.ReviewIntern_AddNewReview, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Create review phase" },
                    new SystemPermission{ Name =  PermissionNames.ReviewIntern_AddNewReviewByCapability, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Create review by capability phase" },
                    new SystemPermission{ Name =  PermissionNames.ReviewIntern_ViewAll, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View all review phase"},
                    new SystemPermission{ Name =  PermissionNames.ReviewIntern_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete review phase" },
                    new SystemPermission{ Name =  PermissionNames.ReviewIntern_Active, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Active review phase" },
                    new SystemPermission{ Name =  PermissionNames.ReviewIntern_DeActive, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Deactive review phase"},
                    new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Review Detail",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ViewAll, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View all review detail" },
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_AddNew, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Add new review detail" },
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_Update, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update review detail" },
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ChangeReviewer, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Change reviewer review detail" },
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_Delete, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Delete review detail" },
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ReviewForOneIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Review (for one intern)" },
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ReviewByCapabilityForOneIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Review by capability(for one intern)"},
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ApproveForOneIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Approve review (for one intern)" },
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ConfirmSalaryForOneIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Chốt lương (for one intern)" },
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_RejectForOneIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Reject review (for one intern)" },
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_SendEmailForOneIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Send email (for one intern)" },
                            new SystemPermission{ Name = PermissionNames.ReviewIntern_ReviewDetail_RejectSentEmailForOneIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Reject sent mail (for one intern) by CEO"},
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_UpdateToHRMForOneIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update to hrm (for one intern)"},
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_SendAllEmailsIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Send all emails Intern" },
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_SendAllEmailsOffical, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Send all emails Offical" },
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_UpdateStarToProject, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update star to Project" },
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_UpdateAllToHRMs, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Send to HRM" },
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ApproveAll, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Approve all" },

                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ViewDetailLevel, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Level" },
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ViewDetailSubLevel, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Sublevel" },
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_ViewFullSalary, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Full Salary" },
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_UpdateDetailSubLevel, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Sublevel"},
                            new SystemPermission{ Name =  PermissionNames.ReviewIntern_ReviewDetail_UpdateDetailFullSalary, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Update Full Lương"},
                        }},
                },
             },
            new SystemPermission{ Name =  PermissionNames.Report, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Report",
                Childrens = new List<SystemPermission>()
                {
                    new SystemPermission{ Name =  PermissionNames.Report_InternsInfo, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Interns info",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.Report_InternsInfo_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View interns info" },
                            new SystemPermission{ Name =  PermissionNames.Report_InternsInfo_ViewLevelIntern, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View level intern" },
                        }
                    },
                    new SystemPermission{ Name =  PermissionNames.Report_NormalWorking, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Normal working",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.Report_NormalWorking_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View normal working" },
                            new SystemPermission{ Name =  PermissionNames.Report_NormalWorking_Export, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Export excel" },
                            new SystemPermission{ Name =  PermissionNames.Report_NormalWorking_LockUnlockTimesheet, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Lock/unlock timesheet" },
                        }
                    },
                    new SystemPermission{ Name =  PermissionNames.Report_OverTime, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Over time",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.Report_OverTime_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View overtime" },
                        }
                    },
                       new SystemPermission{ Name =  PermissionNames.Report_KomuTracker, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Komu tracker",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.Report_KomuTracker_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View Komu tracker" },
                        }
                    },
                    new SystemPermission{ Name =  PermissionNames.Report_TardinessLeaveEarly, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Tardisness / leave early",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.Report_TardinessLeaveEarly_View, MultiTenancySides = MultiTenancySides.Host , DisplayName = "View user tardiness or leave early" },
                            new SystemPermission{ Name =  PermissionNames.Report_TardinessLeaveEarly_GetData, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Get data from FaceID" },
                            new SystemPermission{ Name =  PermissionNames.Report_TardinessLeaveEarly_ExportExcel, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Export excel" },
                            new SystemPermission{ Name =  PermissionNames.Report_TardinessLeaveEarly_Edit, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Edit checkin, checkout time of user" },
                            new SystemPermission{ Name =  PermissionNames.Timekeeping_ReplyUserNote, MultiTenancySides = MultiTenancySides.Host , DisplayName = "Trả lời khiếu lại, chốt phạt user" },
                        }
                    },
                }
            },
             new SystemPermission{ Name =  PermissionNames.TeamBuilding, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Team building",
                Childrens = new List<SystemPermission>()
                {
                    new SystemPermission{ Name =  PermissionNames.TeamBuilding_DetailHR, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Team building detail HR",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.TeamBuilding_DetailHR_ViewAllProject, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View all project"},
                            new SystemPermission{ Name =  PermissionNames.TeamBuilding_DetailHR_GenerateData, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Generate data"},
                            new SystemPermission{ Name =  PermissionNames.TeamBuilding_DetailHR_Management, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Management"},
                        }
                    },
                    new SystemPermission{ Name =  PermissionNames.TeamBuilding_DetailPM, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Team building detail PM",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.TeamBuilding_DetailPM_ViewMyProject, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View my project"},
                            new SystemPermission{ Name =  PermissionNames.TeamBuilding_DetailPM_CreateRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Create request money"},
                        }
                    },
                    new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Team building request",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request_ViewAllRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View all request"},
                            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request_ViewMyRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View my request"},
                            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request_DisburseRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Disburse request"},
                            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request_EditRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Edit request"},
                            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request_ReOpenRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Re-open request"},
                            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request_RejectRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Reject request"},
                            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request_CancelRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Cancel request"},
                            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Request_ViewDetailRequest, MultiTenancySides = MultiTenancySides.Host, DisplayName = "View detail request"},
                        }
                    },
                    new SystemPermission{ Name =  PermissionNames.TeamBuilding_Project, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Team building project",
                        Childrens = new List<SystemPermission>()
                        {
                            new SystemPermission{ Name =  PermissionNames.TeamBuilding_Project_SelectProjectTeamBuilding, MultiTenancySides = MultiTenancySides.Host, DisplayName = "Select project team building"},
                        }
                    },
                }
             }
        };
    }
}