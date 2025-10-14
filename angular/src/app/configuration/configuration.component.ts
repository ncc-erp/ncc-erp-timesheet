import { TimeStartChangingCheckinToCheckoutSettingService } from './../service/api/time-start-changing-checkin-to-checkout-setting.service';
import { TimesCanLateAndEarlyInMonthSettingService } from './../service/api/time-can-late-early-setting.service';
import { UpdatePunishMoneyComponent } from './update-punish-money/update-punish-money.component';
import { MatDialog } from '@angular/material';
import { TimekeepingService } from '@app/service/api/timekeeping.service';
import { CheckInCheckOutPunishmentSettingService } from '../service/api/punish-by-rule.service';
import { SpecialProjectTaskSettingService } from '../service/api/special-project-task-config.service';
import { WfhSettingService } from './../service/api/wfh-setting.service';
import { EmailSaoDoSettingService } from './../service/api/email-sao-do-setting.service';
import { LevelSettingService } from './../service/api/level-setting.service';
import { Component, Injector, OnInit } from '@angular/core';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { BRANCH_CODES } from '@app/constant/api.constants';
import { AutoLockTimesheetService } from '@app/service/api/auto-lock-timesheet.service';
import { AutoSubmitTimesheetSettingService } from '@app/service/api/auto-submit-timesheet-setting.service';
import { EmailSettingService } from '@app/service/api/email-setting.service';
import { GetDataFromFaceIdSettingService } from '@app/service/api/get-data-from-face-id-setting.service';
import { LogTimesheetInFutureSettingService } from '@app/service/api/log-timesheet-in-future-setting.service';
import { SercurityCodeService } from '@app/service/api/sercurity-code.service';
import { SingleSignOnService } from '@app/service/api/single-sign-on-service';
import { AppComponentBase } from '@shared/app-component-base';
import { ConfigurationService } from './../service/api/configuration.service';
import * as _ from 'lodash';
import { SendKomuPunishedCheckInService } from '@app/service/api/send-komu-punished-check-in.service';
import * as moment from 'moment';
import { type } from 'os';
import { MezonSettingService } from '@app/service/api/mezon-setting.service';
import { LogoutAllUserService } from '@app/service/api/logout-all-user.service';
import { LateInternReviewSettingService, LateInternReviewSettingDto } from '@app/service/api/late-intern-review-setting.service';
import { PMReportPunishSettingService, PMReportPunishSettingDto } from '@app/service/api/pm-report-punish-setting.service';
import { BotReportSettingService, BotReportSettingDto, ProjectDto } from '../service/api/bot-report-setting.service';
import { AnomaliesReportSettingService, AnomaliesReportSettingDto } from '../service/api/anomalies-report-setting.service';
import { BranchService } from '@app/service/api/branch.service';
import { OfficeWorkingReportSettingDto, OfficeWorkingReportSettingService } from '../service/api/office-working-report-setting.service';
@Component({
  selector: 'app-configuration',
  templateUrl: './configuration.component.html',
  styleUrls: ['./configuration.component.css']
})
export class ConfigurationComponent extends AppComponentBase implements OnInit {
  VIEW_EMAIL_SETTING = PERMISSIONS_CONSTANT.ViewEmailSetting;
  VIEW_WORKING_TIME_SETTING = PERMISSIONS_CONSTANT.ViewWorkingTimeSetting;
  VIEW_GOOGLE_SETTING = PERMISSIONS_CONSTANT.ViewGoogleSingleSignOnSetting;
  VIEW_AUTO_LOCK_TIMESHEET_SETTING = PERMISSIONS_CONSTANT.ViewAutoLockTimesheetSetting;
  VIEW_SERCURITY_CODE_SETTING = PERMISSIONS_CONSTANT.ViewSercurityCodeSetting;
  VIEW_LOGOUT_SETTING = PERMISSIONS_CONSTANT.ViewLogoutAllUserSetting;
  VIEW_LOG_TIMESHEET_IN_FUTURE = PERMISSIONS_CONSTANT.ViewLogTimesheetInFutureSetting;
  VIEW_AUTO_SUBMIT_TIMESHEET = PERMISSIONS_CONSTANT.ViewAutoSubmitTimesheetSetting;
  EDIT_EMAIL_SETTING = PERMISSIONS_CONSTANT.EditEmailSetting;
  EDIT_WORKING_TIME_SETTING = PERMISSIONS_CONSTANT.EditWorkingTimeSetting;
  EDIT_GOOGLE_SETTING = PERMISSIONS_CONSTANT.EditGoogleSingleSignOnSetting;
  EDIT_AUTO_LOCK_TIMESHEET_SETTING = PERMISSIONS_CONSTANT.EditAutoLockTimesheetSetting;
  EDIT_SERCURITY_CODE_SETTING = PERMISSIONS_CONSTANT.EditSercurityCodeSetting;
  EDIT_LOGOUT_SETTING = PERMISSIONS_CONSTANT.EditLogoutAllUserSetting;
  EDIT_LOG_TIMESHEET_IN_FUTURE = PERMISSIONS_CONSTANT.EditLogTimesheetInFutureSetting;
  EDIT_AUTO_SUBMIT_TIMESHEET = PERMISSIONS_CONSTANT.EditAutoSubmitTimesheetSetting;
  VIEW_HRM_CONFIG = PERMISSIONS_CONSTANT.ViewHRMSetting;
  EDIT_HRM_CONFIG = PERMISSIONS_CONSTANT.EditHRMSetting;
  VIEW_LEVEL_SETTING = PERMISSIONS_CONSTANT.ViewLevelSetting;
  EDIT_LEVEL_SETTING = PERMISSIONS_CONSTANT.EditLevelSetting;
  VIEW_PUNISHBYRULE_SETTING = PERMISSIONS_CONSTANT.ViewCheckInCheckOutPunishmentSetting;
  EDIT_PUNISHBYRULE_SETTING = PERMISSIONS_CONSTANT.EditCheckInCheckOutPunishmentSetting;
  VIEW_PROJECT_CONFIG = PERMISSIONS_CONSTANT.ViewProjectConfig;
  UPDATE_PROJECT_CONFIG = PERMISSIONS_CONSTANT.UpdateProjectConfig;
  VIEW_WFH_SETTING = PERMISSIONS_CONSTANT.ViewWFHSetting;
  EDIT_WFH_SETTING = PERMISSIONS_CONSTANT.EditWFHSetting;
  VIEW_KOMU_CONFIG = PERMISSIONS_CONSTANT.ViewKomuConfig
  UPDATE_KOMU_CONFIG = PERMISSIONS_CONSTANT.UpdateKomuConfig
  VIEW_SPECIAL_PROJECT_TASK_CONFIG = PERMISSIONS_CONSTANT.ViewSpecialProjectTaskSetting
  EDIT_SPECIAL_PROJECT_TASK_CONFIG = PERMISSIONS_CONSTANT.EditSpecialProjectTaskSetting
  VIEW_NOTIFICATION_SETTING = PERMISSIONS_CONSTANT.ViewNotificationSetting
  EDIT_NOTIFICATION_SETTING = PERMISSIONS_CONSTANT.EditNotificationSetting
  VIEW_EMAIL_SAO_DO_SETTING = PERMISSIONS_CONSTANT.ViewEmailSaoDo
  EDIT_EMAIL_SAO_DO_SETTING = PERMISSIONS_CONSTANT.EditEmailSaoDo
  VIEW_CHECKIN_SETTING = PERMISSIONS_CONSTANT.ViewCheckInSetting
  EDIT_CHECKIN_SETTNG = PERMISSIONS_CONSTANT.UpdateCheckInSetting
  VIEW_MEZON_SETTING = PERMISSIONS_CONSTANT.ViewMezonSetting
  EDIT_MEZON_SETTNG = PERMISSIONS_CONSTANT.EditMezonSetting
  VIEW_NRIT_CONFIG = PERMISSIONS_CONSTANT.ViewNRITSetting;
  EDIT_NRIT_CONFIG = PERMISSIONS_CONSTANT.EditNRITSetting;
  VIEW_NRITVMAE_CONFIG = PERMISSIONS_CONSTANT.ViewNRITVMAESetting;
  EDIT_NRITVMAE_CONFIG = PERMISSIONS_CONSTANT.EditNRITVMAESetting;
  VIEW_LATE_INTERN_REVIEW_SETTING = PERMISSIONS_CONSTANT.ViewLateInternReviewSetting;
  EDIT_LATE_INTERN_REVIEW_SETTING = PERMISSIONS_CONSTANT.EditLateInternReviewSetting;
  VIEW_PM_REPORT_PUNISH_SETTING = PERMISSIONS_CONSTANT.ViewPMReportSetting;
  EDIT_PM_REPORT_PUNISH_SETTING = PERMISSIONS_CONSTANT.EditPMReportSetting;
  VIEW_BOT_REPORT_SETTING = PERMISSIONS_CONSTANT.ViewBotReportSetting;
  EDIT_BOT_REPORT_SETTING = PERMISSIONS_CONSTANT.EditBotReportSetting;
  VIEW_ANOMALIES_REPORT_SETTING = PERMISSIONS_CONSTANT.ViewAnomaliesReportSetting;
  EDIT_ANOMALIES_REPORT_SETTING = PERMISSIONS_CONSTANT.EditAnomaliesReportSetting;
  VIEW_UNLOCK_TIMESHEET_SETTING = PERMISSIONS_CONSTANT.ViewUnlockTimesheetSetting;
  UPDATE_UNLOCK_TIMESHEET_SETTING = PERMISSIONS_CONSTANT.UpdateUnlockTimesheetSetting;
  VIEW_PUNISHCHECKIN_CONFIG = PERMISSIONS_CONSTANT.ViewSendKomuPunishedCheckIn;
  UPDATE_PUNISHCHECKIN_CONFIG = PERMISSIONS_CONSTANT.UpdateSendKomuPunishedCheckIn;
  VIEW_RETRONOTIFY_CONFIG = PERMISSIONS_CONSTANT.ViewRetroNotifySetting;
  EDIT_RETRONOTIFY_CONFIG = PERMISSIONS_CONSTANT.EditRetroNotifySetting;
  VIEW_CREATENEWRETRO_CONFIG = PERMISSIONS_CONSTANT.ViewCreateNewRetroSetting;
  EDIT_CREATENEWRETRO_CONFIG = PERMISSIONS_CONSTANT.EditCreateNewRetroSetting;
  VIEW_GENERATERETRORESULT_CONFIG = PERMISSIONS_CONSTANT.ViewGenerateRetroResultSetting;
  EDIT_GENERATERETRORESULT_CONFIG = PERMISSIONS_CONSTANT.EditGenerateRetroResultSetting;
  VIEW_RESETDATATEAMBUILDING_CONFIG = PERMISSIONS_CONSTANT.ViewResetDataTeamBuildingSetting;
  EDIT_RESETDATATEAMBUILDING_CONFIG = PERMISSIONS_CONSTANT.EditResetDataTeamBuildingSetting;

  VIEW_TEAMBUILDING_CONFIG = PERMISSIONS_CONSTANT.ViewTeamBuildingSetting;
  EDIT_TEAMBUILDING_CONFIG = PERMISSIONS_CONSTANT.EditTeamBuildingSetting;
  VIEW_APPROVETIMESHEETNOTIFY_CONFIG = PERMISSIONS_CONSTANT.ViewApproveTimesheetNotifySetting;
  EDIT_APPROVETIMESHEETNOTIFY_CONFIG = PERMISSIONS_CONSTANT.EditApproveTimesheetNotifySetting;
  VIEW_APPROVEREQUESTOFFNOTIFY_CONFIG = PERMISSIONS_CONSTANT.ViewApproveRequestOffNotifySetting;
  EDIT_APPROVEREQUESTOFFNOTIFY_CONFIG = PERMISSIONS_CONSTANT.EditApproveRequestOffNotifySetting;

  VIEW_TIMECANLATEANDEARLY_CONFIG = PERMISSIONS_CONSTANT.ViewTimesCanLateAndEarlyInMonthSetting;
  EDIT_TIMECANLATEANDEARLY_CONFIG = PERMISSIONS_CONSTANT.EditTimesCanLateAndEarlyInMonthSetting;

