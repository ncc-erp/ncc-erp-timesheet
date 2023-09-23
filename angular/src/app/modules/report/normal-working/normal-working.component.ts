import { AfterViewChecked, Component, Injector, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { APP_CONSTANT } from '@app/constant/api.constants';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { BranchService } from '@app/service/api/branch.service';
import { DayOffService } from '@app/service/api/day-off.service';
import { AbsenceDetaiInDay, DateOfMonthDto, OffDayDTO, WorkingHourDto, WorkingReportDTO } from '@app/service/api/model/report-timesheet-Dto';
import { ProjectManagerService } from '@app/service/api/project-manager.service';
import { ReportService } from '@app/service/api/report.service';
import { TimesheetService } from '@app/service/api/timesheet.service';
import { ExportService } from '@app/service/export.service';
import { MonthViewDay } from '@node_modules/calendar-utils';
import { finalize } from '@node_modules/rxjs/operators';
import { AppConsts } from '@shared/AppConsts';
import { PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { BranchDto } from '@shared/service-proxies/service-proxies';
import * as FileSaver from 'file-saver';

@Component({
  selector: 'app-normal-working',
  templateUrl: './normal-working.component.html',
  styleUrls: ['./normal-working.component.scss']
})

export class NormalWorkingComponent extends PagedListingComponentBase<WorkingReportDTO> implements OnInit, AfterViewChecked {
  EXPORT_EXCEL_NORMAL_WORKING = PERMISSIONS_CONSTANT.ExportExcelNormalWorking;
  LOCK_UNLOCK_TIMESHEET = PERMISSIONS_CONSTANT.LockUnlockTimesheet;

  month: string = ((new Date()).getMonth() + 1).toString();
  year: number = (new Date()).getFullYear();

  weekdays = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

  workingReports = [] as WorkingReportDTO[];
  sendMail = true;
  currentDay = new Date();
  datesOfMonth = [] as DateOfMonthDto[];

  years = APP_CONSTANT.ListYear;

  scrollingValue = 480;

  scrollbarPosition = 0;

  userType = -1;

  branchId;
  show = false;

  selectedDays: Map<string, number>;
  monthViewBody: MonthViewDay[];

  isLockForPM = true;

  listBranch: BranchDto[] = [];
  branchSearch: FormControl = new FormControl("")
  listBranchFilter: BranchDto[];
  isThanDefaultWorking: boolean = false;
  isNoOffAndNoCheckIn: boolean = false;
  disabledCheckbox: boolean = false;
  public userTypes = [
    { value: 0, label: 'Staff' },
    { value: 1, label: 'Intern' },
    { value: 2, label: 'CTV' }
  ];

  projectFilter = []
  projectId = APP_CONSTANT.FILTER_DEFAULT.All;
  projectSearch: FormControl = new FormControl("")
  projects = []

  OFF = this.APP_CONSTANT.DayAbsenceType.Off;
  REMOTE = this.APP_CONSTANT.DayAbsenceType.Remote;
  ONSITE = this.APP_CONSTANT.DayAbsenceType.Onsite;
  FULLDAY = this.APP_CONSTANT.AbsenceType.FullDay;
  MORMING = this.APP_CONSTANT.AbsenceType.Morning;
  AFTERNOON = this.APP_CONSTANT.AbsenceType.Afternoon;
  CUSTOM = this.APP_CONSTANT.AbsenceType.Custom;
  DI_MUON = this.APP_CONSTANT.OnDayType.BeginOfDay;
  VE_SOM = this.APP_CONSTANT.OnDayType.EndOfDay;

  tsStatusFilter = APP_CONSTANT.TsStatusFilter['Approved'];
  tsStatusFilterList = Object.keys(this.APP_CONSTANT.TsStatusFilter)
  checkInFilter = APP_CONSTANT.FILTER_DEFAULT['All'];
  checkInFilterList = Object.keys(this.APP_CONSTANT.CheckInFilter)
  isDisabled: boolean = false;
  constructor(
    injector: Injector,
    private timesheetService: TimesheetService,
    private reportService: ReportService,
    private dayOffService: DayOffService,
    private excelService: ExportService,
    private branchService: BranchService,
    private projectManageService: ProjectManagerService,
  ) {
    super(injector);
    this.selectedDays = new Map<string, number>();

    this.branchId = 0;
    this.branchSearch.valueChanges.subscribe(() => {
      this.filterBranch();
    });
    this.projectSearch.valueChanges.subscribe(() => {
      this.filterProject();
    });
  }

  ngOnInit() {
    this.getData();
    this.getListBranch();
    this.getProjects();
  }

  getProjects() {
    this.projectManageService.getProjectFilter().subscribe(data => {
      this.projectFilter = data.result
      this.projects = this.projectFilter
      this.projects.unshift({
        id: -1,
        name: "All"
      })
    })
  }
  filterProject(): void {
    if (this.projectSearch.value) {
      const temp: string = this.projectSearch.value.toLowerCase().trim();
      this.projects = this.projectFilter.filter(data => data.name.toLowerCase().includes(temp));
    } else {
      this.projects = this.projectFilter.slice();
    }
  }

  getListBranch() {
    this.branchService.getAllBranchFilter(true).subscribe(res => {
      this.listBranch = res.result;
      this.listBranchFilter = this.listBranch;
    });
  }

  filterBranch(): void {
    if (this.branchSearch.value) {
      this.listBranch = this.listBranchFilter.filter(data => data.displayName.toLowerCase().includes(this.branchSearch.value.toLowerCase().trim()));
    } else {
      this.listBranch = this.listBranchFilter.slice();
    }
  }

  onChangeThanDefaultWorking(event) {
    this.disabledCheckbox = true;
    this.isThanDefaultWorking = event.checked;
    this.pageNumber = 1;
    this.refresh();
  }

  onChangeNoOffAndNoCheckIn(event) {
    this.disabledCheckbox = true;
    this.isNoOffAndNoCheckIn = event.checked;
    this.pageNumber = 1;
    this.refresh();
  }

  getData() {
    this.refresh();
    this.getOffDays();
  }

  protected list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
    request.filterItems = [];

    if (this.userType !== -1) {
      request.filterItems.push({
        comparison: 0,
        propertyName: 'type',
        value: this.userType,
      });
    }
    this.reportService.getAllPagging(request, Number(this.year), Number(this.month), Number(this.branchId), Number(this.projectId)
      , this.isThanDefaultWorking, Number(this.checkInFilter), Number(this.tsStatusFilter))
      .pipe(finalize(() => {
        finishedCallback();
      }))
      .subscribe(resp => {
        this.disabledCheckbox = false;
        this.workingReports = resp.result.items;
        this.showPaging(resp.result, pageNumber);
      });
  }

  isShowDay(detail: WorkingHourDto) {
    return detail.dayName !== 'Sunday';
  }

  getCssClass(detail: WorkingHourDto) {
    if ((detail.isThanDefaultWorkingHourPerDay && this.isThanDefaultWorking) ||
      (detail.isOffDaySetting && this.isThanDefaultWorking && detail.workingHour != null)
    ) {
      return 'dayTS-log-than-default-working-time'
    }

    if (detail.isNoCheckIn) {
      return 'dayNoCheckIn';
    }

    return detail.isOffDaySetting ? 'dayOff'
      : !detail.isOnsite && detail.workingHour + detail.offHour != 8 && (detail.workingHour != null || detail.offHour != null) && !detail.isOpenTalk ? 'dayWarning'
        : detail.isLock ? 'dayLocked'
          : 'defaultDay'
  }



  getCssClass2(absenceDetaiInDay: AbsenceDetaiInDay) {
    if (!absenceDetaiInDay) return null;
    let resultClass = '';
    let cssClass = ' day-chip-';

    switch (absenceDetaiInDay.type) {
      case this.ONSITE:
        resultClass = 'onsite' + cssClass;
        break;
      case this.REMOTE:
        resultClass = 'remote' + cssClass;
        break;
      default:
        resultClass = 'absence' + cssClass;
    }
    if (absenceDetaiInDay.absenceType === this.FULLDAY) {
      resultClass += 'full-day';
    } else if (absenceDetaiInDay.absenceType === this.MORMING) {
      resultClass += 'morning';
    } else if (absenceDetaiInDay.absenceType === this.AFTERNOON) {
      resultClass += 'afternoon';
    } else if (absenceDetaiInDay.absenceType === this.CUSTOM) {
      resultClass += 'custom';
    }
    return resultClass;
  }

  showContentCell(absenceDetaiInDay: AbsenceDetaiInDay) {
    if (!absenceDetaiInDay) return null;
    let result = 'O-';
    switch (absenceDetaiInDay.type) {
      case this.ONSITE:
        result = 'OS-';
        break;
      case this.REMOTE:
        result = 'R-';
        break;
      default:
        result = 'O-';
    }

    if (absenceDetaiInDay.absenceType === this.FULLDAY) {
      return result += 'FD';
    }
    if (absenceDetaiInDay.absenceType === this.MORMING) {
      return result += 'M';
    }
    if (absenceDetaiInDay.absenceType === this.AFTERNOON) {
      return result += 'A';
    }
    if (absenceDetaiInDay.absenceType === this.CUSTOM &&
      absenceDetaiInDay.absenceTime === this.DI_MUON) {
      return "ĐM " + absenceDetaiInDay.hour;
    }
    return "VS " + absenceDetaiInDay.hour;
  }

  changeTime() {
    this.pageNumber = 1;
    this.getData();
  }
  sendMails() {
    this.sendMail = false;
  }

  getOffDays() {
    this.dayOffService.getAll(this.month, this.year, this.branchId).subscribe(resp => {
      const offDays: OffDayDTO[] = resp.result as OffDayDTO[];
      const offDays2NumberArray: number[] = [...offDays.map(x => (new Date(x.dayOff)).getDate())];

      this.datesOfMonth = [];
      const date = new Date(Number(this.year), Number(this.month) - 1, 1);
      while (Number(date.getMonth() + 1) === Number(this.month)) {
        this.datesOfMonth.push({
          date: date.getDate(),
          day: this.weekdays[date.getDay()],
          isOffDay: offDays2NumberArray.indexOf(date.getDate()) > -1,
        } as DateOfMonthDto);
        date.setDate(date.getDate() + 1);
      }
    });
  }

  ngAfterViewChecked(): void {
    if (this.show) {
      this.viewFull();
      return;
    }
    this.viewNormal();
  }

  exportExcel() {
    this.isDisabled = true;
    var req = new PagedRequestDto();
    req.maxResultCount = this.pageSize;
    req.skipCount = (this.pageNumber - 1) * this.pageSize;
    req.searchText = this.searchText;
    req.filterItems = this.filterItems;

    if (this.userType !== -1) {
      req.filterItems.push({
        comparison: 0,
        propertyName: 'type',
        value: this.userType,
      });
    }
    this.reportService.ExportNormalWorking(req, Number(this.year), Number(this.month), Number(this.branchId), Number(this.projectId)
      , this.isThanDefaultWorking, Number(this.checkInFilter), Number(this.tsStatusFilter)).subscribe(data => {
        const file = new Blob([this.s2ab(atob(data.result))], {
          type: "application/vnd.ms-excel;charset=utf-8"
        });
        const fileName = 'Working Report' + ' Month_' + this.month + ' Year_' + this.year;
        FileSaver.saveAs(file, `${fileName}.xlsx`);
        this.isDisabled = false
      })
  }

  s2ab(s) {
    var buf = new ArrayBuffer(s.length);
    var view = new Uint8Array(buf);
    for (var i = 0; i != s.length; ++i) view[i] = s.charCodeAt(i) & 0xFF;
    return buf;
  }

  nextOrPre(str): void {

    function getMaxChildWidth(elm) {
      const childrenWidth = $.map($('>*', elm), function (el: string) { return $(el).width(); });
      let max = 0;
      for (let i = 0; i < childrenWidth.length; i++) {
        max = childrenWidth[i] > max ? childrenWidth[i] : max;
      }
      return max;
    }

    function getScrollingValue(toLeft, ctx, pos, val) {

      if (toLeft) {
        return pos < 1 ? 0 : val;
      }
      return pos >= getMaxChildWidth(ctx) ? 0 : val;
    }

    if (str === 'left') {
      $('#table-detail').scrollLeft(
        this.scrollbarPosition -= getScrollingValue(true, $('#table-detail'), this.scrollbarPosition, this.scrollingValue)
      );
    } else {
      $('#table-detail').scrollLeft(
        this.scrollbarPosition += getScrollingValue(false, $('#table-detail'), this.scrollbarPosition, this.scrollingValue)
      );
    }
  }

  protected delete(entity: WorkingReportDTO): void {

  }

  onResize() {
    this.show ? this.viewFull() : this.viewNormal();
    this.show = !this.show;
  }

  onUnlock(data: any, type: any) {
    if (!this.permission.isGranted("Report.NormalWorking.LockUnlockTimesheet")) {
      abp.notify.error('Bạn không có quyền thực hiện chức năng này');
      return;
    }
    const req = {
      userId: data.userId,
      type: type
    }
    this.timesheetService.unlockTimesheet(req).subscribe(() => {
      if (type === 0) {
        this.notify.success(`This timesheet now is unlocked, ${data.fullName} can create or submit timesheet.`);
      } else {
        this.notify.success(`Now PM’s right to approve timesheets is unlocked. PM ${data.fullName} can approve/ reject timesheets.`);
      }
      this.refresh();
    });
  }

  onLock(data: any, type: any) {
    if (!this.permission.isGranted("Report.NormalWorking.LockUnlockTimesheet")) {
      abp.notify.error('Bạn không có quyền thực hiện chức năng này');
      return;
    }
    const req = {
      userId: data.userId,
      type: type
    }
    this.timesheetService.lockTimesheet(req).subscribe(() => {
      if (type === 0) {
        this.notify.success(`This timesheet now is locked, ${data.fullName} can’t create or submit timesheet.`);
      } else {
        this.notify.success(`Now PM’s right to approve timesheets is locked. PM ${data.fullName} can’t approve/ reject timesheets. `);
      }
      this.refresh();
    });
  }


  setCss(num) {
    if (this.workingReports) {
      for (let i = 0; i < this.workingReports.length; i++) {
        const id = '#width' + i;
        const idPot = '#widthh' + i;
        $(idPot).css('height', $(id).height() + num + 'px');
      }
    }
  }

  viewNormal() {
    $('#leftsidebar').removeAttr('style');
    $('#secContent').removeAttr('style');
    $('#totalCol').removeAttr('style');
    $('#totalCol').css('height', '61px');
    $('#openTalk').removeAttr('style');
    $('.icon').css('transform', 'scale(1)');
    $('#name').removeAttr('style');
    $('#hours').removeAttr('style');
    $('.tblWidth').removeAttr('style');
    $('.left').removeAttr('style');
    $('.right').removeAttr('style');
    // $('.MWH').css('display','inline-block')
    $('.date-day').css('height', '121px');
    $('.sticky-left:nth-child(2)').removeAttr('style');
    $('.total,.total-column.sticky-left, tbody td:nth-child(3').removeAttr('style');
    $('.total-column.sticky-left:nth-child(2), tbody td:nth-child(4)').removeAttr('style');
    $('.total-column.sticky-left:nth-child(3)').removeAttr('style');
    this.setCss(22);
    $('th').css('padding', '10px');
    $('td').css('padding', '10px');
    $('.half-width').css('width', '20px');
  }

  viewFull() {
    $('#leftsidebar').css('display', 'none');
    $('#secContent').css('margin', '100px 0px 0 0px');
    $('#totalCol').removeAttr('style');
    // $('#totalCol').css('height', '30px');
    $('.total-column').css('width', '30px');
    $('.icon').css('transform', 'scale(0.8)');
    $('.date-day').css('height', '78px');
    // $('#name').css('width', '50px');
    // $('#hours').css('width', '63px');
    // $('.tblWidth').css('width', '528px');
    // $('#1date').css('height', '60px');
    $('.sticky-left:nth-child(2)').css('left', '0');
    $('.total,.total-column.sticky-left, tbody td:nth-child(3').css('left', '0');
    $('.total-column.sticky-left:nth-child(2), tbody td:nth-child(4)').attr('style', 'left: 36px !important');
    $('.total-column.sticky-left:nth-child(3)').attr('style', 'left: 36px !important');
    $('.left').css('display', 'none');
    $('.right').css('display', 'none');

    this.setCss(10);
    $('th').css('padding', '2px');
    $('td').css('padding', '2px');
    $('.half-width').css('width', '33px');
  }
}
