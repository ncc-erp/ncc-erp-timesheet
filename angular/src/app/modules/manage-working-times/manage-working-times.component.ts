import { Component, OnInit, Injector } from "@angular/core";
import { AppComponentBase } from "@shared/app-component-base";
import { AppConsts } from "@shared/AppConsts";

import { ManageWorkingTimeService } from '../../service/api/manage-working-time.service';
import { UserWorkingTime } from '../../service/api/model/working-time.dto';

import { ProjectManagerService } from '../../service/api/project-manager.service';
import { ProjectDto } from '../../service/api/model/project-Dto';
import { PERMISSIONS_CONSTANT } from "@app/constant/permission.constant";
@Component({
  selector: "app-manage-working-times",
  templateUrl: "./manage-working-times.component.html",
  styleUrls: ["./manage-working-times.component.css"],
})
export class ManageWorkingTimesComponent extends AppComponentBase implements OnInit{
  APPROVAL_WORKING_TIME = PERMISSIONS_CONSTANT.ApprovalWorkingTime;
  public isLoading: boolean = false;
  userWorkingTimes: UserWorkingTime[] = [];
  userName = "";
  keyLocalStorageWorkingTime = 'listProjectIdsOfWorkingTimeUser';
  listProjectSelected: Number[] = [];
  listProject: ProjectDto[] = [];
  public userTypes = [
    { value: 0, label: 'Staff' },
    { value: 1, label: 'Intern' },
    { value: 2, label: 'CTV' }
  ];
  status = 1;
  public listStatus = [
    {value: 0, label: "All"},
    {value: 1, label: "Pending", backgroundColor: "#4e9789"},
    {value: 2, label: "Approve", backgroundColor: "green"},
    {value: 3, label: "Reject", backgroundColor: "gray"}
  ]
  constructor(injector: Injector, private workingTimeService: ManageWorkingTimeService,
              private projectService: ProjectManagerService) {
    super(injector);
  }
  ngOnInit() {
    this.getProjectUsers();
  }

  getUserWorkingTimes() {
    this.isLoading = true;
    this.workingTimeService.getAll(this.userName, this.listProjectSelected, this.status).subscribe((resData) => {
      this.userWorkingTimes = resData.result;
      this.isLoading = false
    },() => this.isLoading = false)
  }

  getProjectUsers() {
    this.projectService.getProjectWorkingTimePM().subscribe((resData) => {
      this.listProject = resData.result

      let data = localStorage.getItem(this.keyLocalStorageWorkingTime);
      this.listProject.forEach(item => {
        if (data == null || data == "") {
          this.listProjectSelected.push(item.id);
        }
        if (item.code) {
          item.name = item.name;
        }
      });

      if (data !== null && data !== '') {
        data.split(",").forEach((value: string) => {
          if (this.listProject.some(project => project.id === Number.parseInt(value))) {
            this.listProjectSelected.push(Number.parseInt(value));
          }
        });
      }

      this.getUserWorkingTimes();
    })
  }

  refresh() {
    this.getUserWorkingTimes();
  }

  onChangeListProjectIdSelected(event) {
    this.listProjectSelected = event;
    localStorage.setItem(this.keyLocalStorageWorkingTime, event.toString());
    this.refresh();
  }

  onFilter() {
    this.refresh();
  }


  totalWorkingTime(morningWorkingTime: string, afternoonWorkingTime: string): string {
    let morningWorkingHours = Number(morningWorkingTime.split(":")[0]);
    let morningWorkingMinutes = Number(morningWorkingTime.split(":")[1])?Number(morningWorkingTime.split(":")[1]):0;
    let afternoonWorkingHours = Number(afternoonWorkingTime.split(":")[0]);
    let afternoonWorkingMinutes = Number(afternoonWorkingTime.split(":")[1])?Number(afternoonWorkingTime.split(":")[1]):0;
    let totalMinutes = morningWorkingMinutes + afternoonWorkingMinutes;
    let totalHours = 0;
    if(totalMinutes >= 60) {
      totalHours = morningWorkingHours + afternoonWorkingHours +  Math.floor(totalMinutes / 60);
      totalMinutes = totalMinutes - 60;
    }
    else {
      totalHours = morningWorkingHours + afternoonWorkingHours;
    }
    return this.zeroPad(totalHours)+":"+this.zeroPad(totalMinutes);
  }

  zeroPad(num) {
    var str = String(num);
    if (str.length < 2) {
      return '0' + str;
    }
    return str;
  }

  checkStatusApprove(valueStatus): Boolean {
    if(valueStatus === 2) {
      return true;
    }
    return false;
  }

  checkStatusReject(valueStatus): Boolean {
    if(valueStatus === 3) {
      return true;
    }
    return false;
  }

  approveWorkingTime(u) {
    abp.message.confirm(
      `Approve working time of ${u.fullName}?`,
      (result: boolean) => {
        if (result) {
          this.workingTimeService.updateApproveWorkingTime(u.id).subscribe((resData) => {
            this.notify.success(this.l(`Approve working time of ${u.fullName} Successfully`));
            this.refresh();
          })
        }
      }
    );
  }

  rejectWorkingTime(u) {
    abp.message.confirm(
      `Reject working time of ${u.fullName}??`,
      (result: boolean) => {
        if (result) {
          this.workingTimeService.updateRejectWorkingTime(u.id).subscribe((resData) => {
            this.notify.success(this.l(`Reject working time of ${u.fullName} Successfully`));
            this.refresh();
          })
        }
      }
    );
  }

}
