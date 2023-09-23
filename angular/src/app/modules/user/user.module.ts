import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { NgxPaginationModule } from 'ngx-pagination';
import { UserSecondComponent } from './user.component';
import { UserRoutingModule } from './user-routing.module';
import { CreateUserComponent } from './create-user/create-user.component';
import { RoleUserComponent } from './role-user/role-user.component';
import { UpdateUserComponent } from './update-user/update-user.component';
import { UploadComponent } from '@shared/upload/upload.component';
import { UploadAvatarComponent } from './upload-avatar/upload-avatar.component';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { ImageCropperModule } from 'ngx-image-cropper';
import { NgxCurrencyModule } from "ngx-currency";
import { ClickOutsideModule } from 'ng-click-outside';
import {DragDropModule} from '@angular/cdk/drag-drop';
import { MAT_DATE_LOCALE } from '@angular/material';
import { ImportUserWorkingTimeComponent } from './import-user-working-time/import-user-working-time.component';


@NgModule({
  declarations: [UserSecondComponent, CreateUserComponent,UpdateUserComponent,RoleUserComponent, ImportUserWorkingTimeComponent],
  imports: [
    CommonModule,
    UserRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    NgxPaginationModule,
    FormsModule,
    NgxMatSelectSearchModule,
    ImageCropperModule,
    NgxCurrencyModule,
    ClickOutsideModule,
    DragDropModule
  ],
  entryComponents: [
    CreateUserComponent,
    RoleUserComponent,
    UpdateUserComponent,
    UploadComponent,
    ImportUserWorkingTimeComponent
  ],
  exports: [
    
  ],
  providers: [
    { provide: MAT_DATE_LOCALE, useValue: 'en-GB' }
  ]
})
export class UserSModule { }