  VIEW_TIMESTARTCHANGINGCHECKINTOCHECKOUT_SETTING = PERMISSIONS_CONSTANT.ViewTimeStartChangingCheckInToCheckoutSetting;
  EDIT_TIMESTARTCHANGINGCHECKINTOCHECKOUT_SETTING = PERMISSIONS_CONSTANT.EditTimeStartChangingCheckInToCheckoutSetting;
  VIEW_TIMESTARTCHANGINGCHECKINTOCHECKOUTCASEOFFAFTERNOON_SETTING = PERMISSIONS_CONSTANT.ViewTimeStartChangingCheckInToCheckoutCaseOffAfternoonSetting;

  VIEW_SENDMESSAGEREQUESTPENDINGTEAMBUILDINGTOHR_CONFIG = PERMISSIONS_CONSTANT.ViewSendMessageRequestPendingTeamBuildingToHRConfigSetting;
  EDIT_SENDMESSAGEREQUESTPENDINGTEAMBUILDINGTOHR_CONFIG = PERMISSIONS_CONSTANT.EditSendMessageRequestPendingTeamBuildingToHRConfigSetting;

  VIEW_NOTIFYHRTHEEMPLOYEEMAYHAVELEFT_CONFIG = PERMISSIONS_CONSTANT.ViewNotifyHRTheEmployeeMayHaveLeftConfigSetting;
  EDIT_NOTIFYHRTHEEMPLOYEEMAYHAVELEFT_CONFIG = PERMISSIONS_CONSTANT.EditNotifyHRTheEmployeeMayHaveLeftConfigSetting;

  VIEW_MONEYPMUNLOCKTIMESHEET_CONFIG = PERMISSIONS_CONSTANT.ViewMoneyPMUnlockTimeSheetConfigSetting;
  EDIT_MONEYPMUNLOCKTIMESHEET_CONFIG = PERMISSIONS_CONSTANT.EditMoneyPMUnlockTimeSheetConfigSetting;

  VIEW_SENDMESSAGETOPUNISHUSER_CONFIG = PERMISSIONS_CONSTANT.ViewSendMessageToPunishUserConfigSetting;
  EDIT_SENDMESSAGETOPUNISHUSER_CONFIG = PERMISSIONS_CONSTANT.EditSendMessageToPunishUserConfigSetting;
  emailsetting = {
    enableSsl: "true"
  } as EmailSettingDto;
  isEditing: boolean = false;
  isShowPassword: boolean = true;
  signOn = {} as SingleSignOnDto;
  isEditSignOn: boolean = false;
  isEditTime: boolean = false;
  isEditLockTimesheet: boolean = false;
  isEditSercurityCode: boolean = false;
  isEditLogoutAllUser: boolean = false;
  isEditWFHSetting: boolean = false;
  isEditUnlockSetting: boolean = false;
  isLevelSetting: boolean = false;
  isPunishByRule: boolean = false;
  isEditLogTimesheetInFuture: boolean = false;
  isEditAutoSubmitTimesheet: boolean = false;
  isEditGetDataFromFaceID: boolean = false;
  isEditMezonSetting: boolean = false;
  isEditingKomu: boolean = false;
  isEditHRMConfig: boolean = false;
  isEditNotificationSetting: boolean = false;
  isEditNotifyPunishCheckIn: boolean = false;
  isEditTimesCanLateAndEarlyInMonthSetting: boolean = false;
  logTimesheetInFuture = {} as LogTimesheetInFutureDTO;
  punishByRule = {} as PunishByRuleDTO;
  lockTimesheet = {} as LockTimesheetDTO;
  sercurityCode = {} as SercurityCodeDTO;
  loginBefore = {} as LogoutAllUserDTO;
  wfhSetting = {} as WFHSettingDTO;
  levelSetting = {} as LevelSettingDTO;
  getCheckInCheckOutPunishmentSetting = {} as GetCheckInCheckOutPunishmentSettingDto
  workingTime = {} as WorkingTimeDTO;
  autoSubmitTimesheet = {} as AutoSubmitTimesheetDto;
  getDataFaceID = {} as GetDataFromFaceIDDto;
  mezonSetting = {} as MezonSetting;
  HRMConfig = {} as HRMConfigDto;
  notificationSetting = {} as NotificationSettingDto;
  ProjectConfig = {} as ProjectConfigDto;
  emailSaoDoSetting = {} as EmailSaoDoDto;
  komuSetting = {} as KomuDto;
  punishedCheckInSetting = {} as KomuPunishCheckInDto;
  specialProjectTask = {} as SpecialProjectTaskSettingDTO;
  isEditEmailSaodo: boolean = false;
  public isLoading: boolean = false;
  public isMezonLoading: boolean = false;
  public isEditProjectSetting: boolean = false;
  public isEditSpecialProjectTaskSetting: boolean = false;
  isShowEmailSetting: boolean = false;
  isShowKomuSetting: boolean = false;
  isShowWorkingTime: boolean = false;
  isShowGoogleSetting: boolean = false;
  isShowAutoLogTimesheet: boolean = false;
  isShowSecurityCodeSetting: boolean = false;
  isShowLogoutAllUserSetting: boolean = false;
  isShowLevelSetting: boolean = false;
  isShowPunishByRule: boolean = false;
  isShowLogTSInFuture: boolean = false;
  isShowAutoSubmitTS: boolean = false;
  isShowFaceIDSetting: boolean = false;
  isShowMezonSetting: boolean = false;
  isShowHRMSetting: boolean = false;
  isShowProjectSetting: boolean = false;
  isShowEmailSaodoSetting: boolean = false;
  isShowWFHSetting: boolean = false;
  isShowUnlockSetting: boolean = false;
  isShowTimesCanLateAndEarlyInMonthSetting: boolean = false;
  isShowSpecialProjectTaskSetting: boolean = false;
  isShowNotificationSetting: boolean = false;

  isShowNRITSetting: boolean = false;
  isEditNRITConfig: boolean = false;
  isShowNoticePunishedCheckIn: boolean = false;
  NRITConfig = {} as NRITConfigDto;

  isShowNRITVMAEConfig: boolean = false;
  isEditNRITVMAEConfig: boolean = false;
  NRITVMAEConfig = {} as NotifyReviewInternViaMezonAndEmailConfigDto;

  isShowLateInternReviewSetting: boolean = false;
  isEditLateInternReviewSetting: boolean = false;
  lateInternReviewSetting = {} as LateInternReviewSettingDto;

  isShowPMReportPunishSetting: boolean = false;
  isEditPMReportPunishSetting: boolean = false;
  pmReportPunishSetting = {} as PMReportPunishSettingDto;

  isShowBotReportSetting: boolean = false;
  isEditBotReportSetting: boolean = false;
  isShowAnomaliesReportSetting: boolean = false;
  isEditAnomaliesReportSetting: boolean = false;
  isShowOfficeWorkingReportSetting: boolean = false;
  isEditOfficeWorkingReportSetting: boolean = false;
  botReportSetting = { everyday: false, botUri: '', projectIds: [], branchCodes: [] } as BotReportSettingDto;
  anomaliesReportSetting = { hour: 0, dayofweek: 'Monday', botUri: '', branchCodes: [] } as AnomaliesReportSettingDto;
  officeWorkingReportSetting = { enable: false, everyday: false, hour: 8, officeIds: '', limit: 10, mezonUrl: '' } as OfficeWorkingReportSettingDto;
  selectedOfficeWorkingBranches: string[] = [];
  projects: ProjectDto[] = [];
  selectedProjects: number[] = [];
  selectedBranches: string[] = [];
  selectedAnomaliesBranches: string[] = [];
  isAllOfficeWorkingBranchesSelected: boolean = false;
  isAllProjectsSelected: boolean = false;
  isAllBranchesSelected: boolean = false;
  isAllAnomaliesBranchesSelected: boolean = false;
  branchCodes = BRANCH_CODES;
  toggleAllOfficeWorkingBranches() {
    if (this.isAllOfficeWorkingBranchesSelected) {
      this.selectedOfficeWorkingBranches = [];
    } else {
      this.selectedOfficeWorkingBranches = [...this.branchCodes];
    }
    this.isAllOfficeWorkingBranchesSelected = !this.isAllOfficeWorkingBranchesSelected;
  }

  unlockSetting = {} as UnlockTimesheetConfigDto;
  timesCanLateAndEarlyInMonthSetting = {} as TimesCanLateAndEarlyInMonthSettingDto;
  percentOfTrackerOnWorking: string = "";

  isShowRetroNotifySetting: boolean = false;
  isEditRetroNotifyConfig: boolean = false;
  RetroNotifyConfig = {} as RetroNotifyConfigDto;
  isShowCreateNewRetroSetting: boolean = false;
  isEditCreateNewRetroConfig: boolean = false;
  CreateNewRetroConfig = {} as CreateNewRetroConfigDto;
  isShowGenerateRetroResultSetting: boolean = false;
  isEditGenerateRetroResult: boolean = false;
  GenerateRetroResultConfig = {} as GenerateRetroResultConfigDto;

  isShowTeamBuildingSetting: boolean = false;
  isEditTeamBuildingConfig: boolean = false;
  TeamBuildingConfig = {} as TeamBuildingConfigDto;

  isShowResetDataTeambuildingSetting: boolean = false;
  isEditResetDataTeambuildingConfig: boolean = false;
  ResetDataTeamBuildingConfig = {} as ResetDataTeamBuildingConfigDto;

  isShowTimeStartChangingCheckinToCheckoutSetting: boolean = false;
  isEditTimeStartChangingCheckinToCheckoutSetting: boolean = false;
  TimeStartChangingCheckinToCheckoutSetting = {} as TimeStartChangingToCheckoutSettingDto;

  public projectConnectResult: GetConnectResultDto = {} as GetConnectResultDto;
  public hrmConnectResult: GetConnectResultDto = {} as GetConnectResultDto;

  isShowApproveTimesheetNotifySetting: boolean = false;
  isEditApproveTimesheetNotifyConfig: boolean = false;
  ApproveTimesheetNotifyConfig = {} as ApproveTimesheetNotifyConfigDto;

  isShowApproveRequestOffNotifySetting: boolean = false;
  isEditApproveRequestOffNotifyConfig: boolean = false;
  ApproveRequestOffNotifyConfig = {} as ApproveRequestOffNotifyConfigDto;

  isShowSendMessageRequestPendingTeamBuildingToHRSetting: boolean = false;
  isEditSendMessageRequestPendingTeamBuildingToHRConfig: boolean = false;
  sendMessageRequestPendingTeamBuildingToHRConfig: SendMessageRequestPendingTeamBuildingToHRConfigDto = {};

  isShowNotifyHRTheEmployeeMayHaveLeftSetting: boolean = false;
  isEditNotifyHRTheEmployeeMayHaveLeftConfig: boolean = false;
  notifyHRTheEmployeeMayHaveLeftConfig: NotifyHRTheEmployeeMayHaveLeftConfigDto = {};

  isShowMoneyPMUnlockTimeSheetSetting: boolean = false;
  isEditMoneyPMUnlockTimeSheetConfig: boolean = false;
  moneyPMUnlockTimeSheetConfig: MoneyPMUnlockTimeSheetConfigDto = {};

  isShowSendMessageToPunishUserSetting: boolean = false;
  isEditSendMessageToPunishUserConfig: boolean = false;
  sendMessageToPunishUserConfig: SendMessageToPunishUserConfigDto = {};

  manualOpenTalk = new Date().toISOString().substring(0, 10);

