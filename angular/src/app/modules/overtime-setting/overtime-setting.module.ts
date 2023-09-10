import { NgxPaginationModule } from 'ngx-pagination';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OvertimeSettingRoutingModule } from './overtime-setting-routing.module';
import { OvertimeSettingComponent } from './overtime-setting.component';
import { SharedModule } from '@shared/shared.module';
import { CreateEditOvertimeSettingComponent } from './create-edit-overtime-setting/create-edit-overtime-setting.component';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
// import { LocalizePipe } from '@shared/pipes/localize.pipe';

@NgModule({
  declarations: [OvertimeSettingComponent, CreateEditOvertimeSettingComponent],
  imports: [
    CommonModule,
    OvertimeSettingRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    NgxPaginationModule,
    FormsModule,
    NgxMatSelectSearchModule,
    // LocalizePipe
  ],
  entryComponents: [
    CreateEditOvertimeSettingComponent
  ],
  exports: [
    CreateEditOvertimeSettingComponent
  ]
})
export class OvertimeSettingModule { }
