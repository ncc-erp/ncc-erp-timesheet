import { CheckInCheckOutPunishmentSettingService } from './../../../../service/api/punish-by-rule.service';
import { status } from './../../../retros/retro/retro.component';
import { ComplainReplyComponent } from './../complain-reply/complain-reply.component';
import { MatDialog } from '@angular/material';
import { DatePipe } from '@angular/common';
import { Component, Injector, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { APP_CONSTANT } from '@app/constant/api.constants';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { userDTO } from '@app/modules/user/user.component';
import { TimekeepingDto, UpdateTimekeepingDto } from '@app/service/api/model/report-timesheet-Dto';
import { TimekeepingService } from '@app/service/api/timekeeping.service';
import { UserService } from '@app/service/api/user.service';
import { AppComponentBase } from '@shared/app-component-base';
import { AppConsts } from '@shared/AppConsts';
import { CalendarView } from 'angular-calendar';
import * as moment from 'moment';
import { BranchDto } from '@shared/service-proxies/service-proxies';
import { BranchService } from '@app/service/api/branch.service';
import { isThisSecond } from 'date-fns';

@Component({
  selector: 'app-tardiness-detail',
  templateUrl: './tardiness-detail.component.html',
  styleUrls: ['./tardiness-detail.component.css'],
  providers: [DatePipe]
})
export class TardinessDetailComponent extends AppComponentBase implements OnInit {
  EDIT_TARDINESS_LEAVE_EARLY = PERMISSIONS_CONSTANT.EditTardinessLeaveEarly;
  VIEW_TARDINESS_LEAVE_EARLY = PERMISSIONS_CONSTANT.ViewTardinessLeaveEarly;
  Timekeeping_ReplyUserNote= PERMISSIONS_CONSTANT.Timekeeping_ReplyUserNote;

  // listMonth = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
  // listYear = APP_CONSTANT.ListYear;
  title;
  month;
  months;
  year;
  years;
  branch;
  view: CalendarView;
  viewDate: Date;
  calendarView;
  userSearch: FormControl = new FormControl();
  userControl: FormControl;
  listUserBase = [];
  listUserFiltered: userDTO[];
  isLoadingFileUpload: boolean;
  activeDayIsOpen: boolean = false;
  isFileSupported: boolean;
  fileSCROM: File;
  listTimekeeping: TimekeepingDto[] = [];
  userId: number;
  userName: string;
  isTableLoading: boolean = false;
  selectedDay: number = -1;
  dayList: any = []
  public isComplain : any = true;
  public isPunish : any = -1;
  public selectedBranch = 0;
  public selectedStatus : number = -1;
  public branchList = Object.keys(this.APP_CONSTANT.BRANCH);
  public page:number =1;
  public itemPerPage: number = 50;
  listBranch: BranchDto[] = [];
  branchSearch: FormControl = new FormControl("")
  listBranchFilter : BranchDto[];

  public currentSortColumn: string = "transactionDate";
  public sortDirection: number = 0;
  public iconSort: string = "";
  public originalList: TimekeepingDto[] = [];
  constructor(
    private activatedRoute: ActivatedRoute,
    private userService: UserService,
    private datePipe: DatePipe,
    private timekeepingService: TimekeepingService,
    private branchService: BranchService,
    injector: Injector,
    private router:Router,
    private dialog: MatDialog,
    private punishByRulesService : CheckInCheckOutPunishmentSettingService,
  ) {
    super(injector);
    this.view = CalendarView.Month;
    this.viewDate = new Date();
    this.viewDate.setMonth(new Date().getMonth());
    this.calendarView = CalendarView;
    this.months = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11];
    this.years = this.APP_CONSTANT.ListYear;
    this.userId = Number.parseInt(this.activatedRoute.snapshot.queryParamMap.get("id"));
    this.userControl = new FormControl(this.userId);
    this.updateDay();
    this.userSearch.valueChanges.subscribe(() => {
      this.searchUser();
    });
    if (!this.permission.isGranted(this.VIEW_TARDINESS_LEAVE_EARLY)) {
      this.userName = this.appSession.user.surname + ' ' + this.appSession.user.name;
    }
  }

  ngOnInit() {
    this.setDataDefaul();
    this.getData();
    this.getUser();
    this.onUserChange();
    this.getListBranch();
  }
  setDataDefaul()
  {
    this.month = Number.parseInt(this.activatedRoute.snapshot.queryParamMap.get("month")) - 1;
    this.month = this.month ? this.month : this.viewDate.getMonth();
    this.year = this.viewDate.getFullYear();
  }
  getListBranch() {
    this.branchService.getAllBranchFilter(false).subscribe(res => {
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
  getData() {
    this.isTableLoading = true;
    this.timekeepingService.getDetailTimekeeping(this.year, this.month + 1, this.selectedDay, this.userId, this.selectedBranch, this.isPunish, this.isComplain, this.selectedStatus).subscribe(res => {
      this.listTimekeeping = res.result;
      this.listTimekeeping = res.result.map(item => {
        item.noteReplyToString = !item.noteReply ? "" : item.noteReply;

        item.moneyPunishtmp = !item.moneyPunish ? 0 : item.moneyPunish;
        return item

      })
      this.originalList = res.result;
      // this.listTimekeeping.forEach(item1 => {
      //   res.result.forEach(item2 => {
      //     if (item1.timekeepingId == item2.timekeepingId && !item1.isEditing) {
      //       item1.registrationTimeStart = item2.registrationTimeStart;
      //       item1.registrationTimeEnd = item2.registrationTimeEnd;
      //       item1.checkIn = item2.checkIn;
      //       item1.checkOut = item2.checkOut;
      //       item1.resultCheckIn = item2.resultCheckIn;
      //       item1.resultCheckOut = item2.resultCheckOut;
      //       item1.editByUserName = item2.editByUserName;
      //       item1.status = item2.status;
      //     }
      //   })
      // })
      this.isTableLoading = false;
    });
  }
  onUserChange() {
    if (this.isComplain != true) {
      // const yesterday = moment().add(-1, 'd');
      // this.selectedDay = yesterday.date();
      // this.month = yesterday.month();
      // this.year = yesterday.year();

      // this.userName = "All"
    }
    // if(this.userId == -1 && this.selectedDay == -1){
    //   this.selectedDay = moment().add(-1, 'd').date()
    // }
    this.viewDetail(this.userId);
    this.getCurrentUserName()
    this.getData()
  }

  onComplainChange() : void{
    // this.isComplain = !this.isComplain;
    // if(this.isComplain!=true && this.userId ==-1 && this.selectedDay == -1){
    //   this.onUserChange();
    // }else if(this.isComplain==true){
    //   this.selectedDay = -1;
    //   this.userId = -1;
    //   this.getData()
    // }else{
    //   this.getData()
    // }
    this.getData()

  }

  getUser() {
    if(this.isComplain!=true && this.userId ==-1 && this.selectedDay == -1){
      this.onUserChange();
    }
    if (this.permission.isGranted(this.VIEW_TARDINESS_LEAVE_EARLY)) {
      this.userService.getAllNotPagging().subscribe(rs => {
        this.listUserBase = rs.result;
        this.listUserFiltered = this.listUserBase;
        this.getCurrentUserName()
      });
    }

  }
  getCurrentUserName() {
    const user = this.listUserBase.find(s => s.id == this.userId);
    if (user) this.userName = user.name;
  }
  updateDay(): void {
    this.month = this.viewDate.getMonth();
    this.year = this.viewDate.getFullYear();
    this.getDayByMonthAndYear(this.month, this.year)
  }
  getDayByMonthAndYear(month: number, year: number) {
    let numOfday: number = new Date(year, month + 1, 0).getDate();
    this.dayList = []
    for (let i = 1; i <= numOfday; i++) {
      this.dayList.push(i)
    }
  }

  formatDate(date: string) {
    return new Date(date).toLocaleDateString("vi");
  }

  getCss1(value) {
    value = Number.parseInt(value);
    if (value > 15) {
      return "red";
    }
    return "green";
  }

  searchUser(): void {
    var temp = this.userSearch.value.trim().toLowerCase();
    this.listUserFiltered = this.listUserBase.filter(data =>
      data.emailAddress.toLowerCase().includes(temp)
    );
  }


  onBack() {
    this.viewParent();
    //history.back();
  }

  onDateChange() {
    if (this.selectedDay == -1 && this.isComplain!=true && this.userId ==-1 && this.permission.isGranted(this.VIEW_TARDINESS_LEAVE_EARLY)) {
      // if(this.listUserBase[0].id){
      //   this.userId = this.listUserBase[0].id
      // }
    }
    this.viewDate = new Date(this.year, this.month, this.selectedDay);
    this.getDayByMonthAndYear(this.month, this.year)
    this.getData();
    this.getCurrentUserName()
    this.viewDetail(this.userId);
  }
  closeOpenMonthViewDay() {
    this.activeDayIsOpen = false;
    this.ngOnInit();
  }

  onDblClick(item: TimekeepingDto) {
    if (!this.permission.isGranted(this.EDIT_TARDINESS_LEAVE_EARLY)) return;
    item.isEditing = true;
  }

  onSave(item: TimekeepingDto) {
    var data = new UpdateTimekeepingDto();
    data.id = item.timekeepingId;
    data.date = item.date;
    data.checkIn = item.checkIn;
    data.checkOut = item.checkOut;
    data.registerCheckIn = item.registrationTimeStart;
    data.registerCheckOut = item.registrationTimeEnd;
    data.userCode = item.userCode;
    data.userEmail = item.userEmail;
    data.userId = item.userId;
    data.userName = item.userName;
    data.statusPunish = item.statusPunish;
    data.trackerTime = item.trackerTime;

    this.timekeepingService.updateTimekeeping(data).subscribe(res => {
      if (res.success) {
        abp.notify.success("Update successfully");
        this.getData();
        item.isEditing = false;
      }
    })
  }

  onCancel(item: TimekeepingDto) {
    item.isEditing = false;
    this.getData();
  }
  replyComplain(complain) {
    let dialogRef = this.dialog.open(ComplainReplyComponent, {
      data: complain,
      width: "700px"
    },
    )
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.getData()
      }
    })
  }
  viewDetail(userId:number){
    this.router.navigate(["/app/main/tardiness-leave-early-detail"], {
      queryParams: {
        id: userId,
        month: this.month + 1
      }
    })
  }
  viewParent(){
    this.router.navigate(["/app/main/tardiness-leave-early"], {
      queryParams: {
        month: this.month + 1
      }
    })
  }

  trackerTimeFormat(time) {
    if (time == ""|| time == null) {
      return "";
    }
    if (time == 0) {
      return 0;
    }
    const [hours, minutes] = time.split(":").slice(0, 2);
    const formattedHours = hours.length === 1 ? `0${hours}` : hours;
    const formattedMinutes = minutes.length === 1 ? `0${minutes}` : minutes;
    return `${formattedHours}:${formattedMinutes}`;
  }
  public maskTime = [/[\d]/, /\d/, ':', /\d/, /\d/]

  //Hàm Sort theo các trường
  handleSortByColumn(columnName) {
    if (this.currentSortColumn !== columnName) {
      this.sortDirection = -1;
    }
    this.currentSortColumn = columnName;
    this.sortDirection++;
    if (this.sortDirection > 1) {
      this.iconSort = "";
      this.sortDirection = -1;
    }

    if (this.sortDirection == 0 || this.sortDirection == 1) {
      this.iconSort =
        this.sortDirection == 1
          ? "fas fa-sort-amount-down"
          : "fas fa-sort-amount-up";

      switch (columnName) {
        case "userName":
          this.listTimekeeping.sort((a, b) => {
            return this.sortDirection == 1
              ? b.userName.localeCompare(a.userName)
              : a.userName.localeCompare(b.userName);
          });
          break;
        case "editByUserName":
          this.listTimekeeping.sort((a, b) => {
            return this.sortDirection == 1
              ? b.editByUserName.localeCompare(a.editByUserName)
              : a.editByUserName.localeCompare(b.editByUserName);
          });
          break;
        case "userNote":
          this.listTimekeeping.sort((a, b) => {
            return this.sortDirection == 1
              ? b.userNote.localeCompare(a.userNote)
              : a.userNote.localeCompare(b.userNote);
          });
          break;
        case "noteReplyToString":
          this.listTimekeeping.sort((a, b) => {
            return this.sortDirection == 1
              ? b.noteReplyToString.localeCompare(a.noteReplyToString)
              : a.noteReplyToString.localeCompare(b.noteReplyToString);
            });
          break;
      }
    } else {
      this.iconSort = "fas fa-sort";
      this.listTimekeeping = [...this.originalList];
    }
  }
}
