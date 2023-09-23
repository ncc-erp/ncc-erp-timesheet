import { AppComponentBase } from '@shared/app-component-base';
import { Component, OnInit, Injector, Inject } from '@angular/core';
import { AbsenceTypesDTO } from '../manage-absence-types.component';
import { MAT_DIALOG_DATA } from '@angular/material';

@Component({
  selector: 'app-manage-absence-types-create',
  templateUrl: './manage-absence-types-create.component.html',
  styleUrls: ['./manage-absence-types-create.component.css']
})
export class ManageAbsenceTypesCreateComponent extends AppComponentBase implements OnInit {
  absenceTypes = {} as AbsenceTypesDTO
  saving = false;
  title: string;
  active = true;
  isTrue :any
  day = false
  constructor(  injector: Injector,
    @Inject(MAT_DIALOG_DATA) public data: any) 
    {
      super(injector)
     }
  ngOnInit() {
    this.absenceTypes = this.data
  }
  check(data){
     if(data.status==1) {
        data.length="";
      this.day=false
     }else if (data.status==2) {
       data.length=0;
      this.day= true
      }
      
  }
}
