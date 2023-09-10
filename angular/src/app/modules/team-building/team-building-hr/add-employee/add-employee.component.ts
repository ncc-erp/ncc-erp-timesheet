import { MomentDateAdapter, MAT_MOMENT_DATE_ADAPTER_OPTIONS } from '@angular/material-moment-adapter';
import { DateAdapter, MAT_DATE_LOCALE, MAT_DATE_FORMATS } from '@angular/material/core';
import { MatDatepicker } from '@angular/material/datepicker';
import { FormControl } from '@angular/forms';
import { MatDialogRef, MatInput } from '@node_modules/@angular/material';
import { TeamBuildingHRService } from '@app/service/api/team-building-hr.service';
import { AppComponentBase } from '@shared/app-component-base';
import { Component, Injector, OnInit, ViewChild } from '@angular/core';
import * as _moment from 'moment';
import { DATE_FORMATS, GetProjectDto, GetProjectUserDto, InputToAddEmployee } from '@app/modules/team-building/const/const';
import * as _ from 'lodash';

@Component({
  selector: 'app-add-employee',
  templateUrl: './add-employee.component.html',
  styleUrls: ['./add-employee.component.css'],
  providers: [
    {
      provide: DateAdapter,
      useClass: MomentDateAdapter,
      deps: [MAT_DATE_LOCALE, MAT_MOMENT_DATE_ADAPTER_OPTIONS]
    },

    { provide: MAT_DATE_FORMATS, useValue: DATE_FORMATS }
  ]
})
export class AddEmployeeComponent extends AppComponentBase implements OnInit {
  public isLoading: boolean = false;
  public listProjects = {} as GetProjectDto;

  public listProjectUsers: GetProjectUserDto [] = [];
  public listProjectUsersFilter: GetProjectUserDto [] = [];
  public userSearch: string = "";
  public inputToCreate = {} as InputToAddEmployee;
  public saving: boolean = false;
  public date = new FormControl(_moment());
  public title: string;
  public active: boolean = true;
  public currentDate = new Date();
  disableSelect: boolean = false;
  @ViewChild('input', { read: MatInput }) input: MatInput;

  constructor(injector: Injector,
    private teamBuildingHRService: TeamBuildingHRService,
    private dialogRef: MatDialogRef<AddEmployeeComponent>) {
    super(injector);
  }

  ngOnInit() {
    this.getProjectUser();
    this.inputToCreate.year = this.currentDate.getFullYear();
    this.inputToCreate.month = this.currentDate.getMonth() + 1;
  }

  getProjectUser(): void {
    this.teamBuildingHRService.getAllProjectUser().subscribe(res => {
      this.listProjectUsers = this.listProjectUsersFilter = res.result;
    });
  }

  handleSearchUser() {
    const textSearch = this.userSearch.toLowerCase().trim();
    if (textSearch) {
      this.listProjectUsers = this.listProjectUsersFilter.filter((item) =>
        item.fullName.toLowerCase().trim().includes(textSearch)
      );
    } else {
      this.listProjectUsers = this.listProjectUsersFilter;
    }
  }

  onSaveAndClose() {
    this.saving = true;
    this.teamBuildingHRService.addNew(this.inputToCreate).subscribe((rs) => {
      if (rs) {
        abp.notify.success("Added successful!");
        this.dialogRef.close(true);
        this.saving = false;
      }
    }, () => this.saving = false);
  }

  close() {
    this.dialogRef.close(false);
  }
  chosenYearHandler(normalizedYear: _moment.Moment) {
    const ctrlValue = this.date.value;
    ctrlValue.year(normalizedYear.year());
    this.date.setValue(ctrlValue);
  }

  chosenMonthHandler(normalizedMonth: _moment.Moment, datepicker: MatDatepicker<_moment.Moment>) {
    const ctrlValue = this.date.value;
    ctrlValue.month(normalizedMonth.month());
    this.date.setValue(ctrlValue);
    datepicker.close();
    let reviewDate = new Date(ctrlValue._d);
    this.inputToCreate.year = reviewDate.getFullYear();
    this.inputToCreate.month = reviewDate.getMonth() + 1;
  }

  clearInput() {}
}