  constructor(
    private logTimesheetService: LogTimesheetInFutureSettingService,
    private configurationService: ConfigurationService,
    private emailsettingservice: EmailSettingService,
    private singleSignOnService: SingleSignOnService,
    private levelSettingService: LevelSettingService,
    private autoLockTimeSheetService: AutoLockTimesheetService,
    private sercurityCodeService: SercurityCodeService,
    private logoutAllUserService: LogoutAllUserService,
    private wfhService: WfhSettingService,
    private autoSubmitService: AutoSubmitTimesheetSettingService,
    private getDataFromFaceIdService: GetDataFromFaceIdSettingService,
    private mezonSettingService: MezonSettingService,
    private emailSaoDoSerivice: EmailSaoDoSettingService,
    private specialProjectTaskService: SpecialProjectTaskSettingService,
    private sendKomuPunishedCheckInService: SendKomuPunishedCheckInService,
    private punishByRulesService: CheckInCheckOutPunishmentSettingService,
    private timekeepingService: TimekeepingService,
    private timesCanLateAndEarlyInMonthSettingService: TimesCanLateAndEarlyInMonthSettingService,
    private timeStartChangingCheckinToCheckoutSettingService: TimeStartChangingCheckinToCheckoutSettingService,
    private lateInternReviewSettingService: LateInternReviewSettingService,
    private pmReportPunishSettingService: PMReportPunishSettingService,
    private botReportSettingService: BotReportSettingService,
    private anomaliesReportSettingService: AnomaliesReportSettingService,
    private branchService: BranchService,
    private dialog: MatDialog,
    private officeWorkingReportSettingService: OfficeWorkingReportSettingService,
    injector: Injector) {
    super(injector);
  }
  ngOnInit() {
    this.list();
    this.get();
    this.getWorkingTime();
    this.getLockTimesheet();
    this.getSercurityCode();
    this.getLogoutAllUser();
    this.getLevelSetting();
    this.getLogTimesheetInFuture();
    this.getAutoSubmitTimesheet();
    this.getDataFromFaceID();
    this.getMezonSetting()
    this.getHRMConfig();
    this.getEmailSaoDoSetting();
    this.getProjectConfig();
    this.getKomu();
    this.getWFHSetting();
    this.getSpecialProjectTaskSetting();
    this.getNotificationSetting();
    this.geNRITConfig();
    this.getUnlockSetting();
    this.getChannelSendPunishCheckIn();
    this.checkConnectToProject();
    this.checkConnectToHRM();
    this.getCheckInCheckOutPunishmentSettingForConfig();
    this.getRetroNotifyConfig();
    this.getTeamBuildingConfig();
    this.getTimesCanLateAndEarlyInMonthSetting();
    this.getTimeStartChangingCheckinToCheckoutSetting();
    this.getApproveTimesheetNotifyConfig();
    this.getApproveRequestOffNotifyConfig();
    this.getSendMessageRequestPendingTeamBuildingToHRConfig();
    this.getNotifyHRTheEmployeeMayHaveLeftConfig();
    this.getMoneyPMUnlockTimeSheetConfig();
    this.getCreateNewRetroConfig();
    this.getGenerateRetroResultConfig();
    this.getResetDataTeamBuildingConfig();
    this.getLateInternReviewSetting();
    this.getPMReportPunishSetting();
    this.getBotReportSetting();
    this.getAnomaliesReportSetting();
    
    this.getOfficeWorkingReportSetting();
    this.getSendMessageToPunishUserConfig();
    this.getNRITVMAEConfig();
    this.getLateInternReviewSetting();
  }
  protected list(): void {
    if (this.permission.isGranted(this.VIEW_EMAIL_SETTING)) {
      this.emailsettingservice.getMail().subscribe((result: any) => {
        this.emailsetting = result.result;
      });
    }
  }
  editOfficeWorkingReportSetting() {
    this.isEditOfficeWorkingReportSetting = true;
  }
  getOfficeWorkingReportSetting() {
    this.officeWorkingReportSettingService.get().subscribe((response) => {
      if (response.result) {
        this.officeWorkingReportSetting = {
          enable: response.result.enable,
          everyday: response.result.everyday,
          hour: response.result.hour,
          officeIds: response.result.officeIds || '',
          limit: response.result.limit,
          mezonUrl: response.result.mezonUrl || ''
        };
        this.selectedOfficeWorkingBranches = response.result.officeIds
          ? response.result.officeIds.split(',').filter(id => id)
          : [];
      }
    });
  }
  saveOfficeWorkingReportSetting() {
    if (!this.permission.isGranted(this.EDIT_BOT_REPORT_SETTING)) {
      abp.message.error("You do not have permission to edit this setting!");
      return;
    }
    if (this.officeWorkingReportSetting.hour < 0 || this.officeWorkingReportSetting.hour > 23) {
      abp.message.error("Hour must be between 0 and 23!");
      return;
    }

    if (!this.selectedOfficeWorkingBranches || this.selectedOfficeWorkingBranches.length === 0) {
      abp.message.error("You must select at least one branch!");
      return;
    }
    this.officeWorkingReportSetting.officeIds = this.selectedOfficeWorkingBranches.join(',');

    this.officeWorkingReportSettingService.change(this.officeWorkingReportSetting).subscribe((res: any) => {
      this.isEditOfficeWorkingReportSetting = false;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }
  refreshOfficeWorkingReportSetting() {
    this.isEditOfficeWorkingReportSetting = false;
    this.getOfficeWorkingReportSetting();
  }

  onChangeUseDefaultCredentials(value) {
    this.emailsetting.useDefaultCredentials = value.toString();
  }

  checkConnectToProject() {
    this.projectConnectResult = {} as GetConnectResultDto;
    this.configurationService.checkConnectToProject().subscribe((data) => {
      this.projectConnectResult = data.result;
    })
  }

  checkConnectToHRM() {
    this.hrmConnectResult = {} as GetConnectResultDto;
    this.configurationService.checkConnectToHRM().subscribe((data) => {
      this.hrmConnectResult = data.result;
    })
  }

  getWorkingTime() {
    if (this.permission.isGranted(this.VIEW_WORKING_TIME_SETTING)) {
      this.configurationService.getAll().subscribe((data: any) => {
        this.workingTime = data.result;
      })
    }
  }
  getCheckInCheckOutPunishmentSettingForConfig() {
    if (this.permission.isGranted(this.VIEW_PUNISHBYRULE_SETTING)) {
      this.punishByRulesService.getCheckInCheckOutPunishmentSetting().subscribe(rs => {
        this.getCheckInCheckOutPunishmentSetting = rs.result.checkInCheckOutPunishmentSetting;
        this.percentOfTrackerOnWorking = rs.result.percentOfTrackerOnWorking;
      })
    }
  }
  setCheckInCheckOutPunishmentSettingForConfig(item) {
    let input = {
      id: item.id,
      money: item.money
    } as InputToUpdateSettingDto;
    this.punishByRulesService.setCheckInCheckOutPunishmentSetting(input).subscribe(rs => {
      if (rs) {
        abp.notify.success("Update Successfully")
      }
    })
  }

  setPercentOfTrackerOnWorkingSetting() {
    let input = this.percentOfTrackerOnWorking;
    this.isPunishByRule = false;
    this.punishByRulesService.setPercentOfTrackerOnWorkingSetting(input).subscribe(rs => {
      if (rs) {
        abp.notify.success("Update Successfully")
      }
    })
  }

  onUpdateMoney(item) {
    let input = {
      id: item.id,
      money: item.money
    } as InputToUpdateSettingDto;
    const dg = this.dialog.open(UpdatePunishMoneyComponent, {
      data: input,
      width: '400px',
      maxWidth: 'calc(100% - 12px)'
    });
    dg.afterClosed().subscribe((rs) => {
      if (rs) {
        this.getCheckInCheckOutPunishmentSettingForConfig();
      }
    })
  }
  getLockTimesheet() {
    if (this.permission.isGranted(this.VIEW_AUTO_LOCK_TIMESHEET_SETTING)) {
      this.autoLockTimeSheetService.get().subscribe((data: any) => {
        this.lockTimesheet = data.result;
      })
    }
  }
  getLogTimesheetInFuture() {
    if (this.permission.isGranted(this.VIEW_LOG_TIMESHEET_IN_FUTURE)) {
      this.logTimesheetService.get().subscribe((data: any) => {
        this.logTimesheetInFuture = data.result;
      })
    }
  }
  get() {
    if (this.permission.isGranted(this.VIEW_GOOGLE_SETTING)) {
      this.singleSignOnService.get().subscribe((res: any) => {
        this.signOn = res.result;
      });
    }
  }
  getSercurityCode() {
    if (this.permission.isGranted(this.VIEW_SERCURITY_CODE_SETTING)) {
      this.sercurityCodeService.get().subscribe((res: any) => {
        this.sercurityCode = res.result;
      })
    }
  }
  getLogoutAllUser() {
    if (this.permission.isGranted(this.VIEW_LOGOUT_SETTING)) {
      this.logoutAllUserService.get().subscribe((res: any) => {
        this.loginBefore = res.result;
      })
    }
  }

  getWFHSetting() {
    if (this.permission.isGranted(this.VIEW_WFH_SETTING)) {
      this.wfhService.get().subscribe((res: any) => {
        this.wfhSetting = res.result;
      })
    }
  }
  getUnlockSetting() {
    if (this.permission.isGranted(this.VIEW_UNLOCK_TIMESHEET_SETTING)) {
      this.configurationService.GetUnlockTimesheetConfig().subscribe((res: any) => {
        this.unlockSetting = res.result;
      })
    }
  }

  getSpecialProjectTaskSetting() {
    if (this.permission.isGranted(this.VIEW_SPECIAL_PROJECT_TASK_CONFIG)) {
      this.specialProjectTaskService.get().subscribe((res: any) => {
        this.specialProjectTask = res.result;
      })
    }
  }

  getLevelSetting() {
    if (this.permission.isGranted(this.VIEW_LEVEL_SETTING)) {
      this.levelSettingService.get().subscribe((res: any) => {
        this.levelSetting = res.result;
      })
    }
  }

  getAutoSubmitTimesheet() {
    if (this.permission.isGranted(this.VIEW_AUTO_SUBMIT_TIMESHEET)) {
      this.autoSubmitService.get().subscribe(res => {
        this.autoSubmitTimesheet = res.result;
      })
    }
  }
  getEmailSaoDoSetting() {
    if (this.permission.isGranted(this.VIEW_EMAIL_SAO_DO_SETTING)) {
      this.emailSaoDoSerivice.getSetting().subscribe(data => {
        this.emailSaoDoSetting = data.result
      })
    }
  }

  getDataFromFaceID() {
    if (this.permission.isGranted(this.VIEW_CHECKIN_SETTING)) {
      this.getDataFromFaceIdService.get().subscribe(res => {
        this.getDataFaceID = res.result;
      })
    }
  }
  getMezonSetting() {
    if (this.permission.isGranted(this.VIEW_MEZON_SETTING)) {
      this.isMezonLoading = true;
      this.mezonSettingService.get().subscribe(res => {
        this.isMezonLoading = false;
        this.mezonSetting = res.result;
      })
    }
  }
  getTimesCanLateAndEarlyInMonthSetting() {
    if (this.permission.isGranted(this.VIEW_TIMECANLATEANDEARLY_CONFIG)) {
      this.timesCanLateAndEarlyInMonthSettingService.getTimesCanLateAndEarlyInMonthSetting().subscribe((res: any) => {
        this.timesCanLateAndEarlyInMonthSetting = res.result;
      })
    }
  }
  editSignOn() {
    this.isEditSignOn = true;
  }

  editLogTimesheetInFuture() {
    this.isEditLogTimesheetInFuture = true;
  }

  editSercurityCode() {
    this.isEditSercurityCode = true;
  }
  editLogoutAllUser() {
    this.isEditLogoutAllUser = true;
  }
  editWFHSetting() {
    this.isEditWFHSetting = true;
  }
  editUnlockSetting() {
    this.isEditUnlockSetting = true;
  }

  editSpecialProjectTaskSetting() {
    this.isEditSpecialProjectTaskSetting = true;
  }

  editLevelSetting() {
    this.isLevelSetting = true;
  }
  editPunishByRule() {
    this.isPunishByRule = true;
  }
  editEmailSaodo() {
    this.isEditEmailSaodo = true;
  }
  cancelEmailSaoDo() {
    this.isEditEmailSaodo = false;
    this.getEmailSaoDoSetting();
  }
  saveEmailSaoDo() {
    this.emailSaoDoSerivice.change(this.emailSaoDoSetting).subscribe(rs => {
      this.getEmailSaoDoSetting();
      this.isEditEmailSaodo = false;
      abp.notify.success("Updated Email Sao do")
    })
  }
  onEmailSaoDoCheck(e) {
    if (e.checked == true) {
      this.emailSaoDoSetting.canSendEmailToSaoDo = "true"
    }
    else {
      this.emailSaoDoSetting.canSendEmailToSaoDo = "false"
    }
  }

  editMail() {
    this.isEditing = true;
  }
  editTime() {
    this.isEditTime = true;
  }
  editLockTimesheet() {
    this.isEditLockTimesheet = true;
  }
  editAutoSubmitTimesheet() {
    this.isEditAutoSubmitTimesheet = true;
  }
  onAutoSubmitTimesheetEnableWorker(e) {
    if (e.checked == true) {
      this.autoSubmitTimesheet.autoSubmitTimesheet = "true"
    }
    else {
      this.autoSubmitTimesheet.autoSubmitTimesheet = "false"
    }
  }

  editGetDataFromFaceID() {
    this.isEditGetDataFromFaceID = true;
  }
  editMezonSetting() {
    this.isEditMezonSetting = true;
  }
  SaveSercurityCode() {
    this.sercurityCodeService.change(this.sercurityCode).subscribe((res: any) => {
      this.isEditSercurityCode = !this.editSercurityCode;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }
  SaveLogoutAllUser() {
    this.logoutAllUserService.change(this.loginBefore).subscribe((res: any) => {
      this.isEditLogoutAllUser = !this.isEditLogoutAllUser;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  SaveWFHSetting() {
    this.wfhService.change(this.wfhSetting).subscribe((res: any) => {
      this.isEditWFHSetting = !this.isEditWFHSetting;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
        this.refreshWFHSetting();
      }
    })
  }
  SaveUnlockSetting() {
    this.configurationService.SetUnlockTimesheetConfig(this.unlockSetting).subscribe((res: any) => {
      this.isEditUnlockSetting = !this.isEditUnlockSetting;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
        this.refreshUnlockSetting();
      }
    })
  }

  saveSpecialProjectTaskSetting() {
    this.specialProjectTaskService.change(this.specialProjectTask).subscribe((res: any) => {
      this.isEditSpecialProjectTaskSetting = !this.isEditSpecialProjectTaskSetting;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  SetLevelSetting() {
    this.levelSettingService.set(this.levelSetting).subscribe((res: any) => {
      this.isLevelSetting = !this.isLevelSetting;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  SaveSingleSignOn() {
    this.singleSignOnService.change(this.signOn).subscribe((res: any) => {
      this.isEditSignOn = !this.isEditSignOn;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  SaveMail() {
    this.emailsettingservice.change(this.emailsetting).subscribe((res: any) => {
      this.isEditing = !this.isEditing;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })

  }
  SaveTimesCanLateAndEarlyInMonthSetting() {
    if (_.isEmpty(this.timesCanLateAndEarlyInMonthSetting.timesCanLateAndEarlyInMonth)) {
      abp.message.error("Số lần có thể đi muộn về sớm trong 1 tháng không được để trống!")
      return;
    }
    if (_.isEmpty(this.timesCanLateAndEarlyInMonthSetting.timesCanLateAndEarlyInWeek)) {
      abp.message.error("Số lần có thể đi muộn về sớm trong 1 tuần không được để trống!")
      return;
    }
    this.timesCanLateAndEarlyInMonthSettingService.setTimesCanLateAndEarlyInMonthSetting(this.timesCanLateAndEarlyInMonthSetting).subscribe((res: any) => {
      this.isEditTimesCanLateAndEarlyInMonthSetting = !this.isEditTimesCanLateAndEarlyInMonthSetting;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }
  editTimesCanLateAndEarlyInMonthSetting() {
    this.isEditTimesCanLateAndEarlyInMonthSetting = true;
  }
  formatTime(time: string) {
    if (time) {
      if (time.includes(':')) {
        let t = time.split(':');
        if (t[1]) {
          return moment.utc().hours(Number.parseInt(t[0])).minutes(Number.parseInt(t[1])).format("HH:mm");
        } else {
          return moment.utc().hours(Number.parseInt(t[0])).minutes(0).format("HH:mm");
        }
      } else {
        return moment.utc().hours(Number.parseInt(time)).minutes(0).format("HH:mm");
      }
    }
  }

  calculationTime(timeStart: string, timeEnd: string): number {
    let time = moment(timeEnd, 'HH:mm').diff(moment(timeStart, 'HH:mm'), 'minutes');
    console.log(Math.round(time * 100 / 60) / 100);
    return Math.round(time * 100 / 60) / 100;
  }

  onChangeMSA(event) {
    if (this.workingTime.morningHNEndAt && event) {
      this.workingTime.morningHNWorking = this.calculationTime(event, this.workingTime.morningHNEndAt);
    }
  }

  onChangeMEA(event) {
    if (this.workingTime.morningHNStartAt && event) {
      this.workingTime.morningHNWorking = this.calculationTime(this.workingTime.morningHNStartAt, event);
    }
  }

  onChangeASA(event) {
    if (this.workingTime.afternoonHNEndAt && event) {
      this.workingTime.afternoonHNWorking = this.calculationTime(event, this.workingTime.afternoonHNEndAt);
    }
  }

  onChangeAEA(event) {
    if (this.workingTime.afternoonHNStartAt && event) {
      this.workingTime.afternoonHNWorking = this.calculationTime(this.workingTime.afternoonHNStartAt, event);
    }
  }

  onChangeMSA2(event) {
    if (this.workingTime.morningDNEndAt && event) {
      this.workingTime.morningDNWorking = this.calculationTime(event, this.workingTime.morningDNEndAt);
    }
  }

  onChangeMEA2(event) {
    if (this.workingTime.morningDNStartAt && event) {
      this.workingTime.morningDNWorking = this.calculationTime(this.workingTime.morningDNStartAt, event);
    }
  }

  onChangeASA2(event) {
    if (this.workingTime.afternoonDNEndAt && event) {
      this.workingTime.afternoonDNWorking = this.calculationTime(event, this.workingTime.afternoonDNEndAt);
    }
  }

  onChangeAEA2(event) {
    if (this.workingTime.afternoonDNStartAt && event) {
      this.workingTime.afternoonDNWorking = this.calculationTime(this.workingTime.afternoonDNStartAt, event);
    }
  }

  onChangeMSA3(event) {
    if (this.workingTime.morningHCMEndAt && event) {
      this.workingTime.morningHCMWorking = this.calculationTime(event, this.workingTime.morningHCMEndAt);
    }
  }

  onChangeMEA3(event) {
    if (this.workingTime.morningHCMStartAt && event) {
      this.workingTime.morningHCMWorking = this.calculationTime(this.workingTime.morningHCMStartAt, event);
    }
  }

  onChangeASA3(event) {
    if (this.workingTime.afternoonHCMEndAt && event) {
      this.workingTime.afternoonHCMWorking = this.calculationTime(event, this.workingTime.afternoonHCMEndAt);
    }
  }

  onChangeAEA3(event) {
    if (this.workingTime.afternoonHCMStartAt && event) {
      this.workingTime.afternoonHCMWorking = this.calculationTime(this.workingTime.afternoonHCMStartAt, event);
    }
  }

  SaveTime() {
    this.workingTime.morningHNStartAt = this.formatTime(this.workingTime.morningHNStartAt);
    this.workingTime.morningHNEndAt = this.formatTime(this.workingTime.morningHNEndAt);
    this.workingTime.afternoonHNStartAt = this.formatTime(this.workingTime.afternoonHNStartAt);
    this.workingTime.afternoonHNEndAt = this.formatTime(this.workingTime.afternoonHNEndAt);
    this.workingTime.morningDNStartAt = this.formatTime(this.workingTime.morningDNStartAt);
    this.workingTime.morningDNEndAt = this.formatTime(this.workingTime.morningDNEndAt);
    this.workingTime.afternoonDNStartAt = this.formatTime(this.workingTime.afternoonDNStartAt);
    this.workingTime.afternoonDNEndAt = this.formatTime(this.workingTime.afternoonDNEndAt);
    this.configurationService.change(this.workingTime).subscribe((res: any) => {
      this.isEditTime = !this.isEditTime;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }
  SaveLockTimesheet() {
    this.autoLockTimeSheetService.change(this.lockTimesheet).subscribe(res => {
      this.isEditLockTimesheet = !this.isEditLockTimesheet;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }
  SaveLogTimesheetInFuture() {
    this.logTimesheetService.change(this.logTimesheetInFuture).subscribe(() => {
      this.isEditLogTimesheetInFuture = !this.isEditLogTimesheetInFuture;
    })
  }
  SaveAutoSubmitTimesheet() {
    this.autoSubmitService.change(this.autoSubmitTimesheet).subscribe(() => {
      this.isEditAutoSubmitTimesheet = !this.isEditAutoSubmitTimesheet;
    })
  }
  SaveGetDataFromFaceID() {
    this.getDataFromFaceIdService.change(this.getDataFaceID).subscribe(() => {
      this.isEditGetDataFromFaceID = !this.isEditGetDataFromFaceID;
    })
  }
  SaveMezonSetting() {
    this.isMezonLoading = true;
    this.mezonSettingService.change(this.mezonSetting).subscribe((res) => {
      this.isMezonLoading = false;
      this.isEditMezonSetting = !this.isEditMezonSetting;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  checkShowpass() {
    this.isShowPassword = !this.isShowPassword;
  }

  refreshSercurityCode() {
    this.getSercurityCode();
    this.isEditSercurityCode = false;
  }
  refreshLogoutAllUser() {
    this.getLogoutAllUser();
    this.isEditLogoutAllUser = false;
  }
  refreshWFHSetting() {
    this.getWFHSetting();
    this.isEditWFHSetting = false;
  }
  refreshUnlockSetting() {
    this.getUnlockSetting();
    this.isEditUnlockSetting = false;
  }
  refreshTimesCanLateAndEarlyInMonthSetting() {
    this.getTimesCanLateAndEarlyInMonthSetting();
    this.isEditTimesCanLateAndEarlyInMonthSetting = false;
  }
  refreshSpecialProjectTaskSetting() {
    this.getSpecialProjectTaskSetting();
    this.isEditSpecialProjectTaskSetting = false;
  }

  refreshLevelSetting() {
    this.getLevelSetting();
    this.isLevelSetting = false;
  }
  refreshPunishByRule() {
    this.getCheckInCheckOutPunishmentSettingForConfig();
    this.isPunishByRule = false;
  }

  refreshWorkingTime() {
    this.getWorkingTime();
    this.isEditTime = false;
  }
  refreshSingSignOn() {
    this.get();
    this.isEditSignOn = false;
  }
  refreshEmailSetting() {
    this.list();
    this.isEditing = false;
  }
  refreshLockTimesheet() {
    this.getLockTimesheet();
    this.isEditLockTimesheet = false;
  }
  refreshLogTimesheetInFuture() {
    this.getLogTimesheetInFuture();
    this.isEditLogTimesheetInFuture = false;
  }
  refreshAutoSubmitTimesheet() {
    this.getAutoSubmitTimesheet();
    this.isEditAutoSubmitTimesheet = false;
  }
  refreshGetDataFromFaceID() {
    this.getDataFromFaceID();
    this.isEditGetDataFromFaceID = false;
  }
  refreshMezonSetting() {
    this.getMezonSetting();
    this.isEditMezonSetting = false;
  }
  onChange(value) {
    this.logTimesheetInFuture.canLogTimesheetInFuture = value.toString();
  }

  checkEditDayAllow() {
    if (this.logTimesheetInFuture.canLogTimesheetInFuture == 'true' && this.isEditLogTimesheetInFuture) return true;
    return false;
  }
  //notification setting
  refreshNotificationSetting() {
    this.isEditNotificationSetting = false;
    this.getNotificationSetting();

  }
  getNotificationSetting() {
    if (this.permission.isGranted(this.VIEW_NOTIFICATION_SETTING)) {
      this.configurationService.GetNotificationSetting().subscribe(data => {
        this.notificationSetting = data.result;
      })
    }
  }

  editNotificationSetting() {
    this.isEditNotificationSetting = true;
  }
  saveNotificationSetting() {
    this.configurationService.SetNotificationSetting(this.notificationSetting).subscribe((res) => {
      this.isEditNotificationSetting = !this.isEditNotificationSetting;
      this.getNotificationSetting()
      this.notify.success(this.l('Update Successfully!'));
    })
  }
  onSendMailSubmitTimesheet(value) {
    this.notificationSetting.sendEmailTimesheet = value.toString();
  }
  onSendMailRequest(value) {
    this.notificationSetting.sendEmailRequest = value.toString();
  }
  onSendKomuSubmitTimesheet(value) {
    this.notificationSetting.sendKomuSubmitTimesheet = value.toString();
  }
  onSendKomuRequest(value) {
    this.notificationSetting.sendKomuRequest = value.toString();
  }

  onAllowInternToWorkRemote(value) {
    this.wfhSetting.allowInternToWorkRemote = value.toString();
  }

  onAllowProbationToWorkRemote(value) {
    this.wfhSetting.allowProbationToWorkRemote = value.toString();
  }

  // HRM setting
  refreshHRMConfig() {
    this.getHRMConfig();
    this.isEditHRMConfig = false;
  }
  getHRMConfig() {
    this.isLoading = true;
    if (this.permission.isGranted(this.VIEW_HRM_CONFIG)) {
      this.configurationService.GetHRMConfig().subscribe(data => {
        this.HRMConfig = data.result;
        this.isLoading = false;
      },
        () => this.isLoading = false)
    }
  }
  editHRMConfig() {
    this.isEditHRMConfig = true;
  }
  SaveHRMConfig() {
    this.isLoading = true;
    this.configurationService.SetHRMConfig(this.HRMConfig).subscribe(() => {
      this.isEditHRMConfig = !this.isEditHRMConfig;
      this.isLoading = false;
    },
      () => this.isLoading = false)
  }
  // Komu Setting
  getKomu(): any {
    if (this.permission.isGranted(this.VIEW_KOMU_CONFIG)) {
      this.configurationService.GetKomuConfig().subscribe((result: any) => {
        this.komuSetting = result.result;
        console.log(this.komuSetting);
      });
    }
  }
  editKomu(): any {
    this.isEditingKomu = true;
  }
  saveKomu(): any {
    this.configurationService.SetKomuConfig(this.komuSetting).subscribe((res: any) => {
      this.isEditingKomu = !this.isEditingKomu;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })

  }
  refreshKomuSetting(): any {
    this.getKomu();
    this.isEditingKomu = false;
  }

  saveChannelSendPunishCheckIn(): any {
    this.sendKomuPunishedCheckInService.changePunishedCheckInConfig(this.punishedCheckInSetting).subscribe((res: any) => {
      this.isEditNotifyPunishCheckIn = false;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  getChannelSendPunishCheckIn(): any {
    if (this.permission.isGranted(this.VIEW_PUNISHCHECKIN_CONFIG)) {
      this.sendKomuPunishedCheckInService.getPunishedCheckInConfig().subscribe((result: any) => {
        this.punishedCheckInSetting = result.result;
      })
    }
  }

  editChannelSendPunishCheckIn(): any {
    this.isEditNotifyPunishCheckIn = true;
    this.getChannelSendPunishCheckIn();
  }

  refreshChannelSendPunishCheckIn(): any {
    this.getChannelSendPunishCheckIn();
    this.isEditNotifyPunishCheckIn = false;
  }


  public maskTime = [/[\d]/, /\d/, ':', /\d/, /\d/]

  // Project Setting
  refreshProjectConfig() {
    this.getProjectConfig();
    this.isEditProjectSetting = false;
  }
  getProjectConfig() {
    if (this.permission.isGranted(this.VIEW_PROJECT_CONFIG)) {
      this.configurationService.GetProjectConfig().subscribe(data => {
        this.ProjectConfig = data.result;
      })
    }

  }
  editProjectConfig() {
    this.isEditProjectSetting = true;
  }
  SaveProjectConfig() {
    this.configurationService.SetProjectConfig(this.ProjectConfig).subscribe(() => {
      this.isEditProjectSetting = !this.isEditProjectSetting;
    })
  }

  //NRIT setting
  refreshNRITConfig() {
    this.geNRITConfig();
    this.isEditNRITConfig = false;
  }
  geNRITConfig() {
    if (this.permission.isGranted(this.VIEW_NRIT_CONFIG)) {
      this.configurationService.GetNRITConfig().subscribe(data => {
        this.NRITConfig = data.result;
        const today = new Date();
        const currentMonth = today.getMonth() + 1;
        const currentYear = today.getFullYear();
        this.NRITConfig.notifyReviewDeadline = `${this.NRITConfig.notifyReviewDeadline} (${currentMonth}/${currentYear})`;
      })
    }
  }
  editNRITConfig() {
    this.isEditNRITConfig = true;
  }

  onnotifyEnableWorker(e) {
    if (e.checked == true) {
      this.NRITConfig.notifyEnableWorker = "true"
    }
    else {
      this.NRITConfig.notifyEnableWorker = "false"
    }
  }

  SaveNRITConfig() {
    if (_.isEmpty(this.NRITConfig.notifyAtHourType)) {
      abp.message.error("At hour day required!")
      return;
    }
    if (_.isEmpty(this.NRITConfig.notifyReviewDeadline)) {
      abp.message.error("Notify review deadline required!")
      return;
    }
    if (_.isEmpty(this.NRITConfig.notifyOnDates)) {
      abp.message.error("Notify on dates required!")
      return;
    }
    if (_.isEmpty(this.NRITConfig.notifyToChannels)) {
      abp.message.error("Notify to channels required!")
      return;
    }
    if (_.isEmpty(this.NRITConfig.notifyPenaltyFee)) {
      abp.message.error("Notify Penalty Fee required!")
      return;
    }
    const NRITConfigToSend = { ...this.NRITConfig };
    NRITConfigToSend.notifyReviewDeadline = NRITConfigToSend.notifyReviewDeadline.split(' ')[0];
    this.configurationService.SetNRITConfig(NRITConfigToSend).subscribe((res: any) => {
      this.isEditNRITConfig = !this.isEditNRITConfig;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  // Notify Review Intern via Mezon and Email Setting
  refreshNRITVMAEConfig() {
    this.getNRITVMAEConfig();
    this.isEditNRITVMAEConfig = false;
  }

  getNRITVMAEConfig() {
    if (this.permission.isGranted(this.VIEW_NRITVMAE_CONFIG)) {
      this.configurationService.GetNRITVMAEConfig().subscribe(data => {
        this.NRITVMAEConfig = data.result;
        const today = new Date();
        const currentMonth = today.getMonth() + 1;
        const currentYear = today.getFullYear();
        this.NRITVMAEConfig.notifyHeadPMReviewInternOnDate = `${this.NRITVMAEConfig.notifyHeadPMReviewInternOnDate} (${currentMonth}/${currentYear})`;
        this.NRITVMAEConfig.notifyPresidentReviewInternOnDate = `${this.NRITVMAEConfig.notifyPresidentReviewInternOnDate} (${currentMonth}/${currentYear})`;
      })
    }
  }

  editNRITVMAEConfig() {
    this.isEditNRITVMAEConfig = true;
  }

  onNHPMAPRITEnableWorker(e) {
    if (e.checked == true) {
      this.NRITVMAEConfig.notifyReviewInternEnableWorker = "true"
    }
    else {
      this.NRITVMAEConfig.notifyReviewInternEnableWorker = "false"
    }
  }

  SaveNRITVMAEConfig() {
    if (_.isEmpty(this.NRITVMAEConfig.notifyReviewInternIntervalMinutes)) {
      abp.message.error("Notify Review Intern Interval Minutes required!")
      return;
    }

    if (_.isEmpty(this.NRITVMAEConfig.notifyReviewInternAtHour)) {
      abp.message.error("Notify Review Intern At hour day required!")
      return;
    }

    if (_.isEmpty(this.NRITVMAEConfig.notifyHeadPMReviewInternOnDate)) {
      abp.message.error("Notify HeadPM Review Intern On Date required!")
      return;
    }

    if (_.isEmpty(this.NRITVMAEConfig.notifyPresidentReviewInternOnDate)) {
      abp.message.error("Notify President Review Intern On Date required!")
      return;
    }

    if (_.isEmpty(this.NRITVMAEConfig.notifyHeadPmMail)) {
      abp.message.error("Notify HeadPm Mail required!")
      return;
    }

    if (_.isEmpty(this.NRITVMAEConfig.notifyPresidentEmail)) {
      abp.message.error("Notify President Mail required!")
      return;
    }

    if (_.isEmpty(this.NRITVMAEConfig.notifyHrEmail)) {
      abp.message.error("Notify Hr Mail required!")
      return;
    }

    if (_.isEmpty(this.NRITVMAEConfig.updateTimeCronjobAtHour)) {
      abp.message.error("Update Time Cronjob At Hour required!")
      return;
    }

    if (_.isEmpty(this.NRITVMAEConfig.updateTimeCronjobOnDate)) {
      abp.message.error("Update Time Cronjobn on date required!")
      return;
    }

    if (_.isEmpty(this.NRITVMAEConfig.notifyPmReviewInternOnDates)) {
      abp.message.error("Notify Pm Review Intern On Dates required!")
      return;
    }
    const NRITVMAEConfigToSend = { ...this.NRITVMAEConfig };
    NRITVMAEConfigToSend.notifyHeadPMReviewInternOnDate = NRITVMAEConfigToSend.notifyHeadPMReviewInternOnDate.split(' ')[0];
    NRITVMAEConfigToSend.notifyPresidentReviewInternOnDate = NRITVMAEConfigToSend.notifyPresidentReviewInternOnDate.split(' ')[0];
    this.configurationService.SetNRITVMAEConfig(NRITVMAEConfigToSend).subscribe((res: any) => {
      this.isEditNRITVMAEConfig = !this.isEditNRITVMAEConfig;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  //Retro Notify setting
  refreshRetroNotifyConfig() {
    this.getRetroNotifyConfig();
    this.isEditRetroNotifyConfig = false;
  }

  getRetroNotifyConfig() {
    if (this.permission.isGranted(this.VIEW_RETRONOTIFY_CONFIG)) {
      this.configurationService.getRetroNotifyConfig().subscribe(data => {
        this.RetroNotifyConfig = data.result;
      })
    }
  }

  editRetroNotifyConfig() {
    this.isEditRetroNotifyConfig = true;
  }

  onRetroNotifyEnableWorker(e) {
    if (e.checked == true) {
      this.RetroNotifyConfig.retroNotifyEnableWorker = "true"
    }
    else {
      this.RetroNotifyConfig.retroNotifyEnableWorker = "false"
    }
  }

  //TeamBuilding setting
  refreshTeamBuildingConfig() {
    this.getTeamBuildingConfig();
    this.isEditTeamBuildingConfig = false;
  }
  getTeamBuildingConfig() {
    if (this.permission.isGranted(this.VIEW_TEAMBUILDING_CONFIG)) {
      this.configurationService.GetTeamBuildingConfig().subscribe(data => {
        this.TeamBuildingConfig = data.result;
      })
    }
  }
  editTeamBuildingConfig() {
    this.isEditTeamBuildingConfig = true;
  }

  saveTeamBuildingConfig() {
    if (_.isEmpty(this.TeamBuildingConfig.generateDataOnDate)) {
      abp.message.error("Generate data on date required!")
      return;
    }
    if (_.isEmpty(this.TeamBuildingConfig.teamBuildingMoney)) {
      abp.message.error("Team building money required!")
      return;
    }
    if (_.isEmpty(this.TeamBuildingConfig.vat)) {
      abp.message.error("VAT required!")
      return;
    }
    this.configurationService.SetTeamBuildingConfig(this.TeamBuildingConfig).subscribe((res: any) => {
      this.isEditTeamBuildingConfig = !this.isEditTeamBuildingConfig;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  saveRetroNotifyConfig() {
    if (_.isEmpty(this.RetroNotifyConfig.retroNotifyAtHour)) {
      abp.message.error("At hour day required!")
      return;
    }
    if (_.isEmpty(this.RetroNotifyConfig.retroNotifyDeadline)) {
      abp.message.error("Notify retro deadline required!")
      return;
    }
    if (_.isEmpty(this.RetroNotifyConfig.retroNotifyOnDates)) {
      abp.message.error("Notify on dates required!")
      return;
    }
    if (_.isEmpty(this.RetroNotifyConfig.retroNotifyToChannels)) {
      abp.message.error("Notify to channels required!")
      return;
    }
    this.configurationService.setRetroNotifyConfig(this.RetroNotifyConfig).subscribe((res: any) => {
      this.isEditRetroNotifyConfig = !this.isEditRetroNotifyConfig;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }
  //TimeStartChangingCheckinToCheckout setting
  refreshTimeStartChangingCheckinToCheckoutSetting() {
    this.getTimeStartChangingCheckinToCheckoutSetting();
    this.isEditTimeStartChangingCheckinToCheckoutSetting = false;
  }
  getTimeStartChangingCheckinToCheckoutSetting() {
    if (this.permission.isGranted(this.VIEW_TIMESTARTCHANGINGCHECKINTOCHECKOUT_SETTING)) {
      this.timeStartChangingCheckinToCheckoutSettingService.get().subscribe(data => {
        this.TimeStartChangingCheckinToCheckoutSetting = data.result;
      })
    }
  }

  editTimeStartChangingCheckinToCheckoutSetting() {
    this.isEditTimeStartChangingCheckinToCheckoutSetting = true;
  }

  saveTimeStartChangingCheckinToCheckoutSetting() {
    if (_.isEmpty(this.TimeStartChangingCheckinToCheckoutSetting.timeStartCheckOut)) {
      abp.message.error("Time start checkout required!")
      return;
    }
    if (_.isEmpty(this.TimeStartChangingCheckinToCheckoutSetting.timeStartCheckOutCaseOffAfternoon)) {
      abp.message.error("Time start checkout required!")
      return;
    }
    this.timeStartChangingCheckinToCheckoutSettingService.change(this.TimeStartChangingCheckinToCheckoutSetting).subscribe(rs => {
      this.getTimeStartChangingCheckinToCheckoutSetting();
      this.isEditTimeStartChangingCheckinToCheckoutSetting = false;
      abp.notify.success("Update Successfully")
    })
  }

  //Approve Timesheet Notify setting
  refreshApproveTimesheetNotifyConfig() {
    this.getApproveTimesheetNotifyConfig();
    this.isEditApproveTimesheetNotifyConfig = false;
  }

  getApproveTimesheetNotifyConfig() {
    if (this.permission.isGranted(this.VIEW_APPROVETIMESHEETNOTIFY_CONFIG)) {
      this.configurationService.getApproveTimesheetNotifyConfig().subscribe(data => {
        this.ApproveTimesheetNotifyConfig = data.result;
      })
    }
  }

  editApproveTimesheetNotifyConfig() {
    this.isEditApproveTimesheetNotifyConfig = true;
  }

  onApproveTimesheetNotifyEnableWorker(e) {
    if (e.checked == true) {
      this.ApproveTimesheetNotifyConfig.approveTimesheetNotifyEnableWorker = "true"
    }
    else {
      this.ApproveTimesheetNotifyConfig.approveTimesheetNotifyEnableWorker = "false"
    }
  }

  saveApproveTimesheetNotifyConfig() {
    if (_.isEmpty(this.ApproveTimesheetNotifyConfig.approveTimesheetNotifyAtHour)) {
      abp.message.error("At hour day required!")
      return;
    }
    if (_.isEmpty(this.ApproveTimesheetNotifyConfig.approveTimesheetNotifyOnDates)) {
      abp.message.error("Notify on dates required!")
      return;
    }
    if (_.isEmpty(this.ApproveTimesheetNotifyConfig.approveTimesheetNotifyToChannels)) {
      abp.message.error("Notify to channels required!")
      return;
    }
    if (_.isEmpty(this.ApproveTimesheetNotifyConfig.approveTimesheetNotifyTimePeriodWithPendingRequest)) {
      abp.message.error("Time period with pending request required!")
      return;
    }
    this.configurationService.setApproveTimesheetNotifyConfig(this.ApproveTimesheetNotifyConfig).subscribe((res: any) => {
      this.isEditApproveTimesheetNotifyConfig = !this.isEditApproveTimesheetNotifyConfig;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  //Approve Request Off Notify setting
  refreshApproveRequestOffNotifyConfig() {
    this.getApproveRequestOffNotifyConfig();
    this.isEditApproveRequestOffNotifyConfig = false;
  }

  getApproveRequestOffNotifyConfig() {
    if (this.permission.isGranted(this.VIEW_APPROVEREQUESTOFFNOTIFY_CONFIG)) {
      this.configurationService.getApproveRequestOffNotifyConfig().subscribe(data => {
        this.ApproveRequestOffNotifyConfig = data.result;
      })
    }
  }

  editApproveRequestOffNotifyConfig() {
    this.isEditApproveRequestOffNotifyConfig = true;
  }

  onApproveRequestOffNotifyEnableWorker(e) {
    if (e.checked == true) {
      this.ApproveRequestOffNotifyConfig.approveRequestOffNotifyEnableWorker = "true"
    }
    else {
      this.ApproveRequestOffNotifyConfig.approveRequestOffNotifyEnableWorker = "false"
    }
  }

  saveApproveRequestOffNotifyConfig() {
    if (_.isEmpty(this.ApproveRequestOffNotifyConfig.approveRequestOffNotifyAtHour)) {
      abp.message.error("At hour day required!")
      return;
    }
    if (_.isEmpty(this.ApproveRequestOffNotifyConfig.approveRequestOffNotifyToChannels)) {
      abp.message.error("Notify to channels required!")
      return;
    }
    if (_.isEmpty(this.ApproveRequestOffNotifyConfig.approveRequestOffSendUserAtHour)) {
      abp.message.error("At hour day required!")
      return;
    }
    if (_.isEmpty(this.ApproveRequestOffNotifyConfig.approveRequestOffNotifyTimePeriodWithPendingRequest)) {
      abp.message.error("Time period with pending request required!")
      return;
    }
    this.configurationService.setApproveRequestOffNotifyConfig(this.ApproveRequestOffNotifyConfig).subscribe((res: any) => {
      this.isEditApproveRequestOffNotifyConfig = !this.isEditApproveRequestOffNotifyConfig;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  //Send message request pending teambuilding to HR setting
  refreshSendMessageRequestPendingTeamBuildingToHRConfig() {
    this.getSendMessageRequestPendingTeamBuildingToHRConfig();
    this.isEditSendMessageRequestPendingTeamBuildingToHRConfig = false;
  }

  getSendMessageRequestPendingTeamBuildingToHRConfig() {
    if (this.permission.isGranted(this.VIEW_SENDMESSAGEREQUESTPENDINGTEAMBUILDINGTOHR_CONFIG)) {
      this.configurationService.getSendMessageRequestPendingTeamBuildingToHRConfig().subscribe(data => {
        this.sendMessageRequestPendingTeamBuildingToHRConfig = data.result;
      })
    }
  }

  editSendMessageRequestPendingTeamBuildingToHRConfig() {
    this.isEditSendMessageRequestPendingTeamBuildingToHRConfig = true;
  }

  onSendMessageRequestPendingTeamBuildingToHREnableWorker(e) {
    if (e.checked == true) {
      this.sendMessageRequestPendingTeamBuildingToHRConfig.sendMessageRequestPendingTeamBuildingToHREnableWorker = "true"
    }
    else {
      this.sendMessageRequestPendingTeamBuildingToHRConfig.sendMessageRequestPendingTeamBuildingToHREnableWorker = "false"
    }
  }

  saveSendMessageRequestPendingTeamBuildingToHRConfig() {
    if (_.isEmpty(this.sendMessageRequestPendingTeamBuildingToHRConfig.sendMessageRequestPendingTeamBuildingToHRAtHour)) {
      abp.message.error("At hour day required!")
      return;
    }
    if (_.isEmpty(this.sendMessageRequestPendingTeamBuildingToHRConfig.sendMessageRequestPendingTeamBuildingToHRToChannels)) {
      abp.message.error("Notify to channels required!")
      return;
    }
    if (_.isEmpty(this.sendMessageRequestPendingTeamBuildingToHRConfig.sendMessageRequestPendingTeamBuildingToHREmail)) {
      abp.message.error("Email required!")
      return;
    }
    this.configurationService.setSendMessageRequestPendingTeamBuildingToHRConfig(this.sendMessageRequestPendingTeamBuildingToHRConfig).subscribe((res: any) => {
      this.isEditSendMessageRequestPendingTeamBuildingToHRConfig = !this.isEditSendMessageRequestPendingTeamBuildingToHRConfig;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  // Reset Data TeamBuilding config
  refreshResetDataTeamBuildingConfig() {
    this.getResetDataTeamBuildingConfig();
    this.isEditResetDataTeambuildingConfig = false;
  }
  getResetDataTeamBuildingConfig() {
    if (this.permission.isGranted(this.VIEW_RESETDATATEAMBUILDING_CONFIG)) {
      this.configurationService.getResetDataTeamBuildingConfig().subscribe(data => {
        this.ResetDataTeamBuildingConfig = data.result;
      })
    }
  }
  editResetDataTeamBuildingConfig() {
    this.isEditResetDataTeambuildingConfig = true;
  }

  onResetDataTeamBuildingEnableWorker(e) {
    if (e.checked == true) {
      this.ResetDataTeamBuildingConfig.resetDataTeamBuildingEnableWorker = "true";
    }
    else {
      this.ResetDataTeamBuildingConfig.resetDataTeamBuildingEnableWorker = "false";
    }
  }

  saveResetDataTeamBuildingConfig() {
    if (_.isEmpty(this.ResetDataTeamBuildingConfig.resetDataTeamBuildingAtHour)) {
      abp.message.error("Reset data at hour required!")
      return;
    }
    if (_.isEmpty(this.ResetDataTeamBuildingConfig.resetDataTeamBuildingOnDateAndMonth)) {
      abp.message.error("Reset data on date and month required!")
      return;
    }
    this.configurationService.setResetDataTeamBuildingConfig(this.ResetDataTeamBuildingConfig).subscribe((res: any) => {
      this.isEditResetDataTeambuildingConfig = !this.isEditResetDataTeambuildingConfig;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  //Notify HR The Employee May Have Left Setting
  refreshNotifyHRTheEmployeeMayHaveLeftConfig() {
    this.getNotifyHRTheEmployeeMayHaveLeftConfig();
    this.isEditNotifyHRTheEmployeeMayHaveLeftConfig = false;
  }

  getNotifyHRTheEmployeeMayHaveLeftConfig() {
    if (this.permission.isGranted(this.VIEW_NOTIFYHRTHEEMPLOYEEMAYHAVELEFT_CONFIG)) {
      this.configurationService.getConfigNotifyHRTheEmployeeMayHaveLeft().subscribe(data => {
        this.notifyHRTheEmployeeMayHaveLeftConfig = data.result;
      })
    }
  }

  editNotifyHRTheEmployeeMayHaveLeftConfig() {
    this.isEditNotifyHRTheEmployeeMayHaveLeftConfig = true;
  }

  onNotifyHRTheEmployeeMayHaveLeftEnableWorker(e) {
    if (e.checked == true) {
      this.notifyHRTheEmployeeMayHaveLeftConfig.notifyHRTheEmployeeMayHaveLeftEnableWorker = "true"
    }
    else {
      this.notifyHRTheEmployeeMayHaveLeftConfig.notifyHRTheEmployeeMayHaveLeftEnableWorker = "false"
    }
  }

  saveNotifyHRTheEmployeeMayHaveLeftConfig() {
    if (_.isEmpty(this.notifyHRTheEmployeeMayHaveLeftConfig.notifyHRTheEmployeeMayHaveLeftAtHour)) {
      abp.message.error("At hour day required!")
      return;
    }
    if (_.isEmpty(this.notifyHRTheEmployeeMayHaveLeftConfig.notifyHRTheEmployeeMayHaveLeftToChannels)) {
      abp.message.error("Notify to channels required!")
      return;
    }
    if (_.isEmpty(this.notifyHRTheEmployeeMayHaveLeftConfig.notifyHRTheEmployeeMayHaveLeftToHREmail)) {
      abp.message.error("Email required!")
      return;
    }
    if (_.isEmpty(this.notifyHRTheEmployeeMayHaveLeftConfig.notifyHRTheEmployeeMayHaveLeftTimePeriod)) {
      abp.message.error("Time Period required!")
      return;
    }
    this.configurationService.setConfigNotifyHRTheEmployeeMayHaveLeft(this.notifyHRTheEmployeeMayHaveLeftConfig).subscribe((res: any) => {
      this.isEditNotifyHRTheEmployeeMayHaveLeftConfig = !this.isEditNotifyHRTheEmployeeMayHaveLeftConfig;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }
  // Money Pm Unlock TimeSheet Setting
  refreshMoneyPMUnlockTimeSheetConfig() {
    this.getMoneyPMUnlockTimeSheetConfig();
    this.isEditMoneyPMUnlockTimeSheetConfig = false;
  }

  getMoneyPMUnlockTimeSheetConfig() {
    if (this.permission.isGranted(this.VIEW_MONEYPMUNLOCKTIMESHEET_CONFIG)) {
      this.configurationService.getConfigMoneyPMUnlockTimeSheet().subscribe(data => {
        this.moneyPMUnlockTimeSheetConfig = data.result;
      })
    }
  }

  editMoneyPMUnlockTimeSheetConfig() {
    this.isEditMoneyPMUnlockTimeSheetConfig = true;
  }

  saveMoneyPMUnlockTimeSheetConfig() {
    if (_.isEmpty(this.moneyPMUnlockTimeSheetConfig.moneyPMUnlockTimeSheet)) {
      abp.message.error("Amount of money required!")
      return;
    }

    this.configurationService.setConfigMoneyPMUnlockTimeSheet(this.moneyPMUnlockTimeSheetConfig).subscribe((res: any) => {
      this.isEditMoneyPMUnlockTimeSheetConfig = !this.isEditMoneyPMUnlockTimeSheetConfig;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  //Send Message To Punish User Setting
  refreshSendMessageToPunishUserConfig() {
    this.getSendMessageToPunishUserConfig();
    this.isEditSendMessageToPunishUserConfig = false;
  }

  getSendMessageToPunishUserConfig() {
    if (this.permission.isGranted(this.VIEW_SENDMESSAGETOPUNISHUSER_CONFIG)) {
      this.configurationService.getConfigSendMessageToPunishUser().subscribe(data => {
        this.sendMessageToPunishUserConfig = data.result;
      })
    }
  }

  editSendMessageToPunishUserConfig() {
    this.isEditSendMessageToPunishUserConfig = true;
  }

  onSendMessageToPunishUserEnableWorker(e) {
    if (e.checked == true) {
      this.sendMessageToPunishUserConfig.sendMessageToPunishUserEnableWorker = "true"
    }
    else {
      this.sendMessageToPunishUserConfig.sendMessageToPunishUserEnableWorker = "false"
    }
  }

  saveSendMessageToPunishUserConfig() {
    if (_.isEmpty(this.sendMessageToPunishUserConfig.sendMessageToPunishUserAtHour)) {
      abp.message.error("At hour day required!")
      return;
    }
    this.configurationService.setConfigSendMessageToPunishUser(this.sendMessageToPunishUserConfig).subscribe((res: any) => {
      this.isEditSendMessageToPunishUserConfig = !this.isEditSendMessageToPunishUserConfig;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  toggleEnableTimeStartChangingToCheckout(e) {
    if (e.checked == true) {
      this.TimeStartChangingCheckinToCheckoutSetting.enableTimeStartChangingToCheckout = "true"
    }
    else {
      this.TimeStartChangingCheckinToCheckoutSetting.enableTimeStartChangingToCheckout = "false"
    }
  }

  //Create New Retro setting
  refreshCreateNewRetroConfig() {
    this.getCreateNewRetroConfig();
    this.isEditCreateNewRetroConfig = false;
  }

  getCreateNewRetroConfig() {
    if (this.permission.isGranted(this.VIEW_CREATENEWRETRO_CONFIG)) {
      this.configurationService.getConfigCreateNewRetro().subscribe(data => {
        this.CreateNewRetroConfig = data.result;
      })
    }
  }

  editCreateNewRetroConfig() {
    this.isEditCreateNewRetroConfig = true;
  }

  onCreateNewRetroEnableWorker(e) {
    if (e.checked == true) {
      this.CreateNewRetroConfig.createNewRetroEnableWorker = "true"
    }
    else {
      this.CreateNewRetroConfig.createNewRetroEnableWorker = "false"
    }
  }

  saveCreateNewRetroConfig() {
    if (_.isEmpty(this.CreateNewRetroConfig.createNewRetroAtHour)) {
      abp.message.error("At hour day required!")
      return;
    }
    if (_.isEmpty(this.CreateNewRetroConfig.createNewRetroOnDate)) {
      abp.message.error("Dates required!")
      return;
    }
    if (this.CreateNewRetroConfig.createNewRetroAtHour > this.GenerateRetroResultConfig.generateRetroResultAtHour
      || this.CreateNewRetroConfig.createNewRetroAtHour == this.GenerateRetroResultConfig.generateRetroResultAtHour) {
      abp.message.error("The retro creation time must be less than the retro result creation time")
      return;
    }
    if (this.CreateNewRetroConfig.createNewRetroOnDate > this.GenerateRetroResultConfig.generateRetroResultOnDate) {
      abp.message.error("The retro creation date must be less than the retro result creation date")
      return;
    }
    this.configurationService.setConfigCreateNewRetro(this.CreateNewRetroConfig).subscribe((res: any) => {
      this.isEditCreateNewRetroConfig = !this.isEditCreateNewRetroConfig;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  //Generate Data Retro result setting
  refreshGenerateRetroResultConfig() {
    this.getGenerateRetroResultConfig();
    this.isEditGenerateRetroResult = false;
  }

  getGenerateRetroResultConfig() {
    if (this.permission.isGranted(this.VIEW_GENERATERETRORESULT_CONFIG)) {
      this.configurationService.getConfigGenerateRetroResult().subscribe(data => {
        this.GenerateRetroResultConfig = data.result;
      })
    }
  }

  editGenerateRetroResultConfig() {
    this.isEditGenerateRetroResult = true;
  }

  onRetroResultEnableWorker(e) {
    if (e.checked == true) {
      this.GenerateRetroResultConfig.generateRetroResultEnableWorker = "true"
    }
    else {
      this.GenerateRetroResultConfig.generateRetroResultEnableWorker = "false"
    }
  }

  saveGenerateRetroResultConfig() {
    if (_.isEmpty(this.GenerateRetroResultConfig.generateRetroResultAtHour)) {
      abp.message.error("At hour day required!")
      return;
    }
    if (_.isEmpty(this.GenerateRetroResultConfig.generateRetroResultOnDate)) {
      abp.message.error("Dates required!")
      return;
    }
    if (this.GenerateRetroResultConfig.generateRetroResultAtHour < this.CreateNewRetroConfig.createNewRetroAtHour
      || this.GenerateRetroResultConfig.generateRetroResultAtHour == this.CreateNewRetroConfig.createNewRetroAtHour) {
      abp.message.error("The time to create the retro result must be greater than the time to create the retro")
      return;
    }
    if (this.GenerateRetroResultConfig.generateRetroResultOnDate < this.CreateNewRetroConfig.createNewRetroOnDate) {
      abp.message.error("The retro result creation date must be greater than the retro result creation date")
      return;
    }
    this.configurationService.setConfigGenerateRetroResult(this.GenerateRetroResultConfig).subscribe((res: any) => {
      this.isEditGenerateRetroResult = !this.isEditGenerateRetroResult;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }
  ManualGetOpenTalk() {
    this.isMezonLoading = true;
    this.mezonSettingService.manualGetTimeJoinedOpenTalk(this.manualOpenTalk).subscribe(data => {
      this.isMezonLoading = false;
      if (data.success) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  getLateInternReviewSetting() {
    if (this.permission.isGranted(this.VIEW_LATE_INTERN_REVIEW_SETTING)) {
      this.lateInternReviewSettingService.get().subscribe((data: any) => {
        this.lateInternReviewSetting = data.result;
      })
    }
  }

  editLateInternReviewSetting() {
    this.isEditLateInternReviewSetting = true;
  }

  selectedMonth: string = (() => {
    const date = new Date();
    date.setMonth(date.getMonth() - 1);
    return date.toISOString().slice(0, 7);
  })();

  selectedPmReportMonth: string = new Date().toISOString().slice(0, 7);

  onManualTriggerPMReportPunishment() {
    abp.message.confirm(
      'Are you sure you want to apply PM Report Punishment?',
      'Confirm',
      (result: boolean) => {
        if (result) {
          this.pmReportPunishSettingService.triggerManualPunishment().subscribe(
            () => {
              abp.notify.success('PM Report Punishment has been applied successfully');
            },
            (error) => {
              abp.notify.error('Failed to apply PM Report Punishment: ' +
                (error.error && error.error.error && error.error.error.message || error.message));
            }
          );
        }
      }
    );
  }

  getMonthName(month: number): string {
    const months = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
    return months[month - 1] || '';
  }

  onManualTriggerPunishment() {
    if (!this.selectedMonth) {
      abp.notify.warn('Please select a month/year');
      return;
    }

    const [year, month] = this.selectedMonth.split('-').map(Number);

    abp.message.confirm(
      `Are you sure you want to trigger punishment for ${month}/${year}?`,
      'Confirm Punishment Execution',
      (result: boolean) => {
        if (result) {
          this.lateInternReviewSettingService.triggerManualPunishment(month, year).subscribe(
            () => {
              abp.notify.success('Punishments applied successfully.');
            },
            (error) => {
              const errorMessage = (error.error && error.error.error && error.error.error.message)
                || error.message
                || 'An error occurred';
              abp.notify.error('Error: ' + errorMessage);
            }
          );
        }
      }
    );
  }

  saveLateInternReviewSetting() {
    if (!this.permission.isGranted(this.EDIT_LATE_INTERN_REVIEW_SETTING)) {
      abp.message.error("You do not have permission to edit this setting!");
      return;
    }
    if (!this.lateInternReviewSetting.deadlineDay) {
      abp.message.error("Review Deadline Day is required!");
      return;
    }
    if (!this.lateInternReviewSetting.startDayOfMonth) {
      abp.message.error("Start Day Of Month is required!");
      return;
    }
    if (!this.lateInternReviewSetting.nextRunDate) {
      abp.message.error("Next Run Date is required!");
      return;
    }

    this.lateInternReviewSettingService.change(this.lateInternReviewSetting).subscribe((res: any) => {
      this.isEditLateInternReviewSetting = false;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  refreshLateInternReviewSetting() {
    this.getLateInternReviewSetting();
    this.isEditLateInternReviewSetting = false;
  }

  getPMReportPunishSetting() {
    if (this.permission.isGranted(this.VIEW_PM_REPORT_PUNISH_SETTING)) {
      this.pmReportPunishSettingService.get().subscribe((data: any) => {
        this.pmReportPunishSetting = data.result;
      });
    }
  }

  editPMReportPunishSetting() {
    this.isEditPMReportPunishSetting = true;
  }

  savePMReportPunishSetting() {
    if (!this.permission.isGranted(this.EDIT_PM_REPORT_PUNISH_SETTING)) {
      abp.message.error("You do not have permission to edit this setting!");
      return;
    }
    if (this.pmReportPunishSetting.hour < 0 || this.pmReportPunishSetting.hour > 23) {
      abp.message.error("Hour must be between 0 and 23!");
      return;
    }
    if (!this.pmReportPunishSetting.dayofweek) {
      abp.message.error("Day of week is required!");
      return;
    }

    this.pmReportPunishSettingService.change(this.pmReportPunishSetting).subscribe((res: any) => {
      this.isEditPMReportPunishSetting = false;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }
  refreshPMReportPunishSetting() {
    this.getPMReportPunishSetting();
    this.isEditPMReportPunishSetting = false;
  }

  getBotReportSetting() {
    if (this.permission.isGranted(this.VIEW_BOT_REPORT_SETTING)) {
      this.projects = [];

      this.botReportSettingService.getBotReportSetting().subscribe((data: any) => {
        this.botReportSetting = data.result;
        this.selectedProjects = this.botReportSetting.projectIds || [];

        if (this.botReportSetting.branchCodes && this.botReportSetting.branchCodes.length > 0) {
          this.selectedBranches = [...this.botReportSetting.branchCodes];
        } else {
          this.selectedBranches = [];
        }
        this.updateInitialState();
      });

      this.botReportSettingService.getActiveProjects().subscribe({
        next: (response: any) => {
          this.projects = response.result || [];
        },
        error: (error) => {
          console.error('Error fetching projects:', error);
        }
      });
    }
  }

  getAnomaliesReportSetting() {
    if (this.permission.isGranted(this.VIEW_ANOMALIES_REPORT_SETTING)) {
      this.anomaliesReportSettingService.getAnomaliesReportSetting().subscribe((data: any) => {
        this.anomaliesReportSetting = data.result;
        if (this.anomaliesReportSetting.branchCodes && this.anomaliesReportSetting.branchCodes.length > 0) {
          this.selectedAnomaliesBranches = [...this.anomaliesReportSetting.branchCodes];
        } else {
          this.selectedAnomaliesBranches = [];
        }
        this.updateInitialAnomaliesState();
      });
    }
  }

  editBotReportSetting() {
    this.isEditBotReportSetting = true;
  }

  editAnomaliesReportSetting() {
    this.isEditAnomaliesReportSetting = true;
  }

  saveBotReportSetting() {
    if (!this.permission.isGranted(this.EDIT_BOT_REPORT_SETTING)) {
      abp.message.error("You do not have permission to edit this setting!");
      return;
    }
    if (this.botReportSetting.hour < 0 || this.botReportSetting.hour > 23) {
      abp.message.error("Hour must be between 0 and 23!");
      return;
    }

    if (!this.botReportSetting.everyday && !this.botReportSetting.dayofweek) {
      abp.message.error("Day of week is required!");
      return;
    }

    if (!this.selectedBranches || this.selectedBranches.length === 0) {
      abp.message.error("You must select at least one branch!");
      return;
    }

    this.botReportSetting.branchCodes = [...this.selectedBranches];
    this.botReportSetting.projectIds = this.selectedProjects;
    
    this.botReportSettingService.setBotReportSetting(this.botReportSetting).subscribe((res: any) => {
      this.isEditBotReportSetting = false;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })

  }

  saveAnomaliesReportSetting() {
    if (!this.permission.isGranted(this.EDIT_ANOMALIES_REPORT_SETTING)) {
      abp.message.error("You do not have permission to edit this setting!");
      return;
    }
    if (this.anomaliesReportSetting.hour < 0 || this.anomaliesReportSetting.hour > 23) {
      abp.message.error("Hour must be between 0 and 23!");
      return;
    }
    if (!this.anomaliesReportSetting.dayofweek) {
      abp.message.error("Day of week is required!");
      return;
    }
    if (!this.selectedAnomaliesBranches || this.selectedAnomaliesBranches.length === 0) {
      abp.message.error("You must select at least one branch!");
      return;
    }

    this.anomaliesReportSetting.branchCodes = [...this.selectedAnomaliesBranches];

    this.anomaliesReportSettingService.setAnomaliesReportSetting(this.anomaliesReportSetting).subscribe((res: any) => {
      this.isEditAnomaliesReportSetting = false;
      if (res) {
        this.notify.success(this.localization.localize('Update Successfully!', ''));
      }
    });
  }

  refreshBotReportSetting() {
    this.getBotReportSetting();
    this.isEditBotReportSetting = false;
  }

  refreshAnomaliesReportSetting() {
    this.getAnomaliesReportSetting();
    this.isEditAnomaliesReportSetting = false;
  }

  private updateInitialState() {
    if (this.selectedBranches.length === this.branchCodes.length) {
      this.isAllBranchesSelected = true;
    } else if (this.selectedBranches.length === 0 || (this.selectedBranches.length === 1 && this.selectedBranches[0] === 'HN1')) {
      this.selectedBranches = ['HN1'];
      this.isAllBranchesSelected = false;
    } else {
      this.isAllBranchesSelected = false;
    }
  }

  private updateInitialAnomaliesState() {
    if (this.selectedAnomaliesBranches.length === this.branchCodes.length) {
      this.isAllAnomaliesBranchesSelected = true;
    } else if (this.selectedAnomaliesBranches.length === 0 || (this.selectedAnomaliesBranches.length === 1 && this.selectedAnomaliesBranches[0] === 'HN1')) {
      this.selectedAnomaliesBranches = ['HN1'];
      this.isAllAnomaliesBranchesSelected = false;
    } else {
      this.isAllAnomaliesBranchesSelected = false;
    }
  }

  toggleAllProjects() {
    this.selectedProjects = this.selectedProjects.filter(id => id !== -1);

    if (!this.isAllProjectsSelected) {
      this.selectedProjects = this.projects.map(project => project.id);
      this.isAllProjectsSelected = true;
    } else {
      this.selectedProjects = [];
      this.isAllProjectsSelected = false;
    }
  }

  toggleAllBranches() {
    this.selectedBranches = this.selectedBranches.filter(code => code !== '-1');

    if (!this.isAllBranchesSelected) {
      this.selectedBranches = [...this.branchCodes];
      this.isAllBranchesSelected = true;
    } else {
      this.selectedBranches = ['HN1'];
      this.isAllBranchesSelected = false;
      abp.notify.info('At least one branch must be selected. Branch HN1 has been selected by default.');
    }
  }

  toggleAllAnomaliesBranches() {
    this.selectedAnomaliesBranches = this.selectedAnomaliesBranches.filter(code => code !== '-1');
    if (!this.isAllAnomaliesBranchesSelected) {
      this.selectedAnomaliesBranches = [...this.branchCodes];
      this.isAllAnomaliesBranchesSelected = true;
    } else {
      this.selectedAnomaliesBranches = ['HN1'];
      this.isAllAnomaliesBranchesSelected = false;
      abp.notify.info('At least one branch must be selected. Branch HN1 has been selected by default.');
    }
  }
}

export class EmailSettingDto {
  fromDisplayName: string;
  userName: string;
  port: string;
  host: string;
  password: string;
  enableSsl: string;
  defaultFromAddress: string;
  useDefaultCredentials: string;
}
export class SingleSignOnDto {
  clientAppId: string;
  registerSecretCode: String;
}
export class WorkingTimeDTO {
  morningHNStartAt: string;
  morningHNEndAt: string;
  afternoonHNStartAt: string;
  afternoonHNEndAt: string;
  morningDNStartAt: string;
  morningDNEndAt: string;
  afternoonDNStartAt: string;
  afternoonDNEndAt: string;
  morningHCMStartAt: string;
  morningHCMEndAt: string;
  afternoonHCMStartAt: string;
  afternoonHCMEndAt: string;
  morningHNWorking: number;
  morningDNWorking: number;
  morningHCMWorking: number;
  afternoonHNWorking: number;
  afternoonDNWorking: number;
  afternoonHCMWorking: number;
  emailHR: string;
  emailHRDN: string;
  emailHRHCM: string;
}

export class LockTimesheetDTO {
  lockDayOfUser: string;
  lockHourOfUser: string;
  lockMinuteOfUser: string;
  lockDayOfPM: string;
  lockHourOfPM: string;
  lockMinuteOfPM: string;
  lockDayAfterUnlock: string;
  lockHourAfterUnlock: string;
  lockMinuteAfterUnlock: string;
}

export class SercurityCodeDTO {
  sercurityCode: string;
}
export class LogoutAllUserDTO {
  loginBefore: string;
}
export class WFHSettingDTO {
  numOfRemoteDays: string;
  allowInternToWorkRemote: string;
  allowProbationToWorkRemote: string;
  totalTimeTardinessAndEarlyLeave : string;
}

export class UnlockSettingDTO {
  weeksCanUnlockBefor: string;
}

export class SpecialProjectTaskSettingDTO {
  projectTaskId: string;
}

export class LevelSettingDTO {
  percentSalaryProbationary: string;
  userLevelSetting: string;
}

export class PunishByRuleDTO {
  checkInCheckOutPunishmentSetting: string
}

export class LogTimesheetInFutureDTO {
  canLogTimesheetInFuture: string;
  dayAllowLogTimesheetInFuture: string;
  dateToLockTimesheetOfLastMonth: string;
  maxTimeOfDayLogTimesheet: string;
}

export class AutoSubmitTimesheetDto {
  autoSubmitTimesheet: string;
  autoSubmitAt: string;
}

export class GetDataFromFaceIDDto {
  getDataAt: string;
  accountID: string;
  secretCode: string;
  uri: string;
}

export class MezonSetting {
  enable: boolean;
  hour: number;
  dayofweek: string;
  secretCode: string;
  uri: string;
}

export class HRMConfigDto {
  hrmUri: string;
  secretCode: string;
}

export class NRITConfigDto {
  notifyEnableWorker: string;
  notifyAtHourType: string;
  notifyReviewDeadline: string;
  notifyOnDates: string;
  notifyToChannels: string;
  notifyPenaltyFee: string;
}

export class NotifyReviewInternViaMezonAndEmailConfigDto {
  notifyReviewInternEnableWorker: string;
  notifyReviewInternIntervalMinutes: string;
  notifyReviewInternAtHour: string;
  notifyHeadPMReviewInternOnDate: string;
  notifyPresidentReviewInternOnDate: string;
  notifyHeadPmMail: string;
  notifyPresidentEmail: string;
  notifyHrEmail: string;
  updateTimeCronjobAtHour: string;
  updateTimeCronjobOnDate: string;
  notifyPmReviewInternOnDates: string;
}

export class ProjectConfigDto {
  projectUri: string;
  secretCode: string;
}

export class EmailSaoDoDto {
  emailSaoDo: string;
  canSendEmailToSaoDo: string;
}

export class KomuDto {
  komuUri: string;
  komuSecretCode: string;
  komuChannelIdDevMode: string;
  komuUserNameDevMode: string;
}

export class NotificationSettingDto {
  sendEmailTimesheet: string;
  sendEmailRequest: string;
  sendKomuSubmitTimesheet: string;
  sendKomuRequest: string;
}

export class UnlockTimesheetConfigDto {
  weeksCanUnlockBefor: string;
}

export class KomuPunishCheckInDto {
  timeSendPunishUser: string;
  channelNotifyPunishUser: string;
  percentOfTrackerOnWorking: string;
}

export class GetConnectResultDto {
  isConnected: boolean;
  message: string;
}

export class GetCheckInCheckOutPunishmentSettingDto {
  id: number;
  name: string;
  note: string;
  money: number;

}

export class InputToUpdateSettingDto {
  id: number;
  money: number;
}

export class RetroNotifyConfigDto {
  retroNotifyEnableWorker: string;
  retroNotifyAtHour: string;
  retroNotifyDeadline: string;
  retroNotifyOnDates: string;
  retroNotifyToChannels: string;
}

export class TeamBuildingConfigDto {
  generateDataOnDate: string;
  teamBuildingMoney: string;
  vat: string;
}

export class TimesCanLateAndEarlyInMonthSettingDto {
  timesCanLateAndEarlyInMonth: string;
  timesCanLateAndEarlyInWeek: string;
}

export class TimeStartChangingToCheckoutSettingDto {
  enableTimeStartChangingToCheckout: string;
  timeStartCheckOut: string;
  timeStartCheckOutCaseOffAfternoon: string;
}

export class ApproveTimesheetNotifyConfigDto {
  approveTimesheetNotifyEnableWorker: string;
  approveTimesheetNotifyAtHour: string;
  approveTimesheetNotifyOnDates: string;
  approveTimesheetNotifyToChannels: string;
  approveTimesheetNotifyTimePeriodWithPendingRequest: string;
}

export class ApproveRequestOffNotifyConfigDto {
  approveRequestOffNotifyEnableWorker: string;
  approveRequestOffNotifyAtHour: string;
  approveRequestOffSendUserAtHour: string;
  approveRequestOffNotifyToChannels: string;
  approveRequestOffNotifyTimePeriodWithPendingRequest: string;
}

export type SendMessageRequestPendingTeamBuildingToHRConfigDto = {
  sendMessageRequestPendingTeamBuildingToHREnableWorker?: string;
  sendMessageRequestPendingTeamBuildingToHRAtHour?: string;
  sendMessageRequestPendingTeamBuildingToHRToChannels?: string;
  sendMessageRequestPendingTeamBuildingToHREmail?: string;
}

export type NotifyHRTheEmployeeMayHaveLeftConfigDto = {
  notifyHRTheEmployeeMayHaveLeftEnableWorker?: string;
  notifyHRTheEmployeeMayHaveLeftAtHour?: string;
  notifyHRTheEmployeeMayHaveLeftToChannels?: string;
  notifyHRTheEmployeeMayHaveLeftToHREmail?: string;
  notifyHRTheEmployeeMayHaveLeftTimePeriod?: string;
}
export type MoneyPMUnlockTimeSheetConfigDto = {
  moneyPMUnlockTimeSheet?: string;
}

export type SendMessageToPunishUserConfigDto = {
  sendMessageToPunishUserEnableWorker?: string;
  sendMessageToPunishUserAtHour?: string;
}

export class CreateNewRetroConfigDto {
  createNewRetroEnableWorker: string;
  createNewRetroAtHour: string;
  createNewRetroOnDate: string;
}

export class GenerateRetroResultConfigDto {
  generateRetroResultEnableWorker: string;
  generateRetroResultAtHour: string;
  generateRetroResultOnDate: string;
}

export class ResetDataTeamBuildingConfigDto {
  resetDataTeamBuildingEnableWorker: string;
  resetDataTeamBuildingAtHour: string;
  resetDataTeamBuildingOnDateAndMonth: string;
}