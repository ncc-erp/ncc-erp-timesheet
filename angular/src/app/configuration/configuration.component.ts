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
  VIEW_LOG_TIMESHEET_IN_FUTURE = PERMISSIONS_CONSTANT.ViewLogTimesheetInFutureSetting;
  VIEW_AUTO_SUBMIT_TIMESHEET = PERMISSIONS_CONSTANT.ViewAutoSubmitTimesheetSetting;
  EDIT_EMAIL_SETTING = PERMISSIONS_CONSTANT.EditEmailSetting;
  EDIT_WORKING_TIME_SETTING = PERMISSIONS_CONSTANT.EditWorkingTimeSetting;
  EDIT_GOOGLE_SETTING = PERMISSIONS_CONSTANT.EditGoogleSingleSignOnSetting;
  EDIT_AUTO_LOCK_TIMESHEET_SETTING = PERMISSIONS_CONSTANT.EditAutoLockTimesheetSetting;
  EDIT_SERCURITY_CODE_SETTING = PERMISSIONS_CONSTANT.EditSercurityCodeSetting;
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
  VIEW_WFH_SETTING=PERMISSIONS_CONSTANT.ViewWFHSetting;
  EDIT_WFH_SETTING= PERMISSIONS_CONSTANT.EditWFHSetting;
  VIEW_KOMU_CONFIG=PERMISSIONS_CONSTANT.ViewKomuConfig
  UPDATE_KOMU_CONFIG= PERMISSIONS_CONSTANT.UpdateKomuConfig
  VIEW_SPECIAL_PROJECT_TASK_CONFIG=PERMISSIONS_CONSTANT.ViewSpecialProjectTaskSetting
  EDIT_SPECIAL_PROJECT_TASK_CONFIG= PERMISSIONS_CONSTANT.EditSpecialProjectTaskSetting
  VIEW_NOTIFICATION_SETTING= PERMISSIONS_CONSTANT.ViewNotificationSetting
  EDIT_NOTIFICATION_SETTING= PERMISSIONS_CONSTANT.EditNotificationSetting
  VIEW_EMAIL_SAO_DO_SETTING= PERMISSIONS_CONSTANT.ViewEmailSaoDo
  EDIT_EMAIL_SAO_DO_SETTING= PERMISSIONS_CONSTANT.EditEmailSaoDo
  VIEW_CHECKIN_SETTING= PERMISSIONS_CONSTANT.ViewCheckInSetting
  EDIT_CHECKIN_SETTNG = PERMISSIONS_CONSTANT. UpdateCheckInSetting
  VIEW_NRIT_CONFIG = PERMISSIONS_CONSTANT.ViewNRITSetting;
  EDIT_NRIT_CONFIG = PERMISSIONS_CONSTANT.EditNRITSetting;
  VIEW_UNLOCK_TIMESHEET_SETTING = PERMISSIONS_CONSTANT.ViewUnlockTimesheetSetting;
  UPDATE_UNLOCK_TIMESHEET_SETTING = PERMISSIONS_CONSTANT.UpdateUnlockTimesheetSetting;
  VIEW_PUNISHCHECKIN_CONFIG = PERMISSIONS_CONSTANT.ViewSendKomuPunishedCheckIn;
  UPDATE_PUNISHCHECKIN_CONFIG = PERMISSIONS_CONSTANT.UpdateSendKomuPunishedCheckIn;
  VIEW_RETRONOTIFY_CONFIG = PERMISSIONS_CONSTANT.ViewRetroNotifySetting;
  EDIT_RETRONOTIFY_CONFIG = PERMISSIONS_CONSTANT.EditRetroNotifySetting;
  VIEW_TEAMBUILDING_CONFIG = PERMISSIONS_CONSTANT.ViewTeamBuildingSetting;
  EDIT_TEAMBUILDING_CONFIG = PERMISSIONS_CONSTANT.EditTeamBuildingSetting;
  VIEW_APPROVETIMESHEETNOTIFY_CONFIG = PERMISSIONS_CONSTANT.ViewApproveTimesheetNotifySetting;
  EDIT_APPROVETIMESHEETNOTIFY_CONFIG = PERMISSIONS_CONSTANT.EditApproveTimesheetNotifySetting;
  VIEW_APPROVEREQUESTOFFNOTIFY_CONFIG = PERMISSIONS_CONSTANT.ViewApproveRequestOffNotifySetting;
  EDIT_APPROVEREQUESTOFFNOTIFY_CONFIG = PERMISSIONS_CONSTANT.EditApproveRequestOffNotifySetting;

  VIEW_TIMECANLATEANDEARLY_CONFIG = PERMISSIONS_CONSTANT.ViewTimesCanLateAndEarlyInMonthSetting;
  EDIT_TIMECANLATEANDEARLY_CONFIG = PERMISSIONS_CONSTANT.EditTimesCanLateAndEarlyInMonthSetting;

  VIEW_TIMESTARTCHANGINGCHECKINTOCHECKOUT_SETTING=PERMISSIONS_CONSTANT.ViewTimeStartChangingCheckInToCheckoutSetting;
  EDIT_TIMESTARTCHANGINGCHECKINTOCHECKOUT_SETTING=PERMISSIONS_CONSTANT.EditTimeStartChangingCheckInToCheckoutSetting;
  VIEW_TIMESTARTCHANGINGCHECKINTOCHECKOUTCASEOFFAFTERNOON_SETTING=PERMISSIONS_CONSTANT.ViewTimeStartChangingCheckInToCheckoutCaseOffAfternoonSetting;

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
  isEditWFHSetting: boolean = false;
  isEditUnlockSetting: boolean = false;
  isLevelSetting: boolean = false;
  isPunishByRule: boolean = false;
  isEditLogTimesheetInFuture: boolean = false;
  isEditAutoSubmitTimesheet: boolean = false;
  isEditGetDataFromFaceID: boolean = false;
  isEditingKomu: boolean = false;
  isEditHRMConfig: boolean = false;
  isEditNotificationSetting: boolean = false;
  isEditNotifyPunishCheckIn: boolean = false;
  isEditTimesCanLateAndEarlyInMonthSetting: boolean = false;
  logTimesheetInFuture = {} as LogTimesheetInFutureDTO;
  punishByRule = {} as PunishByRuleDTO;
  lockTimesheet = {} as LockTimesheetDTO;
  sercurityCode = {} as SercurityCodeDTO;
  wfhSetting = {} as WFHSettingDTO;
  levelSetting = {} as LevelSettingDTO;
  getCheckInCheckOutPunishmentSetting = {} as GetCheckInCheckOutPunishmentSettingDto
  workingTime = {} as WorkingTimeDTO;
  autoSubmitTimesheet = {} as AutoSubmitTimesheetDto;
  getDataFaceID = {} as GetDataFromFaceIDDto;
  HRMConfig = {} as HRMConfigDto;
  notificationSetting = {} as NotificationSettingDto;
  ProjectConfig = {} as ProjectConfigDto;
  emailSaoDoSetting = {} as EmailSaoDoDto;
  komuSetting = {} as KomuDto;
  punishedCheckInSetting = {} as KomuPunishCheckInDto;
  specialProjectTask = {} as SpecialProjectTaskSettingDTO;
  isEditEmailSaodo: boolean = false;
  public isLoading: boolean = false;
  public isEditProjectSetting: boolean = false;
  public isEditSpecialProjectTaskSetting: boolean = false;
  isShowEmailSetting: boolean = false;
  isShowKomuSetting: boolean = false;
  isShowWorkingTime: boolean = false;
  isShowGoogleSetting: boolean = false;
  isShowAutoLogTimesheet: boolean = false;
  isShowSecurityCodeSetting: boolean =false;
  isShowLevelSetting: boolean = false;
  isShowPunishByRule: boolean = false;
  isShowLogTSInFuture: boolean = false;
  isShowAutoSubmitTS: boolean= false;
  isShowFaceIDSetting: boolean = false;
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
  unlockSetting = {} as UnlockTimesheetConfigDto;
  timesCanLateAndEarlyInMonthSetting = {} as TimesCanLateAndEarlyInMonthSettingDto;
  percentOfTrackerOnWorking: string = "";

  isShowRetroNotifySetting: boolean = false;
  isEditRetroNotifyConfig: boolean = false;
  RetroNotifyConfig = {} as RetroNotifyConfigDto;

  isShowTeamBuildingSetting: boolean = false;
  isEditTeamBuildingConfig: boolean = false;
  TeamBuildingConfig = {} as TeamBuildingConfigDto;

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

  constructor(
    private logTimesheetService: LogTimesheetInFutureSettingService,
    private configurationService: ConfigurationService,
    private emailsettingservice: EmailSettingService,
    private singleSignOnService: SingleSignOnService,
    private levelSettingService: LevelSettingService,
    private autoLockTimeSheetService: AutoLockTimesheetService,
    private sercurityCodeService: SercurityCodeService,
    private wfhService: WfhSettingService,
    private autoSubmitService: AutoSubmitTimesheetSettingService,
    private getDataFromFaceIdService: GetDataFromFaceIdSettingService,
    private emailSaoDoSerivice: EmailSaoDoSettingService,
    private specialProjectTaskService: SpecialProjectTaskSettingService,
    private sendKomuPunishedCheckInService: SendKomuPunishedCheckInService,
    private punishByRulesService: CheckInCheckOutPunishmentSettingService,
    private timekeepingService: TimekeepingService,
    private timesCanLateAndEarlyInMonthSettingService :TimesCanLateAndEarlyInMonthSettingService,
    private timeStartChangingCheckinToCheckoutSettingService:TimeStartChangingCheckinToCheckoutSettingService,
    private dialog : MatDialog,
    injector: Injector) {
    super(injector);
  }
  ngOnInit() {
    this.list();
    this.get();
    this.getWorkingTime();
    this.getLockTimesheet();
    this.getSercurityCode();
    this.getLevelSetting();
    this.getLogTimesheetInFuture();
    this.getAutoSubmitTimesheet();
    this.getDataFromFaceID();
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

    this.getSendMessageToPunishUserConfig();
  }
  protected list(): void {
    if (this.permission.isGranted(this.VIEW_EMAIL_SETTING)) {
      this.emailsettingservice.getMail().subscribe((result: any) => {
        this.emailsetting = result.result;
      });
    }
  }

  onChangeUseDefaultCredentials(value){
    this.emailsetting.useDefaultCredentials = value.toString();
  }

  checkConnectToProject(){
    this.projectConnectResult = {} as GetConnectResultDto;
    this.configurationService.checkConnectToProject().subscribe((data) => {
      this.projectConnectResult = data.result;
    })
  }

  checkConnectToHRM(){
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
      width: "500px"
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
  editGetDataFromFaceID() {
    this.isEditGetDataFromFaceID = true;
  }

  SaveSercurityCode() {
    this.sercurityCodeService.change(this.sercurityCode).subscribe((res: any) => {
      this.isEditSercurityCode = !this.editSercurityCode;
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
    this.timesCanLateAndEarlyInMonthSettingService.setTimesCanLateAndEarlyInMonthSetting(this.timesCanLateAndEarlyInMonthSetting).subscribe((res:any) => {
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

  checkShowpass() {
    this.isShowPassword = !this.isShowPassword;
  }

  refreshSercurityCode() {
    this.getSercurityCode();
    this.isEditSercurityCode = false;
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
  editKomu() : any{
    this.isEditingKomu = true;
  }
  saveKomu() : any{
    this.configurationService.SetKomuConfig(this.komuSetting).subscribe((res: any) => {
      this.isEditingKomu = !this.isEditingKomu;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })

  }
  refreshKomuSetting(): any{
    this.getKomu();
    this.isEditingKomu = false;
  }

  saveChannelSendPunishCheckIn(): any{
    this.sendKomuPunishedCheckInService.changePunishedCheckInConfig(this.punishedCheckInSetting).subscribe((res: any) => {
      this.isEditNotifyPunishCheckIn = false;
      if(res){
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }

  getChannelSendPunishCheckIn(): any{
    if(this.permission.isGranted(this.VIEW_PUNISHCHECKIN_CONFIG)){
      this.sendKomuPunishedCheckInService.getPunishedCheckInConfig().subscribe((result: any)=>{
        this.punishedCheckInSetting = result.result;
      })
    }
  }

  editChannelSendPunishCheckIn(): any{
    this.isEditNotifyPunishCheckIn = true;
    this.getChannelSendPunishCheckIn();
  }

  refreshChannelSendPunishCheckIn(): any{
    this.getChannelSendPunishCheckIn();
    this.isEditNotifyPunishCheckIn = false;
  }


  public maskTime = [/[\d]/, /\d/, ':', /\d/, /\d/]

  // Project Setting
  refreshProjectConfig(){
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
  editProjectConfig(){
    this.isEditProjectSetting = true;
  }
  SaveProjectConfig(){
    this.configurationService.SetProjectConfig(this.ProjectConfig).subscribe(()=>{
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
    if (_.isEmpty(this.NRITConfig.notifyAtHour)) {
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
    this.configurationService.SetNRITConfig(this.NRITConfig).subscribe((res:any) => {
      this.isEditNRITConfig = !this.isEditNRITConfig;
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
    if (_.isEmpty(this.TeamBuildingConfig.billPercentage)) {
      abp.message.error("Bill percentage required!")
      return;
    }
    this.configurationService.SetTeamBuildingConfig(this.TeamBuildingConfig).subscribe((res:any) => {
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
    this.configurationService.setRetroNotifyConfig(this.RetroNotifyConfig).subscribe((res:any) => {
      this.isEditRetroNotifyConfig = !this.isEditRetroNotifyConfig;
      if (res) {
        this.notify.success(this.l('Update Successfully!'));
      }
    })
  }
   //TimeStartChangingCheckinToCheckout setting
  refreshTimeStartChangingCheckinToCheckoutSetting() {
    this.getTimeStartChangingCheckinToCheckoutSetting();
    this.isEditTimeStartChangingCheckinToCheckoutSetting=false;
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
    this.configurationService.setApproveTimesheetNotifyConfig(this.ApproveTimesheetNotifyConfig).subscribe((res:any) => {
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
    if (_.isEmpty(this.ApproveRequestOffNotifyConfig.approveRequestOffNotifyTimePeriodWithPendingRequest)) {
      abp.message.error("Time period with pending request required!")
      return;
    }
    this.configurationService.setApproveRequestOffNotifyConfig(this.ApproveRequestOffNotifyConfig).subscribe((res:any) => {
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
    this.configurationService.setSendMessageRequestPendingTeamBuildingToHRConfig(this.sendMessageRequestPendingTeamBuildingToHRConfig).subscribe((res:any) => {
      this.isEditSendMessageRequestPendingTeamBuildingToHRConfig = !this.isEditSendMessageRequestPendingTeamBuildingToHRConfig;
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
    this.configurationService.setConfigNotifyHRTheEmployeeMayHaveLeft(this.notifyHRTheEmployeeMayHaveLeftConfig).subscribe((res:any) => {
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

    this.configurationService.setConfigMoneyPMUnlockTimeSheet(this.moneyPMUnlockTimeSheetConfig).subscribe((res:any) => {
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
      this.configurationService.setConfigSendMessageToPunishUser(this.sendMessageToPunishUserConfig).subscribe((res:any) => {
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

export class WFHSettingDTO {
  numOfRemoteDays: string;
  allowInternToWorkRemote: string;
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

export class HRMConfigDto {
  hrmUri: string;
  secretCode: string;
}

export class NRITConfigDto {
  notifyEnableWorker: string;
  notifyAtHour: string;
  notifyReviewDeadline: string;
  notifyOnDates: string;
  notifyToChannels: string;
  notifyPenaltyFee: string;
}

export class ProjectConfigDto {
  projectUri: string;
  secretCode: string;
}

export class EmailSaoDoDto {
  emailSaoDo: string;
  canSendEmailToSaoDo: string;
}

export class KomuDto{
  komuUri: string;
  komuSecretCode: string;
  komuChannelIdDevMode: string;
  komuUserNameDevMode: string;
}

export class NotificationSettingDto{
  sendEmailTimesheet: string;
  sendEmailRequest: string;
  sendKomuSubmitTimesheet: string;
  sendKomuRequest: string;
}

export class UnlockTimesheetConfigDto{
  weeksCanUnlockBefor: string;
}

export class KomuPunishCheckInDto{
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

export class InputToUpdateSettingDto{
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
  billPercentage: string;
}

export class TimesCanLateAndEarlyInMonthSettingDto {
  timesCanLateAndEarlyInMonth: string;
  timesCanLateAndEarlyInWeek: string;
}

export class TimeStartChangingToCheckoutSettingDto{
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
