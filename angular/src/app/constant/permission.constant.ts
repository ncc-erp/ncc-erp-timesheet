export const PERMISSIONS_CONSTANT = {
    Home: "Home",
    //user
    ViewUsers: "Admin.Users.View",
    AddUser: "Admin.Users.AddNew",
    EditUser: "Admin.Users.Edit",
    EditUserRole: "Admin.Users.EditRole",
    DeleteUser: "Admin.Users.Delete",
    ResetPassword: "Admin.Users.ResetPassword",
    UploadAvatar: "Admin.Users.UploadAvatar",
    ChangeStatusUser: "Admin.Users.ChangeStatus",
    UploadWorkingTime:"Admin.Users.UploadWorkingTime",
    UpdateUserWorkingTime: "Admin.Users.UpdateUserWorkingTime",
    ViewLevelUser: "Admin.Users.ViewLevelUser",

    //role
    ViewRoles: "Admin.Roles.View",
    ViewDetailRole: "Admin.Roles.ViewDetail",
    AddRole: "Admin.Roles.AddNew",
    EditRole: "Admin.Roles.Edit",
    DeleteRole: "Admin.Roles.Delete",

    //setting
    Configuration:"Admin.Configuration",
    ViewEmailSetting: "Admin.Configuration.Email.ViewEmail",
    ViewWorkingTimeSetting: "Admin.Configuration.WorkingDay.ViewWorkingDay",
    ViewGoogleSingleSignOnSetting: "Admin.Configuration.GoogleSignOn.ViewGoogleSignOn",
    ViewAutoLockTimesheetSetting: "Admin.Configuration.AutoLockTimesheet.ViewAutoLockTimesheet",
    ViewSercurityCodeSetting: "Admin.Configuration.SercurityCode.ViewSercurityCode",
    ViewLogTimesheetInFutureSetting: "Admin.Configuration.LogTimesheetInFuture.ViewLogTimesheetInFuture",
    ViewAutoSubmitTimesheetSetting: "Admin.Configuration.AutoSubmitTimesheet.ViewAutoSubmitTimesheet",
    ViewHRMSetting: "Admin.Configuration.HRMConfig.ViewHRMConfig",
    EditEmailSetting: "Admin.Configuration.Email.EditEmail",
    EditWorkingTimeSetting: "Admin.Configuration.WorkingDay.EditWorkingDay",
    EditGoogleSingleSignOnSetting: "Admin.Configuration.GoogleSignOn.EditGoogleSignOn",
    EditAutoLockTimesheetSetting: "Admin.Configuration.AutoLockTimesheet.EditAutoLockTimesheet",
    EditSercurityCodeSetting: "Admin.Configuration.SercurityCode.EditSercurityCode",
    EditLogTimesheetInFutureSetting: "Admin.Configuration.LogTimesheetInFuture.EditLogTimesheetInFuture",
    EditAutoSubmitTimesheetSetting: "Admin.Configuration.AutoSubmitTimesheet.EditAutoSubmitTimesheet",
    EditHRMSetting: "Admin.Configuration.HRMConfig.UpdateHRMConfig",
    EditLevelSetting: "Admin.Configuration.LevelSetting.EditLevelSetting",
    ViewLevelSetting: "Admin.Configuration.LevelSetting.ViewLevelSetting",
    EditCheckInCheckOutPunishmentSetting: "Admin.Configuration.CheckInCheckOutPunishmentSetting.EditCheckInCheckOutPunishmentSetting",
    ViewCheckInCheckOutPunishmentSetting: "Admin.Configuration.CheckInCheckOutPunishmentSetting.ViewCheckInCheckOutPunishmentSetting",
    ViewProjectConfig: "Admin.Configuration.ProjectConfig.ViewProjectConfig",
    UpdateProjectConfig: "Admin.Configuration.ProjectConfig.UpdateProjectConfig",
    UpdateKomuConfig: "Admin.Configuration.KomuConfig.UpdateKomuConfig",
    ViewKomuConfig: "Admin.Configuration.KomuConfig.ViewKomuConfig",
    ViewWFHSetting: "Admin.Configuration.RemoteSetting.ViewRemoteSetting",
    EditWFHSetting: "Admin.Configuration.RemoteSetting.EditRemoteSetting",
    ViewSpecialProjectTaskSetting: "Admin.Configuration.SpecialProjectTaskSetting.ViewSpecialProjectTaskSetting",
    EditSpecialProjectTaskSetting: "Admin.Configuration.SpecialProjectTaskSetting.EditSpecialProjectTaskSetting",
    ViewNotificationSetting: "Admin.Configuration.NotificationSetting.ViewNotificationSetting",
    EditNotificationSetting: "Admin.Configuration.NotificationSetting.EditNotificationSetting",
    ViewEmailSaoDo: "Admin.Configuration.EmailSaoDo.ViewEmailSaoDo",
    EditEmailSaoDo: "Admin.Configuration.EmailSaoDo.EditEmailSaoDo",
    ViewCheckInSetting: "Admin.Configuration.CheckInSetting.ViewCheckInSetting",
    UpdateCheckInSetting: "Admin.Configuration.CheckInSetting.UpdateCheckInSetting",
    ViewNRITSetting: "Admin.Configuration.NRITConfig.ViewNRITConfig",
    EditNRITSetting: "Admin.Configuration.NRITConfig.UpdateNRITConfig",
    ViewUnlockTimesheetSetting: "Admin.Configuration.UnlockTimesheetSetting.ViewUnlockTimesheetSetting",
    UpdateUnlockTimesheetSetting: "Admin.Configuration.UnlockTimesheetSetting.UpdateUnlockTimesheetSetting",
    ViewSendKomuPunishedCheckIn: "Admin.Configuration.SettingWorkerNoticeKomuPunishmentUserNoCheckInOut.View",
    UpdateSendKomuPunishedCheckIn: "Admin.Configuration.SettingWorkerNoticeKomuPunishmentUserNoCheckInOut.Update",
    ViewRetroNotifySetting: "Admin.Configuration.RetroNotifyConfig.ViewRetroNotifyConfig",
    EditRetroNotifySetting: "Admin.Configuration.RetroNotifyConfig.UpdateRetroNotifyConfig",
    ViewTeamBuildingSetting: "Admin.Configuration.TeamBuilding.ViewTeamBuildingConfig",
    EditTeamBuildingSetting: "Admin.Configuration.TeamBuilding.UpdateTeamBuildingConfig",
    ViewApproveTimesheetNotifySetting: "Admin.Configuration.ApproveTimesheetNotifyConfig.ViewApproveTimesheetNotifyConfig",
    EditApproveTimesheetNotifySetting: "Admin.Configuration.ApproveTimesheetNotifyConfig.UpdateApproveTimesheetNotifyConfig",
    ViewApproveRequestOffNotifySetting: "Admin.Configuration.ApproveRequestOffNotifyConfig.ViewApproveRequestOffNotifyConfig",
    EditApproveRequestOffNotifySetting: "Admin.Configuration.ApproveRequestOffNotifyConfig.UpdateApproveRequestOffNotifyConfig",

    ViewTimesCanLateAndEarlyInMonthSetting: "Admin.Configuration.TimesCanLateAndEarlyInMonthSetting.ViewTimesCanLateAndEarlyInMonthSetting",
    EditTimesCanLateAndEarlyInMonthSetting: "Admin.Configuration.TimesCanLateAndEarlyInMonthSetting.EditTimesCanLateAndEarlyInMonthSetting",

    ViewTimeStartChangingCheckInToCheckoutSetting: "Admin.Configuration.TimeStartChangingCheckInToCheckOutSetting.ViewTimeStartChangingCheckInToCheckOut",
    ViewTimeStartChangingCheckInToCheckoutCaseOffAfternoonSetting: "Admin.Configuration.TimeStartChangingCheckInToCheckOutSetting.ViewTimeStartChangingCheckInToCheckOutCaseOffAfternoon",
    EditTimeStartChangingCheckInToCheckoutSetting: "Admin.Configuration.TimeStartChangingCheckInToCheckOutSetting.UpdateTimeStartChangingCheckInToCheckOut",

    ViewSendMessageRequestPendingTeamBuildingToHRConfigSetting: "Admin.Configuration.SendMessageRequestPendingTeamBuildingToHRConfig.ViewSendMessageRequestPendingTeamBuildingToHRConfig",
    EditSendMessageRequestPendingTeamBuildingToHRConfigSetting: "Admin.Configuration.SendMessageRequestPendingTeamBuildingToHRConfig.UpdateSendMessageRequestPendingTeamBuildingToHRConfig",

    ViewNotifyHRTheEmployeeMayHaveLeftConfigSetting: "Admin.Configuration.NotifyHRTheEmployeeMayHaveLeftConfig.ViewNotifyHRTheEmployeeMayHaveLeftConfig",
    EditNotifyHRTheEmployeeMayHaveLeftConfigSetting: "Admin.Configuration.NotifyHRTheEmployeeMayHaveLeftConfig.UpdateNotifyHRTheEmployeeMayHaveLeftConfig",
    
    ViewMoneyPMUnlockTimeSheetConfigSetting: "Admin.Configuration.MoneyPMUnlockTimeSheetConfig.ViewMoneyPMUnlockTimeSheetConfig",
    EditMoneyPMUnlockTimeSheetConfigSetting: "Admin.Configuration.MoneyPMUnlockTimeSheetConfig.UpdateMoneyPMUnlockTimeSheetConfig",

    ViewSendMessageToPunishUserConfigSetting: "Admin.Configuration.SendMessageToPunishUserConfig.ViewSendMessageToPunishUserConfig",
    EditSendMessageToPunishUserConfigSetting: "Admin.Configuration.SendMessageToPunishUserConfig.UpdateSendMessageToPunishUserConfig",
    //client
    ViewClients: "Admin.Clients.View",
    AddClient: "Admin.Clients.AddNew",
    EditClient: "Admin.Clients.Edit",
    DeleteClient: "Admin.Clients.Delete",

    //task
    ViewTasks: "Admin.Tasks.View",
    AddTask: "Admin.Tasks.AddNew",
    EditTask: "Admin.Tasks.Edit",
    DeleteTask: "Admin.Tasks.Delete",
    ChangeStatusTask: "Admin.Tasks.ChangeStatus",

    //leave type
    ViewLeaveTypes: "Admin.LeaveTypes.View",
    AddLeaveType: "Admin.LeaveTypes.AddNew",
    EditLeaveType: "Admin.LeaveTypes.Edit",
    DeleteLeaveType: "Admin.LeaveTypes.Delete",


    //branch
    ViewBranch: "Admin.Branchs.View",
    AddNewBranch: "Admin.Branchs.AddNew",
    EditBranch: "Admin.Branchs.Edit",
    DeleteBranch: "Admin.Branchs.Delete",

    //Bg job

    Admin_BackgroundJob: "Admin.BackgroundJob",
    Admin_BackgroundJob_View: "Admin.BackgroundJob.View",
    Admin_BackgroundJob_Delete: "Admin.BackgroundJob.Delete",

    //project
    ViewProjects: "Project.View",
    AddProject: "Project.AddNew",
    EditProject: "Project.Edit",
    DeleteProject: "Project.Delete",
    ChangeStatusProject: "Project.ChangeStatus",
    ViewDetailProject: "Project.ViewDetail",
    ExportExcelProject: "Project.Export",
    UpdateDefaultProjectTask: "Project.UpdateDefaultProjectTask",
    EditTypeWork: "Project.EditTeamWorkType",

    //my timesheet
    AddMyTimesheet: "MyTimesheet.AddNew",
    EditMyTimesheet: "MyTimesheet.Edit",
    DeleteMyTimesheet: "MyTimesheet.Delete",
    SubmitMyTimesheet: "MyTimesheet.Submit",
    ViewMyTimesheet: "MyTimesheet.View",

    //my profile
    ViewMyProfile : "MyProfile.View",
    RequestUpdateInfo: "MyProfile.RequestUpdateInfo",

    //timesheet
    ViewTimesheets: "Timesheet.View",
    ApprovalTimesheets: "Timesheet.Approval",
    ExportExcelTimesheets: "Timesheet.Export",

    //timesheet supervision
    ViewTimeSheetsSupervision: "TimesheetSupervision.View",

    //Review TTS
    ViewReview: "ReviewsTTS.View",

    //my absence day
    ViewMyAbsenceDay: "	MyAbsenceDay.View",
    AddMyAbsenceDay: "MyAbsenceDay.AddNew",
    SendMyAbsenceDay: "MyAbsenceDay.SendRequest",

    //absence day
    ViewLeaveDays: "AbsenceDay.View",
    ApprovalLeaveDay: "AbsenceDay.Approval",

    //absence day by project
    ViewAbsenceDayByProject: "AbsenceDayByProject.View",
    ViewAbsenceDayByBranch: "AbsenceDayByProject.ViewByBranch",
    ApprovalAbsenceDayByProject: "AbsenceDayByProject.Approval",
    ViewDetailAbsenceDayByProject: "AbsenceDayByProject.ViewDetail",
    ExportTeamWorkingCalender: "AbsenceDayOfTeam.ExportTeamWorkingCalender",

    //absence day of team
    ViewAbsenceDayOfTeam: "AbsenceDayOfTeam.View",
    ViewDetailAbsenceDayOfTeam: "AbsenceDayOfTeam.ViewDetail",
    NotifyPmAbsenceDayOfTeam : "AbsenceDayOfTeam.NotifyPm",

    //day off
    ViewDayOffs: "DayOff.View",
    AddDayOff: "DayOff.AddNew",
    EditDayOff: "DayOff.Edit",
    DeleteDayOff: "DayOff.Delete",

    //my working time
    ViewMyWorkingTime: "MyWorkingTime.View",
    RegistrationWorkingTime: "MyWorkingTime.RegistrationTime",
    EditMyWorkingTime: "MyWorkingTime.Edit",
    DeleteMyWorkingTime: "MyWorkingTime.Delete",

    //manage working time   
    ViewManageWorkingTime: "ManageWorkingTime.ViewDetail",
    ViewAllManageWorkingTime: "ManageWorkingTime.ViewAll",
    ApprovalWorkingTime: "ManageWorkingTime.Approval",

    //over time setting
    ViewOverTimeSetting: "OverTimeSetting.View",
    AddNewOverTimeSetting: "OverTimeSetting.AddNew",
    EditOverTimeSetting: "OverTimeSetting.Edit",
    DeleteOverTimeSetting: "OverTimeSetting.Delete",

    //normalworking
    ViewNormalWorking: "Report.NormalWorking.View",
    ExportExcelNormalWorking: "Report.NormalWorking.Export",
    LockUnlockTimesheet: "Report.NormalWorking.LockUnlockTimesheet",

    //overtime
    ViewOvertime: "Report.OverTime.View",

     //Komu tracker
     ViewKomuTracker: "Report.KomuTracker.View",

    //tardiness & leave early
    ViewTardinessLeaveEarly: "Report.TardinessLeaveEarly.View",
    GetDataFromFaceId: "Report.TardinessLeaveEarly.GetData",
    ExportExcelTardinessLeaveEarly: "Report.TardinessLeaveEarly.ExportExcel",
    EditTardinessLeaveEarly: "Report.TardinessLeaveEarly.Edit",
    ViewOnlyMeTardinessLeaveEarly:"MyTimeSheet.ViewMyTardinessDetail",
    Timekeeping_UserNote: "Timekeeping.UserNote",
    Timekeeping_ReplyUserNote: "Timekeeping.ReplyUserNote",

    //review intern
    ReviewIntern: "ReviewIntern",
    ReviewIntern_ViewAllReport: "ReviewIntern.ViewAllReport",
    ReviewIntern_Active: "ReviewIntern.Active",
    ReviewIntern_AddNewReview: "ReviewIntern.AddNewReview",
    ReviewIntern_AddNewReviewByCapability: "ReviewIntern.AddNewReviewByCapability",
    ReviewIntern_DeActive: "ReviewIntern.DeActive",
    ReviewIntern_Delete: "ReviewIntern.Delete",
    ReviewIntern_ReviewDetail: "ReviewIntern.ReviewDetail",
    ReviewIntern_ReviewDetail_AddNew: "ReviewIntern.ReviewDetail.AddNew",
    ReviewIntern_ReviewDetail_Delete: "ReviewIntern.ReviewDetail.Delete",
    ReviewIntern_ReviewDetail_ReviewForOneIntern: "ReviewIntern.ReviewDetail.ReviewForOneIntern",
    ReviewIntern_ReviewDetail_ReviewByCapabilityForOneIntern: "ReviewIntern.ReviewDetail.ReviewByCapabilityForOneIntern",
    ReviewIntern_ReviewDetail_SendAllEmailsIntern: "ReviewIntern.ReviewDetail.SendAllEmailsIntern",
    ReviewIntern_ReviewDetail_SendAllEmailsOffical: "ReviewIntern.ReviewDetail.SendAllEmailsOffical",
    ReviewIntern_ReviewDetail_SendEmailForOneIntern: "ReviewIntern.ReviewDetail.SendEmailForOneIntern",
    ReviewIntern_ReviewDetail_Update: "ReviewIntern.ReviewDetail.Update",
    ReviewIntern_ReviewDetail_ChangeReviewer: "ReviewIntern.ReviewDetail.ChangeReviewer",
    ReviewIntern_ReviewDetail_SendAllToHRM: "ReviewIntern.ReviewDetail.SendAllToHRM",
    ReviewIntern_ReviewDetail_UpdateToHRMForOneIntern: "ReviewIntern.ReviewDetail.UpdateToHRMForOneIntern",
    ReviewIntern_ReviewDetail_ViewAll: "ReviewIntern.ReviewDetail.ViewAll",
    ReviewIntern_ReviewDetail_ApproveForOneIntern:"ReviewIntern.ReviewDetail.ApproveForOneIntern",
    ReviewIntern_ReviewDetail_ConfirmSalaryForOneIntern:"ReviewIntern.ReviewDetail.ConfirmSalaryForOneIntern",
    ReviewIntern_ReviewDetail_RejectForOneIntern: "ReviewIntern.ReviewDetail.RejectForOneIntern",
    ReviewIntern_ReviewDetail_RejectSentEmailForOneIntern:"ReviewIntern.ReviewDetail.RejectSentEmailForOneIntern",
    ReviewIntern_ReviewDetail_UpdateStarToProject: "ReviewIntern.ReviewDetail.UpdateStarToProject",
    ReviewIntern_ApproveAll: "ReviewIntern.ApproveAll",

    ReviewIntern_ViewAll: "ReviewIntern.ViewAll",
    ReviewIntern_ReviewDetail_ViewDetailLevel: "ReviewIntern.ReviewDetail.ViewDetailLevel",
    ReviewIntern_ReviewDetail_ViewDetailSubLevel: "ReviewIntern.ReviewDetail.ViewDetailSubLevel",
    ReviewIntern_ReviewDetail_ViewFullSalary: "ReviewIntern.ReviewDetail.ViewFullSalary",
    ReviewIntern_ReviewDetail_UpdateDetailSubLevel: "ReviewIntern.ReviewDetail.UpdateDetailSubLevel",
    ReviewIntern_ReviewDetail_UpdateDetailFullSalary: "ReviewIntern.ReviewDetail.UpdateDetailFullSalary",

    ReviewIntern_ExportReport: "ReviewIntern.ExportReport",


    //Capability
    ViewCapability: "Admin.Capability.View",
    AddNewCapability: "Admin.Capability.AddNew",
    EditCapability: "Admin.Capability.Edit",
    DeleteCapability: "Admin.Capability.Delete",

    //CapabilitySetting
    ViewCapabilitySetting: "Admin.CapabilitySetting.View",
    AddNewCapabilitySetting: "Admin.CapabilitySetting.AddNew",
    EditCapabilitySetting: "Admin.CapabilitySetting.Edit",
    DeleteCapabilitySetting: "Admin.CapabilitySetting.Delete",
    CloneCapabilitySetting: "Admin.CapabilitySetting.Clone",

    //position
    ViewPosition: "Admin.Position.View",
    AddNewPosition: "Admin.Position.AddNew",
    EditPosition: "Admin.Position.Edit",
    DeletePosition: "Admin.Position.Delete",

    // Retro
    //Retro: "Retro",
    Retro: "Retro",
    ViewRetro: "Retro.View",
    AddNewRetro: "Retro.AddNew",
    EditRetro: "Retro.Edit",
    DeleteRetro: "Retro.Delete",
    ChangeStatus: "Retro.ChangeStatus",


    // Retro Detail
    RetroDetail_ViewAllTeam: "Retro.RetroDetail.ViewAllTeam",
    RetroDetail_ViewMyTeam: "Retro.RetroDetail.ViewMyTeam",
    RetroDetail_ViewLevel: "Retro.RetroDetail.ViewLevel",
    RetroDetail_AddEmployeeMyTeam: "Retro.RetroDetail.AddEmployeeMyTeam",
    RetroDetail_AddEmployeeAllTeam: "Retro.RetroDetail.AddEmployeeAllTeam",
    RetroDetail_DeleteEmployee: "Retro.RetroDetail.Delete",
    RetroDetail_EditEmployee: "Retro.RetroDetail.Edit",
    RetroDetail_Import: "Retro.RetroDetail.Import",
    RetroDetail_DownloadTemplate: "Retro.RetroDetail.DownloadTemplate",
    RetroDetail_Export: "Retro.RetroDetail.Export",
    RetroDetail_GenerateData: "Retro.RetroDetail.GenerateData",

    //InternInfo
    InternsInfo: "InternsInfo",
    InternsInfo_View: "InternsInfo.View",
    InternsInfo_ViewLevelIntern: "Report.InternsInfo.ViewLevelIntern",

    // Team building
    TeamBuilding : "TeamBuilding",
    TeamBuilding_DetailHR : "TeamBuilding.DetailHR",
    TeamBuilding_DetailHR_ViewAllProject : "TeamBuilding.DetailHR.ViewAllProject",
    TeamBuilding_DetailHR_GenerateData : "TeamBuilding.DetailHR.GenerateData",
    TeamBuilding_DetailHR_Management : "TeamBuilding.DetailHR.Management",
    TeamBuilding_DetailPM : "TeamBuilding.DetailPM",
    TeamBuilding_DetailPM_ViewMyProject : "TeamBuilding.DetailPM.ViewMyProject",
    TeamBuilding_DetailPM_CreateRequest : "TeamBuilding.DetailPM.CreateRequest",
    TeamBuilding_Request : "TeamBuilding.Request",
    TeamBuilding_Request_ViewAllRequest : "TeamBuilding.Request.ViewAllRequest",
    TeamBuilding_Request_ViewMyRequest : "TeamBuilding.Request.ViewMyRequest",
    TeamBuilding_Request_DisburseRequest : "TeamBuilding.Request.DisburseRequest",
    TeamBuilding_Request_EditRequest : "TeamBuilding.Request.EditRequest",
    TeamBuilding_Request_ReOpenRequest : "TeamBuilding.Request.ReOpenRequest",
    TeamBuilding_Request_RejectRequest : "TeamBuilding.Request.RejectRequest",
    TeamBuilding_Request_CancelRequest : "TeamBuilding.Request.CancelRequest",
    TeamBuilding_Request_ViewDetailRequest : "TeamBuilding.Request.ViewDetailRequest",
    TeamBuilding_Project : "TeamBuilding.Project",
    TeamBuilding_Project_SelectProjectTeamBuilding : "TeamBuilding.Project.SelectProjectTeamBuilding",

}
