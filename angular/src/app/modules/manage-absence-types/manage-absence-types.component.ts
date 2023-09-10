import { PagedListingComponentBase } from 'shared/paged-listing-component-base';
import { ManageAbsenceTypesService } from './../../service/api/manage-absence-types.service';
import { ManageAbsenceTypesCreateComponent } from './manage-absence-types-edit/manage-absence-types-create.component';
import { Component, OnInit, inject, Injector } from '@angular/core';
import { MatDialog } from '@angular/material';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';

@Component({
  selector: 'app-manage-absence-types',
  templateUrl: './manage-absence-types.component.html',
  styleUrls: ['./manage-absence-types.component.css']
})
export class ManageAbsenceTypesComponent extends PagedListingComponentBase<AbsenceTypesDTO> implements OnInit {
  ADD_LEAVE_TYPE = PERMISSIONS_CONSTANT.AddLeaveType;
  EDIT_LEAVE_TYPE = PERMISSIONS_CONSTANT.EditLeaveType;
  DELETE_LEAVE_TYPE = PERMISSIONS_CONSTANT.DeleteLeaveType;
  
  protected list(request: import("../../../shared/paged-listing-component-base").PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
    throw new Error("Method not implemented.");
  }
  protected delete(entity: AbsenceTypesDTO): void {
    throw new Error("Method not implemented.");
  }
  isTableLoading: boolean;

  constructor(
    inject : Injector,
    private manageAbsenceTypesService: ManageAbsenceTypesService,
    private _dialog: MatDialog,

  ) {
    super(inject)
   }
  viewDate: Date;
  year;
  month;
  isTrue = true;
  absenceTypes = []
  ngOnInit() {
    this.getAllData()
  }
  getAllData() {
    this.isTableLoading = true;
    this.manageAbsenceTypesService.getAll().subscribe(res => {
      this.absenceTypes = res.result;
      this.isTableLoading = false;
    });
  }
  createAbsence(): void {
    this.manageAbsenceTypesService.getAll().subscribe(data => { 
    })
    let absenceTypes = {} as AbsenceTypesDTO
    this.showDialog(absenceTypes)
  }

  showDialog(absenceTypes: AbsenceTypesDTO): void {
    let item = { id: absenceTypes.id, name: absenceTypes.name, length: absenceTypes.length, status: absenceTypes.status } as AbsenceTypesDTO;
    const dialogRef = this._dialog.open(ManageAbsenceTypesCreateComponent, {
      disableClose : true,
      data: item
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.manageAbsenceTypesService.save(result).subscribe(res => {
          abp.notify.success('Save Absence Types : ' + result.name);
          this.getAllData()
        })
      
      }
    });
  }
  deleteTask(absence): void {
    abp.message.confirm(
      "Delete project: '" + absence.name + "'?",
      (result: boolean) => {
        if (result) {
          this.manageAbsenceTypesService.delete(absence.id).subscribe(res => {
            abp.notify.success('Delete Task Successfully');
            this.getAllData();
          });
        }
      })
  }
  editTask(task): void {
    if(task.status==2){ 
      this.isTrue= false
    } 
    this.showDialog(task);
  }
}
export class AbsenceTypesDTO {
  id: number;
  name: string;
  length: number;
  status: number;
}