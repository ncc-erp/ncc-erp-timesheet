import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MyProfileComponent } from './my-profile.component';
import { MyProfileRoutingModule } from './my-profile-routing.module';
import { SharedModule } from '@shared/shared.module';
import { FormsModule } from '@angular/forms';
import { UpdateUserProfileComponent } from './update-user-profile/update-user-profile.component';
import { MessageWarningChooseBankComponent } from './update-user-profile/message-warning-choose-bank/message-warning-choose-bank.component';

@NgModule({
  declarations: [MyProfileComponent, UpdateUserProfileComponent, MessageWarningChooseBankComponent],
  imports: [
    CommonModule,
    FormsModule,
    SharedModule,
    MyProfileRoutingModule
  ],
  entryComponents: [
    UpdateUserProfileComponent,
    MessageWarningChooseBankComponent
  ],
  
})
export class MyProfileModule { }
