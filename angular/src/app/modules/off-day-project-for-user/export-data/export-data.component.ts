import { Component, Injector, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import {  MatDialogRef } from '@angular/material';
import { APP_CONSTANT } from '@app/constant/api.constants';
import { AbsenceRequestService } from '@app/service/api/absence-request.service';
import { GetProjectDto } from '@app/service/api/model/project-Dto';
import { ProjectManagerService } from '@app/service/api/project-manager.service';
import { TeamBuildingPMService } from '@app/service/api/team-building-pm.service';
import { AppComponentBase } from '@shared/app-component-base';
import { BranchDto } from '@shared/service-proxies/service-proxies';
import * as FileSaver from 'file-saver';

@Component({
  selector: 'app-export-data',
  templateUrl: './export-data.component.html',
  styleUrls: ['./export-data.component.css']
})
export class ExportDataComponent extends AppComponentBase implements OnInit {
  FormExport: FormGroup
  isLoading = false;
  isDisabled: boolean = true;
  
  dayAbsentStatus = APP_CONSTANT.AbsenceStatusFilter['Pending'];
  dayAbsentStatusList = Object.keys(this.APP_CONSTANT.AbsenceStatusFilter)

  listProject: GetProjectDto[] = [];
  listProjectFiltered: GetProjectDto[] = [];
  listProjectSelected: number[] = [];

  dayAbsentTypeList = Object.keys(this.APP_CONSTANT.DayAbsenceType)
  dayTypeList = Object.keys(this.APP_CONSTANT.AbsenceType)
  absentDayType = -1;

  public listBranch: BranchDto[] = [];
  public branchId: string = "";
  public branchSearch: FormControl = new FormControl("");
  public listBranchFilter: BranchDto[];


  constructor(
    injector: Injector,
    private projectService: ProjectManagerService,
    private absenceService: AbsenceRequestService,
    private branchService: TeamBuildingPMService,
    private _dialogRef: MatDialogRef<ExportDataComponent>,
    private formBuilder: FormBuilder,
  ) {
    super(injector);
    this.getListProject();
    this.branchSearch.valueChanges.subscribe(() => {
    this.filterBranch();
    })
  }

  ngOnInit() {
    this.getListBranch();
    this.FormExport = this.formBuilder.group({
      fromDateCustomTime: ['', Validators.required],
      toDateCustomTime: ['', Validators.required]
    })
    this.FormExport.statusChanges.subscribe((status: string) => {
      this.isDisabled = status !== 'VALID';
    });
   
  }

  getListBranch() {
    this.branchService.getAllRequestByBranch().subscribe(res => {
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

  getListProject() {
    this.isLoading = true;
    this.projectService.getProjectUser().subscribe(res => { // get list project cua PM
      this.listProject = res.result;
      let data = localStorage.getItem("listProjectIdsOfUser");
      this.listProject.forEach(item => {
        if (data == null || data == "") {
          this.listProjectSelected.push(item.id);
        }
        if (item.code) {
          item.name = item.code + " - " + item.name;
        }
      });

      if (data !== null && data !== '') {
        data.split(",").forEach((value: string) => {
          if (this.listProject.some(project => project.id === Number.parseInt(value))) {
            this.listProjectSelected.push(Number.parseInt(value));
          }
        });
      }
    }, () => {
      this.isLoading = false;
      this.notify.error("An error has occured!");
    });
  }
  
    onChangeListProjectIdSelected(event) {
        this.listProjectSelected = event;
        localStorage.setItem('listProjectIdsOfUser', event.toString());
    }

  exportExcelTeamWorkingCalender() {
  this.isDisabled = true;
  const formData = this.FormExport.value; 
  const startDate = formData.fromDateCustomTime;
  const endDate = formData.toDateCustomTime;
  const dayAbsentStatus = this.dayAbsentStatus;
  const listProjectSelected = this.listProjectSelected;
  const branchId = this.branchId;
  const dayOffTypeId =-1;
  
    this.absenceService.ExportTeamWorkingCalender(startDate,endDate,listProjectSelected,this.absentDayType,dayAbsentStatus,branchId,dayOffTypeId).subscribe((rs) => {
      const file = new Blob([this.convertFile(atob(rs.result.base64))], {
        type: "application/vnd.ms-excel;charset=utf-8",
      });
      FileSaver.saveAs(file, "TeamWorkCalender.xlsx");
    });
  }

  private convertFile(fileData) {
    var buf = new ArrayBuffer(fileData.length);
    var view = new Uint8Array(buf);
    for (var i = 0; i != fileData.length; ++i)
      view[i] = fileData.charCodeAt(i) & 0xff;
    return buf;
  }

  close(res): void {
    this._dialogRef.close(res);
  }
}
