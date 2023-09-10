import { Component, Inject, Injector, OnInit } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DATE_FORMATS, MAT_DATE_LOCALE, MAT_DIALOG_DATA } from '@angular/material';
import { MomentDateAdapter } from '@angular/material-moment-adapter';
import { GetInfoToUpdateProfileDto, ItemInfoDto, MyProfileDto, UpdateUserInfoDto } from '@app/service/api/model/my-profile-dto';
import { MyProfileService } from '@app/service/api/my-profile.service';
import { AppComponentBase } from '@shared/app-component-base';
import { APP_DATE_FORMATS } from '@shared/custom-date-adapter';
import { DateAdapter } from '@angular/material/core';

import * as moment from 'moment';
import { MessageWarningChooseBankComponent } from './message-warning-choose-bank/message-warning-choose-bank.component';

@Component({
  selector: 'app-update-user-profile',
  templateUrl: './update-user-profile.component.html',
  styleUrls: ['./update-user-profile.component.css'],
  providers: [

    { provide: DateAdapter, useClass: MomentDateAdapter, deps: [MAT_DATE_LOCALE] },
    { provide: MAT_DATE_LOCALE, useValue: 'en-GB' },
    {
      provide: MAT_DATE_FORMATS,
      useValue: APP_DATE_FORMATS
    }
  ]
})
export class UpdateUserProfileComponent extends AppComponentBase implements OnInit {

  constructor(@Inject(MAT_DIALOG_DATA) public data: any,
    injector: Injector,
    private myProfileService: MyProfileService,
    public dialogRef: MatDialogRef<UpdateUserProfileComponent>,
    public dialog: MatDialog,

  ) {

    super(injector);

  }

  public userEmail: string;
  public userProfile = {} as GetInfoToUpdateProfileDto;
  public inputToUpdate = {} as UpdateUserInfoDto;
  public listBanks: ItemInfoDto[] = [];
  public listIssuedBys = [
    "CỤC CẢNH SÁT QUẢN LÝ HÀNH CHÍNH VỀ TRẬT TỰ XÃ HỘI",
    "CỤC CẢNH SÁT ĐKQL CƯ TRÚ VÀ DLQG VỀ DÂN CƯ"
  ]
  public listInsuranceStatus:string[] = [];
  public isLoading:boolean = false;
  public employeeBankIdBefore;


  ngOnInit() {
    this.userEmail = this.data.email;
    this.employeeBankIdBefore = this.data.bankId;
    console.log(this.data)
    this.getAllBanks();
    this.getInfoToUpdate();

  }
  public getAllBanks() {
    this.myProfileService.getAllBanks().subscribe((rs) => {
      this.listBanks = rs.result;
    })
  }



  public setDataToUpdate(){
    this.inputToUpdate = {
      email: this.userEmail,
      phone : this.userProfile.phone,
      birthday:this.userProfile.birthday? moment(this.userProfile.birthday).format("YYYY-MM-DD"): this.userProfile.birthday,
      bankId: this.userProfile.bankId,
      bankAccountNumber: this.userProfile.bankAccountNumber,
      taxCode: this.userProfile.taxCode,
      idCard: this.userProfile.idCard,
      address: this.userProfile.address,
      placeOfPermanent: this.userProfile.placeOfPermanent,
      issuedBy: this.userProfile.issuedBy,
      issuedOn: this.userProfile.issuedOn? moment(this.userProfile.issuedOn).format("YYYY-MM-DD"): this.userProfile.issuedOn,
    }
  }
  onSubmit() {
    this.setDataToUpdate()
    this.isLoading = true;
    this.myProfileService.updateUserInfo(this.inputToUpdate).subscribe((rs)=>{
      this.isLoading = false;
      this.dialogRef.close(rs.result);
    }, ()=> {this.isLoading = false});
  }

  public onPickIssuedBy(item: string){
    this.userProfile.issuedBy = item;
  }

  public getInfoToUpdate(){
    this.myProfileService.getInfoToUpdate(this.userEmail).subscribe((rs)=>{
      this.userProfile = rs.result;
    })
  }

  public onChangeBank(value){
    var checkDefault = this.listBanks.find(x=> x.id == value).isDefault;
    if(!checkDefault && this.employeeBankIdBefore){
      this.showMessage();
    }

  }

  public showMessage(){
    var dialog = this.dialog.open(MessageWarningChooseBankComponent);
    dialog.afterClosed().subscribe((rs)=>{
      this.userProfile.bankId = this.employeeBankIdBefore;
    })
  }

}
